// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EmptyShadowValuesFactoryFactory : SnapshotFactoryFactory
    {
        /// <inheritdoc />
        protected override int GetPropertyIndex(IPropertyBase propertyBase)
            => propertyBase.GetShadowIndex();

        /// <inheritdoc />
        protected override int GetPropertyCount(IEntityType entityType)
            => entityType.ShadowPropertyCount();

        /// <inheritdoc />
        protected override ValueComparer GetValueComparer(IProperty property)
            => null;

        /// <inheritdoc />
        protected override bool UseEntityVariable
            => false;

        /// <inheritdoc />
        protected override Expression CreateReadShadowValueExpression(ParameterExpression parameter, IPropertyBase property)
            => Expression.Default(property.ClrType);

        /// <inheritdoc />
        protected override Expression CreateReadValueExpression(ParameterExpression parameter, IPropertyBase property)
            => Expression.Default(property.ClrType);
    }
}
