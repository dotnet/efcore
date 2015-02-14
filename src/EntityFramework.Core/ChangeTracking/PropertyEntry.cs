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

        public PropertyEntry([NotNull] InternalEntityEntry internalEntry, [NotNull] string name)
        {
            Check.NotNull(internalEntry, nameof(internalEntry));
            Check.NotEmpty(name, nameof(name));

            _internalEntry = internalEntry;
            Metadata = internalEntry.EntityType.GetProperty(name);
        }

        public virtual bool IsModified
        {
            get { return _internalEntry.IsPropertyModified(Metadata); }
            set { _internalEntry.SetPropertyModified(Metadata, value); }
        }

        public virtual IProperty Metadata { get; }

        public virtual object CurrentValue
        {
            get { return _internalEntry[Metadata]; }
            [param: CanBeNull] set { _internalEntry[Metadata] = value; }
        }

        public virtual object OriginalValue
        {
            get { return _internalEntry.OriginalValues[Metadata]; }
            [param: CanBeNull] set { _internalEntry.OriginalValues[Metadata] = value; }
        }
    }
}
