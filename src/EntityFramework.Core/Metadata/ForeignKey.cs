// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ForeignKey : Annotatable, IForeignKey
    {
        private readonly Key _principalKey;

        private bool _isRequiredSet;
        private Navigation _dependentToPrincipal;
        private Navigation _principalToDependent;

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

            if (principalEntityType?.GetKeys().Contains(principalKey) == false)
            {
                throw new ArgumentException(
                    Strings.ForeignKeyReferencedEntityKeyMismatch(
                        principalKey,
                        principalEntityType));
            }

            _principalKey = principalKey;

            PrincipalEntityType = principalEntityType ?? _principalKey.EntityType;
        }

        public virtual Navigation DependentToPrincipal
        {
            get { return _dependentToPrincipal; }
            [param: CanBeNull]
            set
            {
                CheckNavigation(value, _principalToDependent);

                _dependentToPrincipal = value;
            }
        }

        public virtual Navigation PrincipalToDependent
        {
            get { return _principalToDependent; }
            [param: CanBeNull]
            set
            {
                CheckNavigation(value, _dependentToPrincipal);

                _principalToDependent = value;
            }
        }

        private void CheckNavigation(Navigation value, Navigation otherNavigation)
        {
            if (value != null)
            {
                if (value.ForeignKey != this)
                {
                    throw new InvalidOperationException(
                        Strings.NavigationForWrongForeignKey(value.Name, value.EntityType.DisplayName(), this, value.ForeignKey));
                }

                if (value == otherNavigation)
                {
                    throw new InvalidOperationException(Strings.NavigationToSelfDuplicate(value.Name));
                }
            }
        }

        public virtual IReadOnlyList<Property> Properties { get; }

        public virtual EntityType EntityType => Properties[0].EntityType;

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
                    var nullableTypeProperties = Properties.Where(p => p.ClrType.IsNullableType()).ToList();
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

        protected virtual bool DefaultIsRequired => !((IForeignKey)this).Properties.Any(p => p.IsNullable);

        IReadOnlyList<IProperty> IForeignKey.Properties => Properties;

        IEntityType IForeignKey.EntityType => EntityType;

        IEntityType IForeignKey.PrincipalEntityType => PrincipalEntityType;

        IKey IForeignKey.PrincipalKey => PrincipalKey;

        INavigation IForeignKey.DependentToPrincipal => DependentToPrincipal;

        INavigation IForeignKey.PrincipalToDependent => PrincipalToDependent;

        bool IForeignKey.IsUnique => IsUnique ?? DefaultIsUnique;

        bool IForeignKey.IsRequired => IsRequired ?? DefaultIsRequired;

        public override string ToString()
            => $"'{EntityType.DisplayName()}' {Property.Format(Properties)} -> '{PrincipalEntityType.DisplayName()}' {Property.Format(PrincipalKey.Properties)}";
    }
}
