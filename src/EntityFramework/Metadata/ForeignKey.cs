// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ForeignKey : Key, IForeignKey
    {
        private readonly Key _referencedKey;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected ForeignKey()
        {
        }

        public ForeignKey([NotNull] Key referencedKey, [NotNull] IReadOnlyList<Property> dependentProperties)
            : base(dependentProperties)
        {
            Check.NotNull(referencedKey, "referencedKey");

            var principalProperties = referencedKey.Properties;

            if (principalProperties.Count != dependentProperties.Count)
            {
                throw new ArgumentException(
                    Strings.FormatForeignKeyCountMismatch(dependentProperties[0].EntityType.Name, referencedKey.EntityType.Name));
            }

            if (!principalProperties.Select(p => p.UnderlyingType).SequenceEqual(dependentProperties.Select(p => p.UnderlyingType)))
            {
                throw new ArgumentException(
                    Strings.FormatForeignKeyTypeMismatch(dependentProperties[0].EntityType.Name, referencedKey.EntityType.Name));
            }

            _referencedKey = referencedKey;
        }

        public virtual IReadOnlyList<Property> ReferencedProperties
        {
            get { return _referencedKey.Properties; }
        }

        public virtual Key ReferencedKey
        {
            get { return _referencedKey; }
        }

        public virtual EntityType ReferencedEntityType
        {
            get { return _referencedKey.EntityType; }
        }

        public virtual bool IsUnique { get; set; }

        public virtual bool IsRequired
        {
            get { return Properties.Any(p => !p.IsNullable); }
        }

        IReadOnlyList<IProperty> IForeignKey.ReferencedProperties
        {
            get { return ReferencedProperties; }
        }

        IEntityType IForeignKey.ReferencedEntityType
        {
            get { return ReferencedEntityType; }
        }

        IKey IForeignKey.ReferencedKey
        {
            get { return ReferencedKey; }
        }
    }
}
