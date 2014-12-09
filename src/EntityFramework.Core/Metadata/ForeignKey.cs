// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
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

        public ForeignKey([NotNull] IReadOnlyList<Property> dependentProperties, [NotNull] Key referencedKey)
            : base(dependentProperties)
        {
            Check.NotNull(referencedKey, "referencedKey");

            var principalProperties = referencedKey.Properties;

            if (principalProperties.Count != dependentProperties.Count)
            {
                throw new ArgumentException(
                    Strings.ForeignKeyCountMismatch(
                        Property.Format(dependentProperties),
                        dependentProperties[0].EntityType.Name,
                        Property.Format(principalProperties),
                        referencedKey.EntityType.Name));
            }

            if (!principalProperties.Select(p => p.UnderlyingType).SequenceEqual(dependentProperties.Select(p => p.UnderlyingType)))
            {
                throw new ArgumentException(
                    Strings.ForeignKeyTypeMismatch(
                        Property.Format(dependentProperties),
                        dependentProperties[0].EntityType.Name, referencedKey.EntityType.Name));
            }

            _referencedKey = referencedKey;
        }

        [NotNull]
        public virtual IReadOnlyList<Property> ReferencedProperties
        {
            get { return _referencedKey.Properties; }
        }

        [NotNull]
        public virtual Key ReferencedKey
        {
            get { return _referencedKey; }
        }

        public virtual EntityType ReferencedEntityType
        {
            get { return _referencedKey.EntityType; }
        }

        public virtual bool? IsUnique { get; set; }

        protected virtual bool DefaultIsUnique
        {
            get { return false; }
        }

        public virtual bool? IsRequired
        {
            get
            {
                return Properties.Any(p => p.IsNullable.HasValue)
                    ? !Properties.Any(p => ((IProperty)p).IsNullable) as bool?
                    : null;
            }
            set
            {
                var properties = Properties;
                if (value.HasValue
                    && !value.Value)
                {
                    var nullableTypeProperties = Properties.Where(p => p.PropertyType.IsNullableType()).ToList();
                    if (nullableTypeProperties.Any())
                    {
                        properties = nullableTypeProperties;
                    }
                }

                foreach (var property in properties)
                {
                    // TODO: Depending on resolution of #723 this may change
                    property.IsNullable = !value;
                }
            }
        }

        protected virtual bool DefaultIsRequired
        {
            get { return !((IForeignKey)this).Properties.Any(p => p.IsNullable); }
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

        bool IForeignKey.IsUnique
        {
            get { return IsUnique ?? DefaultIsUnique; }
        }

        bool IForeignKey.IsRequired
        {
            get { return IsRequired ?? DefaultIsRequired; }
        }
    }
}
