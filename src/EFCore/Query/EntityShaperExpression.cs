// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents creation of an entity instance in <see cref="ShapedQueryExpression.ShaperExpression" />.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class EntityShaperExpression : Expression, IPrintableExpression
    {
        private static readonly MethodInfo _createUnableToDiscriminateException
            = typeof(EntityShaperExpression).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateUnableToDiscriminateException));

        [UsedImplicitly]
        private static Exception CreateUnableToDiscriminateException(IEntityType entityType, object discriminator)
            => new InvalidOperationException(CoreStrings.UnableToDiscriminate(entityType.DisplayName(), discriminator?.ToString()));

        /// <summary>
        ///     Creates a new instance of the <see cref="EntityShaperExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="valueBufferExpression"> An expression of ValueBuffer to get values for properties of the entity. </param>
        /// <param name="nullable"> A bool value indicating whether this entity instance can be null. </param>
        public EntityShaperExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueBufferExpression,
            bool nullable)
            : this(entityType, valueBufferExpression, nullable, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="EntityShaperExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="valueBufferExpression"> An expression of ValueBuffer to get values for properties of the entity. </param>
        /// <param name="nullable"> Whether this entity instance can be null. </param>
        /// <param name="materializationCondition">
        ///     An expression of <see cref="Func{ValueBuffer, IEntityType}" /> to determine which entity type to
        ///     materialize.
        /// </param>
        protected EntityShaperExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueBufferExpression,
            bool nullable,
            [CanBeNull] LambdaExpression materializationCondition)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(valueBufferExpression, nameof(valueBufferExpression));

            if (materializationCondition == null)
            {
                materializationCondition = GenerateMaterializationCondition(entityType, nullable);
            }
            else if (materializationCondition.Parameters.Count != 1
                || materializationCondition.Parameters[0].Type != typeof(ValueBuffer)
                || materializationCondition.ReturnType != typeof(IEntityType))
            {
                throw new InvalidOperationException(CoreStrings.QueryEntityMaterializationConditionWrongShape(entityType.DisplayName()));
            }

            EntityType = entityType;
            ValueBufferExpression = valueBufferExpression;
            IsNullable = nullable;
            MaterializationCondition = materializationCondition;
        }

        /// <summary>
        ///     Creates an expression to throw an exception when unable to determine entity type
        ///     to materialize based on discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type for which materialization was requested. </param>
        /// <param name="discriminatorValue"> The expression containing value of discriminator. </param>
        /// <returns> An expression of <see cref="Func{ValueBuffer, IEntityType}" /> representing materilization condition for the entity type. </returns>
        protected static Expression CreateUnableToDiscriminateExceptionExpression([NotNull] IEntityType entityType, [NotNull] Expression discriminatorValue)
            => Block(
                Throw(Call(_createUnableToDiscriminateException,
                    Constant(Check.NotNull(entityType, nameof(entityType))),
                    Convert(Check.NotNull(discriminatorValue, nameof(discriminatorValue)), typeof(object)))),
                Constant(null, typeof(IEntityType)));

        /// <summary>
        ///     Creates an expression of <see cref="Func{ValueBuffer, IEntityType}" /> to determine which entity type to materialize.
        /// </summary>
        /// <param name="entityType"> The entity type to create materialization condition for. </param>
        /// <param name="nullable"> Whether this entity instance can be null. </param>
        /// <returns> An expression of <see cref="Func{ValueBuffer, IEntityType}" /> representing materilization condition for the entity type. </returns>
        protected virtual LambdaExpression GenerateMaterializationCondition([NotNull] IEntityType entityType, bool nullable)
        {
            Check.NotNull(entityType, nameof(EntityType));

            var valueBufferParameter = Parameter(typeof(ValueBuffer));
            Expression body;
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToArray();
            var discriminatorProperty = entityType.GetDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                var discriminatorValueVariable = Variable(discriminatorProperty.ClrType, "discriminator");
                var expressions = new List<Expression>
                {
                    Assign(
                        discriminatorValueVariable,
                        valueBufferParameter.CreateValueBufferReadValueExpression(
                            discriminatorProperty.ClrType, discriminatorProperty.GetIndex(), discriminatorProperty))
                };

                var exception = CreateUnableToDiscriminateExceptionExpression(entityType, discriminatorValueVariable);

                var discriminatorComparer = discriminatorProperty.GetKeyValueComparer();
                if (discriminatorComparer.IsDefault())
                {
                    var switchCases = new SwitchCase[concreteEntityTypes.Length];
                    for (var i = 0; i < concreteEntityTypes.Length; i++)
                    {
                        var discriminatorValue = Constant(concreteEntityTypes[i].GetDiscriminatorValue(), discriminatorProperty.ClrType);
                        switchCases[i] = SwitchCase(Constant(concreteEntityTypes[i], typeof(IEntityType)), discriminatorValue);
                    }

                    expressions.Add(Switch(discriminatorValueVariable, exception, switchCases));
                }
                else
                {
                    Expression conditions = exception;
                    for (var i = concreteEntityTypes.Length - 1; i >= 0; i--)
                    {
                        conditions = Condition(
                            discriminatorComparer.ExtractEqualsBody(
                                discriminatorValueVariable,
                                Constant(
                                    concreteEntityTypes[i].GetDiscriminatorValue(),
                                    discriminatorProperty.ClrType)),
                            Constant(concreteEntityTypes[i], typeof(IEntityType)),
                            conditions);
                    }

                    expressions.Add(conditions);
                }
                body = Block(new[] { discriminatorValueVariable }, expressions);
            }
            else
            {
                body = Constant(concreteEntityTypes.Length == 1 ? concreteEntityTypes[0] : entityType, typeof(IEntityType));
            }

            if (entityType.FindPrimaryKey() == null
                && nullable)
            {
                body = Condition(
                    entityType.GetProperties()
                        .Select(
                            p => NotEqual(
                                valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                Constant(null)))
                        .Aggregate((a, b) => OrElse(a, b)),
                    body,
                    Default(typeof(IEntityType)));
            }

            return Lambda(body, valueBufferParameter);
        }

        /// <summary>
        ///     The entity type being shaped.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     The expression representing a <see cref="ValueBuffer" /> to get values from that are used to create the entity instance.
        /// </summary>
        public virtual Expression ValueBufferExpression { get; }

        /// <summary>
        ///     A value indicating whether this entity instance can be null.
        /// </summary>
        public virtual bool IsNullable { get; }

        /// <summary>
        ///     The materilization condition to use for shaping this entity.
        /// </summary>
        public virtual LambdaExpression MaterializationCondition { get; }

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var valueBufferExpression = visitor.Visit(ValueBufferExpression);

            return Update(valueBufferExpression);
        }

        /// <summary>
        ///     Changes the entity type being shaped by this entity shaper.
        /// </summary>
        /// <param name="entityType"> The new entity type to use. </param>
        /// <returns> This expression if entity type not changed, or an expression with updated entity type. </returns>
        public virtual EntityShaperExpression WithEntityType([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType != EntityType
                ? new EntityShaperExpression(entityType, ValueBufferExpression, IsNullable)
                : this;
        }

        /// <summary>
        ///     Marks this shaper as nullable, indicating that it can shape null entity instances.
        /// </summary>
        /// <returns> This expression if nullability not changed, or an expression with updated nullability. </returns>
        public virtual EntityShaperExpression MarkAsNullable()
            => !IsNullable
                // Marking nullable requires recomputation of materialization condition
                ? new EntityShaperExpression(EntityType, ValueBufferExpression, true)
                : this;

        /// <summary>
        ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
        ///     return this expression.
        /// </summary>
        /// <param name="valueBufferExpression"> The <see cref="ValueBufferExpression" /> property of the result. </param>
        /// <returns> This expression if no children changed, or an expression with the updated children. </returns>
        public virtual EntityShaperExpression Update([NotNull] Expression valueBufferExpression)
        {
            Check.NotNull(valueBufferExpression, nameof(valueBufferExpression));

            return valueBufferExpression != ValueBufferExpression
                ? new EntityShaperExpression(EntityType, valueBufferExpression, IsNullable, MaterializationCondition)
                : this;
        }

        /// <inheritdoc />
        public override Type Type
            => EntityType.ClrType;

        /// <inheritdoc />
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <inheritdoc />
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.AppendLine(nameof(EntityShaperExpression) + ": ");
            using (expressionPrinter.Indent())
            {
                expressionPrinter.AppendLine(EntityType.ToString());
                expressionPrinter.AppendLine(nameof(ValueBufferExpression) + ": ");
                using (expressionPrinter.Indent())
                {
                    expressionPrinter.Visit(ValueBufferExpression);
                    expressionPrinter.AppendLine();
                }

                expressionPrinter.Append(nameof(IsNullable) + ": ");
                expressionPrinter.AppendLine(IsNullable.ToString());
            }
        }
    }
}
