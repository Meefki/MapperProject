using MapperProject.Abstractions;
using System.Linq.Expressions;

namespace MapperProject.Models;

public class PropertyBuilder<TDest, TSource, TProperty>
    : IPropertyBuilder, 
    IPropertyBuilder<TDest, TSource, TProperty>
{
    public Type PropertyType => typeof(TProperty);

    public string DestPropertyName { get; private set; }
    public string? DestFieldName { get; private set; }

    public string? SourcePropertyName { get; private set; }
    public string? SourceFieldName { get; private set; }

    public bool IsIgnored { get; private set; }

    public bool IsNested { get; private set; }
    public Type? ParentType { get; private set; }

    public PropertyBuilder(
        string name, 
        bool isNested,
        Type? parentType)
    {
        DestPropertyName = name;
        IsIgnored = false;
        IsNested = isNested;

        if (isNested && parentType is null)
            throw new ArgumentException($"Parameter {nameof(parentType)} can't be null when property is nested");

        ParentType = parentType;
    }

    public IPropertyBuilder<TDest, TSource, TProperty> HasField(string fieldName)
    {
        DestFieldName = fieldName;

        return this;
    }

    public IPropertyBuilder<TDest, TSource, TProperty> MapFrom(Expression<Func<TSource, object>> propertyExpression)
    {
        var expression = (MemberExpression)propertyExpression.Body;
        SourcePropertyName = expression.Member.Name;

        return this;
    }

    public IPropertyBuilder<TDest, TSource, TProperty> WithDestinationField(string fieldName)
    {
        SourceFieldName = fieldName;

        return this;
    }

    public IPropertyBuilder<TDest, TSource, TProperty> Ignore(bool isIgnore = true)
    {
        IsIgnored = isIgnore;

        return this;
    }
}
