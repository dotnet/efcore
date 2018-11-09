// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalMixedEntityEntry : InternalEntityEntry
    {
        private readonly ISnapshot _shadowValues;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalMixedEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] object entity)
            : base(stateManager, entityType)
        {
            Entity = entity;
            _shadowValues = entityType.GetEmptyShadowValuesFactory()();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            MarkShadowPropertiesNotSet(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalMixedEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            in ValueBuffer valueBuffer)
            : base(stateManager, entityType)
        {
            Entity = entity;
            _shadowValues = ((EntityType)entityType).ShadowValuesFactory(valueBuffer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object Entity { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override T ReadShadowValue<T>(int shadowIndex)
            => _shadowValues.GetValue<T>(shadowIndex);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override object ReadPropertyValue(IPropertyBase propertyBase)
            => !propertyBase.IsShadowProperty
                ? base.ReadPropertyValue(propertyBase)
                : _shadowValues[propertyBase.GetShadowIndex()];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool PropertyHasDefaultValue(IPropertyBase propertyBase)
            => !propertyBase.IsShadowProperty
                ? base.PropertyHasDefaultValue(propertyBase)
                : propertyBase.ClrType.IsDefaultValue(_shadowValues[propertyBase.GetShadowIndex()]);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
        {
            if (!propertyBase.IsShadowProperty)
            {
                base.WritePropertyValue(propertyBase, value);
            }
            else
            {
                _shadowValues[propertyBase.GetShadowIndex()] = value;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object GetOrCreateCollection(INavigation navigation)
            => navigation.IsShadowProperty
                ? GetOrCreateCollectionTyped(navigation)
                : base.GetOrCreateCollection(navigation);

        private ICollection<object> GetOrCreateCollectionTyped(INavigation navigation)
        {
            if (!(_shadowValues[navigation.GetShadowIndex()] is ICollection<object> collection))
            {
                collection = new HashSet<object>();
                _shadowValues[navigation.GetShadowIndex()] = collection;
            }

            return collection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CollectionContains(INavigation navigation, InternalEntityEntry value)
            => navigation.IsShadowProperty
                ? GetOrCreateCollectionTyped(navigation).Contains(value.Entity)
                : base.CollectionContains(navigation, value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool AddToCollection(INavigation navigation, InternalEntityEntry value)
        {
            if (!navigation.IsShadowProperty)
            {
                return base.AddToCollection(navigation, value);
            }

            if (navigation.GetTargetType().ClrType == null)
            {
                return false;
            }

            var collection = GetOrCreateCollectionTyped(navigation);
            if (!collection.Contains(value.Entity))
            {
                collection.Add(value.Entity);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void RemoveFromCollection(INavigation navigation, InternalEntityEntry value)
        {
            if (navigation.IsShadowProperty)
            {
                GetOrCreateCollectionTyped(navigation).Remove(value.Entity);
            }
            else
            {
                base.RemoveFromCollection(navigation, value);
            }
        }
    }
}
