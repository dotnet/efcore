// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class DependentKeyValueFactoryFactory
    {
        public virtual IDependentKeyValueFactory<TKey> Create<TKey>([NotNull] IForeignKey foreignKey)
            => foreignKey.Properties.Count == 1
                ? CreateSimple<TKey>(foreignKey)
                : (IDependentKeyValueFactory<TKey>)CreateComposite(foreignKey);

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

            if (principalType.IsNullableType())
            {
                return (IDependentKeyValueFactory<TKey>)Activator.CreateInstance(
                    typeof(SimpleNullablePrincipalDependentKeyValueFactory<,>).MakeGenericType(
                        typeof(TKey), typeof(TKey).UnwrapNullableType()), propertyAccessors);
            }

            return new SimpleNonNullableDependentKeyValueFactory<TKey>(propertyAccessors);
        }

        public virtual IDependentKeyValueFactory<object[]> CreateComposite([NotNull] IForeignKey foreignKey)
            => new CompositeDependentValueFactory(foreignKey);
    }
}
