// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class DependentKeyValueFactoryFactory
    {
        public virtual IDependentKeyValueFactory<TKey> Create<TKey>([NotNull] IForeignKey foreignKey)
        {
            if (foreignKey.Properties.Count == 1)
            {
                var keyType = foreignKey.PrincipalKey.Properties.Single().ClrType;
                if (!typeof(IStructuralEquatable).GetTypeInfo().IsAssignableFrom(keyType.GetTypeInfo()))
                {
                    return CreateSimple<TKey>(foreignKey);
                }
            }

            return (IDependentKeyValueFactory<TKey>)CreateComposite(foreignKey);
        }

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

        public virtual IDependentKeyValueFactory<IKeyValue> CreateComposite([NotNull] IForeignKey foreignKey)
            => new CompositeDependentValueFactory(foreignKey);
    }
}
