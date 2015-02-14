// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    [DebuggerDisplay("{InternalEntry,nq}")]
    public class EntityEntry
    {
        public EntityEntry([NotNull] DbContext context, [NotNull] InternalEntityEntry internalEntry)
        {
            Check.NotNull(internalEntry, nameof(internalEntry));
            Check.NotNull(context, nameof(context));

            InternalEntry = internalEntry;
            Context = context;
        }

        public virtual object Entity => InternalEntry.Entity;

        public virtual EntityState State
        {
            get { return InternalEntry.EntityState; }
            set
            {
                Check.IsDefined(value, nameof(value));

                InternalEntry.SetEntityState(value);
            }
        }

        public virtual InternalEntityEntry InternalEntry { get; }

        public virtual DbContext Context { get; }

        public virtual IEntityType Metadata => InternalEntry.EntityType;

        public virtual PropertyEntry Property([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            return new PropertyEntry(InternalEntry, propertyName);
        }

        public virtual bool IsKeySet => InternalEntry.IsKeySet;
    }
}
