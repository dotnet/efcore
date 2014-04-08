// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntryFactory
    {
        private readonly ContextConfiguration _configuration;
        private readonly EntityMaterializerSource _materializerSource;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateEntryFactory()
        {
        }

        public StateEntryFactory(
            [NotNull] ContextConfiguration configuration,
            [NotNull] EntityMaterializerSource materializerSource)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(materializerSource, "materializerSource");

            _configuration = configuration;
            _materializerSource = materializerSource;
        }

        public virtual StateEntry Create([NotNull] IEntityType entityType, [CanBeNull] object entity)
        {
            Check.NotNull(entityType, "entityType");

            return NewStateEntry(entityType, entity);
        }

        public virtual StateEntry Create([NotNull] IEntityType entityType, [NotNull] IValueReader valueReader)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotNull(valueReader, "valueReader");

            return NewStateEntry(entityType, valueReader);
        }

        private StateEntry NewStateEntry(IEntityType entityType, object entity)
        {
            if (!entityType.HasClrType)
            {
                return new ShadowStateEntry(_configuration, entityType);
            }

            Check.NotNull(entity, "entity");

            return entityType.ShadowPropertyCount > 0
                ? (StateEntry)new MixedStateEntry(_configuration, entityType, entity)
                : new ClrStateEntry(_configuration, entityType, entity);
        }

        private StateEntry NewStateEntry(IEntityType entityType, IValueReader valueReader)
        {
            if (!entityType.HasClrType)
            {
                return new ShadowStateEntry(_configuration, entityType, valueReader);
            }

            var entity = _materializerSource.GetMaterializer(entityType)(valueReader);

            return entityType.ShadowPropertyCount > 0
                ? (StateEntry)new MixedStateEntry(_configuration, entityType, entity, valueReader)
                : new ClrStateEntry(_configuration, entityType, entity);
        }
    }
}
