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
    public class InternalShadowEntityEntry : InternalEntityEntry
    {
        private readonly ISnapshot _propertyValues;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object Entity => null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalShadowEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType)
            : base(stateManager, entityType)
        {
            _propertyValues = entityType.GetEmptyShadowValuesFactory()();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            MarkShadowPropertiesNotSet(entityType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalShadowEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            in ValueBuffer valueBuffer)
            : base(stateManager, entityType)
        {
            _propertyValues = ((EntityType)entityType).ShadowValuesFactory(valueBuffer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override T ReadShadowValue<T>(int shadowIndex)
            => _propertyValues.GetValue<T>(shadowIndex);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override object ReadPropertyValue(IPropertyBase propertyBase)
            => _propertyValues[propertyBase.GetShadowIndex()];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool PropertyHasDefaultValue(IPropertyBase propertyBase)
            => propertyBase.ClrType.IsDefaultValue(_propertyValues[propertyBase.GetShadowIndex()]);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
            => _propertyValues[propertyBase.GetShadowIndex()] = value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override object GetOrCreateCollection(INavigation navigation)
            => GetOrCreateCollectionTyped(navigation);

        private ICollection<object> GetOrCreateCollectionTyped(INavigation navigation)
        {
            if (!(_propertyValues[navigation.GetShadowIndex()] is ICollection<object> collection))
            {
                collection = new HashSet<object>();
                _propertyValues[navigation.GetShadowIndex()] = collection;
            }

            return collection;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CollectionContains(INavigation navigation, InternalEntityEntry value)
            => GetOrCreateCollectionTyped(navigation).Contains(value.Entity);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool AddToCollection(INavigation navigation, InternalEntityEntry value)
        {
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
            => GetOrCreateCollectionTyped(navigation).Remove(value.Entity);
    }
}
