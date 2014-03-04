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
        private readonly ImmutableList<Property> _dependentProperties;

        private Property[] _principalProperties;

        private string _storageName;

        // Intended only for creation of test doubles
        internal ForeignKey()
        {
        }

        public ForeignKey(
            [NotNull] EntityType principalType,
            [NotNull] IEnumerable<Property> dependentProperties)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentProperties, "dependentProperties");

            _principalType = principalType;
            _dependentProperties = ImmutableList.CreateRange(dependentProperties);
        }

        public virtual IEnumerable<Property> DependentProperties
        {
            get { return _dependentProperties; }
        }

        public virtual IEnumerable<Property> PrincipalProperties
        {
            get { return _principalProperties ?? _principalType.Key; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                _principalProperties = value.ToArray();
            }
        }

        public virtual EntityType PrincipalType
        {
            get { return _principalType; }
        }

        public virtual bool IsUnique { get; set; }

        public virtual bool IsRequired
        {
            get { return DependentProperties.Any(p => !p.IsNullable); }
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

        IEnumerable<IProperty> IForeignKey.DependentProperties
        {
            get { return _dependentProperties; }
        }

        IEnumerable<IProperty> IForeignKey.PrincipalProperties
        {
            get { return PrincipalProperties; }
        }

        IEntityType IForeignKey.PrincipalType
        {
            get { return _principalType; }
        }
    }
}
