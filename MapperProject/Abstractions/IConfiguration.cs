using System.Linq.Expressions;

namespace MapperProject.Abstractions;

public interface IConfiguration<TDest, TSource>
{
    public Type DestType { get; }
    public Type SourceType { get; }

    public IPropertyBuilder<TDest, TSource, TProperty> Property<TProperty>(Expression<Func<TDest, TProperty>> propertyExpression);
    public IPropertyBuilder<TDest, TSource, TProperty> Property<TProperty>(string propertyName);

    //public IPropertyBuilder<TDest, TSource, TProperty> NestedProperty<TProperty>(Expression<Func<TDest, TProperty>> propertyExpression);
    //public IPropertyBuilder<TDest, TSource, TProperty> NestedProperty<TProperty>(string propertyName);
}
