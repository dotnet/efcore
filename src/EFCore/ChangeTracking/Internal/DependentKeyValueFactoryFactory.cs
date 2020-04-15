// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
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
    public class DependentKeyValueFactoryFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IDependentKeyValueFactory<TKey> Create<TKey>([NotNull] IForeignKey foreignKey)
            => foreignKey.Properties.Count == 1
                ? CreateSimple<TKey>(foreignKey)
                : (IDependentKeyValueFactory<TKey>)CreateComposite(foreignKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IDependentKeyValueFactory<TKey> CreateSimple<TKey>([NotNull] IForeignKey foreignKey)
        {
            var dependentProperty = foreignKey.Properties.Single();
            var principalType = foreignKey.PrincipalKey.Properties.Single().ClrType;
            var propertyAccessors = dependentProperty.GetPropertyAccessors();

            if (dependentProperty.ClrType.IsNullableType()
                && principalType.IsNullableType())
            {
                return new SimpleFullyNullableDependentKeyValueFactory<TKey>(dependentProperty, propertyAccessors);
            }

            if (dependentProperty.ClrType.IsNullableType())
            {
                return (IDependentKeyValueFactory<TKey>)Activator.CreateInstance(
                    typeof(SimpleNullableDependentKeyValueFactory<>).MakeGenericType(
                        typeof(TKey)), dependentProperty, propertyAccessors);
            }

            return principalType.IsNullableType()
                ? (IDependentKeyValueFactory<TKey>)Activator.CreateInstance(
                    typeof(SimpleNullablePrincipalDependentKeyValueFactory<,>).MakeGenericType(
                        typeof(TKey), typeof(TKey).UnwrapNullableType()), dependentProperty, propertyAccessors)
                : new SimpleNonNullableDependentKeyValueFactory<TKey>(dependentProperty, propertyAccessors);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IDependentKeyValueFactory<object[]> CreateComposite([NotNull] IForeignKey foreignKey)
            => new CompositeValueFactory(foreignKey.Properties);
    }
}
