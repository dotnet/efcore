// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EntityType : TypeBase, IMutableEntityType, IConventionEntityType
    {
        private readonly SortedSet<ForeignKey> _foreignKeys
            = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance);

        private readonly SortedDictionary<string, Navigation> _navigations
            = new SortedDictionary<string, Navigation>(StringComparer.Ordinal);

        private readonly SortedDictionary<IReadOnlyList<IProperty>, Index> _indexes
            = new SortedDictionary<IReadOnlyList<IProperty>, Index>(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, Property> _properties;

        private readonly SortedDictionary<IReadOnlyList<IProperty>, Key> _keys
            = new SortedDictionary<IReadOnlyList<IProperty>, Key>(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, ServiceProperty> _serviceProperties
            = new SortedDictionary<string, ServiceProperty>(StringComparer.Ordinal);

        private List<object> _data;
        private Key _primaryKey;
        private bool? _isKeyless;
        private EntityType _baseType;

        private ConfigurationSource? _primaryKeyConfigurationSource;
        private ConfigurationSource? _isKeylessConfigurationSource;
        private ConfigurationSource? _baseTypeConfigurationSource;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private PropertyCounts _counts;

        private Func<InternalEntityEntry, ISnapshot> _relationshipSnapshotFactory;
        private Func<InternalEntityEntry, ISnapshot> _originalValuesFactory;
        private Func<InternalEntityEntry, ISnapshot> _sidecarValuesFactory;
        private Func<ValueBuffer, ISnapshot> _shadowValuesFactory;
        private Func<ISnapshot> _emptyShadowValuesFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityType([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : base(name, model, configurationSource)
        {
            _properties = new SortedDictionary<string, Property>(new PropertyComparer(this));
            Builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityType([NotNull] Type clrType, [NotNull] Model model, ConfigurationSource configurationSource)
            : base(clrType, model, configurationSource)
        {
            if (!clrType.IsValidEntityType())
            {
                throw new ArgumentException(CoreStrings.InvalidEntityType(clrType));
            }

            _properties = new SortedDictionary<string, Property>(new PropertyComparer(this));
            Builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityType(
            [NotNull] string name,
            [NotNull] Model model,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
            : this(name, model, configurationSource)
        {
            DefiningNavigationName = definingNavigationName;
            DefiningEntityType = definingEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityType(
            [NotNull] Type clrType,
            [NotNull] Model model,
            [NotNull] string definingNavigationName,
            [NotNull] EntityType definingEntityType,
            ConfigurationSource configurationSource)
            : this(clrType, model, configurationSource)
        {
            DefiningNavigationName = definingNavigationName;
            DefiningEntityType = definingEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder Builder
        {
            [DebuggerStepThrough] get;
            [DebuggerStepThrough]
            [param: CanBeNull]
            set;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType BaseType => _baseType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsKeyless
        {
            get => RootType()._isKeyless ?? false;
            set => HasNoKey(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string DefiningNavigationName { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType DefiningEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void HasNoKey(
            bool? keyless,
            ConfigurationSource configurationSource)
        {
            if (_isKeyless == keyless)
            {
                UpdateIsKeylessConfigurationSource(configurationSource);
                return;
            }

            if (keyless == true)
            {
                if (_baseType != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DerivedEntityTypeHasNoKey(this.DisplayName(), RootType().DisplayName()));
                }

                if (_keys.Any())
                {
                    throw new InvalidOperationException(CoreStrings.KeylessTypeExistingKey(this.DisplayName()));
                }
            }

            _isKeyless = keyless;

            if (keyless == null)
            {
                _isKeylessConfigurationSource = null;
            }
            else
            {
                UpdateIsKeylessConfigurationSource(configurationSource);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsKeylessConfigurationSource() => _isKeylessConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateIsKeylessConfigurationSource(ConfigurationSource configurationSource)
            => _isKeylessConfigurationSource = configurationSource.Max(_isKeylessConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void HasBaseType(
            [CanBeNull] EntityType newBaseType,
            ConfigurationSource configurationSource)
        {
            Debug.Assert(Builder != null);

            if (_baseType == newBaseType)
            {
                UpdateBaseTypeConfigurationSource(configurationSource);
                newBaseType?.UpdateConfigurationSource(configurationSource);
                return;
            }

            if (this.HasDefiningNavigation())
            {
                throw new InvalidOperationException(CoreStrings.WeakDerivedType(this.DisplayName()));
            }

            var originalBaseType = _baseType;

            _baseType?._directlyDerivedTypes.Remove(this);
            _baseType = null;

            if (newBaseType != null)
            {
                if (this.HasClrType())
                {
                    if (!newBaseType.HasClrType())
                    {
                        throw new InvalidOperationException(CoreStrings.NonClrBaseType(this.DisplayName(), newBaseType.DisplayName()));
                    }

                    if (!newBaseType.ClrType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
                    {
                        throw new InvalidOperationException(
                            CoreStrings.NotAssignableClrBaseType(
                                this.DisplayName(), newBaseType.DisplayName(), ClrType.ShortDisplayName(),
                                newBaseType.ClrType.ShortDisplayName()));
                    }

                    if (newBaseType.HasDefiningNavigation())
                    {
                        throw new InvalidOperationException(CoreStrings.WeakBaseType(this.DisplayName(), newBaseType.DisplayName()));
                    }
                }

                if (!this.HasClrType()
                    && newBaseType.HasClrType())
                {
                    throw new InvalidOperationException(CoreStrings.NonShadowBaseType(this.DisplayName(), newBaseType.DisplayName()));
                }

                if (newBaseType.InheritsFrom(this))
                {
                    throw new InvalidOperationException(CoreStrings.CircularInheritance(this.DisplayName(), newBaseType.DisplayName()));
                }

                if (_keys.Count > 0)
                {
                    throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(this.DisplayName()));
                }

                if (IsKeyless)
                {
                    throw new InvalidOperationException(CoreStrings.DerivedEntityCannotBeKeyless(this.DisplayName()));
                }

                var propertyCollisions = newBaseType.GetProperties()
                    .Select(p => p.Name)
                    .SelectMany(FindDerivedPropertiesInclusive)
                    .ToList();

                if (propertyCollisions.Count > 0)
                {
                    var derivedProperty = propertyCollisions.First();
                    var baseProperty = newBaseType.FindProperty(derivedProperty.Name);
                    throw new InvalidOperationException(
                        CoreStrings.DuplicatePropertiesOnBase(
                            this.DisplayName(),
                            newBaseType.DisplayName(),
                            derivedProperty.DeclaringEntityType.DisplayName(),
                            derivedProperty.Name,
                            baseProperty.DeclaringEntityType.DisplayName(),
                            baseProperty.Name));
                }

                var navigationCollisions = newBaseType.GetNavigations()
                    .Select(p => p.Name)
                    .SelectMany(FindNavigationsInHierarchy)
                    .ToList();
                if (navigationCollisions.Count > 0)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DuplicateNavigationsOnBase(
                            this.DisplayName(),
                            newBaseType.DisplayName(),
                            string.Join(", ", navigationCollisions.Select(p => p.Name))));
                }

                _baseType = newBaseType;
                _baseType._directlyDerivedTypes.Add(this);
            }

            UpdateBaseTypeConfigurationSource(configurationSource);
            newBaseType?.UpdateConfigurationSource(configurationSource);

            Model.ConventionDispatcher.OnEntityTypeBaseTypeChanged(Builder, newBaseType, originalBaseType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void OnTypeRemoved()
        {
            Builder = null;
            _baseType?._directlyDerivedTypes.Remove(this);

            Model.ConventionDispatcher.OnEntityTypeRemoved(Model.Builder, this);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetBaseTypeConfigurationSource() => _baseTypeConfigurationSource;

        private void UpdateBaseTypeConfigurationSource(ConfigurationSource configurationSource)
            => _baseTypeConfigurationSource = configurationSource.Max(_baseTypeConfigurationSource);

        private readonly SortedSet<EntityType> _directlyDerivedTypes = new SortedSet<EntityType>(EntityTypePathComparer.Instance);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        // Note this is ISet because there is no suitable readonly interface in the profiles we are using
        public virtual ISet<EntityType> GetDirectlyDerivedTypes() => _directlyDerivedTypes;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<EntityType> GetDerivedTypes()
        {
            var derivedTypes = new List<EntityType>();
            var type = this;
            var currentTypeIndex = 0;
            while (type != null)
            {
                derivedTypes.AddRange(type.GetDirectlyDerivedTypes());
                type = derivedTypes.Count > currentTypeIndex
                    ? derivedTypes[currentTypeIndex]
                    : null;
                currentTypeIndex++;
            }

            return derivedTypes;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<EntityType> GetDerivedTypesInclusive()
            => new[]
            {
                this
            }.Concat(GetDerivedTypes());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetForeignKeysInHierarchy()
            => GetForeignKeys().Concat(GetDerivedForeignKeys());

        private bool InheritsFrom(EntityType entityType)
        {
            var et = this;

            do
            {
                if (entityType == et)
                {
                    return true;
                }
            }
            while ((et = et._baseType) != null);

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual EntityType RootType() => (EntityType)((IEntityType)this).RootType();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString() => this.ToDebugString();

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override IConventionAnnotation OnAnnotationSet(
            string name, IConventionAnnotation annotation, IConventionAnnotation oldAnnotation)
            => Model.ConventionDispatcher.OnEntityTypeAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<PropertyBase> FindMembersInHierarchy([NotNull] string name)
            => FindPropertiesInHierarchy(name).Cast<PropertyBase>()
                .Concat(FindServicePropertiesInHierarchy(name))
                .Concat(FindNavigationsInHierarchy(name));

        #region Primary and Candidate Keys

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key SetPrimaryKey([CanBeNull] Property property, ConfigurationSource configurationSource)
            => SetPrimaryKey(
                property == null
                    ? null
                    : new[]
                    {
                        property
                    }, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key SetPrimaryKey(
            [CanBeNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Debug.Assert(Builder != null);

            if (_baseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(this.DisplayName(), RootType().DisplayName()));
            }

            var oldPrimaryKey = _primaryKey;
            var propertiesNullOrEmpty = (properties?.Count ?? 0) == 0;
            if (oldPrimaryKey == null
                && propertiesNullOrEmpty)
            {
                return null;
            }

            Key newKey = null;
            if (!propertiesNullOrEmpty)
            {
                newKey = FindKey(properties);
                if (oldPrimaryKey != null
                    && oldPrimaryKey == newKey)
                {
                    UpdatePrimaryKeyConfigurationSource(configurationSource);
                    newKey.UpdateConfigurationSource(configurationSource);
                    return newKey;
                }

                if (newKey == null)
                {
                    newKey = AddKey(properties, configurationSource);
                }
            }

            if (oldPrimaryKey != null)
            {
                foreach (var property in oldPrimaryKey.Properties)
                {
                    _properties.Remove(property.Name);
                    property.PrimaryKey = null;
                }

                _primaryKey = null;

                foreach (var property in oldPrimaryKey.Properties)
                {
                    _properties.Add(property.Name, property);
                }
            }

            if (!propertiesNullOrEmpty)
            {
                foreach (var property in newKey.Properties)
                {
                    _properties.Remove(property.Name);
                    property.PrimaryKey = newKey;
                }

                _primaryKey = newKey;

                foreach (var property in newKey.Properties)
                {
                    _properties.Add(property.Name, property);
                }

                UpdatePrimaryKeyConfigurationSource(configurationSource);
            }
            else
            {
                SetPrimaryKeyConfigurationSource(null);
            }

            return (Key)Model.ConventionDispatcher.OnPrimaryKeyChanged(Builder, newKey, oldPrimaryKey);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key FindPrimaryKey()
            => _baseType?.FindPrimaryKey() ?? FindDeclaredPrimaryKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key FindDeclaredPrimaryKey() => _primaryKey;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key FindPrimaryKey([CanBeNull] IReadOnlyList<Property> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            if (_baseType != null)
            {
                return _baseType.FindPrimaryKey(properties);
            }

            return _primaryKey != null
                   && PropertyListComparer.Instance.Compare(_primaryKey.Properties, properties) == 0
                ? _primaryKey
                : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetPrimaryKeyConfigurationSource() => _primaryKeyConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private void SetPrimaryKeyConfigurationSource(ConfigurationSource? configurationSource)
            => _primaryKeyConfigurationSource = configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private void UpdatePrimaryKeyConfigurationSource(ConfigurationSource configurationSource)
            => _primaryKeyConfigurationSource = configurationSource.Max(_primaryKeyConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key AddKey([NotNull] Property property, ConfigurationSource configurationSource)
            => AddKey(
                new[]
                {
                    property
                }, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key AddKey(
            [NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            if (_baseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(this.DisplayName(), _baseType.DisplayName()));
            }

            if (IsKeyless)
            {
                throw new InvalidOperationException(CoreStrings.KeylessTypeWithKey(properties.Format(), this.DisplayName()));
            }

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                for (var j = i + 1; j < properties.Count; j++)
                {
                    if (property == properties[j])
                    {
                        throw new InvalidOperationException(CoreStrings.DuplicatePropertyInList(properties.Format(), property.Name));
                    }
                }

                if (FindProperty(property.Name) != property
                    || property.Builder == null)
                {
                    throw new InvalidOperationException(CoreStrings.KeyPropertiesWrongEntity(properties.Format(), this.DisplayName()));
                }

                if (property.ValueGenerated != ValueGenerated.Never
                    && property.GetContainingForeignKeys().Any(k => k.DeclaringEntityType != this))
                {
                    throw new InvalidOperationException(CoreStrings.KeyPropertyInForeignKey(property.Name, this.DisplayName()));
                }

                if (property.IsNullable)
                {
                    throw new InvalidOperationException(CoreStrings.NullableKey(this.DisplayName(), property.Name));
                }
            }

            var key = FindKey(properties);
            if (key != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateKey(
                        properties.Format(), this.DisplayName(), key.DeclaringEntityType.DisplayName()));
            }

            key = new Key(properties, configurationSource);
            _keys.Add(properties, key);

            foreach (var property in properties)
            {
                if (property.Keys == null)
                {
                    property.Keys = new List<Key>
                    {
                        key
                    };
                }
                else
                {
                    property.Keys.Add(key);
                }
            }

            return (Key)Model.ConventionDispatcher.OnKeyAdded(key.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key FindKey([NotNull] IProperty property) => FindKey(
            new[]
            {
                property
            });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key FindKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredKey(properties) ?? _baseType?.FindKey(properties);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Key> GetDeclaredKeys() => _keys.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key FindDeclaredKey([NotNull] IReadOnlyList<IProperty> properties)
            => _keys.TryGetValue(Check.NotEmpty(properties, nameof(properties)), out var key)
                ? key
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key RemoveKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            var key = FindDeclaredKey(properties);
            return key == null
                ? null
                : RemoveKey(key);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key RemoveKey([NotNull] Key key)
        {
            Debug.Assert(Builder != null);
            if (key.DeclaringEntityType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.KeyWrongType(key.Properties.Format(), key.DeclaringEntityType.DisplayName(), this.DisplayName()));
            }

            CheckKeyNotInUse(key);

            if (_primaryKey == key)
            {
                SetPrimaryKey((IReadOnlyList<Property>)null, ConfigurationSource.Explicit);
                _primaryKeyConfigurationSource = null;
            }

            var removed = _keys.Remove(key.Properties);
            Debug.Assert(removed);
            key.Builder = null;

            foreach (var property in key.Properties)
            {
                if (property.Keys != null)
                {
                    property.Keys.Remove(key);
                    if (property.Keys.Count == 0)
                    {
                        property.Keys = null;
                    }
                }
            }

            return (Key)Model.ConventionDispatcher.OnKeyRemoved(Builder, key);
        }

        private void CheckKeyNotInUse(Key key)
        {
            var foreignKey = key.GetReferencingForeignKeys().FirstOrDefault();
            if (foreignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.KeyInUse(key.Properties.Format(), this.DisplayName(), foreignKey.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Key> GetKeys() => _baseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;

        #endregion

        #region Foreign Keys

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey AddForeignKey(
            [NotNull] Property property,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource? componentConfigurationSource,
            ConfigurationSource configurationSource)
            => AddForeignKey(
                new[]
                {
                    property
                }, principalKey, principalEntityType, componentConfigurationSource, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource? componentConfigurationSource,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            var foreignKey = new ForeignKey(
                properties, principalKey, this, principalEntityType, configurationSource);

            principalEntityType.UpdateConfigurationSource(configurationSource);
            if (componentConfigurationSource.HasValue)
            {
                foreignKey.UpdatePropertiesConfigurationSource(componentConfigurationSource.Value);
                foreignKey.UpdatePrincipalKeyConfigurationSource(componentConfigurationSource.Value);
                foreignKey.UpdatePrincipalEndConfigurationSource(componentConfigurationSource.Value);
            }

            OnForeignKeyUpdated(foreignKey);

            return (ForeignKey)Model.ConventionDispatcher.OnForeignKeyAdded(foreignKey.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void OnForeignKeyUpdating(ForeignKey foreignKey)
        {
            var removed = _foreignKeys.Remove(foreignKey);
            Debug.Assert(removed);

            foreach (var property in foreignKey.Properties)
            {
                if (property.ForeignKeys != null)
                {
                    property.ForeignKeys.Remove(foreignKey);
                    if (property.ForeignKeys.Count == 0)
                    {
                        property.ForeignKeys = null;
                    }
                }
            }

            removed = foreignKey.PrincipalKey.ReferencingForeignKeys.Remove(foreignKey);
            Debug.Assert(removed);
            removed = foreignKey.PrincipalEntityType.DeclaredReferencingForeignKeys.Remove(foreignKey);
            Debug.Assert(removed);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void OnForeignKeyUpdated(ForeignKey foreignKey)
        {
            var added = _foreignKeys.Add(foreignKey);
            Debug.Assert(added);

            foreach (var property in foreignKey.Properties)
            {
                if (property.ForeignKeys == null)
                {
                    property.ForeignKeys = new List<ForeignKey>
                    {
                        foreignKey
                    };
                }
                else
                {
                    property.ForeignKeys.Add(foreignKey);
                }
            }

            var principalKey = foreignKey.PrincipalKey;
            if (principalKey.ReferencingForeignKeys == null)
            {
                principalKey.ReferencingForeignKeys = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance)
                {
                    foreignKey
                };
            }
            else
            {
                added = principalKey.ReferencingForeignKeys.Add(foreignKey);
                Debug.Assert(added);
            }

            var principalEntityType = foreignKey.PrincipalEntityType;
            if (principalEntityType.DeclaredReferencingForeignKeys == null)
            {
                principalEntityType.DeclaredReferencingForeignKeys = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance)
                {
                    foreignKey
                };
            }
            else
            {
                added = principalEntityType.DeclaredReferencingForeignKeys.Add(foreignKey);
                Debug.Assert(added);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeys([NotNull] IProperty property)
            => FindForeignKeys(
                new[]
                {
                    property
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeys([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return _baseType?.FindForeignKeys(properties)?.Concat(FindDeclaredForeignKeys(properties))
                   ?? FindDeclaredForeignKeys(properties);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey FindForeignKey(
            [NotNull] IProperty property,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => FindForeignKey(
                new[]
                {
                    property
                }, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey FindForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            return FindDeclaredForeignKey(properties, principalKey, principalEntityType)
                   ?? _baseType?.FindForeignKey(properties, principalKey, principalEntityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey FindOwnership()
            => GetForeignKeys().FirstOrDefault(fk => fk.IsOwnership);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey FindDeclaredOwnership()
            => GetDeclaredForeignKeys().FirstOrDefault(fk => fk.IsOwnership);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDeclaredForeignKeys() => _foreignKeys;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedForeignKeys()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredForeignKeys());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetForeignKeys()
            => _baseType?.GetForeignKeys().Concat(_foreignKeys) ?? _foreignKeys;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDeclaredForeignKeys([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            return _foreignKeys.Where(fk => PropertyListComparer.Instance.Equals(fk.Properties, properties));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey FindDeclaredForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            foreach (var fk in FindDeclaredForeignKeys(properties))
            {
                if (PropertyListComparer.Instance.Equals(fk.PrincipalKey.Properties, principalKey.Properties)
                    && StringComparer.Ordinal.Equals(fk.PrincipalEntityType.Name, principalEntityType.Name))
                {
                    return fk;
                }
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            [NotNull] IReadOnlyList<IProperty> properties)
            => GetDerivedTypes().SelectMany(et => et.FindDeclaredForeignKeys(properties));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => GetDerivedTypes().Select(et => et.FindDeclaredForeignKey(properties, principalKey, principalEntityType))
                .Where(fk => fk != null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            [NotNull] IReadOnlyList<IProperty> properties)
            => FindForeignKeys(properties).Concat(FindDerivedForeignKeys(properties));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => ToEnumerable(FindForeignKey(properties, principalKey, principalEntityType))
                .Concat(FindDerivedForeignKeys(properties, principalKey, principalEntityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey RemoveForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.NotEmpty(properties, nameof(properties));

            var foreignKey = FindDeclaredForeignKey(properties, principalKey, principalEntityType);
            return foreignKey == null
                ? null
                : RemoveForeignKey(foreignKey);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            if (foreignKey.DeclaringEntityType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.ForeignKeyWrongType(
                        foreignKey.Properties.Format(),
                        foreignKey.PrincipalKey.Properties.Format(),
                        foreignKey.PrincipalEntityType.DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName(),
                        this.DisplayName()));
            }

            if (foreignKey.DependentToPrincipal != null)
            {
                foreignKey.DeclaringEntityType.RemoveNavigation(foreignKey.DependentToPrincipal.Name);
            }

            if (foreignKey.PrincipalToDependent != null)
            {
                foreignKey.PrincipalEntityType.RemoveNavigation(foreignKey.PrincipalToDependent.Name);
            }

            OnForeignKeyUpdating(foreignKey);

            foreignKey.Builder = null;

            if (foreignKey.DependentToPrincipal != null)
            {
                Model.ConventionDispatcher.OnNavigationRemoved(
                    Builder,
                    foreignKey.PrincipalEntityType.Builder,
                    foreignKey.DependentToPrincipal.Name,
                    foreignKey.DependentToPrincipal.GetIdentifyingMemberInfo());
            }

            if (foreignKey.PrincipalToDependent != null)
            {
                Model.ConventionDispatcher.OnNavigationRemoved(
                    foreignKey.PrincipalEntityType.Builder,
                    Builder,
                    foreignKey.PrincipalToDependent.Name,
                    foreignKey.PrincipalToDependent.GetIdentifyingMemberInfo());
            }

            Model.ConventionDispatcher.OnForeignKeyRemoved(Builder, foreignKey);

            return foreignKey;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
            => _baseType?.GetReferencingForeignKeys().Concat(GetDeclaredReferencingForeignKeys())
               ?? GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDeclaredReferencingForeignKeys()
            => DeclaredReferencingForeignKeys ?? Enumerable.Empty<ForeignKey>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedReferencingForeignKeys()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredReferencingForeignKeys());

        private SortedSet<ForeignKey> DeclaredReferencingForeignKeys { get; set; }

        #endregion

        #region Navigations

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation AddNavigation(
            [NotNull] string name,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(foreignKey, nameof(foreignKey));

            return AddNavigation(new MemberIdentity(name), foreignKey, pointsToPrincipal);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation AddNavigation(
            [NotNull] MemberInfo navigationProperty,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal)
        {
            Check.NotNull(navigationProperty, nameof(navigationProperty));
            Check.NotNull(foreignKey, nameof(foreignKey));

            return AddNavigation(new MemberIdentity(navigationProperty), foreignKey, pointsToPrincipal);
        }

        private Navigation AddNavigation(MemberIdentity propertyIdentity, ForeignKey foreignKey, bool pointsToPrincipal)
        {
            var name = propertyIdentity.Name;
            var duplicateNavigation = FindNavigationsInHierarchy(name).FirstOrDefault();
            if (duplicateNavigation != null)
            {
                if (duplicateNavigation.ForeignKey != foreignKey)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NavigationForWrongForeignKey(
                            duplicateNavigation.Name,
                            duplicateNavigation.DeclaringEntityType.DisplayName(),
                            foreignKey.Properties.Format(),
                            duplicateNavigation.ForeignKey.Properties.Format()));
                }

                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, this.DisplayName(), duplicateNavigation.DeclaringEntityType.DisplayName()));
            }

            var duplicateProperty = FindPropertiesInHierarchy(name).Cast<PropertyBase>()
                .Concat(FindServicePropertiesInHierarchy(name)).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, this.DisplayName(),
                        duplicateProperty.DeclaringType.DisplayName()));
            }

            Debug.Assert(
                !GetNavigations().Any(n => n.ForeignKey == foreignKey && n.IsDependentToPrincipal() == pointsToPrincipal),
                "There is another navigation corresponding to the same foreign key and pointing in the same direction.");

            Debug.Assert(
                (pointsToPrincipal ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType) == this,
                "EntityType mismatch");

            var navigationProperty = propertyIdentity.MemberInfo
                                     ?? ClrType?.GetMembersInHierarchy(name).FirstOrDefault();
            if (ClrType != null)
            {
                Navigation.IsCompatible(
                    propertyIdentity.Name,
                    navigationProperty,
                    this,
                    pointsToPrincipal ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType,
                    !pointsToPrincipal && !foreignKey.IsUnique,
                    shouldThrow: true);
            }

            var navigation = new Navigation(name, navigationProperty as PropertyInfo, navigationProperty as FieldInfo, foreignKey);

            _navigations.Add(name, navigation);

            return navigation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation FindNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindDeclaredNavigation(name) ?? _baseType?.FindNavigation(name);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation FindNavigation([NotNull] MemberInfo memberInfo)
            => FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation FindDeclaredNavigation([NotNull] string name)
            => _navigations.TryGetValue(Check.NotEmpty(name, nameof(name)), out var navigation)
                ? navigation
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDeclaredNavigations() => _navigations.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDerivedNavigations()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDerivedNavigationsInclusive()
            => GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredNavigations());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> FindDerivedNavigations([NotNull] string navigationName)
        {
            Check.NotNull(navigationName, nameof(navigationName));

            return GetDerivedTypes().Select(et => et.FindDeclaredNavigation(navigationName)).Where(n => n != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsInHierarchy([NotNull] string navigationName)
            => ToEnumerable(FindNavigation(navigationName)).Concat(FindDerivedNavigations(navigationName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation RemoveNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var navigation = FindDeclaredNavigation(name);
            if (navigation == null)
            {
                return null;
            }

            _navigations.Remove(name);

            return navigation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> GetNavigations()
            => _baseType?.GetNavigations().Concat(_navigations.Values) ?? _navigations.Values;

        #endregion

        #region Indexes

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index AddIndex(
            [NotNull] Property property,
            ConfigurationSource configurationSource)
            => AddIndex(
                new[]
                {
                    property
                }, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index AddIndex(
            [NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                for (var j = i + 1; j < properties.Count; j++)
                {
                    if (property == properties[j])
                    {
                        throw new InvalidOperationException(CoreStrings.DuplicatePropertyInList(properties.Format(), property.Name));
                    }
                }

                if (FindProperty(property.Name) != property
                    || property.Builder == null)
                {
                    throw new InvalidOperationException(CoreStrings.IndexPropertiesWrongEntity(properties.Format(), this.DisplayName()));
                }
            }

            var duplicateIndex = FindIndexesInHierarchy(properties).FirstOrDefault();
            if (duplicateIndex != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateIndex(properties.Format(), this.DisplayName(), duplicateIndex.DeclaringEntityType.DisplayName()));
            }

            var index = new Index(properties, this, configurationSource);
            _indexes.Add(properties, index);

            foreach (var property in properties)
            {
                if (property.Indexes == null)
                {
                    property.Indexes = new List<Index>
                    {
                        index
                    };
                }
                else
                {
                    property.Indexes.Add(index);
                }
            }

            return (Index)Model.ConventionDispatcher.OnIndexAdded(index.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index FindIndex([NotNull] IProperty property)
            => FindIndex(
                new[]
                {
                    property
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index FindIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredIndex(properties) ?? _baseType?.FindIndex(properties);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> GetDeclaredIndexes() => _indexes.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> GetDerivedIndexes()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredIndexes());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index FindDeclaredIndex([NotNull] IReadOnlyList<IProperty> properties)
            => _indexes.TryGetValue(Check.NotEmpty(properties, nameof(properties)), out var index)
                ? index
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> FindDerivedIndexes([NotNull] IReadOnlyList<IProperty> properties)
            => GetDerivedTypes().Select(et => et.FindDeclaredIndex(properties)).Where(i => i != null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> FindIndexesInHierarchy([NotNull] IReadOnlyList<IProperty> properties)
            => ToEnumerable(FindIndex(properties)).Concat(FindDerivedIndexes(properties));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index RemoveIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            var index = FindDeclaredIndex(properties);
            return index == null
                ? null
                : RemoveIndex(index);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index RemoveIndex(Index index)
        {
            if (!_indexes.Remove(index.Properties))
            {
                throw new InvalidOperationException(
                    CoreStrings.IndexWrongType(index.Properties.Format(), index.DeclaringEntityType.DisplayName(), this.DisplayName()));
            }

            index.Builder = null;

            foreach (var property in index.Properties)
            {
                if (property.Indexes != null)
                {
                    property.Indexes.Remove(index);
                    if (property.Indexes.Count == 0)
                    {
                        property.Indexes = null;
                    }
                }
            }

            Model.ConventionDispatcher.OnIndexRemoved(Builder, index);

            return index;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> GetIndexes() => _baseType?.GetIndexes().Concat(_indexes.Values) ?? _indexes.Values;

        #endregion

        #region Properties

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property AddProperty(
            [NotNull] string name,
            [NotNull] Type propertyType,
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(propertyType, nameof(propertyType));

            return AddProperty(
                name,
                propertyType,
                null,
                typeConfigurationSource,
                configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property AddProperty(
            [NotNull] MemberInfo memberInfo,
            ConfigurationSource configurationSource)
            => AddProperty(
                memberInfo.GetSimpleMemberName(),
                memberInfo.GetMemberType(),
                memberInfo,
                configurationSource,
                configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property AddProperty(
            [NotNull] string name,
            ConfigurationSource configurationSource)
        {
            var clrMember = ClrType?.GetMembersInHierarchy(name).FirstOrDefault();
            if (clrMember == null)
            {
                throw new InvalidOperationException(CoreStrings.NoPropertyType(name, this.DisplayName()));
            }

            return AddProperty(clrMember, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property AddProperty(
            string name,
            Type propertyType,
            MemberInfo memberInfo,
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(propertyType, nameof(propertyType));
            Debug.Assert(Builder != null);

            var conflictingMember = FindMembersInHierarchy(name).FirstOrDefault();
            if (conflictingMember != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, this.DisplayName(),
                        conflictingMember.DeclaringType.DisplayName()));
            }

            if (memberInfo != null)
            {
                if (ClrType == null)
                {
                    throw new InvalidOperationException(CoreStrings.ClrPropertyOnShadowEntity(memberInfo.Name, this.DisplayName()));
                }

                if (memberInfo.DeclaringType?.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()) != true)
                {
                    throw new ArgumentException(
                        CoreStrings.PropertyWrongEntityClrType(
                            memberInfo.Name, this.DisplayName(), memberInfo.DeclaringType?.ShortDisplayName()));
                }

                if (name != memberInfo.GetSimpleMemberName())
                {
                    if ((memberInfo as PropertyInfo)?.IsEFIndexerProperty() != true)
                    {
                        if (typeConfigurationSource != null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.PropertyWrongName(
                                    name,
                                    this.DisplayName(),
                                    memberInfo.GetSimpleMemberName()));
                        }

                        propertyType = memberInfo.GetMemberType();
                    }
                    else
                    {
                        var clashingMemberInfo = ClrType?.GetMembersInHierarchy(name).FirstOrDefault();
                        if (clashingMemberInfo != null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.PropertyClashingNonIndexer(
                                    name,
                                    this.DisplayName()));
                        }
                    }
                }
            }
            else
            {
                memberInfo = ClrType?.GetMembersInHierarchy(name).FirstOrDefault();
            }

            if (memberInfo != null
                && propertyType != memberInfo.GetMemberType()
                && (memberInfo as PropertyInfo)?.IsEFIndexerProperty() != true)
            {
                if (typeConfigurationSource != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyWrongClrType(
                            name,
                            this.DisplayName(),
                            memberInfo.GetMemberType().ShortDisplayName(),
                            propertyType.ShortDisplayName()));
                }

                propertyType = memberInfo.GetMemberType();
            }

            var property = new Property(
                name, propertyType, memberInfo as PropertyInfo, memberInfo as FieldInfo, this,
                configurationSource, typeConfigurationSource);

            _properties.Add(property.Name, property);

            return (Property)Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property FindProperty([NotNull] string name)
            => FindDeclaredProperty(Check.NotEmpty(name, nameof(name))) ?? _baseType?.FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property FindDeclaredProperty([NotNull] string name)
            => _properties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
                ? property
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> GetDeclaredProperties() => _properties.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> FindDerivedProperties([NotNull] string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            return GetDerivedTypes().Select(et => et.FindDeclaredProperty(propertyName)).Where(p => p != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> FindDerivedPropertiesInclusive([NotNull] string propertyName)
            => ToEnumerable(FindDeclaredProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> FindPropertiesInHierarchy([NotNull] string propertyName)
            => ToEnumerable(FindProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Property> FindProperties([NotNull] IReadOnlyList<string> propertyNames)
        {
            var properties = new List<Property>(propertyNames.Count);
            foreach (var propertyName in propertyNames)
            {
                var property = FindProperty(propertyName);
                if (property == null)
                {
                    return null;
                }
                properties.Add(property);
            }

            return properties;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property RemoveProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindDeclaredProperty(name);
            return property == null
                ? null
                : RemoveProperty(property);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property RemoveProperty(Property property)
        {
            if (property.DeclaringEntityType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyWrongType(
                        property.Name,
                        property.DeclaringEntityType.DisplayName(),
                        this.DisplayName()));
            }

            CheckPropertyNotInUse(property);

            var removed = _properties.Remove(property.Name);
            Debug.Assert(removed);
            property.Builder = null;

            return property;
        }

        private void CheckPropertyNotInUse(Property property)
        {
            var containingKey = property.Keys?.FirstOrDefault();
            if (containingKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseKey(property.Name, this.DisplayName(), containingKey.Properties.Format()));
            }

            var containingForeignKey = property.ForeignKeys?.FirstOrDefault();
            if (containingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseForeignKey(
                        property.Name, this.DisplayName(),
                        containingForeignKey.Properties.Format(), containingForeignKey.DeclaringEntityType.DisplayName()));
            }

            var containingIndex = property.Indexes?.FirstOrDefault();
            if (containingIndex != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseIndex(
                        property.Name, this.DisplayName(),
                        containingIndex.Properties.Format(), containingIndex.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> GetProperties()
            => _baseType?.GetProperties().Concat(_properties.Values) ?? _properties.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyCounts Counts
            => NonCapturingLazyInitializer.EnsureInitialized(ref _counts, this, entityType => entityType.CalculateCounts());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> RelationshipSnapshotFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _relationshipSnapshotFactory, this,
                entityType => new RelationshipSnapshotFactoryFactory().Create(entityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> OriginalValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _originalValuesFactory, this,
                entityType => new OriginalValuesFactoryFactory().Create(entityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> SidecarValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _sidecarValuesFactory, this,
                entityType => new SidecarValuesFactoryFactory().Create(entityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<ValueBuffer, ISnapshot> ShadowValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _shadowValuesFactory, this,
                entityType => new ShadowValuesFactoryFactory().Create(entityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<ISnapshot> EmptyShadowValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _emptyShadowValuesFactory, this,
                entityType => new EmptyShadowValuesFactoryFactory().CreateEmpty(entityType));

        #endregion

        #region Service properties

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty AddServiceProperty(
            [NotNull] MemberInfo memberInfo,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource)
        {
            Check.NotNull(memberInfo, nameof(memberInfo));
            var name = memberInfo.GetSimpleMemberName();

            var duplicateMember = FindMembersInHierarchy(name).FirstOrDefault();
            if (duplicateMember != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, this.DisplayName(),
                        duplicateMember.DeclaringType.DisplayName()));
            }

            var serviceProperty = new ServiceProperty(
                name,
                memberInfo as PropertyInfo,
                memberInfo as FieldInfo,
                this,
                configurationSource);

            var duplicateServiceProperty = GetServiceProperties().FirstOrDefault(p => p.ClrType == serviceProperty.ClrType);
            if (duplicateServiceProperty != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateServicePropertyType(
                        name,
                        serviceProperty.ClrType.ShortDisplayName(),
                        this.DisplayName(),
                        duplicateServiceProperty.Name,
                        duplicateServiceProperty.DeclaringEntityType.DisplayName()));
            }

            _serviceProperties[serviceProperty.Name] = serviceProperty;

            return serviceProperty;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty FindServiceProperty([NotNull] string name)
            => FindDeclaredServiceProperty(Check.NotEmpty(name, nameof(name))) ?? _baseType?.FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property FindServiceProperty([NotNull] MemberInfo memberInfo)
            => FindProperty(memberInfo.GetSimpleMemberName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty FindDeclaredServiceProperty([NotNull] string name)
            => _serviceProperties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
                ? property
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindDerivedServiceProperties([NotNull] string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            return GetDerivedTypes().Select(et => et.FindDeclaredServiceProperty(propertyName)).Where(p => p != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindDerivedServicePropertiesInclusive([NotNull] string propertyName)
            => ToEnumerable(FindDeclaredServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindServicePropertiesInHierarchy([NotNull] string propertyName)
            => ToEnumerable(FindServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty RemoveServiceProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindServiceProperty(name);
            return property == null
                ? null
                : RemoveServiceProperty(property);
        }

        private ServiceProperty RemoveServiceProperty(ServiceProperty property)
        {
            _serviceProperties.Remove(property.Name);
            property.Builder = null;

            return property;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> GetServiceProperties()
            => _baseType?.GetServiceProperties().Concat(_serviceProperties.Values) ?? _serviceProperties.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> GetDeclaredServiceProperties()
            => _serviceProperties.Values;

        #endregion

        #region Ignore

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override ConfigurationSource? FindIgnoredConfigurationSource(string name)
        {
            var ignoredSource = FindDeclaredIgnoredConfigurationSource(name);

            return BaseType == null ? ignoredSource : BaseType.FindIgnoredConfigurationSource(name).Max(ignoredSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void OnTypeMemberIgnored(string name)
            => Model.ConventionDispatcher.OnEntityTypeMemberIgnored(Builder, name);

        #endregion

        #region Data

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IDictionary<string, object>> GetSeedData(bool providerValues = false)
        {
            if (_data == null
                || _data.Count == 0)
            {
                return Enumerable.Empty<IDictionary<string, object>>();
            }

            var data = new List<Dictionary<string, object>>();
            var valueConverters = new Dictionary<string, ValueConverter>(StringComparer.Ordinal);
            var properties = this.GetPropertiesAndNavigations().ToDictionary(p => p.Name);
            foreach (var rawSeed in _data)
            {
                var seed = new Dictionary<string, object>(StringComparer.Ordinal);
                data.Add(seed);
                var type = rawSeed.GetType();
                foreach (var memberInfo in type.GetMembersInHierarchy())
                {
                    if (!properties.TryGetValue(memberInfo.GetSimpleMemberName(), out var propertyBase))
                    {
                        continue;
                    }

                    ValueConverter valueConverter = null;
                    if (providerValues
                        && !valueConverters.TryGetValue(memberInfo.Name, out valueConverter))
                    {
                        if (propertyBase is IProperty property)
                        {
                            valueConverter = property.GetTypeMapping().Converter
                                             ?? property.GetValueConverter();
                        }

                        valueConverters[memberInfo.Name] = valueConverter;
                    }

                    object value = null;
                    switch (memberInfo)
                    {
                        case PropertyInfo propertyInfo:
                            value = propertyInfo.GetValue(rawSeed);
                            break;
                        case FieldInfo fieldInfo:
                            value = fieldInfo.GetValue(rawSeed);
                            break;
                    }

                    seed[memberInfo.Name] = valueConverter == null
                        ? value
                        : valueConverter.ConvertToProvider(value);
                }
            }

            return data;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddData([NotNull] IEnumerable<object> data)
        {
            if (_data == null)
            {
                _data = new List<object>();
            }

            foreach (var entity in data)
            {
                if (ClrType != null
                    && ClrType != entity.GetType()
                    && ClrType.GetTypeInfo().IsAssignableFrom(entity.GetType().GetTypeInfo()))
                {
                    throw new InvalidOperationException(
                        CoreStrings.SeedDatumDerivedType(
                            this.DisplayName(), entity.GetType().ShortDisplayName()));
                }

                _data.Add(entity);
            }
        }

        #endregion

        #region Annotations

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy, ConfigurationSource configurationSource)
        {
            if (changeTrackingStrategy != null)
            {
                var errorMessage = CheckChangeTrackingStrategy(changeTrackingStrategy.Value);
                if (errorMessage != null)
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.ChangeTrackingStrategy, changeTrackingStrategy, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string CheckChangeTrackingStrategy(ChangeTrackingStrategy value)
        {
            if (ClrType != null)
            {
                if (value != ChangeTrackingStrategy.Snapshot
                    && !typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
                {
                    return CoreStrings.ChangeTrackingInterfaceMissing(this.DisplayName(), value, nameof(INotifyPropertyChanged));
                }

                if ((value == ChangeTrackingStrategy.ChangingAndChangedNotifications
                     || value == ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
                    && !typeof(INotifyPropertyChanging).GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
                {
                    return CoreStrings.ChangeTrackingInterfaceMissing(this.DisplayName(), value, nameof(INotifyPropertyChanging));
                }
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetQueryFilter([CanBeNull] LambdaExpression queryFilter, ConfigurationSource configurationSource)
        {
            var errorMessage = CheckQueryFilter(queryFilter);
            if (errorMessage != null)
            {
                throw new InvalidOperationException(errorMessage);
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.QueryFilter, queryFilter, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string CheckQueryFilter([CanBeNull] LambdaExpression queryFilter)
        {
            if (queryFilter != null
                && (queryFilter.Parameters.Count != 1
                    || queryFilter.Parameters[0].Type != ClrType
                    || queryFilter.ReturnType != typeof(bool)))
            {
                return CoreStrings.BadFilterExpression(queryFilter, this.DisplayName(), ClrType);
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetDefiningQuery([CanBeNull] LambdaExpression definingQuery, ConfigurationSource configurationSource)
        {
            this.SetOrRemoveAnnotation(CoreAnnotationNames.DefiningQuery, definingQuery, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetDiscriminatorProperty([CanBeNull] IProperty property, ConfigurationSource configurationSource)
        {
            CheckDiscriminatorProperty(property);

            if ((property == null
                 || !property.ClrType.IsInstanceOfType(this.GetDiscriminatorValue())))
            {
                ((IMutableEntityType)this).RemoveDiscriminatorValue();
                if (BaseType == null)
                {
                    foreach (var derivedType in GetDerivedTypes())
                    {
                        ((IMutableEntityType)derivedType).RemoveDiscriminatorValue();
                    }
                }
            }

            SetAnnotation(CoreAnnotationNames.DiscriminatorProperty, property?.Name, configurationSource);
        }

        private void CheckDiscriminatorProperty(IProperty property)
        {
            if (property != null)
            {
                if (BaseType != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DiscriminatorPropertyMustBeOnRoot(this.DisplayName()));
                }

                if (property.DeclaringEntityType != this)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DiscriminatorPropertyNotFound(property.Name, this.DisplayName()));
                }
            }
        }

        public virtual void CheckDiscriminatorValue(IEntityType entityType, object value)
        {
            if (value != null
                && entityType.GetDiscriminatorProperty() == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoDiscriminatorForValue(entityType.DisplayName(), entityType.RootType().DisplayName()));
            }

            if (value != null
                && !entityType.GetDiscriminatorProperty().ClrType.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
            {
                throw new InvalidOperationException(
                    CoreStrings.DiscriminatorValueIncompatible(
                        value, entityType.GetDiscriminatorProperty().Name, entityType.GetDiscriminatorProperty().ClrType));
            }
        }

        #endregion

        #region Explicit interface implementations

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IModel ITypeBase.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableModel IMutableTypeBase.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableModel IMutableEntityType.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType IEntityType.BaseType
        {
            [DebuggerStepThrough] get => _baseType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableEntityType.BaseType
        {
            get => _baseType;
            set => HasBaseType((EntityType)value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType IEntityType.DefiningEntityType
        {
            [DebuggerStepThrough] get => DefiningEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableKey IMutableEntityType.SetPrimaryKey(IReadOnlyList<IMutableProperty> properties)
            => SetPrimaryKey(properties?.Cast<Property>().ToList(), ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IKey IEntityType.FindPrimaryKey() => FindPrimaryKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableKey IMutableEntityType.FindPrimaryKey() => FindPrimaryKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableKey IMutableEntityType.AddKey(IReadOnlyList<IMutableProperty> properties)
            => AddKey(properties.Cast<Property>().ToList(), ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IKey IEntityType.FindKey(IReadOnlyList<IProperty> properties) => FindKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableKey IMutableEntityType.FindKey(IReadOnlyList<IProperty> properties) => FindKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IKey> IEntityType.GetKeys() => GetKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IMutableKey> IMutableEntityType.GetKeys() => GetKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IMutableEntityType.RemoveKey(IMutableKey key) => RemoveKey((Key)key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableForeignKey IMutableEntityType.AddForeignKey(
            IReadOnlyList<IMutableProperty> properties, IMutableKey principalKey, IMutableEntityType principalEntityType)
            => AddForeignKey(
                properties.Cast<Property>().ToList(),
                (Key)principalKey,
                (EntityType)principalEntityType,
                ConfigurationSource.Explicit,
                ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableForeignKey IMutableEntityType.FindForeignKey(
            IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IForeignKey IEntityType.FindForeignKey(IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IForeignKey> IEntityType.GetForeignKeys() => GetForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IMutableForeignKey> IMutableEntityType.GetForeignKeys() => GetForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IMutableEntityType.RemoveForeignKey(IMutableForeignKey foreignKey)
            => RemoveForeignKey((ForeignKey)foreignKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableIndex IMutableEntityType.AddIndex(IReadOnlyList<IMutableProperty> properties)
            => AddIndex(properties.Cast<Property>().ToList(), ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IIndex IEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableIndex IMutableEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IIndex> IEntityType.GetIndexes() => GetIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IMutableIndex> IMutableEntityType.GetIndexes() => GetIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IMutableEntityType.RemoveIndex(IMutableIndex index) => RemoveIndex((Index)index);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableProperty IMutableEntityType.AddProperty(string name, Type propertyType, MemberInfo memberInfo)
            => AddProperty(name, propertyType, memberInfo, ConfigurationSource.Explicit, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IProperty IEntityType.FindProperty(string name) => FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableProperty IMutableEntityType.FindProperty(string name) => FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IProperty> IEntityType.GetProperties() => GetProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IMutableProperty> IMutableEntityType.GetProperties() => GetProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IMutableEntityType.RemoveProperty(IMutableProperty property) => RemoveProperty((Property)property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableServiceProperty IMutableEntityType.AddServiceProperty(MemberInfo memberInfo)
            => AddServiceProperty(memberInfo, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IServiceProperty IEntityType.FindServiceProperty(string name) => FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableServiceProperty IMutableEntityType.FindServiceProperty(string name) => FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IServiceProperty> IEntityType.GetServiceProperties() => GetServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IMutableServiceProperty> IMutableEntityType.GetServiceProperties() => GetServiceProperties();

        /// <summary>
          ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
          ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
          ///     any release. You should only use it directly in your code with extreme caution and knowing that
          ///     doing so can result in application failures when updating to a new Entity Framework Core release.
          /// </summary>
          IMutableServiceProperty IMutableEntityType.RemoveServiceProperty(string name) => RemoveServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityType.Builder
        {
            [DebuggerStepThrough] get => Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionModel IConventionEntityType.Model
        {
            [DebuggerStepThrough] get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionEntityType.BaseType
        {
            [DebuggerStepThrough] get => BaseType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionEntityType.HasBaseType(IConventionEntityType entityType, bool fromDataAnnotation)
            => HasBaseType(
                (EntityType)entityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionEntityType.HasNoKey(bool? keyless, bool fromDataAnnotation)
            => HasNoKey(keyless, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionKey IConventionEntityType.SetPrimaryKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => SetPrimaryKey(
                properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionKey IConventionEntityType.FindPrimaryKey() => FindPrimaryKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionKey IConventionEntityType.FindKey(IReadOnlyList<IProperty> properties) => FindKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IConventionKey> IConventionEntityType.GetKeys() => GetKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionForeignKey IConventionEntityType.FindForeignKey(
            IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IConventionForeignKey> IConventionEntityType.GetForeignKeys() => GetForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionIndex IConventionEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IConventionIndex> IConventionEntityType.GetIndexes() => GetIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionProperty IConventionEntityType.FindProperty(string name) => FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IConventionProperty> IConventionEntityType.GetProperties() => GetProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionServiceProperty IConventionEntityType.FindServiceProperty(string name) => FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEnumerable<IConventionServiceProperty> IConventionEntityType.GetServiceProperties() => GetServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionServiceProperty IConventionEntityType.RemoveServiceProperty(string name) => RemoveServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionEntityType.RemoveProperty(IConventionProperty property) => RemoveProperty((Property)property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionServiceProperty IConventionEntityType.AddServiceProperty(MemberInfo memberInfo, bool fromDataAnnotation)
            => AddServiceProperty(memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionEntityType.RemoveIndex(IConventionIndex index) => RemoveIndex((Index)index);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionProperty IConventionEntityType.AddProperty(
            string name, Type propertyType, MemberInfo memberInfo, bool setTypeConfigurationSource, bool fromDataAnnotation)
            => AddProperty(
                name,
                propertyType,
                memberInfo,
                setTypeConfigurationSource
                    ? fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention
                    : (ConfigurationSource?)null,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionEntityType.RemoveForeignKey(IConventionForeignKey foreignKey)
            => RemoveForeignKey((ForeignKey)foreignKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionIndex IConventionEntityType.AddIndex(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => AddIndex(
                properties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionEntityType.RemoveKey(IConventionKey key) => RemoveKey((Key)key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionForeignKey IConventionEntityType.AddForeignKey(
            IReadOnlyList<IConventionProperty> properties,
            IConventionKey principalKey,
            IConventionEntityType principalEntityType,
            bool setComponentConfigurationSource,
            bool fromDataAnnotation)
            => AddForeignKey(
                properties.Cast<Property>().ToList(),
                (Key)principalKey,
                (EntityType)principalEntityType,
                setComponentConfigurationSource
                    ? fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention
                    : (ConfigurationSource?)null,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionKey IConventionEntityType.AddKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => AddKey(
                properties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        #endregion

        private static IEnumerable<T> ToEnumerable<T>(T element)
            where T : class
            => element == null
                ? Enumerable.Empty<T>()
                : new[]
                {
                    element
                };

        private class PropertyComparer : IComparer<string>
        {
            private readonly EntityType _entityType;

            public PropertyComparer(EntityType entityType)
            {
                _entityType = entityType;
            }

            public int Compare(string x, string y)
            {
                var xIndex = -1;
                var yIndex = -1;

                var properties = _entityType.FindPrimaryKey()?.Properties;

                if (properties != null)
                {
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var name = properties[i].Name;

                        if (name == x)
                        {
                            xIndex = i;
                        }

                        if (name == y)
                        {
                            yIndex = i;
                        }
                    }
                }

                // Neither property is part of the Primary Key
                // Compare the property names
                if (xIndex == -1
                    && yIndex == -1)
                {
                    return StringComparer.Ordinal.Compare(x, y);
                }

                // Both properties are part of the Primary Key
                // Compare the indices
                if (xIndex > -1
                    && yIndex > -1)
                {
                    return xIndex - yIndex;
                }

                // One property is part of the Primary Key
                // The primary key property is first
                return xIndex > yIndex
                    ? -1
                    : 1;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public class Snapshot
        {
            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public Snapshot(
                [NotNull] EntityType entityType,
                [CanBeNull] PropertiesSnapshot properties,
                [CanBeNull] List<InternalIndexBuilder> indexes,
                [CanBeNull] List<(InternalKeyBuilder, ConfigurationSource?)> keys,
                [CanBeNull] List<RelationshipSnapshot> relationships)
            {
                EntityType = entityType;
                Properties = properties ?? new PropertiesSnapshot(null, null, null, null);
                if (indexes != null)
                {
                    Properties.Add(indexes);
                }

                if (keys != null)
                {
                    Properties.Add(keys);
                }

                if (relationships != null)
                {
                    Properties.Add(relationships);
                }
            }

            private EntityType EntityType { [DebuggerStepThrough] get; }
            private PropertiesSnapshot Properties { [DebuggerStepThrough] get; }

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual void Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
            {
                entityTypeBuilder.MergeAnnotationsFrom(EntityType);

                foreach (var ignoredMember in EntityType.GetIgnoredMembers())
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    entityTypeBuilder.Ignore(ignoredMember, EntityType.FindDeclaredIgnoredConfigurationSource(ignoredMember).Value);
                }

                Properties.Attach(entityTypeBuilder);

                var rawData = EntityType._data;
                if (rawData != null)
                {
                    entityTypeBuilder.Metadata.AddData(Enumerable.ToArray(rawData));
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView<EntityType> DebugView
            => new DebugView<EntityType>(this, m => m.ToDebugString(false));
    }
}
