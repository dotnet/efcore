﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression representing an entity or a collection of entities mapped to a JSON column and the path to access it.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class JsonQueryExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="JsonQueryExpression" /> class.
    /// </summary>
    /// <param name="structuralType">The structural type represented by this expression.</param>
    /// <param name="jsonColumn">A column containing the JSON value.</param>
    /// <param name="keyPropertyMap">For owned entities, a map of key properties and columns they map to in the database. For complex types, <see langword="null" />.</param>
    /// <param name="type">The CLR represented by this expression.</param>
    /// <param name="collection">Whether this expression represents a collection.</param>
    public JsonQueryExpression(
        ITypeBase structuralType,
        ColumnExpression jsonColumn,
        IReadOnlyDictionary<IProperty, ColumnExpression>? keyPropertyMap,
        Type type,
        bool collection)
        : this(
            structuralType,
            jsonColumn,
            keyPropertyMap,
            path: [],
            type,
            collection,
            jsonColumn.IsNullable)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="JsonQueryExpression" /> class.
    /// </summary>
    /// <param name="structuralType">The structural type represented by this expression.</param>
    /// <param name="jsonColumn">A column containing the JSON value.</param>
    /// <param name="keyPropertyMap">For owned entities, a map of key properties and columns they map to in the database. For complex types, <see langword="null" />.</param>
    /// <param name="path">The list of path segments leading to the entity from the root of the JSON stored in the column.</param>
    /// <param name="type">The CLR represented by this expression.</param>
    /// <param name="collection">Whether this expression represents a collection.</param>
    /// <param name="nullable">Whether this expression is nullable.</param>
    public JsonQueryExpression(
        ITypeBase structuralType,
        ColumnExpression jsonColumn,
        IReadOnlyDictionary<IProperty, ColumnExpression>? keyPropertyMap,
        IReadOnlyList<PathSegment> path,
        Type type,
        bool collection,
        bool nullable)
    {
        Check.DebugAssert(structuralType is not IEntityType entityType || entityType.FindPrimaryKey() is not null, "JsonQueryExpression over keyless entity type");

        StructuralType = structuralType;
        JsonColumn = jsonColumn;
        IsCollection = collection;
        KeyPropertyMap = keyPropertyMap;
        Type = type;
        Path = path;
        IsNullable = nullable;
    }

    /// <summary>
    ///     The structural type represented by this expression.
    /// </summary>
    public virtual ITypeBase StructuralType { get; }

    /// <summary>
    ///     The column containing the JSON value.
    /// </summary>
    public virtual ColumnExpression JsonColumn { get; }

    /// <summary>
    ///     Whether this expression represents a collection.
    /// </summary>
    public virtual bool IsCollection { get; }

    /// <summary>
    ///     The list of path segments leading to the entity from the root of the JSON stored in the column.
    /// </summary>
    public virtual IReadOnlyList<PathSegment> Path { get; }

    /// <summary>
    ///     Whether this expression is nullable.
    /// </summary>
    public virtual bool IsNullable { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual IReadOnlyDictionary<IProperty, ColumnExpression>? KeyPropertyMap { get; }

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type { get; }

    /// <summary>
    ///     Binds a property with this JSON query expression to get the SQL representation.
    /// </summary>
    public virtual SqlExpression BindProperty(IProperty property)
    {
        if (!StructuralType.IsAssignableFrom(property.DeclaringType)
            && !property.DeclaringType.IsAssignableFrom(StructuralType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, StructuralType.DisplayName()));
        }

        if (KeyPropertyMap?.TryGetValue(property, out var match) == true)
        {
            return match;
        }

        return new JsonScalarExpression(
            JsonColumn,
            [.. Path, new(property.GetJsonPropertyName()!)],
            property.ClrType.UnwrapNullableType(),
            property.FindRelationalTypeMapping()!,
            IsNullable || property.IsNullable);
    }

    /// <summary>
    ///     Binds a relationship with this JSON query expression to get the SQL representation.
    /// </summary>
    /// <param name="relationship">The navigation or complex property to bind.</param>
    /// <returns>An JSON query expression for the target entity or complex type.</returns>
    public virtual JsonQueryExpression BindRelationship(IPropertyBase relationship)
    {
        switch (relationship)
        {
            case INavigation navigation:
            {
                if (StructuralType is not IEntityType entityType)
                {
                    throw new UnreachableException("Navigation on complex JSON type");
                }

                Check.DebugAssert(KeyPropertyMap is not null);

                if (navigation.ForeignKey.DependentToPrincipal == navigation)
                {
                    // issue #28645
                    throw new InvalidOperationException(
                        RelationalStrings.JsonCantNavigateToParentEntity(
                            navigation.ForeignKey.DeclaringEntityType.DisplayName(),
                            navigation.ForeignKey.PrincipalEntityType.DisplayName(),
                            navigation.Name));
                }

                var targetEntityType = navigation.TargetEntityType;
                var newPath = Path.ToList();
                newPath.Add(new PathSegment(targetEntityType.GetJsonPropertyName()!));

                var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
                var targetPrimaryKeyProperties = targetEntityType.FindPrimaryKey()!.Properties.Take(KeyPropertyMap.Count);
                var sourcePrimaryKeyProperties = entityType.FindPrimaryKey()!.Properties.Take(KeyPropertyMap.Count);
                foreach (var (target, source) in targetPrimaryKeyProperties.Zip(sourcePrimaryKeyProperties, (t, s) => (t, s)))
                {
                    newKeyPropertyMap[target] = KeyPropertyMap[source];
                }

                return new JsonQueryExpression(
                    targetEntityType,
                    JsonColumn,
                    newKeyPropertyMap,
                    newPath,
                    navigation.ClrType,
                    navigation.IsCollection,
                    IsNullable || !navigation.ForeignKey.IsRequiredDependent);
            }

            case IComplexProperty complexProperty:
            {
                if (StructuralType is not IComplexType complexType)
                {
                    throw new UnreachableException("Navigation on complex JSON type");
                }

                Check.DebugAssert(KeyPropertyMap is null);

                var targetComplexType = complexProperty.ComplexType;
                var newPath = Path.ToList();
                newPath.Add(new PathSegment(targetComplexType.GetJsonPropertyName()!));

                return new JsonQueryExpression(
                    targetComplexType,
                    JsonColumn,
                    keyPropertyMap: null,
                    newPath,
                    complexProperty.ClrType,
                    complexProperty.IsCollection,
                    IsNullable || complexProperty.IsNullable);
            }

            default:
                throw new UnreachableException();
        }
    }

    /// <summary>
    ///     Binds a collection element access with this JSON query expression to get the SQL representation.
    /// </summary>
    /// <param name="collectionIndexExpression">The collection index to bind.</param>
    public virtual JsonQueryExpression BindCollectionElement(SqlExpression collectionIndexExpression)
    {
        // this needs to be changed IF JsonQueryExpression will also be used for collection of primitives
        // see issue #28688
        Check.DebugAssert(Path.Count == 0 || Path[^1].ArrayIndex == null, "Already accessing JSON array element.");

        var newPath = Path.ToList();
        newPath.Add(new PathSegment(collectionIndexExpression));

        return new JsonQueryExpression(
            StructuralType,
            JsonColumn,
            KeyPropertyMap,
            newPath,
            StructuralType.ClrType,
            collection: false,
            // TODO: computing nullability might be more complicated when we allow strict mode
            // see issue #28656
            nullable: true);
    }

    /// <summary>
    ///     Makes this JSON query expression nullable.
    /// </summary>
    /// <returns>A new expression which has <see cref="IsNullable" /> property set to true.</returns>
    public virtual JsonQueryExpression MakeNullable()
        => new JsonQueryExpression(
            StructuralType,
            JsonColumn.MakeNullable(),
            KeyPropertyMap?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.MakeNullable()),
            Path,
            Type,
            IsCollection,
            nullable: true);

    /// <inheritdoc />
    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(JsonColumn);
        expressionPrinter
            .Append(" Q-> ")
            .Append(string.Join(".", Path.Select(e => e.ToString())));
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var jsonColumn = (ColumnExpression)visitor.Visit(JsonColumn);

        if (KeyPropertyMap is null)
        {
            return Update(jsonColumn, keyPropertyMap: null);
        }

        var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, column) in KeyPropertyMap)
        {
            newKeyPropertyMap[property] = (ColumnExpression)visitor.Visit(column);
        }

        return Update(jsonColumn, newKeyPropertyMap);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="jsonColumn">The <see cref="JsonColumn" /> property of the result.</param>
    /// <param name="keyPropertyMap">The map of key properties and columns they map to.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual JsonQueryExpression Update(
        ColumnExpression jsonColumn,
        IReadOnlyDictionary<IProperty, ColumnExpression>? keyPropertyMap)
        => (jsonColumn == JsonColumn
            && ((keyPropertyMap is null && KeyPropertyMap is null)
                || (keyPropertyMap is not null
                    && KeyPropertyMap is not null
                    && keyPropertyMap.Count == KeyPropertyMap.Count
                    && KeyPropertyMapEquals(keyPropertyMap))))
                ? this
                : new JsonQueryExpression(StructuralType, jsonColumn, keyPropertyMap, Path, Type, IsCollection, IsNullable);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is JsonQueryExpression jsonQueryExpression
                && Equals(jsonQueryExpression));

    private bool Equals(JsonQueryExpression jsonQueryExpression)
        => StructuralType.Equals(jsonQueryExpression.StructuralType)
            && JsonColumn.Equals(jsonQueryExpression.JsonColumn)
            && IsCollection.Equals(jsonQueryExpression.IsCollection)
            && IsNullable == jsonQueryExpression.IsNullable
            && Path.SequenceEqual(jsonQueryExpression.Path)
            && KeyPropertyMapEquals(jsonQueryExpression.KeyPropertyMap);

    private bool KeyPropertyMapEquals(IReadOnlyDictionary<IProperty, ColumnExpression>? other)
    {
        if (KeyPropertyMap is null && other is null)
        {
            return true;
        }

        if (KeyPropertyMap is null || other is null || KeyPropertyMap.Count != other.Count)
        {
            return false;
        }

        foreach (var (key, value) in KeyPropertyMap)
        {
            if (!other.TryGetValue(key, out var column) || !value.Equals(column))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
        // not incorporating _keyPropertyMap into the hash, too much work
        => HashCode.Combine(StructuralType, JsonColumn, IsCollection, Path, IsNullable);
}
