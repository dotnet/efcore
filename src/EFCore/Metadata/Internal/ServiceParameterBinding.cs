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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class ServiceParameterBinding : ParameterBinding
    {
        private Func<MaterializationContext, IEntityType, object, object> _serviceDelegate;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type ServiceType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
            => BindToParameter(
                bindingInfo.MaterializationContextExpression,
                Expression.Constant(bindingInfo.EntityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract Expression BindToParameter(
            [NotNull] Expression materializationExpression,
            [NotNull] Expression entityTypeExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<MaterializationContext, IEntityType, object, object> ServiceDelegate
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _serviceDelegate, this, b =>
                {
                    var materializationContextParam = Expression.Parameter(typeof(MaterializationContext));
                    var entityTypeParam = Expression.Parameter(typeof(IEntityType));
                    var entityParam = Expression.Parameter(typeof(object));

                    return Expression.Lambda<Func<MaterializationContext, IEntityType, object, object>>(
                        b.BindToParameter(materializationContextParam, entityTypeParam),
                        materializationContextParam,
                        entityTypeParam,
                        entityParam).Compile();
                });
    }
}
