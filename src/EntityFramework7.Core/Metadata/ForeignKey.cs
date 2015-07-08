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
        private Navigation _dependentToPrincipal;
        private Navigation _principalToDependent;

        public ForeignKey(
            [NotNull] IReadOnlyList<Property> dependentProperties,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType)
        {
            Check.NotEmpty(dependentProperties, nameof(dependentProperties));
            Check.HasNoNulls(dependentProperties, nameof(dependentProperties));
            MetadataHelper.CheckSameEntityType(dependentProperties, nameof(dependentProperties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            Properties = dependentProperties;

            PrincipalKey = principalKey;

            PrincipalEntityType = principalEntityType;

            Property.EnsureCompatible(principalKey.Properties, dependentProperties, PrincipalEntityType, DeclaringEntityType);

            if (principalEntityType.GetKeys().Contains(principalKey) == false)
            {
                throw new ArgumentException(
                    Strings.ForeignKeyReferencedEntityKeyMismatch(
                        principalKey,
                        principalEntityType));
            }
        }

        public virtual Navigation DependentToPrincipal
        {
            get { return _dependentToPrincipal; }
            [param: CanBeNull]
            set
            {
                CheckNavigation(value, DeclaringEntityType);

                if (value == null
                    && _dependentToPrincipal != null
                    && DeclaringEntityType.Navigations.Contains(_dependentToPrincipal))
                {
                    throw new InvalidOperationException(
                        Strings.NavigationStillOnEntityType(_dependentToPrincipal.Name, DeclaringEntityType.Name));
                }

                _dependentToPrincipal = value;
            }
        }

        public virtual Navigation PrincipalToDependent
        {
            get { return _principalToDependent; }
            [param: CanBeNull]
            set
            {
                CheckNavigation(value, PrincipalEntityType);

                if (value == null
                    && _principalToDependent != null
                    && PrincipalEntityType.Navigations.Contains(_principalToDependent))
                {
                    throw new InvalidOperationException(
                        Strings.NavigationStillOnEntityType(_principalToDependent.Name, PrincipalEntityType.Name));
                }

                _principalToDependent = value;
            }
        }

        private void CheckNavigation(Navigation value, EntityType entityType)
        {
            if (value != null)
            {
                if (value.ForeignKey != this)
                {
                    throw new InvalidOperationException(
                        Strings.NavigationForWrongForeignKey(value.Name, value.DeclaringEntityType.DisplayName(), Property.Format(Properties), Property.Format(value.ForeignKey.Properties)));
                }
                
                if (!entityType.Navigations.Contains(value))
                {
                    throw new InvalidOperationException(Strings.NavigationNotFound(value.Name, entityType.Name));
                }
            }
        }

        public virtual IReadOnlyList<Property> Properties { get; }

        public virtual EntityType DeclaringEntityType => Properties[0].DeclaringEntityType;

        public virtual Key PrincipalKey { get; }

        public virtual EntityType PrincipalEntityType { get; }

        public virtual bool? IsUnique { get; set; }

        protected virtual bool DefaultIsUnique => false;

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
                if (value == IsRequired)
                {
                    return;
                }

                var properties = Properties;
                if (value.HasValue
                    && !value.Value)
                {
                    var nullableTypeProperties = Properties.Where(p => p.ClrType.IsNullableType()).ToList();
                    if (nullableTypeProperties.Any())
                    {
                        properties = nullableTypeProperties;
                    }
                    // If no properties can be made nullable, let it fail
                }

                foreach (var property in properties)
                {
                    property.IsNullable = !value;
                }
            }
        }

        protected virtual bool DefaultIsRequired => !((IForeignKey)this).Properties.Any(p => p.IsNullable);

        IReadOnlyList<IProperty> IForeignKey.Properties => Properties;

        IEntityType IForeignKey.DeclaringEntityType => DeclaringEntityType;

        IEntityType IForeignKey.PrincipalEntityType => PrincipalEntityType;

        IKey IForeignKey.PrincipalKey => PrincipalKey;

        INavigation IForeignKey.DependentToPrincipal => DependentToPrincipal;

        INavigation IForeignKey.PrincipalToDependent => PrincipalToDependent;

        bool IForeignKey.IsUnique => IsUnique ?? DefaultIsUnique;

        bool IForeignKey.IsRequired => IsRequired ?? DefaultIsRequired;

        public override string ToString()
            => $"'{DeclaringEntityType.DisplayName()}' {Property.Format(Properties)} -> '{PrincipalEntityType.DisplayName()}' {Property.Format(PrincipalKey.Properties)}";
    }
}
