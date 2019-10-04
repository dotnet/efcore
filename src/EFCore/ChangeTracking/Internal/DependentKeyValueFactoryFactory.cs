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
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DependentKeyValueFactoryFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDependentKeyValueFactory<TKey> Create<TKey>([NotNull] IForeignKey foreignKey)
            => foreignKey.Properties.Count == 1
                ? CreateSimple<TKey>(foreignKey)
                : (IDependentKeyValueFactory<TKey>)CreateComposite(foreignKey);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDependentKeyValueFactory<TKey> CreateSimple<TKey>([NotNull] IForeignKey foreignKey)
        {
            var dependentProperty = foreignKey.Properties.Single();
            var principalType = foreignKey.PrincipalKey.Properties.Single().ClrType;
            var propertyAccessors = dependentProperty.GetPropertyAccessors();

            if (dependentProperty.ClrType.IsNullableType()
                && principalType.IsNullableType())
            {
                return new SimpleFullyNullableDependentKeyValueFactory<TKey>(propertyAccessors);
            }

            if (dependentProperty.ClrType.IsNullableType())
            {
                return (IDependentKeyValueFactory<TKey>)Activator.CreateInstance(
                    typeof(SimpleNullableDependentKeyValueFactory<>).MakeGenericType(
                        typeof(TKey)), propertyAccessors);
            }

            return principalType.IsNullableType()
                ? (IDependentKeyValueFactory<TKey>)Activator.CreateInstance(
                    typeof(SimpleNullablePrincipalDependentKeyValueFactory<,>).MakeGenericType(
                        typeof(TKey), typeof(TKey).UnwrapNullableType()), propertyAccessors)
                : new SimpleNonNullableDependentKeyValueFactory<TKey>(propertyAccessors);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDependentKeyValueFactory<object[]> CreateComposite([NotNull] IForeignKey foreignKey)
            => new CompositeValueFactory(foreignKey.Properties);
    }
}
