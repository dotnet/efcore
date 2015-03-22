// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ForeignKey : Annotatable, IForeignKey
    {
        private readonly Key _principalKey;

        private bool _isRequiredSet;
        
        public ForeignKey(
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] Key principalKey,
            [CanBeNull] EntityType principalEntityType = null)
        {
            Check.NotEmpty(dependentProperties, nameof(dependentProperties));
            Check.HasNoNulls(dependentProperties, nameof(dependentProperties));
            MetadataHelper.CheckSameEntityType(dependentProperties, nameof(dependentProperties));
            Check.NotNull(principalKey, nameof(principalKey));

            Properties = dependentProperties;

            var principalProperties = principalKey.Properties;

            Property.EnsureCompatible(principalProperties, dependentProperties);

            if (principalEntityType?.Keys.Contains(principalKey) == false)
            {
                throw new ArgumentException(
                    Strings.ForeignKeyReferencedEntityKeyMismatch(
                        principalKey,
                        principalEntityType));
            }

            _principalKey = principalKey;

            PrincipalEntityType = principalEntityType ?? _principalKey.EntityType;
        }

        [NotNull]
        public virtual IReadOnlyList<Property> Properties { get; }

        public virtual EntityType EntityType => Properties[0].EntityType;

        [NotNull]
        public virtual Key PrincipalKey => _principalKey;

        public virtual EntityType PrincipalEntityType { get; }

        public virtual bool? IsUnique { get; set; }

        protected virtual bool DefaultIsUnique => false;

        public virtual bool? IsRequired
        {
            get
            {
                if (!_isRequiredSet)
                {
                    return null;
                }

                return Properties.Any(p => p.IsNullable.HasValue)
                    ? !Properties.Any(p => ((IProperty)p).IsNullable) as bool?
                    : null;
            }
            set
            {
                _isRequiredSet = value.HasValue;
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

        IReadOnlyList<IProperty> IForeignKey.Properties => Properties;

        IEntityType IForeignKey.EntityType => EntityType;

        IEntityType IForeignKey.PrincipalEntityType => PrincipalEntityType;

        IKey IForeignKey.PrincipalKey => PrincipalKey;

        bool IForeignKey.IsUnique => IsUnique ?? DefaultIsUnique;

        bool IForeignKey.IsRequired => IsRequired ?? DefaultIsRequired;

        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "'{0}' {1} -> '{2}' {3}",
                EntityType.DisplayName(),
                Property.Format(Properties),
                PrincipalEntityType.DisplayName(),
                Property.Format(PrincipalKey.Properties));
        }
    }
}
