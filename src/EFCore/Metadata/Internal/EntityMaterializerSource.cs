// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class EntityMaterializerSource : IEntityMaterializerSource
    {
        private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> _materializers;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression CreateReadValueExpression(
            Expression valueBuffer,
            Type type,
            int index)
            => Expression.Call(
                TryReadValueMethod.MakeGenericMethod(type),
                valueBuffer,
                Expression.Constant(index));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo TryReadValueMethod
            = typeof(EntityMaterializerSource).GetTypeInfo()
                .GetDeclaredMethod(nameof(TryReadValue));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue TryReadValue<TValue>(
            in ValueBuffer valueBuffer, int index)
            => (TValue)valueBuffer[index];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression CreateMaterializeExpression(
            IEntityType entityType,
            Expression materializationExpression,
            int[] indexMap = null)
        {
            if (!entityType.HasClrType())
            {
                throw new InvalidOperationException(CoreStrings.NoClrType(entityType.DisplayName()));
            }

            if (entityType.IsAbstract())
            {
                throw new InvalidOperationException(CoreStrings.CannotMaterializeAbstractType(entityType));
            }

            var constructorBinding = (ConstructorBinding)entityType[CoreAnnotationNames.ConstructorBinding];

            if (constructorBinding == null)
            {
                var constructorInfo = entityType.ClrType.GetDeclaredConstructor(null);

                if (constructorInfo == null)
                {
                    throw new InvalidOperationException(CoreStrings.NoParameterlessConstructor(entityType.DisplayName()));
                }

                constructorBinding = new DirectConstructorBinding(constructorInfo, Array.Empty<ParameterBinding>());
            }

            // This is to avoid breaks because this method used to expect ValueBuffer but now expects MaterializationContext
            var valueBufferExpression = materializationExpression;
            if (valueBufferExpression.Type == typeof(MaterializationContext))
            {
                valueBufferExpression = Expression.Call(materializationExpression, MaterializationContext.GetValueBufferMethod);
            }
            else
            {
                materializationExpression = Expression.New(MaterializationContext.ObsoleteConstructor, materializationExpression);
            }

            var bindingInfo = new ParameterBindingInfo(
                entityType,
                materializationExpression,
                indexMap);

            var properties = new HashSet<IPropertyBase>(
                entityType.GetServiceProperties().Cast<IPropertyBase>()
                    .Concat(
                        entityType
                            .GetProperties()
                            .Where(p => !p.IsShadowProperty())));

            foreach (var consumedProperty in constructorBinding
                .ParameterBindings
                .SelectMany(p => p.ConsumedProperties))
            {
                properties.Remove(consumedProperty);
            }

            var constructorExpression = constructorBinding.CreateConstructorExpression(bindingInfo);

            if (properties.Count == 0)
            {
                return constructorExpression;
            }

            var instanceVariable = Expression.Variable(constructorBinding.RuntimeType, "instance");

            var blockExpressions
                = new List<Expression>
                {
                    Expression.Assign(
                        instanceVariable,
                        constructorExpression)
                };

            var indexerPropertyInfo = entityType.FindIndexerProperty();

            foreach (var property in properties)
            {
                var memberInfo = property.GetMemberInfo(forConstruction: true, forSet: true);

                var readValueExpression
                    = property is IServiceProperty serviceProperty
                        ? serviceProperty.GetParameterBinding().BindToParameter(bindingInfo)
                        : CreateReadValueExpression(
                            valueBufferExpression,
                            memberInfo.GetMemberType(),
                            indexMap?[property.GetIndex()] ?? property.GetIndex());

                blockExpressions.Add(
                    property.IsIndexedProperty()
                        ? Expression.Assign(
                            Expression.MakeIndex(
                                instanceVariable,
                                indexerPropertyInfo,
                                new[] { Expression.Constant(property.Name) }),
                            readValueExpression)
                        : Expression.MakeMemberAccess(
                            instanceVariable,
                            memberInfo).Assign(
                            readValueExpression));
            }

            blockExpressions.Add(instanceVariable);

            return Expression.Block(new[] { instanceVariable }, blockExpressions);
        }

        private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> Materializers
            => LazyInitializer.EnsureInitialized(
                ref _materializers,
                () => new ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>>());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<MaterializationContext, object> GetMaterializer(IEntityType entityType)
            => Materializers.GetOrAdd(
                entityType, e =>
                {
                    var materializationContextParameter
                        = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

                    return Expression.Lambda<Func<MaterializationContext, object>>(
                            CreateMaterializeExpression(e, materializationContextParameter),
                            materializationContextParameter)
                        .Compile();
                });
    }
}
