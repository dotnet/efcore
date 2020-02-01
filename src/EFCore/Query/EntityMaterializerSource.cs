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
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class EntityMaterializerSource : IEntityMaterializerSource
    {
        private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> _materializers;

        public EntityMaterializerSource([NotNull] EntityMaterializerSourceDependencies dependencies)
        {
        }

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

        public static readonly MethodInfo TryReadValueMethod
            = typeof(EntityMaterializerSource).GetTypeInfo()
                .GetDeclaredMethod(nameof(TryReadValue));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TValue TryReadValue<TValue>(
            in ValueBuffer valueBuffer, int index, IPropertyBase property)
            => valueBuffer[index] is TValue value ? value : default;

        public virtual Expression CreateMaterializeExpression(
            IEntityType entityType,
            string entityInstanceName,
            Expression materializationContextExpression)
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
                materializationContextExpression);

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
                var memberInfo = property.GetMemberInfo(forMaterialization: true, forSet: true);

                var readValueExpression
                    = property is IServiceProperty serviceProperty
                        ? serviceProperty.GetParameterBinding().BindToParameter(bindingInfo)
                        : CreateReadValueExpression(
                            valueBufferExpression,
                            memberInfo.GetMemberType(),
                            property.GetIndex(),
                            property);

                blockExpressions.Add(CreateMemberAssignment(instanceVariable, memberInfo, property, readValueExpression));
            }

            blockExpressions.Add(instanceVariable);

            return Expression.Block(new[] { instanceVariable }, blockExpressions);

            static Expression CreateMemberAssignment(Expression parameter, MemberInfo memberInfo, IPropertyBase property, Expression value)
            {
                return property.IsIndexerProperty()
                    ? Expression.Assign(
                        Expression.MakeIndex(
                            parameter, (PropertyInfo)memberInfo, new List<Expression>() { Expression.Constant(property.Name) }),
                        value)
                    : Expression.MakeMemberAccess(parameter, memberInfo).Assign(value);
            }
        }

        private ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>> Materializers
            => LazyInitializer.EnsureInitialized(
                ref _materializers,
                () => new ConcurrentDictionary<IEntityType, Func<MaterializationContext, object>>());

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
