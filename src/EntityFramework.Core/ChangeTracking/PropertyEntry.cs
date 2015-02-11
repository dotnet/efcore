// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class PropertyEntry
    {
        private readonly InternalEntityEntry _internalEntry;
        private readonly IProperty _property;

        public PropertyEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
        {
            Check.NotNull(internalEntry, nameof(internalEntry));
            Check.NotEmpty(name, nameof(name));

            _internalEntry = internalEntry;
            _property = internalEntry.EntityType.GetProperty(name);
        }

        public virtual bool IsModified
        {
            get { return _internalEntry.IsPropertyModified(_property); }
            set { _internalEntry.SetPropertyModified(_property, value); }
        }

        public virtual string Name => _property.Name;

        public virtual object CurrentValue
        {
            get { return _internalEntry[_property]; }
            [param: CanBeNull] set { _internalEntry[_property] = value; }
        }

        public virtual object OriginalValue
        {
            get { return _internalEntry.OriginalValues[_property]; }
            [param: CanBeNull] set { _internalEntry.OriginalValues[_property] = value; }
        }
    }
}
