using MapperProject.Abstractions;
using System.Linq.Expressions;

namespace MapperProject.Models;

public abstract class Configuration
{
    public abstract Type DestType { get; }
    public abstract Type SourceType { get; }
}

public class Configuration<TDest, TSource>
    : Configuration,
    IConfiguration<TDest, TSource>
    where TDest : class
    where TSource : class
{
    private readonly List<IPropertyBuilder> _propertyBuilders;
    public IReadOnlyCollection<IPropertyBuilder> PropertyBuilders => _propertyBuilders.AsReadOnly();

    public override Type DestType => typeof(TDest);
    public override Type SourceType => typeof(TSource);

    public Configuration()
    {
        _propertyBuilders = new();
        Configure(this);
    }

    public virtual void Configure(Configuration<TDest, TSource> config)
    {
        throw new NotImplementedException();
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

        PropertyBuilder<TDest, TSource, TProperty> propertyBuilder = new(propertyName, false, null);

        _propertyBuilders.Add(propertyBuilder);

        return propertyBuilder;
    }

    //public IPropertyBuilder<TDest, TSource, TProperty> NestedProperty<TProperty>(Expression<Func<TDest, TProperty>> propertyExpression)
    //{
    //    string propertyName = GetPropertyNameFromExpression(propertyExpression);

    //    return NestedProperty<TProperty>(propertyName);
    //}

    //public IPropertyBuilder<TDest, TSource, TProperty> NestedProperty<TProperty>(string propertyName)
    //{
    //    if (IsBuilderExist<TProperty>(propertyName))
    //    {
    //        throw new ArgumentException($"Property builder for the property {propertyName} " +
    //            $"for mapping from {GetTypeName<TDest>()} to {GetTypeName<TSource>()} already exists", nameof(propertyName));
    //    }

    //    PropertyBuilder<TDest, TSource, TProperty> propertyBuilder = new(propertyName, true, this.GetType().GenericTypeArguments[0]);

    //    _propertyBuilders.Add(propertyBuilder);

    //    return propertyBuilder;
    //}

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
}
