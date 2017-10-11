// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        private ConcurrentDictionary<IEntityType, Func<ValueBuffer, object>> _materializers;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression CreateReadValueExpression(
            Expression valueBuffer,
            Type type,
            int index,
            IProperty property)
        {
            return Expression.Call(
                TryReadValueMethod.MakeGenericMethod(type),
                valueBuffer,
                Expression.Constant(index),
                Expression.Constant(property, typeof(IPropertyBase)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo TryReadValueMethod
            = typeof(EntityMaterializerSource).GetTypeInfo()
                .GetDeclaredMethod(nameof(TryReadValue));

        private static TValue TryReadValue<TValue>(
            ValueBuffer valueBuffer,
            int index,
            IPropertyBase property = null)
        {
            var untypedValue = valueBuffer[index];
            try
            {
                return (TValue)untypedValue;
            }
            catch (Exception e)
            {
                ThrowReadValueException<TValue>(e, untypedValue, property);
            }

            return default;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo ThrowReadValueExceptionMethod
            = typeof(EntityMaterializerSource).GetTypeInfo()
                .GetDeclaredMethod(nameof(ThrowReadValueException));

        private static TValue ThrowReadValueException<TValue>(
            Exception exception, object value, IPropertyBase property = null)
        {
            var expectedType = typeof(TValue);
            var actualType = value?.GetType();

            string message;

            if (property != null)
            {
                var entityType = property.DeclaringType.DisplayName();
                var propertyName = property.Name;

                message
                    = exception is NullReferenceException
                        ? CoreStrings.ErrorMaterializingPropertyNullReference(entityType, propertyName, expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingPropertyInvalidCast(entityType, propertyName, expectedType, actualType)
                            : CoreStrings.ErrorMaterializingProperty(entityType, propertyName);
            }
            else
            {
                message
                    = exception is NullReferenceException
                        ? CoreStrings.ErrorMaterializingValueNullReference(expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                            : CoreStrings.ErrorMaterializingValue;
            }

            throw new InvalidOperationException(message, exception);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete("Use CreateReadValueExpression making sure to pass bound property if available.")]
        public virtual Expression CreateReadValueCallExpression(Expression valueBuffer, int index)
            => Expression.Call(valueBuffer, ValueBuffer.GetValueMethod, Expression.Constant(index));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression CreateMaterializeExpression(
            IEntityType entityType,
            Expression valueBufferExpression,
            int[] indexMap = null)
        {
            if (entityType is IEntityMaterializer materializer)
            {
                return Expression.Call(
                    Expression.Constant(materializer),
                    ((Func<ValueBuffer, object>)materializer.CreateEntity).GetMethodInfo(),
                    valueBufferExpression);
            }

            if (!entityType.HasClrType())
            {
                throw new InvalidOperationException(CoreStrings.NoClrType(entityType.DisplayName()));
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
                from property in entityType.GetProperties().Where(p => !p.IsShadowProperty)
                let targetMember = Expression.MakeMemberAccess(
                    instanceVariable,
                    property.GetMemberInfo(forConstruction: true, forSet: true))
                select
                    Expression.Assign(
                        targetMember,
                        CreateReadValueExpression(
                            valueBufferExpression,
                            targetMember.Type,
                            indexMap?[property.GetIndex()] ?? property.GetIndex(),
                            property)));

            blockExpressions.Add(instanceVariable);

            return Expression.Block(new[] { instanceVariable }, blockExpressions);
        }

        private ConcurrentDictionary<IEntityType, Func<ValueBuffer, object>> Materializers
            => _materializers
               ?? (_materializers = new ConcurrentDictionary<IEntityType, Func<ValueBuffer, object>>());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<ValueBuffer, object> GetMaterializer(IEntityType entityType)
            => Materializers.GetOrAdd(
                entityType, e =>
                    {
                        var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer), "values");

                        return Expression.Lambda<Func<ValueBuffer, object>>(
                                CreateMaterializeExpression(e, valueBufferParameter),
                                valueBufferParameter)
                            .Compile();
                    });
    }
}
