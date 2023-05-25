using MapperProject.Abstractions;
using MapperProject.Models;
using System.Reflection;

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

    public void AddConfiguration<TConfiguration>()
    {
        //_configurations.Add
    }

    public TDest Map<TDest, TSource>(TSource source)
        where TDest : class
        where TSource : class
    {
        if (_configurations.FirstOrDefault(c => c.DestType == typeof(TDest) && 
                                           c.SourceType == typeof(TSource)) 
            is not Configuration<TDest, TSource> configuration)
        {
            throw new InvalidOperationException($"Cannot find a configuration for the destination type {typeof(TDest).FullName ?? typeof(TDest).Name} and " +
                $"the source type {typeof(TSource).FullName ?? typeof(TSource).Name}");
        }

        ConstructorInfo destConstructorInfo = configuration.DestType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null)
            ?? throw new InvalidOperationException($"Couldn't reach the parameterless constructor of {configuration.DestType.FullName ?? configuration.DestType.Name} type");

        TDest dest = (TDest)destConstructorInfo.Invoke(null);

        var destProperties = typeof(TDest).GetProperties();

        foreach (var prop in destProperties)
        {
            var propertyBuilder = configuration.PropertyBuilders.FirstOrDefault(pb => pb.DestPropertyName == prop.Name);

            string destPropertyName = prop.Name;
            string? destFieldName = null;

            string sourcePropertyName = prop.Name;
            string? sourceFieldName = null;

            if (propertyBuilder is not null)
            {
                if (propertyBuilder.IsIgnored)
                    continue;

                destPropertyName = propertyBuilder.DestPropertyName;
                destFieldName = propertyBuilder.DestFieldName;
                sourcePropertyName = propertyBuilder.SourcePropertyName ?? propertyBuilder.DestPropertyName;
                sourceFieldName = propertyBuilder.SourceFieldName;
            }

            try
            {
                FieldInfo? destField = (string.IsNullOrWhiteSpace(destFieldName) || typeof(TDest).GetField(destFieldName) is null ?
                FindBackingField<TDest>(destPropertyName) :
                typeof(TDest).GetField(destFieldName)) ??
                throw new ArgumentException($"Cannot find a field of property {destPropertyName} of {typeof(TDest).FullName ?? typeof(TDest).Name}");

                FieldInfo? sourceField = (string.IsNullOrEmpty(sourceFieldName) || typeof(TSource).GetField(sourceFieldName) is null ?
                    FindBackingField<TSource>(sourcePropertyName) :
                    typeof(TSource).GetField(sourceFieldName)) ??
                    throw new ArgumentException($"Cannot find a field of property {sourceFieldName} of {typeof(TSource).FullName ?? typeof(TSource).Name}");

                var sourceValue = sourceField.GetValue(source);
                destField.SetValue(dest, sourceValue);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERR] ");
                Console.ResetColor();
                Console.Write($"{DateTime.Now:G}: ");
                Console.WriteLine(ex.Message);

                throw;
            }
        }

        return dest;
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
