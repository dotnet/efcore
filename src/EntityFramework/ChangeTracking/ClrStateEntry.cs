// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class ClrStateEntry : StateEntry
    {
        private readonly object _entity;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ClrStateEntry()
        {
        }

        public ClrStateEntry(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] StateEntryMetadataServices metadataServices,
            [NotNull] object entity)
            : base(configuration, entityType, metadataServices)
        {
            Check.NotNull(entity, "entity");

            _entity = entity;
        }

        [NotNull]
        public override object Entity
        {
            get { return _entity; }
        }
    }
}
