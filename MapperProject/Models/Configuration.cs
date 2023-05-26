using MapperProject.Abstractions;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace MapperProject.Models;

public abstract class Configuration
{
    public abstract Type DestType { get; }
    public abstract Type SourceType { get; }
    public abstract IReadOnlyCollection<Configuration> NestedConfigurations { get; }
    public abstract IReadOnlyCollection<IPropertyBuilder> PropertyBuilders { get; }
}

public class Configuration<TDest, TSource>
    : Configuration,
    IConfiguration<TDest, TSource>
    where TDest : class
    where TSource : class
{
    private readonly List<Configuration> _nestedConfigurations;
    public override IReadOnlyCollection<Configuration> NestedConfigurations => _nestedConfigurations.AsReadOnly();

    private readonly List<IPropertyBuilder> _propertyBuilders;
    public override IReadOnlyCollection<IPropertyBuilder> PropertyBuilders => _propertyBuilders.AsReadOnly();

    public override Type DestType => typeof(TDest);
    public override Type SourceType => typeof(TSource);

    public Configuration()
    {
        _nestedConfigurations = new();
        _propertyBuilders = new();
        Configure(this);
    }

    public virtual void Configure(Configuration<TDest, TSource> config)
    {
        _propertyBuilders.AddRange(config._propertyBuilders);
        _nestedConfigurations.AddRange(config._nestedConfigurations);
    }

    private void AddPropertyBuilder<D, S, TProperty>(PropertyBuilder<D, S, TProperty> propertyBuilder)
    {
        string propertyName = propertyBuilder.DestPropertyName;

        if (IsBuilderExist<TProperty>(propertyName))
        {
            throw new ArgumentException($"Property builder for the property {propertyName} " +
                $"for mapping from {GetTypeName<D>()} to {GetTypeName<S>()} already exists", nameof(propertyName));
        }

        _propertyBuilders.Add(propertyBuilder);
    }

    public IPropertyBuilder<TDest, TSource, TProperty> Property<TProperty>(Expression<Func<TDest, TProperty>> propertyExpression)
    {
        string propertyName = GetPropertyNameFromExpression(propertyExpression);

        return Property<TProperty>(propertyName);
    }

    public IPropertyBuilder<TDest, TSource, TProperty> Property<TProperty>(string propertyName)
    {
        if (IsBuilderExist<TProperty>(propertyName))
        {
            throw new ArgumentException($"Property builder for the property {propertyName} " +
                $"for mapping from {GetTypeName<TDest>()} to {GetTypeName<TSource>()} already exists", nameof(propertyName));
        }

        PropertyBuilder<TDest, TSource, TProperty> propertyBuilder = new(propertyName);

        _propertyBuilders.Add(propertyBuilder);

        return propertyBuilder;
    }

    private string GetPropertyNameFromExpression<TProperty>(Expression<Func<TDest, TProperty>> propertyExpression)
    {
        var expression = (MemberExpression)propertyExpression.Body;
        string propertyName = expression.Member.Name;

        return propertyName;
    }

    private bool IsBuilderExist<TProperty>(string propertyName)
    {
        var existingPropertyBuilder = _propertyBuilders.FirstOrDefault(pb => pb.SourcePropertyName == propertyName);
        return existingPropertyBuilder is not null &&
            existingPropertyBuilder.GetType().GenericTypeArguments[0] == typeof(TDest) &&
            existingPropertyBuilder.GetType().GenericTypeArguments[1] == typeof(TSource) &&
            existingPropertyBuilder.GetType().GenericTypeArguments[2] == typeof(TProperty);
    }

    private string GetTypeName<T>()
    {
        return typeof(T).FullName ?? typeof(T).Name;
    }

    public void NestedProperty<TDestProperty, TSourceProperty>(Expression<Func<TDest, TDestProperty>> destPropertyExpression, Expression<Func<TSource, TSourceProperty>> sourcePropertyExpression, Action<Configuration<TDestProperty, TSourceProperty>> configAction)
        where TDestProperty : class
        where TSourceProperty : class
    {
        Configuration<TDestProperty, TSourceProperty> nestedConfig = new();

        var destExpression = (MemberExpression)destPropertyExpression.Body;
        var destPropertyName = destExpression.Member.Name;

        var sourceExpression = sourcePropertyExpression.Body;
        string? sourcePropertyName = sourceExpression.NodeType switch
        {
            ExpressionType.MemberAccess => ((MemberExpression)sourceExpression).Member.Name,
            ExpressionType.Parameter => null,
            _ => throw new ArgumentException($"Unsupported type of expression {nameof(sourcePropertyExpression)}")
        };

        PropertyBuilder<TDestProperty, TSourceProperty, TDestProperty> propertyBuilder = new(destPropertyName, sourcePropertyName);
        nestedConfig.AddPropertyBuilder(propertyBuilder);

        configAction(nestedConfig);
        _nestedConfigurations.Add(nestedConfig);
    }
}
