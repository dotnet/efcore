// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityMaterializerSource : IEntityMaterializerSource
    {
        private static readonly MethodInfo _readValue
            = typeof(ValueBuffer).GetTypeInfo().DeclaredProperties
                .Single(p => p.GetIndexParameters().Any()).GetMethod;

        private readonly IMemberMapper _memberMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityMaterializerSource([NotNull] IMemberMapper memberMapper)
        {
            _memberMapper = memberMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression CreateReadValueExpression(Expression valueBuffer, Type type, int index)
            => Expression.Convert(CreateReadValueCallExpression(valueBuffer, index), type);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression CreateReadValueCallExpression(Expression valueBuffer, int index)
            => Expression.Call(valueBuffer, _readValue, Expression.Constant(index));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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
                throw new InvalidOperationException(CoreStrings.NoClrType(entityType.Name));
            }

            if (entityType.IsAbstract())
            {
                throw new InvalidOperationException(CoreStrings.CannotMaterializeAbstractType(entityType));
            }

            var constructorInfo = entityType.ClrType.GetDeclaredConstructor(null);

            if (constructorInfo == null)
            {
                throw new InvalidOperationException(CoreStrings.NoParameterlessConstructor(entityType.DisplayName()));
            }

            var instanceVariable = Expression.Variable(entityType.ClrType, "instance");

            var blockExpressions
                = new List<Expression>
                {
                    Expression.Assign(
                        instanceVariable,
                        Expression.New(constructorInfo))
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
                            indexMap?[mapping.Item1.GetIndex()] ?? mapping.Item1.GetIndex())));

            blockExpressions.Add(instanceVariable);

            return Expression.Block(new[] { instanceVariable }, blockExpressions);
        }
    }
}
