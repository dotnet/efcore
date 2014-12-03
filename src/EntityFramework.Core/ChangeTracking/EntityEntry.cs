// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    [DebuggerDisplay("{_stateEntry,nq}")]
    public class EntityEntry
    {
        private readonly StateEntry _stateEntry;

        public EntityEntry([NotNull] StateEntry stateEntry)
        {
            Check.NotNull(stateEntry, "stateEntry");

            _stateEntry = stateEntry;
        }

        public virtual object Entity
        {
            get { return _stateEntry.Entity; }
        }

        public virtual EntityState State
        {
            get { return _stateEntry.EntityState; }
            set
            {
                Check.IsDefined(value, "value");

                _stateEntry.EntityState = value;
            }
        }

        public virtual StateEntry StateEntry
        {
            get { return _stateEntry; }
        }

        public virtual PropertyEntry Property([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            return new PropertyEntry(_stateEntry, propertyName);
        }
    }
}
