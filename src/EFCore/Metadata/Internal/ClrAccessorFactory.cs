// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class ClrAccessorFactory<TAccessor>
        where TAccessor : class
    {
        private static readonly MethodInfo _genericCreate
            = typeof(ClrAccessorFactory<TAccessor>).GetTypeInfo().GetDeclaredMethods(nameof(CreateGeneric)).Single();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TAccessor Create([NotNull] IPropertyBase property)
            => property as TAccessor ?? Create(property.PropertyInfo, property);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TAccessor Create([NotNull] PropertyInfo propertyInfo)
            => Create(propertyInfo, null);

        private TAccessor Create(PropertyInfo propertyInfo, IPropertyBase propertyBase)
        {
            var boundMethod = propertyBase != null
                ? _genericCreate.MakeGenericMethod(
                    propertyBase.DeclaringType.ClrType,
                    propertyBase.ClrType,
                    propertyBase.ClrType.UnwrapNullableType())
                : _genericCreate.MakeGenericMethod(
                    propertyInfo.DeclaringType,
                    propertyInfo.PropertyType,
                    propertyInfo.PropertyType.UnwrapNullableType());

            try
            {
                return (TAccessor)boundMethod.Invoke(this, new object[] { propertyInfo, propertyBase });
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract TAccessor CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] IPropertyBase propertyBase)
            where TEntity : class;
    }
}
