// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
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
            [NotNull] ContextConfiguration configuration,
            [NotNull] IEntityType entityType,
            [NotNull] object entity)
            : base(configuration, entityType)
        {
            Check.NotNull(entity, "entity");

            _entity = entity;
        }

        [NotNull]
        public override object Entity
        {
            get { return _entity; }
        }

        protected override object ReadPropertyValue(IProperty property)
        {
            Check.NotNull(property, "property");

            return Configuration.ClrPropertyGetterSource.GetAccessor(property).GetClrValue(_entity);
        }

        protected override void WritePropertyValue(IProperty property, object value)
        {
            Check.NotNull(property, "property");

            Configuration.ClrPropertySetterSource.GetAccessor(property).SetClrValue(_entity, value);
        }
    }
}
