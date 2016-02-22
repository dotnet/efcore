// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalShadowEntityEntry : InternalEntityEntry
    {
        private readonly ISnapshot _propertyValues;

        public override object Entity => null;

        public InternalShadowEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType)
            : base(stateManager, entityType)
        {
            _propertyValues = entityType.GetEmptyShadowValuesFactory()();
        }

        public InternalShadowEntityEntry(
            [NotNull] IStateManager stateManager,
            [NotNull] IEntityType entityType,
            ValueBuffer valueBuffer)
            : base(stateManager, entityType)
        {
            _propertyValues = entityType.GetShadowValuesFactory()(valueBuffer);
        }

        protected override T ReadShadowValue<T>(int shadowIndex)
            => _propertyValues.GetValue<T>(shadowIndex);

        protected override object ReadPropertyValue(IPropertyBase propertyBase)
        {
            var property = propertyBase as IProperty;
            Debug.Assert((property != null) && property.IsShadowProperty);

            return _propertyValues[property.GetShadowIndex()];
        }

        protected override void WritePropertyValue(IPropertyBase propertyBase, object value)
        {
            var property = propertyBase as IProperty;
            Debug.Assert((property != null) && property.IsShadowProperty);

            _propertyValues[property.GetShadowIndex()] = value;
        }
    }
}
