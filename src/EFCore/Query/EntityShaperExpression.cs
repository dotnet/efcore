// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class EntityShaperExpression : Expression, IPrintableExpression
    {
        private static readonly MethodInfo _createUnableToDiscriminateException
            = typeof(EntityShaperExpression).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateUnableToDiscriminateException));

        [UsedImplicitly]
        private static Exception CreateUnableToDiscriminateException(IEntityType entityType, object discriminator)
            => new InvalidOperationException(CoreStrings.UnableToDiscriminate(entityType.DisplayName(), discriminator?.ToString()));

        public EntityShaperExpression(
            [NotNull] IEntityType entityType,
            [NotNull] Expression valueBufferExpression,
            bool nullable)
            : this(entityType, valueBufferExpression, nullable, null)
        {
        }

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

                var switchCases = new SwitchCase[concreteEntityTypes.Length];
                for (var i = 0; i < concreteEntityTypes.Length; i++)
                {
                    var discriminatorValue = Constant(concreteEntityTypes[i].GetDiscriminatorValue(), discriminatorProperty.ClrType);
                    switchCases[i] = SwitchCase(Constant(concreteEntityTypes[i], typeof(IEntityType)), discriminatorValue);
                }

                var exception = Block(
                    Throw(Call(
                        _createUnableToDiscriminateException, Constant(entityType), Convert(discriminatorValueVariable, typeof(object)))),
                    Constant(null, typeof(IEntityType)));

                expressions.Add(Switch(discriminatorValueVariable, exception, switchCases));
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
                        .Select(p => NotEqual(
                            valueBufferParameter.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                            Constant(null)))
                        .Aggregate((a, b) => OrElse(a, b)),
                    body,
                    Default(typeof(IEntityType)));
            }

            return Lambda(body, valueBufferParameter);
        }

        public virtual IEntityType EntityType { get; }
        public virtual Expression ValueBufferExpression { get; }
        public virtual bool IsNullable { get; }
        public virtual LambdaExpression MaterializationCondition { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var valueBufferExpression = visitor.Visit(ValueBufferExpression);

            return Update(valueBufferExpression);
        }

        public virtual EntityShaperExpression WithEntityType([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType != EntityType
                ? new EntityShaperExpression(entityType, ValueBufferExpression, IsNullable)
                : this;
        }

        public virtual EntityShaperExpression MarkAsNullable()
            => !IsNullable
                // Marking nullable requires recomputation of materialization condition
                ? new EntityShaperExpression(EntityType, ValueBufferExpression, true)
                : this;

        public virtual EntityShaperExpression Update([NotNull] Expression valueBufferExpression)
        {
            Check.NotNull(valueBufferExpression, nameof(valueBufferExpression));

            return valueBufferExpression != ValueBufferExpression
                ? new EntityShaperExpression(EntityType, valueBufferExpression, IsNullable, MaterializationCondition)
                : this;
        }

        public override Type Type => EntityType.ClrType;

        public sealed override ExpressionType NodeType => ExpressionType.Extension;

        public virtual void Print(ExpressionPrinter expressionPrinter)
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
