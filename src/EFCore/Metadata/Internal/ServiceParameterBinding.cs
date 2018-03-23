// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class ServiceParameterBinding : ParameterBinding
    {
        private Func<MaterializationContext, IEntityType, object, object> _serviceDelegate;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ServiceParameterBinding(
            [NotNull] Type parameterType,
            [NotNull] Type serviceType,
            [CanBeNull] IPropertyBase consumedProperty = null)
            : base(parameterType, consumedProperty != null ? new[] { consumedProperty } : Array.Empty<IPropertyBase>())
        {
            ServiceType = serviceType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ServiceType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
            => BindToParameter(
                bindingInfo.MaterializationContextExpression,
                Expression.Constant(bindingInfo.EntityType),
                null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract Expression BindToParameter(
            [NotNull] Expression materializationExpression,
            [NotNull] Expression entityTypeExpression,
            [CanBeNull] Expression entityExpression);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<MaterializationContext, IEntityType, object, object> ServiceDelegate
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _serviceDelegate, this, b =>
                {
                    var materializationContextParam = Expression.Parameter(typeof(MaterializationContext));
                    var entityTypeParam = Expression.Parameter(typeof(IEntityType));
                    var entityParam = Expression.Parameter(typeof(object));

                    return Expression.Lambda<Func<MaterializationContext, IEntityType, object, object>>(
                        b.BindToParameter(materializationContextParam, entityTypeParam, entityParam),
                        materializationContextParam,
                        entityTypeParam,
                        entityParam).Compile();
                });
    }
}
