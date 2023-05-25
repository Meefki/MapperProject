﻿using System.Linq.Expressions;

namespace MapperProject.Abstractions;

public interface IPropertyBuilder
{
    public Type PropertyType { get; }

    public string DestPropertyName { get; }
    public string? DestFieldName { get; }

    public string? SourcePropertyName { get; }
    public string? SourceFieldName { get; }
    public bool IsIgnored { get; }
}

public interface IPropertyBuilder<TDest, TSource, TProperty>
{
    public IPropertyBuilder<TDest, TSource, TProperty> HasField(string fieldName);
    public IPropertyBuilder<TDest, TSource, TProperty> Ignore(bool isIgnore = true);
    public IPropertyBuilder<TDest, TSource, TProperty> MapFrom<TSourceProperty>(Expression<Func<TSource, TSourceProperty>> propertyExpression);
    public IPropertyBuilder<TDest, TSource, TProperty> WithDestinationField(string fieldName);
}
