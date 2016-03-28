// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalMixedEntityEntry : InternalEntityEntry
    {
        private readonly ISnapshot _shadowValues;

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

        public InternalMixedEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            [NotNull] object entity,
            ValueBuffer valueBuffer)
            : base(stateManager, entityType)
        {
            Entity = entity;
            _shadowValues = entityType.GetShadowValuesFactory()(valueBuffer);
        }

        public override object Entity { get; }

        protected override T ReadShadowValue<T>(int shadowIndex)
            => _shadowValues.GetValue<T>(shadowIndex);

        protected override object ReadPropertyValue(IPropertyBase propertyBase)
        {
            var property = propertyBase as IProperty;

            return (property == null) || !property.IsShadowProperty
                ? base.ReadPropertyValue(propertyBase)
                : _shadowValues[property.GetShadowIndex()];
        }

        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
        {
            var property = propertyBase as IProperty;

            if ((property == null)
                || !property.IsShadowProperty)
            {
                base.WritePropertyValue(propertyBase, value);
            }
            else
            {
                _shadowValues[property.GetShadowIndex()] = value;
            }
        }
    }
}
