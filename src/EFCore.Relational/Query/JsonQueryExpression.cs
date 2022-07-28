// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Expression representing an entity or a collection of entities mapped to a JSON column and the path to access it.
    /// </summary>
    public class JsonQueryExpression : Expression, IPrintableExpression
    {
        private readonly IReadOnlyDictionary<IProperty, ColumnExpression> _keyPropertyMap;
        private readonly bool _nullable;

        /// <summary>
        ///     Creates a new instance of the <see cref="JsonQueryExpression" /> class.
        /// </summary>
        /// <param name="entityType">An entity type being represented by this expression.</param>
        /// <param name="jsonColumn">A column containing JSON.</param>
        /// <param name="collection">A value indicating whether this expression represents a collection.</param>
        /// <param name="keyPropertyMap">A map of key properties and columns they map to in the database.</param>
        /// <param name="type">A type of the element represented by this expression.</param>
        public JsonQueryExpression(
            IEntityType entityType,
            ColumnExpression jsonColumn,
            bool collection,
            IReadOnlyDictionary<IProperty, ColumnExpression> keyPropertyMap,
            Type type)
            : this(
                  entityType,
                  jsonColumn,
                  collection,
                  keyPropertyMap,
                  type,
                  jsonPath: new SqlConstantExpression(Constant("$"), typeMapping: null),
                  jsonColumn.IsNullable)
        {
        }

        private JsonQueryExpression(
            IEntityType entityType,
            ColumnExpression jsonColumn,
            bool collection,
            IReadOnlyDictionary<IProperty, ColumnExpression> keyPropertyMap,
            Type type,
            SqlExpression jsonPath,
            bool nullable)
        {
            Check.DebugAssert(entityType.FindPrimaryKey() != null, "primary key is null.");

            EntityType = entityType;
            JsonColumn = jsonColumn;
            IsCollection = collection;
            _keyPropertyMap = keyPropertyMap;
            Type = type;
            JsonPath = jsonPath;
            _nullable = nullable;
        }

        /// <summary>
        ///     The entity type being projected out.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     The column containg JSON value on which the path is applied.
        /// </summary>
        public virtual ColumnExpression JsonColumn { get; }

        /// <summary>
        ///     The value indicating whether this expression represents a collection.
        /// </summary>
        public virtual bool IsCollection { get; }

        /// <summary>
        ///     The JSON path leading to the entity from the root of the JSON stored in the column.
        /// </summary>
        public virtual SqlExpression JsonPath { get; }

        /// <summary>
        ///     The value indicating whether this expression is nullable.
        /// </summary>
        public virtual bool IsNullable => _nullable;

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;

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

            var pathSegment = new SqlConstantExpression(
                Constant(property.GetJsonPropertyName()),
                typeMapping: null);

            var newPath = new SqlBinaryExpression(
                ExpressionType.Add,
                JsonPath,
                pathSegment,
                typeof(string),
                typeMapping: null);

            return new JsonScalarExpression(
                JsonColumn,
                property,
                newPath,
                _nullable || property.IsNullable);
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
            var pathSegment = new SqlConstantExpression(
                Constant(navigation.TargetEntityType.GetJsonPropertyName()),
                typeMapping: null);

            var newJsonPath = new SqlBinaryExpression(
                ExpressionType.Add,
                JsonPath,
                pathSegment,
                typeof(string),
                typeMapping: null);

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
                navigation.IsCollection,
                newKeyPropertyMap,
                navigation.ClrType,
                newJsonPath,
                _nullable || !navigation.ForeignKey.IsRequiredDependent);
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

            return MakeNullable(keyPropertyMap);
        }

        /// <summary>
        ///     Makes this JSON query expression nullable re-using existing nullable key properties
        /// </summary>
        /// <returns>A new expression which has <see cref="IsNullable" /> property set to true.</returns>
        [EntityFrameworkInternal]
        public virtual JsonQueryExpression MakeNullable(IReadOnlyDictionary<IProperty, ColumnExpression> nullableKeyPropertyMap)
            => Update(
                JsonColumn.MakeNullable(),
                nullableKeyPropertyMap,
                JsonPath,
                nullable: true);

        /// <inheritdoc />
        public virtual void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("JsonQueryExpression(");
            expressionPrinter.Visit(JsonColumn);
            expressionPrinter.Append($", \"{string.Join(".", JsonPath)}\")");
        }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var jsonColumn = (ColumnExpression)visitor.Visit(JsonColumn);
            var jsonPath = (SqlExpression)visitor.Visit(JsonPath);

            // TODO: also visit columns in the _keyPropertyMap?
            return Update(jsonColumn, _keyPropertyMap, jsonPath, IsNullable);
        }

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="jsonColumn">The <see cref="JsonColumn" /> property of the result.</param>
        /// <param name="keyPropertyMap">The map of key properties and columns they map to.</param>
        /// <param name="jsonPath">The <see cref="JsonPath" /> property of the result.</param>
        /// <param name="nullable">The <see cref="IsNullable" /> property of the result.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public virtual JsonQueryExpression Update(
            ColumnExpression jsonColumn,
            IReadOnlyDictionary<IProperty, ColumnExpression> keyPropertyMap,
            SqlExpression jsonPath,
            bool nullable)
            => jsonColumn != JsonColumn
            || keyPropertyMap.Count != _keyPropertyMap.Count
            || keyPropertyMap.Zip(_keyPropertyMap, (n, o) => n.Value != o.Value).Any(x => x)
            || jsonPath != JsonPath
                ? new JsonQueryExpression(EntityType, jsonColumn, IsCollection, keyPropertyMap, Type, jsonPath, nullable)
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
                && JsonPath.Equals(jsonQueryExpression.JsonPath)
                && IsNullable == jsonQueryExpression.IsNullable
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
            => HashCode.Combine(EntityType, JsonColumn, IsCollection, JsonPath, IsNullable);
    }
}
