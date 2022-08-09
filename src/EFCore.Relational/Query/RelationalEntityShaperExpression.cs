// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of an entity instance for a relational provider in
///         <see cref="ShapedQueryExpression.ShaperExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalEntityShaperExpression : EntityShaperExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalEntityShaperExpression" /> class.
    /// </summary>
    /// <param name="entityType">The entity type to shape.</param>
    /// <param name="valueBufferExpression">An expression of ValueBuffer to get values for properties of the entity.</param>
    /// <param name="nullable">A bool value indicating whether this entity instance can be null.</param>
    public RelationalEntityShaperExpression(IEntityType entityType, Expression valueBufferExpression, bool nullable)
        : base(entityType, valueBufferExpression, nullable, null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalEntityShaperExpression" /> class.
    /// </summary>
    /// <param name="entityType">The entity type to shape.</param>
    /// <param name="valueBufferExpression">An expression of ValueBuffer to get values for properties of the entity.</param>
    /// <param name="nullable">Whether this entity instance can be null.</param>
    /// <param name="materializationCondition">
    ///     An expression of <see cref="Func{T,TResult}" /> to determine which entity type to
    ///     materialize.
    /// </param>
    protected RelationalEntityShaperExpression(
        IEntityType entityType,
        Expression valueBufferExpression,
        bool nullable,
        LambdaExpression? materializationCondition)
        : base(entityType, valueBufferExpression, nullable, materializationCondition)
    {
    }

    /// <inheritdoc />
    protected override LambdaExpression GenerateMaterializationCondition(IEntityType entityType, bool nullable)
    {
        LambdaExpression baseCondition;
        // Generate discriminator condition
        var containsDiscriminatorProperty = entityType.FindDiscriminatorProperty() != null;
        if (!containsDiscriminatorProperty
            && entityType.GetDirectlyDerivedTypes().Any())
        {
            // TPT/TPC
            var valueBufferParameter = Parameter(typeof(ValueBuffer));
            var discriminatorValueVariable = Variable(typeof(string), "discriminator");
            var expressions = new List<Expression>
            {
                Assign(
                    discriminatorValueVariable,
                    valueBufferParameter.CreateValueBufferReadValueExpression(typeof(string), 0, null))
            };

            var derivedConcreteEntityTypes = entityType.GetDerivedTypes().Where(dt => !dt.IsAbstract()).ToArray();
            var switchCases = new SwitchCase[derivedConcreteEntityTypes.Length];
            for (var i = 0; i < derivedConcreteEntityTypes.Length; i++)
            {
                var discriminatorValue = Constant(derivedConcreteEntityTypes[i].ShortName(), typeof(string));
                switchCases[i] = SwitchCase(Constant(derivedConcreteEntityTypes[i], typeof(IEntityType)), discriminatorValue);
            }

            var defaultBlock = entityType.IsAbstract()
                ? CreateUnableToDiscriminateExceptionExpression(entityType, discriminatorValueVariable)
                : Constant(entityType, typeof(IEntityType));

            expressions.Add(Switch(discriminatorValueVariable, defaultBlock, switchCases));
            baseCondition = Lambda(Block(new[] { discriminatorValueVariable }, expressions), valueBufferParameter);
        }
        else
        {
            baseCondition = base.GenerateMaterializationCondition(entityType, nullable);
        }

        if (containsDiscriminatorProperty
            || entityType.FindPrimaryKey() == null
            || entityType.GetRootType() != entityType
            || entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
        {
            return baseCondition;
        }

        var table = entityType.GetViewOrTableMappings().SingleOrDefault(e => e.IsSplitEntityTypePrincipal ?? true)?.Table
            ?? entityType.GetDefaultMappings().Single().Table;
        if (table.IsOptional(entityType))
        {
            // Optional dependent
            var body = baseCondition.Body;
            var valueBufferParameter = baseCondition.Parameters[0];
            Expression? condition = null;
            var requiredNonPkProperties = entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
            if (requiredNonPkProperties.Count > 0)
            {
                condition = requiredNonPkProperties
                    .Select(
                        p => NotEqual(
                            valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                            Constant(null)))
                    .Aggregate((a, b) => AndAlso(a, b));
            }

            var allNonPrincipalSharedNonPkProperties = entityType.GetNonPrincipalSharedNonPkProperties(table);
            // We don't need condition for nullable property if there exist at least one required property which is non shared.
            if (allNonPrincipalSharedNonPkProperties.Any()
                && allNonPrincipalSharedNonPkProperties.All(p => p.IsNullable))
            {
                var atLeastOneNonNullValueInNullablePropertyCondition = allNonPrincipalSharedNonPkProperties
                    .Select(
                        p => NotEqual(
                            valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                            Constant(null)))
                    .Aggregate((a, b) => OrElse(a, b));

                condition = condition == null
                    ? atLeastOneNonNullValueInNullablePropertyCondition
                    : AndAlso(condition, atLeastOneNonNullValueInNullablePropertyCondition);
            }

            if (condition != null)
            {
                body = Condition(condition, body, Default(typeof(IEntityType)));
            }

            return Lambda(body, valueBufferParameter);
        }

        return baseCondition;
    }

    /// <inheritdoc />
    public override EntityShaperExpression WithEntityType(IEntityType entityType)
        => entityType != EntityType
            ? new RelationalEntityShaperExpression(entityType, ValueBufferExpression, IsNullable)
            : this;

    /// <inheritdoc />
    public override EntityShaperExpression MakeNullable(bool nullable = true)
        => IsNullable != nullable
            // Marking nullable requires re-computation of Discriminator condition
            ? new RelationalEntityShaperExpression(EntityType, ValueBufferExpression, true)
            : this;

    /// <inheritdoc />
    public override EntityShaperExpression Update(Expression valueBufferExpression)
        => valueBufferExpression != ValueBufferExpression
            ? new RelationalEntityShaperExpression(EntityType, valueBufferExpression, IsNullable, MaterializationCondition)
            : this;
}
