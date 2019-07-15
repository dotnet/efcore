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
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class EntityMaterializerSource : IEntityMaterializerSource
    {
        private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> _materializers;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression CreateReadValueExpression(
            Expression valueBufferExpression,
            Type type,
            int index,
            IPropertyBase property)
            => Expression.Call(
                TryReadValueMethod.MakeGenericMethod(type),
                valueBufferExpression,
                Expression.Constant(index),
                Expression.Constant(property, typeof(IPropertyBase)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly MethodInfo TryReadValueMethod
            = typeof(EntityMaterializerSource).GetTypeInfo()
                .GetDeclaredMethod(nameof(TryReadValue));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue TryReadValue<TValue>(
            in ValueBuffer valueBuffer, int index, IPropertyBase property)
            => (TValue)valueBuffer[index];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression CreateMaterializeExpression(
            IEntityType entityType,
            string entityInstanceName,
            Expression materializationContextExpression,
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

            var constructorBinding = (InstantiationBinding)entityType[CoreAnnotationNames.ConstructorBinding];

            if (constructorBinding == null)
            {
                var constructorInfo = entityType.ClrType.GetDeclaredConstructor(null);

                if (constructorInfo == null)
                {
                    throw new InvalidOperationException(CoreStrings.NoParameterlessConstructor(entityType.DisplayName()));
                }

                constructorBinding = new ConstructorBinding(constructorInfo, Array.Empty<ParameterBinding>());
            }

            var bindingInfo = new ParameterBindingInfo(
                entityType,
                materializationContextExpression,
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

            var instanceVariable = Expression.Variable(constructorBinding.RuntimeType, entityInstanceName);

            var blockExpressions
                = new List<Expression>
                {
                    Expression.Assign(
                        instanceVariable,
                        constructorExpression)
                };

            var valueBufferExpression = Expression.Call(materializationContextExpression, MaterializationContext.GetValueBufferMethod);

            foreach (var property in properties)
            {
                var memberInfo = property.GetMemberInfo(forConstruction: true, forSet: true);

                var readValueExpression
                    = property is IServiceProperty serviceProperty
                        ? serviceProperty.GetParameterBinding().BindToParameter(bindingInfo)
                        : CreateReadValueExpression(
                            valueBufferExpression,
                            memberInfo.GetMemberType(),
                            indexMap?[property.GetIndex()] ?? property.GetIndex(),
                            property);

                blockExpressions.Add(
                    Expression.MakeMemberAccess(
                        instanceVariable,
                        memberInfo).Assign(
                        readValueExpression));
            }

            blockExpressions.Add(instanceVariable);

            return Expression.Block(
                new[]
                {
                    instanceVariable
                }, blockExpressions);
        }

        private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> Materializers
            => LazyInitializer.EnsureInitialized(
                ref _materializers,
                () => new ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>>());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<MaterializationContext, object> GetMaterializer(IEntityType entityType)
            => Materializers.GetOrAdd(
                entityType, e =>
                {
                    var materializationContextParameter
                        = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

                    return Expression.Lambda<Func<MaterializationContext, object>>(
                            CreateMaterializeExpression(e, "instance", materializationContextParameter),
                            materializationContextParameter)
                        .Compile();
                });
    }
}
