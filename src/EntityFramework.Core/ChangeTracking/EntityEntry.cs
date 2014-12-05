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
        public EntityEntry([NotNull] DbContext context, [NotNull] StateEntry stateEntry)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(context, "context");

            StateEntry = stateEntry;
            Context = context;
        }

        public virtual object Entity => StateEntry.Entity;

        public virtual EntityState State
        {
            get { return StateEntry.EntityState; }
            set
            {
                Check.IsDefined(value, "value");

                StateEntry.EntityState = value;
            }
        }

        public virtual StateEntry StateEntry { get; }
        public virtual DbContext Context { get; }

        public virtual PropertyEntry Property([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            return new PropertyEntry(StateEntry, propertyName);
        }
    }
}
