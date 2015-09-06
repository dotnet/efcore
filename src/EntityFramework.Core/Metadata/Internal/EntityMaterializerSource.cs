// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class EntityMaterializerSource : IEntityMaterializerSource
    {
        private static readonly MethodInfo _readValue
            = typeof(ValueBuffer).GetTypeInfo().DeclaredProperties
                .Single(p => p.GetIndexParameters().Any()).GetMethod;

        private readonly IMemberMapper _memberMapper;

        public EntityMaterializerSource([NotNull] IMemberMapper memberMapper)
        {
            _memberMapper = memberMapper;
        }

        public virtual Expression CreateReadValueExpression(Expression valueBuffer, Type type, int index) 
            => Expression.Convert(CreateReadValueCallExpression(valueBuffer, index), type);

        public virtual Expression CreateReadValueCallExpression(Expression valueBuffer, int index) 
            => Expression.Call(valueBuffer, _readValue, Expression.Constant(index));

        public virtual Expression CreateMaterializeExpression(
            IEntityType entityType,
            Expression valueBufferExpression,
            int[] indexMap = null)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var materializer = entityType as IEntityMaterializer;

            if (materializer != null)
            {
                return Expression.Call(
                    Expression.Constant(materializer),
                    ((Func<ValueBuffer, object>)materializer.CreateEntity).GetMethodInfo(),
                    valueBufferExpression);
            }

            if (!entityType.HasClrType())
            {
                throw new InvalidOperationException(Strings.NoClrType(entityType.Name));
            }

            if (entityType.IsAbstract())
            {
                throw new InvalidOperationException(Strings.CannotMaterializeAbstractType(entityType));
            }

            var instanceVariable = Expression.Variable(entityType.ClrType, "instance");

            var blockExpressions
                = new List<Expression>
                {
                    Expression.Assign(
                        instanceVariable,
                        Expression.New(entityType.ClrType.GetDeclaredConstructor(null)))
                };

            blockExpressions.AddRange(
                from mapping in _memberMapper.MapPropertiesToMembers(entityType)
                let propertyInfo = mapping.Item2 as PropertyInfo
                let targetMember
                    = propertyInfo != null
                        ? Expression.Property(instanceVariable, propertyInfo)
                        : Expression.Field(instanceVariable, (FieldInfo)mapping.Item2)
                select
                    Expression.Assign(
                        targetMember,
                        CreateReadValueExpression(
                            valueBufferExpression,
                            targetMember.Type,
                            indexMap?[mapping.Item1.Index] ?? mapping.Item1.Index)));

            blockExpressions.Add(instanceVariable);

            return Expression.Block(new[] { instanceVariable }, blockExpressions);
        }
    }
}
