// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ForeignKey : MetadataBase, IForeignKey
    {
        private readonly EntityType _referencedEntityType;
        private readonly ImmutableList<Property> _properties;

        private string _storageName;

        public ForeignKey(
            [NotNull] EntityType referencedEntityType, [NotNull] IEnumerable<Property> properties)
        {
            Check.NotNull(referencedEntityType, "referencedEntityType");
            Check.NotNull(properties, "properties");

            _referencedEntityType = referencedEntityType;
            _properties = ImmutableList.CreateRange(properties);
        }

        public virtual IEnumerable<Property> Properties
        {
            get { return _properties; }
        }

        public virtual EntityType ReferencedEntityType
        {
            get { return _referencedEntityType; }
        }

        public virtual bool IsUnique { get; set; }

        public virtual bool IsRequired
        {
            get { return Properties.Any(p => !p.IsNullable); }
        }

        public virtual string StorageName
        {
            get { return _storageName; }
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");

                _storageName = value;
            }
        }
    }
}
