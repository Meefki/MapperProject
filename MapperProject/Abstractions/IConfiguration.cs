using MapperProject.Models;
using System.Linq.Expressions;

namespace MapperProject.Abstractions;

public interface IConfiguration<TDest, TSource>
{
    public Type DestType { get; }
    public Type SourceType { get; }
    public IReadOnlyCollection<Configuration> NestedConfigurations { get; }

    public IPropertyBuilder<TDest, TSource, TProperty> Property<TProperty>(Expression<Func<TDest, TProperty>> propertyExpression);
    public IPropertyBuilder<TDest, TSource, TProperty> Property<TProperty>(string propertyName);
    public void NestedProperty<TDestProperty, TSourceProperty>(
        Expression<Func<TDest, TDestProperty>> destPropertyExpression, 
        Expression<Func<TSource, TSourceProperty>> sourcePropertyExpression, 
        Action<Configuration<TDestProperty, TSourceProperty>> config)
        where TDestProperty : class
        where TSourceProperty : class;
}
