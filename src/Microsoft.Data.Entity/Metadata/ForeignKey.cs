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
        private readonly EntityType _principalType;
        private readonly ImmutableList<PropertyPair> _properties;

        private string _storageName;

        // Intended only for creation of test doubles
        internal ForeignKey()
        {
        }

        public ForeignKey(
            [NotNull] EntityType principalType, 
            [NotNull] IEnumerable<PropertyPair> properties)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(properties, "properties");

            _principalType = principalType;
            _properties = ImmutableList.CreateRange(properties);
        }

        public virtual IEnumerable<PropertyPair> Properties
        {
            get { return _properties; }
        }

        public virtual EntityType PrincipalType
        {
            get { return _principalType; }
        }

        public virtual bool IsUnique { get; set; }

        public virtual bool IsRequired
        {
            get { return Properties.Any(p => !p.Dependent.IsNullable); }
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

        IEnumerable<IPropertyPair> IForeignKey.Properties
        {
            get { return _properties; }
        }

        IEntityType IForeignKey.PrincipalType
        {
            get { return _principalType; }
        }
    }
}
