using MapperProject.Abstractions;
using MapperProject.Models;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MapperProject;

public class Mapper : IMapper
{
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
        var configuration = GetConfiguration<TDest, TSource>(_configurations);

        var destConstructorInfo = GetConstructorInfo(configuration);

        // Creating an instasce of the destination type
        dest ??= (TDest)destConstructorInfo.Invoke(null);

        // Mapping processing
        var destProperties = typeof(TDest).GetProperties();

        foreach (var prop in destProperties)
        {
            bool isNestedConfigExist = configuration.NestedConfigurations.ToList().Exists(nc => nc.DestType == prop.PropertyType);

            if (!isNestedConfigExist)
                MapAsFlatProperty(dest, source, prop, configuration);

            if (isNestedConfigExist)
                MapAsNestedProperty(dest, source, prop, configuration);
        }

        return dest;
    }

    private static Configuration<TDest, TSource> GetConfiguration<TDest, TSource>(IEnumerable<Configuration> configurations)
        where TDest : class
        where TSource: class
    {
        // Try to find a configuration for the types mapping
        if (configurations.FirstOrDefault(c => c.DestType == typeof(TDest) &&
                                           c.SourceType == typeof(TSource))
            is not Configuration<TDest, TSource> configuration)
        {
            throw new InvalidOperationException($"Cannot find a configuration for the destination type {typeof(TDest).FullName ?? typeof(TDest).Name} and " +
                $"the source type {typeof(TSource).FullName ?? typeof(TSource).Name}");
        }

        return configuration;
    }

    private static ConstructorInfo GetConstructorInfo<TDest, TSource>(Configuration<TDest, TSource> configuration)
        where TDest : class
        where TSource : class
    {
        // Try to get a parameterless constructor from a type
        ConstructorInfo constructorInfo = configuration.DestType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null)
            ?? throw new InvalidOperationException($"Couldn't reach the parameterless constructor of {configuration.DestType.FullName ?? configuration.DestType.Name} type");

        return constructorInfo;
    }

    private static void MapAsNestedProperty<TDest, TSource>(TDest dest, TSource source, PropertyInfo prop, Configuration<TDest, TSource> configuration)
        where TDest : class
        where TSource : class
    {
        // TODO:
        // 1. get nested config (we already know it is exist if we are here)
        // 2. get dest property instance (prop) somehow...
        // 3. get source property (or not then it gonna be easier) instance (at first by configuration.SourceType, at second by name I guess, but what the name will it be?) somehow...
        // 4. call a main map method with the source and the dest objects as params
        // 5. well done!
    }

    private static void MapAsFlatProperty<TDest, TSource>(TDest dest, TSource source, PropertyInfo prop, Configuration<TDest, TSource> configuration)
        where TDest : class
        where TSource : class
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
        FieldInfo? destField = (string.IsNullOrWhiteSpace(destFieldName) || typeof(TDest).GetField(destFieldName) is null ?
        FindBackingField<TDest>(destPropertyName) :
        typeof(TDest).GetField(destFieldName)) ??
        throw new ArgumentException($"Cannot find a field of property {destPropertyName} of {typeof(TDest).FullName ?? typeof(TDest).Name}");

        // Finding a source field by a property name
        FieldInfo? sourceField = (string.IsNullOrEmpty(sourceFieldName) || typeof(TSource).GetField(sourceFieldName) is null ?
            FindBackingField<TSource>(sourcePropertyName) :
            typeof(TSource).GetField(sourceFieldName)) ??
            throw new ArgumentException($"Cannot find a field of property {sourceFieldName} of {typeof(TSource).FullName ?? typeof(TSource).Name}");

        // Put value from a source object to a dest object
        var sourceValue = sourceField.GetValue(source);
        destField.SetValue(dest, sourceValue);
    }

    private static FieldInfo FindBackingField<T>(string propertyName)
    {
        FieldInfo? fieldInfo = GetField<T>(BackingFieldConvention.BackingFieldFormats.First(), propertyName);

        propertyName = PropertyNameToFieldName(propertyName);
        if (fieldInfo is null)
        {
            foreach (var backingPropertyNameFormat in BackingFieldConvention.FieldFormats)
            {
                fieldInfo = GetField<T>(backingPropertyNameFormat, propertyName);
                if (fieldInfo is not null)
                    break;
            }
        }

        if (fieldInfo is null)
            throw new ArgumentException($"Cannot find backing field with name {propertyName}");

        return fieldInfo!;
    }

    private static FieldInfo? GetField<T>(string format, string value)
    {
        string fieldName = string.Format(format, value);

        return typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private static string PropertyNameToFieldName(string propertyName)
    {
        return propertyName[..1].ToLower() + propertyName[1..];
    }
}
