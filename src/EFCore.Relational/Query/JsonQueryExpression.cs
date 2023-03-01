// Licensed to the .NET Foundation under one or more agreements.
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
    private readonly IReadOnlyDictionary<IProperty, ColumnExpression> _keyPropertyMap;

    /// <summary>
    ///     Creates a new instance of the <see cref="JsonQueryExpression" /> class.
    /// </summary>
    /// <param name="entityType">An entity type being represented by this expression.</param>
    /// <param name="jsonColumn">A column containing JSON value.</param>
    /// <param name="keyPropertyMap">A map of key properties and columns they map to in the database.</param>
    /// <param name="type">A type of the element represented by this expression.</param>
    /// <param name="collection">A value indicating whether this expression represents a collection or not.</param>
    public JsonQueryExpression(
        IEntityType entityType,
        ColumnExpression jsonColumn,
        IReadOnlyDictionary<IProperty, ColumnExpression> keyPropertyMap,
        Type type,
        bool collection)
        : this(
            entityType,
            jsonColumn,
            keyPropertyMap,
            path: new List<PathSegment> { new("$") },
            type,
            collection,
            jsonColumn.IsNullable)
    {
    }

    private JsonQueryExpression(
        IEntityType entityType,
        ColumnExpression jsonColumn,
        IReadOnlyDictionary<IProperty, ColumnExpression> keyPropertyMap,
        IReadOnlyList<PathSegment> path,
        Type type,
        bool collection,
        bool nullable)
    {
        Check.DebugAssert(entityType.FindPrimaryKey() != null, "primary key is null.");

        EntityType = entityType;
        JsonColumn = jsonColumn;
        IsCollection = collection;
        _keyPropertyMap = keyPropertyMap;
        Type = type;
        Path = path;
        IsNullable = nullable;
    }

    /// <summary>
    ///     The entity type being represented by this expression.
    /// </summary>
    public virtual IEntityType EntityType { get; }

    /// <summary>
    ///     The column containg JSON value.
    /// </summary>
    public virtual ColumnExpression JsonColumn { get; }

    /// <summary>
    ///     The value indicating whether this expression represents a collection.
    /// </summary>
    public virtual bool IsCollection { get; }

    /// <summary>
    ///     The list of path segments leading to the entity from the root of the JSON stored in the column.
    /// </summary>
    public virtual IReadOnlyList<PathSegment> Path { get; }

    /// <summary>
    ///     The value indicating whether this expression is nullable.
    /// </summary>
    public virtual bool IsNullable { get; }

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
        if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
            && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
        }

        if (_keyPropertyMap.TryGetValue(property, out var match))
        {
            return match;
        }

        var newPath = Path.ToList();
        newPath.Add(new PathSegment(property.GetJsonPropertyName()!));

        return new JsonScalarExpression(
            JsonColumn,
            property,
            newPath,
            IsNullable || property.IsNullable);
    }

    /// <summary>
    ///     Binds a navigation with this JSON query expression to get the SQL representation.
    /// </summary>
    /// <param name="navigation">The navigation to bind.</param>
    /// <returns>An JSON query expression for the target entity type of the navigation.</returns>
    public virtual JsonQueryExpression BindNavigation(INavigation navigation)
    {
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
        var targetPrimaryKeyProperties = targetEntityType.FindPrimaryKey()!.Properties.Take(_keyPropertyMap.Count);
        var sourcePrimaryKeyProperties = EntityType.FindPrimaryKey()!.Properties.Take(_keyPropertyMap.Count);
        foreach (var (target, source) in targetPrimaryKeyProperties.Zip(sourcePrimaryKeyProperties, (t, s) => (t, s)))
        {
            newKeyPropertyMap[target] = _keyPropertyMap[source];
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

    /// <summary>
    ///     Binds a collection element access with this JSON query expression to get the SQL representation.
    /// </summary>
    /// <param name="collectionIndexExpression">The collection index to bind.</param>
    public virtual JsonQueryExpression BindCollectionElement(SqlExpression collectionIndexExpression)
    {
        // this needs to be changed IF JsonQueryExpression will also be used for collection of primitives
        // see issue #28688
        Check.DebugAssert(
            Path.Last().ArrayIndex == null,
            "Already accessing JSON array element.");

        var newPath = Path.ToList();
        newPath.Add(new PathSegment(collectionIndexExpression));

        return new JsonQueryExpression(
            EntityType,
            JsonColumn,
            _keyPropertyMap,
            newPath,
            EntityType.ClrType,
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
    {
        var keyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, columnExpression) in _keyPropertyMap)
        {
            keyPropertyMap[property] = columnExpression.MakeNullable();
        }

        return new JsonQueryExpression(
            EntityType,
            JsonColumn.MakeNullable(),
            keyPropertyMap,
            Path,
            Type,
            IsCollection,
            nullable: true);
    }

    /// <inheritdoc />
    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("JsonQueryExpression(");
        expressionPrinter.Visit(JsonColumn);
        expressionPrinter.Append($", {string.Join("", Path.Select(e => e.ToString()))})");
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var jsonColumn = (ColumnExpression)visitor.Visit(JsonColumn);
        var newKeyPropertyMap = new Dictionary<IProperty, ColumnExpression>();
        foreach (var (property, column) in _keyPropertyMap)
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
        IReadOnlyDictionary<IProperty, ColumnExpression> keyPropertyMap)
        => jsonColumn != JsonColumn
            || keyPropertyMap.Count != _keyPropertyMap.Count
            || keyPropertyMap.Zip(_keyPropertyMap, (n, o) => n.Value != o.Value).Any(x => x)
                ? new JsonQueryExpression(EntityType, jsonColumn, keyPropertyMap, Path, Type, IsCollection, IsNullable)
                : this;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is JsonQueryExpression jsonQueryExpression
                && Equals(jsonQueryExpression));

    private bool Equals(JsonQueryExpression jsonQueryExpression)
        => EntityType.Equals(jsonQueryExpression.EntityType)
            && JsonColumn.Equals(jsonQueryExpression.JsonColumn)
            && IsCollection.Equals(jsonQueryExpression.IsCollection)
            && IsNullable == jsonQueryExpression.IsNullable
            && Path.SequenceEqual(jsonQueryExpression.Path)
            && KeyPropertyMapEquals(jsonQueryExpression._keyPropertyMap);

    private bool KeyPropertyMapEquals(IReadOnlyDictionary<IProperty, ColumnExpression> other)
    {
        if (_keyPropertyMap.Count != other.Count)
        {
            return false;
        }

        foreach (var (key, value) in _keyPropertyMap)
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
        => HashCode.Combine(EntityType, JsonColumn, IsCollection, Path, IsNullable);
}
