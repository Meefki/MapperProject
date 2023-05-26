using LanguageExt;
using MapperProject.Abstractions;
using MapperProject.Models;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MapperProject;

public class Mapper : IMapper
{
    private static readonly Func<PropertyInfo, Configuration, bool> isNestedConfiguration = 
        (pi, cfg) => 
            pi.PropertyType == cfg.DestType && 
            cfg.PropertyBuilders
                .Exists(pb => pb.DestPropertyName == pi.Name);

    private readonly List<Configuration> _configurations;

    public Mapper()
    {
        _configurations = new();
    }

    public void AddConfiguration<TDest, TSource>(Action<Configuration<TDest, TSource>> configurationAction)
        where TDest : class
        where TSource : class
    {
        Configuration<TDest, TSource> configuration = new();
        configurationAction(configuration);

        _configurations.Add(configuration);
    }

    public void AddConfiguration<TDest, TSource>(Configuration<TDest, TSource> configuration)
        where TDest : class
        where TSource : class
    {
        _configurations.Add(configuration);
    }

    public TDest Map<TDest, TSource>(TSource source, TDest? dest = null)
        where TDest : class
        where TSource : class
    {
        return (TDest)Map(source, typeof(TSource), typeof(TDest), dest);
    }

    private object Map(
        object source, 
        Type sourceType, 
        Type destType, 
        object? dest = null,
        IEnumerable<Configuration>? configurations = null)
    {
        var configuration = GetConfiguration(configurations ?? _configurations, destType, sourceType);

        if (configuration is null)
        {
            throw new InvalidOperationException($"Cannot find a configuration for the destination type {destType.FullName ?? destType.Name} and " +
                $"the source type {sourceType.FullName ?? sourceType.Name}");
        }

        var destConstructorInfo = GetParameterlessConstructorInfo(configuration);

        // Creating an instasce of the destination type
        dest ??= destConstructorInfo.Invoke(null);

        // Mapping processing
        var destProperties = destType.GetProperties();

        foreach (var prop in destProperties)
        {
            bool isNestedConfigExist = configuration.NestedConfigurations.ToList().Exists(nc => isNestedConfiguration(prop, nc));

            if (!isNestedConfigExist)
                MapAsFlatProperty(dest, source, destType, sourceType, prop, configuration);

            if (isNestedConfigExist)
                MapAsNestedProperty(dest, source, destType, sourceType, prop, configuration);
        }

        return dest;
    }

    private void MapAsNestedProperty(
        object dest, 
        object source,
        Type destType,
        Type sourceType,
        PropertyInfo destProp, 
        Configuration configuration)
    {
        // get nested config
        Configuration nestedConfig = configuration.NestedConfigurations.ToList().Find(nc => isNestedConfiguration(destProp, nc))!;

        // get source property instance
        object? sourcePropInstance = null;
        if (sourceType == nestedConfig.SourceType)
            sourcePropInstance = source;

        if (sourcePropInstance is null)
        {
            IPropertyBuilder? propertyBuilder = nestedConfig.PropertyBuilders.FirstOrDefault(pb => pb.PropertyType == nestedConfig.DestType);

            if (propertyBuilder is null || string.IsNullOrWhiteSpace(propertyBuilder.SourcePropertyName))
                throw new InvalidOperationException($"Coudn't find a builder for nested type mapping {nestedConfig.DestType.FullName ?? nestedConfig.DestType.Name}");

            FieldInfo sourceFieldInfo = FindBackingField(propertyBuilder.SourcePropertyName!, sourceType);
            sourcePropInstance = sourceFieldInfo.GetValue(source);
        }

        // get dest property instance
        var destPropInstance = destProp.GetValue(dest);
        if (destPropInstance is null)
        {
            var constructorInfo = GetParameterlessConstructorInfo(nestedConfig);
            destPropInstance = constructorInfo.Invoke(null);

            FieldInfo destFieldInfo = FindBackingField(destProp.Name, destType);
            destFieldInfo.SetValue(dest, destPropInstance);
        }

        Map(sourcePropInstance!, nestedConfig.SourceType, nestedConfig.DestType, destPropInstance, configuration.NestedConfigurations);
    }

    private static void MapAsFlatProperty(
        object dest,
        object source,
        Type destType,
        Type sourceType,
        PropertyInfo prop, 
        Configuration configuration)
    {
        // Finding builder for the current property and fill names of properties/fields
        var propertyBuilder = configuration.PropertyBuilders.FirstOrDefault(pb => pb.DestPropertyName == prop.Name);

        string destPropertyName = prop.Name;
        string? destFieldName = null;

        string sourcePropertyName = prop.Name;
        string? sourceFieldName = null;

        if (propertyBuilder is not null)
        {
            // Skip iteration if a field is configured as ignorable
            if (propertyBuilder.IsIgnored)
                return;

            destPropertyName = propertyBuilder.DestPropertyName;
            destFieldName = propertyBuilder.DestFieldName;
            sourcePropertyName = propertyBuilder.SourcePropertyName ?? propertyBuilder.DestPropertyName;
            sourceFieldName = propertyBuilder.SourceFieldName;
        }

        // Finding a destination field by a property name
        FieldInfo? destField = (string.IsNullOrWhiteSpace(destFieldName) || destType.GetField(destFieldName) is null ?
        FindBackingField(destPropertyName, destType) :
        destType.GetField(destFieldName)) ??
        throw new ArgumentException($"Cannot find a field of property {destPropertyName} of {destType.FullName ?? destType.Name}");

        // Finding a source field by a property name
        FieldInfo? sourceField = (string.IsNullOrEmpty(sourceFieldName) || sourceType.GetField(sourceFieldName) is null ?
            FindBackingField(sourcePropertyName, sourceType) :
            sourceType.GetField(sourceFieldName)) ??
            throw new ArgumentException($"Cannot find a field of property {sourceFieldName} of {sourceType.FullName ?? sourceType.Name}");

        // Put value from a source object to a dest object
        var sourceValue = sourceField.GetValue(source);
        destField.SetValue(dest, sourceValue);
    }

    private static Configuration? GetConfiguration(IEnumerable<Configuration> configurations, Type destType, Type sourceType)
    {
        // Try to find a configuration for the types mapping
        Configuration? configuration = configurations.FirstOrDefault(c => c.DestType == destType && c.SourceType == sourceType);

        return configuration;
    }

    private static ConstructorInfo GetParameterlessConstructorInfo(Configuration configuration)
    {
        // Try to get a parameterless constructor from a type
        ConstructorInfo constructorInfo = configuration.DestType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null)
            ?? throw new InvalidOperationException($"Couldn't reach the parameterless constructor of {configuration.DestType.FullName ?? configuration.DestType.Name} type");

        return constructorInfo;
    }

    private static FieldInfo FindBackingField(string propertyName, Type propertyType)
    {
        FieldInfo? fieldInfo = GetField(BackingFieldConvention.BackingFieldFormats.First(), propertyName, propertyType);

        propertyName = PropertyNameToFieldName(propertyName);
        if (fieldInfo is null)
        {
            foreach (var backingPropertyNameFormat in BackingFieldConvention.FieldFormats)
            {
                fieldInfo = GetField(backingPropertyNameFormat, propertyName, propertyType);
                if (fieldInfo is not null)
                    break;
            }
        }

        if (fieldInfo is null)
            throw new ArgumentException($"Cannot find backing field with name {propertyName}");

        return fieldInfo!;
    }

    private static FieldInfo? GetField(string format, string value, Type type)
    {
        string fieldName = string.Format(format, value);

        return type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static string PropertyNameToFieldName(string propertyName)
    {
        return propertyName[..1].ToLower() + propertyName[1..];
    }
}
