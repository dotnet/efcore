// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class ClrAccessorFactory<TAccessor>
        where TAccessor : class
    {
        private static readonly MethodInfo _genericCreate
            = typeof(ClrAccessorFactory<TAccessor>).GetTypeInfo().GetDeclaredMethods(nameof(CreateGeneric)).Single();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public abstract TAccessor Create([NotNull] IPropertyBase property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TAccessor Create([NotNull] MemberInfo memberInfo)
            => Create(memberInfo, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual TAccessor Create(MemberInfo memberInfo, IPropertyBase propertyBase)
        {
            var boundMethod = propertyBase != null
                ? _genericCreate.MakeGenericMethod(
                    propertyBase.DeclaringType.ClrType,
                    propertyBase.ClrType,
                    propertyBase.ClrType.UnwrapNullableType())
                : _genericCreate.MakeGenericMethod(
                    memberInfo.DeclaringType,
                    memberInfo.GetMemberType(),
                    memberInfo.GetMemberType().UnwrapNullableType());

            try
            {
                return (TAccessor)boundMethod.Invoke(
                    this, new object[]
                    {
                        memberInfo, propertyBase
                    });
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract TAccessor CreateGeneric<TEntity, TValue, TNonNullableEnumValue>(
            [CanBeNull] MemberInfo memberInfo,
            [CanBeNull] IPropertyBase propertyBase)
            where TEntity : class;
    }
}
