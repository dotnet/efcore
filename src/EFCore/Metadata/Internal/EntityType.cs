// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    // Issue#11266 This type is being used by provider code. Do not break.
    public class EntityType : TypeBase, IMutableEntityType
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

        private List<object> _data;

        private readonly SortedDictionary<string, ServiceProperty> _serviceProperties
            = new SortedDictionary<string, ServiceProperty>(StringComparer.Ordinal);

        private Key _primaryKey;
        private EntityType _baseType;
        private LambdaExpression _queryFilter;

        private ChangeTrackingStrategy? _changeTrackingStrategy;

        private ConfigurationSource? _baseTypeConfigurationSource;
        private ConfigurationSource? _primaryKeyConfigurationSource;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private PropertyCounts _counts;

        private Func<InternalEntityEntry, ISnapshot> _relationshipSnapshotFactory;
        private Func<InternalEntityEntry, ISnapshot> _originalValuesFactory;
        private Func<InternalEntityEntry, ISnapshot> _sidecarValuesFactory;
        private Func<ValueBuffer, ISnapshot> _shadowValuesFactory;
        private Func<ISnapshot> _emptyShadowValuesFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityType([NotNull] string name, [NotNull] Model model, ConfigurationSource configurationSource)
            : base(name, model, configurationSource)
        {
            _properties = new SortedDictionary<string, Property>(new PropertyComparer(this));
            Builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Builder
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            [param: CanBeNull]
            set;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType BaseType => _baseType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual LambdaExpression QueryFilter
        {
            get => _queryFilter;
            [param: CanBeNull]
            set
            {
                if (value != null
                    && (value.Parameters.Count != 1
                        || value.Parameters[0].Type != ClrType
                        || value.ReturnType != typeof(bool)))
                {
                    throw new InvalidOperationException(
                        CoreStrings.BadFilterExpression(value, this.DisplayName(), ClrType));
                }

                _queryFilter = value;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual LambdaExpression DefiningQuery { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsQueryType { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string DefiningNavigationName { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DefiningEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void HasBaseType(
            [CanBeNull] EntityType entityType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            if (_baseType == entityType)
            {
                UpdateBaseTypeConfigurationSource(configurationSource);
                entityType?.UpdateConfigurationSource(configurationSource);
                return;
            }

            if (this.HasDefiningNavigation())
            {
                throw new InvalidOperationException(CoreStrings.WeakDerivedType(this.DisplayName()));
            }

            var originalBaseType = _baseType;

            _baseType?._directlyDerivedTypes.Remove(this);
            _baseType = null;

            if (entityType != null)
            {
                if (IsQueryType != entityType.IsQueryType)
                {
                    throw new InvalidOperationException(
                        CoreStrings.MixedQueryEntityTypeInheritance(entityType.DisplayName(), this.DisplayName()));
                }

                if (this.HasClrType())
                {
                    if (!entityType.HasClrType())
                    {
                        throw new InvalidOperationException(CoreStrings.NonClrBaseType(this.DisplayName(), entityType.DisplayName()));
                    }

                    if (!entityType.ClrType.GetTypeInfo().IsAssignableFrom(ClrType.GetTypeInfo()))
                    {
                        throw new InvalidOperationException(CoreStrings.NotAssignableClrBaseType(this.DisplayName(), entityType.DisplayName(), ClrType.ShortDisplayName(), entityType.ClrType.ShortDisplayName()));
                    }

                    if (entityType.HasDefiningNavigation())
                    {
                        throw new InvalidOperationException(CoreStrings.WeakBaseType(this.DisplayName(), entityType.DisplayName()));
                    }
                }

                if (!this.HasClrType()
                    && entityType.HasClrType())
                {
                    throw new InvalidOperationException(CoreStrings.NonShadowBaseType(this.DisplayName(), entityType.DisplayName()));
                }

                if (entityType.InheritsFrom(this))
                {
                    throw new InvalidOperationException(CoreStrings.CircularInheritance(this.DisplayName(), entityType.DisplayName()));
                }

                if (_keys.Count > 0)
                {
                    throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(this.DisplayName()));
                }

                var propertyCollisions = entityType.GetProperties()
                    .Select(p => p.Name)
                    .SelectMany(FindDerivedPropertiesInclusive)
                    .ToList();

                if (propertyCollisions.Count > 0)
                {
                    var derivedProperty = propertyCollisions.First();
                    var baseProperty = entityType.FindProperty(derivedProperty.Name);
                    throw new InvalidOperationException(
                        CoreStrings.DuplicatePropertiesOnBase(
                            this.DisplayName(),
                            entityType.DisplayName(),
                            derivedProperty.DeclaringEntityType.DisplayName(),
                            derivedProperty.Name,
                            baseProperty.DeclaringEntityType.DisplayName(),
                            baseProperty.Name));
                }

                var navigationCollisions = entityType.GetNavigations()
                    .Select(p => p.Name)
                    .SelectMany(FindNavigationsInHierarchy)
                    .ToList();
                if (navigationCollisions.Count > 0)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DuplicateNavigationsOnBase(
                            this.DisplayName(),
                            entityType.DisplayName(),
                            string.Join(", ", navigationCollisions.Select(p => p.Name))));
                }

                _baseType = entityType;
                _baseType._directlyDerivedTypes.Add(this);
            }

            PropertyMetadataChanged();
            UpdateBaseTypeConfigurationSource(configurationSource);
            entityType?.UpdateConfigurationSource(configurationSource);

            Model.ConventionDispatcher.OnBaseEntityTypeChanged(Builder, originalBaseType);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void OnTypeRemoved()
        {
            Builder = null;
            _baseType?._directlyDerivedTypes.Remove(this);

            Model.ConventionDispatcher.OnEntityTypeRemoved(Model.Builder, this);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetBaseTypeConfigurationSource() => _baseTypeConfigurationSource;

        private void UpdateBaseTypeConfigurationSource(ConfigurationSource configurationSource)
            => _baseTypeConfigurationSource = configurationSource.Max(_baseTypeConfigurationSource);

        private readonly SortedSet<EntityType> _directlyDerivedTypes = new SortedSet<EntityType>(EntityTypePathComparer.Instance);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // Note this is ISet because there is no suitable readonly interface in the profiles we are using
        public virtual ISet<EntityType> GetDirectlyDerivedTypes() => _directlyDerivedTypes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<EntityType> GetDerivedTypesInclusive()
            => new[] { this }.Concat(GetDerivedTypes());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public virtual EntityType RootType() => (EntityType)((IEntityType)this).RootType();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString() => this.ToDebugString();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void ClearCaches()
        {
            base.ClearCaches();

            RemoveAnnotation(RelationshipDiscoveryConvention.AmbiguousNavigationsAnnotationName);
            RemoveAnnotation(RelationshipDiscoveryConvention.NavigationCandidatesAnnotationName);
            RemoveAnnotation(InversePropertyAttributeConvention.InverseNavigationsAnnotationName);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ChangeTrackingStrategy ChangeTrackingStrategy
        {
            get => _changeTrackingStrategy ?? Model.ChangeTrackingStrategy;
            set
            {
                var errorMessage = this.CheckChangeTrackingStrategy(value);
                if (errorMessage != null)
                {
                    throw new InvalidOperationException(errorMessage);
                }

                _changeTrackingStrategy = value;

                PropertyMetadataChanged();
            }
        }

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override Annotation OnAnnotationSet(string name, Annotation annotation, Annotation oldAnnotation)
            => Model.ConventionDispatcher.OnEntityTypeAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<PropertyBase> FindMembersInHierarchy([NotNull] string name)
            => FindPropertiesInHierarchy(name).Cast<PropertyBase>()
                .Concat(FindServicePropertiesInHierarchy(name))
                .Concat(FindNavigationsInHierarchy(name));

        #region Primary and Candidate Keys

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key SetPrimaryKey([CanBeNull] Property property)
            => SetPrimaryKey(property == null ? null : new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key SetPrimaryKey(
            [CanBeNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            if (_baseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(this.DisplayName(), _baseType.DisplayName()));
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
                // ReSharper disable once AssignNullToNotNullAttribute
                newKey = GetOrAddKey(properties);
                if (oldPrimaryKey == newKey)
                {
                    UpdatePrimaryKeyConfigurationSource(configurationSource);
                    return newKey;
                }
            }

            if (oldPrimaryKey != null)
            {
                foreach (var property in _primaryKey.Properties)
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

            PropertyMetadataChanged();
            if (Builder != null)
            {
                Model.ConventionDispatcher.OnPrimaryKeyChanged(Builder, oldPrimaryKey);
            }

            return _primaryKey;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key GetOrSetPrimaryKey([NotNull] Property property)
            => GetOrSetPrimaryKey(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key GetOrSetPrimaryKey([NotNull] IReadOnlyList<Property> properties)
            => FindPrimaryKey(properties) ?? SetPrimaryKey(properties);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindPrimaryKey()
            => _baseType?.FindPrimaryKey() ?? FindDeclaredPrimaryKey();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindDeclaredPrimaryKey() => _primaryKey;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetPrimaryKeyConfigurationSource() => _primaryKeyConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private void SetPrimaryKeyConfigurationSource(ConfigurationSource? configurationSource)
            => _primaryKeyConfigurationSource = configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private void UpdatePrimaryKeyConfigurationSource(ConfigurationSource configurationSource)
            => _primaryKeyConfigurationSource = configurationSource.Max(_primaryKeyConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key AddKey([NotNull] Property property, ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddKey(new[] { property }, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key AddKey(
            [NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));

            if (_baseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(this.DisplayName(), _baseType.DisplayName()));
            }

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                for (var j = i + 1; j < properties.Count; j++)
                {
                    if (property == properties[j])
                    {
                        throw new InvalidOperationException(CoreStrings.DuplicatePropertyInList(Property.Format(properties), property.Name));
                    }
                }

                if (FindProperty(property.Name) != property
                    || property.Builder == null)
                {
                    throw new InvalidOperationException(CoreStrings.KeyPropertiesWrongEntity(Property.Format(properties), this.DisplayName()));
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
                throw new InvalidOperationException(CoreStrings.DuplicateKey(Property.Format(properties), this.DisplayName(), key.DeclaringEntityType.DisplayName()));
            }

            key = new Key(properties, configurationSource);
            _keys.Add(properties, key);

            foreach (var property in properties)
            {
                if (property.Keys == null)
                {
                    property.Keys = new List<IKey>
                    {
                        key
                    };
                }
                else
                {
                    property.Keys.Add(key);
                }
            }

            PropertyMetadataChanged();

            return Model.ConventionDispatcher.OnKeyAdded(key.Builder)?.Metadata;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key GetOrAddKey([NotNull] Property property)
            => GetOrAddKey(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key GetOrAddKey([NotNull] IReadOnlyList<Property> properties)
            => FindKey(properties)
               ?? AddKey(properties);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindKey([NotNull] IProperty property) => FindKey(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredKey(properties) ?? _baseType?.FindKey(properties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Key> GetDeclaredKeys() => _keys.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Key FindDeclaredKey([NotNull] IReadOnlyList<IProperty> properties)
            => _keys.TryGetValue(Check.NotEmpty(properties, nameof(properties)), out var key)
                ? key
                : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public virtual Key RemoveKey([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            var key = FindDeclaredKey(properties);
            return key == null
                ? null
                : RemoveKey(key);
        }

        private Key RemoveKey([NotNull] Key key)
        {
            CheckKeyNotInUse(key);

            if (_primaryKey == key)
            {
                SetPrimaryKey((IReadOnlyList<Property>)null);
                _primaryKeyConfigurationSource = null;
            }

            _keys.Remove(key.Properties);
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

            PropertyMetadataChanged();

            if (Builder != null)
            {
                Model.ConventionDispatcher.OnKeyRemoved(Builder, key);
            }

            return key;
        }

        private void CheckKeyNotInUse(Key key)
        {
            var foreignKey = key.GetReferencingForeignKeys().FirstOrDefault();
            if (foreignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.KeyInUse(Property.Format(key.Properties), this.DisplayName(), foreignKey.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Key> GetKeys() => _baseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;

        #endregion

        #region Foreign Keys

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey AddForeignKey(
            [NotNull] Property property,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddForeignKey(new[] { property }, principalKey, principalEntityType, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey AddForeignKey(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] Key principalKey,
            [NotNull] EntityType principalEntityType,
            ConfigurationSource? configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                for (var j = i + 1; j < properties.Count; j++)
                {
                    if (property == properties[j])
                    {
                        throw new InvalidOperationException(CoreStrings.DuplicatePropertyInList(Property.Format(properties), property.Name));
                    }
                }

                var actualProperty = FindProperty(property.Name);
                if (actualProperty?.DeclaringEntityType.IsAssignableFrom(property.DeclaringEntityType) != true
                    || property.Builder == null)
                {
                    throw new InvalidOperationException(CoreStrings.ForeignKeyPropertiesWrongEntity(Property.Format(properties), this.DisplayName()));
                }
            }

            ForeignKey.AreCompatible(
                principalEntityType,
                dependentEntityType: this,
                navigationToPrincipal: null,
                navigationToDependent: null,
                dependentProperties: properties,
                principalProperties: principalKey.Properties,
                unique: null,
                required: null,
                shouldThrow: true);

            var duplicateForeignKey = FindForeignKeysInHierarchy(properties, principalKey, principalEntityType).FirstOrDefault();
            if (duplicateForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateForeignKey(
                        Property.Format(properties),
                        this.DisplayName(),
                        duplicateForeignKey.DeclaringEntityType.DisplayName(),
                        Property.Format(principalKey.Properties),
                        principalEntityType.DisplayName()));
            }

            var foreignKey = new ForeignKey(properties, principalKey, this, principalEntityType, configurationSource ?? ConfigurationSource.Convention);
            if (configurationSource.HasValue)
            {
                principalEntityType.UpdateConfigurationSource(configurationSource.Value);
                foreignKey.UpdateForeignKeyPropertiesConfigurationSource(configurationSource.Value);
                foreignKey.UpdatePrincipalKeyConfigurationSource(configurationSource.Value);
                foreignKey.UpdatePrincipalEndConfigurationSource(configurationSource.Value);
            }

            if (principalEntityType.Model != Model)
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeModelMismatch(this.DisplayName(), principalEntityType.DisplayName()));
            }

            _foreignKeys.Add(foreignKey);

            foreach (var property in properties)
            {
                if (property.ForeignKeys == null)
                {
                    property.ForeignKeys = new List<IForeignKey>
                    {
                        foreignKey
                    };
                }
                else
                {
                    property.ForeignKeys.Add(foreignKey);
                }
            }

            if (principalKey.ReferencingForeignKeys == null)
            {
                principalKey.ReferencingForeignKeys = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance)
                    {
                        foreignKey
                    };
            }
            else
            {
                var added = principalKey.ReferencingForeignKeys.Add(foreignKey);
                Debug.Assert(added);
            }

            if (principalEntityType.DeclaredReferencingForeignKeys == null)
            {
                principalEntityType.DeclaredReferencingForeignKeys = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance)
                    {
                        foreignKey
                    };
            }
            else
            {
                var added = principalEntityType.DeclaredReferencingForeignKeys.Add(foreignKey);
                Debug.Assert(added);
            }

            PropertyMetadataChanged();

            var builder = Model.ConventionDispatcher.OnForeignKeyAdded(foreignKey.Builder);

            foreignKey = builder?.Metadata;

            return foreignKey;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] Property property, [NotNull] Key principalKey, [NotNull] EntityType principalEntityType)
            => GetOrAddForeignKey(new[] { property }, principalKey, principalEntityType);

        // Note: this will return an existing foreign key even if it doesn't have the same referenced key
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey GetOrAddForeignKey(
            [NotNull] IReadOnlyList<Property> properties, [NotNull] Key principalKey, [NotNull] EntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType)
               ?? AddForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeys([NotNull] IProperty property)
            => FindForeignKeys(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeys([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return _baseType?.FindForeignKeys(properties)?.Concat(FindDeclaredForeignKeys(properties))
                   ?? FindDeclaredForeignKeys(properties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey FindForeignKey(
            [NotNull] IProperty property,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => FindForeignKey(new[] { property }, principalKey, principalEntityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDeclaredForeignKeys() => _foreignKeys;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedForeignKeys()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredForeignKeys());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedForeignKeysInclusive()
            => GetDeclaredForeignKeys().Concat(GetDerivedForeignKeys());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetForeignKeys()
            => _baseType?.GetForeignKeys().Concat(_foreignKeys) ?? _foreignKeys;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDeclaredForeignKeys([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            return _foreignKeys.Where(fk => PropertyListComparer.Instance.Equals(fk.Properties, properties));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ForeignKey FindDeclaredForeignKey(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            return FindDeclaredForeignKeys(properties).SingleOrDefault(
                fk =>
                    PropertyListComparer.Instance.Equals(fk.PrincipalKey.Properties, principalKey.Properties)
                    && StringComparer.Ordinal.Equals(fk.PrincipalEntityType.Name, principalEntityType.Name));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            [NotNull] IReadOnlyList<IProperty> properties)
            => GetDerivedTypes().SelectMany(et => et.FindDeclaredForeignKeys(properties));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => GetDerivedTypes().Select(et => et.FindDeclaredForeignKey(properties, principalKey, principalEntityType))
                .Where(fk => fk != null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            [NotNull] IReadOnlyList<IProperty> properties)
            => FindForeignKeys(properties).Concat(FindDerivedForeignKeys(properties));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            [NotNull] IReadOnlyList<IProperty> properties,
            [NotNull] IKey principalKey,
            [NotNull] IEntityType principalEntityType)
            => ToEnumerable(FindForeignKey(properties, principalKey, principalEntityType))
                .Concat(FindDerivedForeignKeys(properties, principalKey, principalEntityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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

        private ForeignKey RemoveForeignKey([NotNull] ForeignKey foreignKey)
        {
            if (foreignKey.DependentToPrincipal != null)
            {
                foreignKey.DeclaringEntityType.RemoveNavigation(foreignKey.DependentToPrincipal.Name);
            }

            if (foreignKey.PrincipalToDependent != null)
            {
                foreignKey.PrincipalEntityType.RemoveNavigation(foreignKey.PrincipalToDependent.Name);
            }

            var removed = _foreignKeys.Remove(foreignKey);
            foreignKey.Builder = null;

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

            foreignKey.PrincipalKey.ReferencingForeignKeys.Remove(foreignKey);
            foreignKey.PrincipalEntityType.DeclaredReferencingForeignKeys.Remove(foreignKey);

            PropertyMetadataChanged();

            if (removed)
            {
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

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
            => _baseType?.GetReferencingForeignKeys().Concat(GetDeclaredReferencingForeignKeys())
               ?? GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDeclaredReferencingForeignKeys()
            => DeclaredReferencingForeignKeys ?? Enumerable.Empty<ForeignKey>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedReferencingForeignKeys()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredReferencingForeignKeys());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedReferencingForeignKeysInclusive()
            => GetDeclaredReferencingForeignKeys().Concat(GetDerivedReferencingForeignKeys());

        private SortedSet<ForeignKey> DeclaredReferencingForeignKeys { get; set; }

        #endregion

        #region Navigations

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation AddNavigation(
            [NotNull] string name,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(foreignKey, nameof(foreignKey));

            return AddNavigation(new PropertyIdentity(name), foreignKey, pointsToPrincipal);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation AddNavigation(
            [NotNull] MemberInfo navigationProperty,
            [NotNull] ForeignKey foreignKey,
            bool pointsToPrincipal)
        {
            Check.NotNull(navigationProperty, nameof(navigationProperty));
            Check.NotNull(foreignKey, nameof(foreignKey));

            return AddNavigation(new PropertyIdentity(navigationProperty), foreignKey, pointsToPrincipal);
        }

        private Navigation AddNavigation(PropertyIdentity propertyIdentity, ForeignKey foreignKey, bool pointsToPrincipal)
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
                            Property.Format(foreignKey.Properties),
                            Property.Format(duplicateNavigation.ForeignKey.Properties)));
                }

                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(name, this.DisplayName(), duplicateNavigation.DeclaringEntityType.DisplayName()));
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

            var navigationProperty = propertyIdentity.Property;
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

            var navigation = new Navigation(name, propertyIdentity.Property as PropertyInfo, propertyIdentity.Property as FieldInfo, foreignKey);

            _navigations.Add(name, navigation);

            PropertyMetadataChanged();

            return navigation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation FindNavigation([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindDeclaredNavigation(name) ?? _baseType?.FindNavigation(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation FindNavigation([NotNull] MemberInfo memberInfo)
            => FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Navigation FindDeclaredNavigation([NotNull] string name)
            => _navigations.TryGetValue(Check.NotEmpty(name, nameof(name)), out var navigation)
                ? navigation
                : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDeclaredNavigations() => _navigations.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDerivedNavigations()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDerivedNavigationsInclusive()
            => GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredNavigations());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> FindDerivedNavigations([NotNull] string navigationName)
        {
            Check.NotNull(navigationName, nameof(navigationName));

            return GetDerivedTypes().Select(et => et.FindDeclaredNavigation(navigationName)).Where(n => n != null);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsInHierarchy([NotNull] string navigationName)
            => ToEnumerable(FindNavigation(navigationName)).Concat(FindDerivedNavigations(navigationName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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

            PropertyMetadataChanged();

            return navigation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Navigation> GetNavigations()
            => _baseType?.GetNavigations().Concat(_navigations.Values) ?? _navigations.Values;

        #endregion

        #region Indexes

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index AddIndex(
            [NotNull] Property property,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
            => AddIndex(new[] { property }, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index AddIndex(
            [NotNull] IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
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
                        throw new InvalidOperationException(CoreStrings.DuplicatePropertyInList(Property.Format(properties), property.Name));
                    }
                }

                if (FindProperty(property.Name) != property
                    || property.Builder == null)
                {
                    throw new InvalidOperationException(CoreStrings.IndexPropertiesWrongEntity(Property.Format(properties), this.DisplayName()));
                }
            }

            var duplicateIndex = FindIndexesInHierarchy(properties).FirstOrDefault();
            if (duplicateIndex != null)
            {
                throw new InvalidOperationException(CoreStrings.DuplicateIndex(Property.Format(properties), this.DisplayName(), duplicateIndex.DeclaringEntityType.DisplayName()));
            }

            var index = new Index(properties, this, configurationSource);
            _indexes.Add(properties, index);

            foreach (var property in properties)
            {
                if (property.Indexes == null)
                {
                    property.Indexes = new List<IIndex>
                    {
                        index
                    };
                }
                else
                {
                    property.Indexes.Add(index);
                }
            }

            return Model.ConventionDispatcher.OnIndexAdded(index.Builder)?.Metadata;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index GetOrAddIndex([NotNull] Property property)
            => GetOrAddIndex(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index GetOrAddIndex([NotNull] IReadOnlyList<Property> properties)
            => FindIndex(properties) ?? AddIndex(properties);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index FindIndex([NotNull] IProperty property)
            => FindIndex(new[] { property });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index FindIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return FindDeclaredIndex(properties) ?? _baseType?.FindIndex(properties);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetDeclaredIndexes() => _indexes.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetDerivedIndexes()
            => GetDerivedTypes().SelectMany(et => et.GetDeclaredIndexes());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetDerivedIndexesInclusive()
            => GetDeclaredIndexes().Concat(GetDerivedTypes().SelectMany(et => et.GetDeclaredIndexes()));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index FindDeclaredIndex([NotNull] IReadOnlyList<IProperty> properties)
            => _indexes.TryGetValue(Check.NotEmpty(properties, nameof(properties)), out var index)
                ? index
                : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> FindDerivedIndexes([NotNull] IReadOnlyList<IProperty> properties)
            => GetDerivedTypes().Select(et => et.FindDeclaredIndex(properties)).Where(i => i != null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> FindIndexesInHierarchy([NotNull] IReadOnlyList<IProperty> properties)
            => ToEnumerable(FindIndex(properties)).Concat(FindDerivedIndexes(properties));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Index RemoveIndex([NotNull] IReadOnlyList<IProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            var index = FindDeclaredIndex(properties);
            return index == null
                ? null
                : RemoveIndex(index);
        }

        private Index RemoveIndex(Index index)
        {
            _indexes.Remove(index.Properties);
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetIndexes() => _baseType?.GetIndexes().Concat(_indexes.Values) ?? _indexes.Values;

        #endregion

        #region Properties

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property AddProperty(
            [NotNull] string name,
            [CanBeNull] Type propertyType = null,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            ConfigurationSource? typeConfigurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(name, nameof(name));

            return AddProperty(
                name,
                propertyType,
                ClrType?.GetMembersInHierarchy(name).FirstOrDefault(),
                configurationSource,
                typeConfigurationSource,
                isIndexedProperty: false);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property AddProperty(
            [NotNull] MemberInfo memberInfo,
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(memberInfo, nameof(memberInfo));

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

            return AddProperty(
                memberInfo.GetSimpleMemberName(),
                memberInfo.GetMemberType(),
                memberInfo,
                configurationSource,
                configurationSource,
                isIndexedProperty: false);
        }


        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property AddIndexedProperty(
            [NotNull] string name,
            [NotNull] Type propertyType,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit,
            ConfigurationSource? typeConfigurationSource = ConfigurationSource.Explicit)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(propertyType, nameof(propertyType));

            var indexerProperty = this.EFIndexerProperty();
            if (indexerProperty == null)
            {
                throw new InvalidOperationException(CoreStrings.NoIndexer(name, this.DisplayName()));
            }

            return AddProperty(
                name,
                propertyType,
                indexerProperty,
                configurationSource,
                typeConfigurationSource,
                isIndexedProperty: true);
        }

        private Property AddProperty(
            string name,
            Type propertyType,
            MemberInfo memberInfo,
            ConfigurationSource configurationSource,
            ConfigurationSource? typeConfigurationSource,
            bool isIndexedProperty)
        {
            var conflictingMember = FindMembersInHierarchy(name).FirstOrDefault();
            if (conflictingMember != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, this.DisplayName(),
                        conflictingMember.DeclaringType.DisplayName()));
            }

            if (propertyType == null)
            {
                if (memberInfo == null)
                {
                    throw new InvalidOperationException(CoreStrings.NoPropertyType(name, this.DisplayName()));
                }

                propertyType = memberInfo.GetMemberType();
                typeConfigurationSource = ConfigurationSource.Convention.Max(typeConfigurationSource);
            }
            else
            {
                if (memberInfo != null
                    && !isIndexedProperty
                    && propertyType != memberInfo.GetMemberType())
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyWrongClrType(
                            name,
                            this.DisplayName(),
                            memberInfo.GetMemberType().ShortDisplayName(),
                            propertyType.ShortDisplayName()));
                }
            }

            var property = new Property(
                name, propertyType, memberInfo as PropertyInfo, memberInfo as FieldInfo, this,
                configurationSource, typeConfigurationSource);

            _properties.Add(property.Name, property);
            PropertyMetadataChanged();

            return Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property GetOrAddProperty([NotNull] PropertyInfo propertyInfo)
            => FindProperty(propertyInfo) ?? AddProperty(propertyInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property GetOrAddProperty([NotNull] string name, [CanBeNull] Type propertyType)
            => FindProperty(name) ?? AddProperty(name, propertyType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindProperty([NotNull] PropertyInfo propertyInfo)
            => FindProperty(propertyInfo.GetSimpleMemberName());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindProperty([NotNull] string name)
            => FindDeclaredProperty(Check.NotEmpty(name, nameof(name))) ?? _baseType?.FindProperty(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindDeclaredProperty([NotNull] string name)
            => _properties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
                ? property
                : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> GetDeclaredProperties() => _properties.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> FindDerivedProperties([NotNull] string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            return GetDerivedTypes().Select(et => et.FindDeclaredProperty(propertyName)).Where(p => p != null);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> FindDerivedPropertiesInclusive([NotNull] string propertyName)
            => ToEnumerable(FindDeclaredProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> FindPropertiesInHierarchy([NotNull] string propertyName)
            => ToEnumerable(FindProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property RemoveProperty([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindDeclaredProperty(name);
            return property == null
                ? null
                : RemoveProperty(property);
        }

        private Property RemoveProperty(Property property)
        {
            CheckPropertyNotInUse(property);

            _properties.Remove(property.Name);
            property.Builder = null;

            PropertyMetadataChanged();

            return property;
        }

        private void CheckPropertyNotInUse(Property property)
        {
            var containingKey = property.Keys?.FirstOrDefault();
            if (containingKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseKey(property.Name, this.DisplayName(), Property.Format(containingKey.Properties)));
            }

            var containingForeignKey = property.ForeignKeys?.FirstOrDefault();
            if (containingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseForeignKey(
                        property.Name, this.DisplayName(),
                        Property.Format(containingForeignKey.Properties), containingForeignKey.DeclaringEntityType.DisplayName()));
            }

            var containingIndex = property.Indexes?.FirstOrDefault();
            if (containingIndex != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseIndex(
                        property.Name, this.DisplayName(),
                        Property.Format(containingIndex.Properties), containingIndex.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Property> GetProperties()
            => _baseType?.GetProperties().Concat(_properties.Values) ?? _properties.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void PropertyMetadataChanged()
        {
            foreach (var property in GetProperties())
            {
                property.PropertyIndexes = null;
            }

            foreach (var navigation in GetNavigations())
            {
                navigation.PropertyIndexes = null;
            }

            // This path should only kick in when the model is still mutable and therefore access does not need
            // to be thread-safe.
            _counts = null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyCounts Counts
            => NonCapturingLazyInitializer.EnsureInitialized(ref _counts, this, entityType => entityType.CalculateCounts());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> RelationshipSnapshotFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _relationshipSnapshotFactory, this,
                entityType => new RelationshipSnapshotFactoryFactory().Create(entityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> OriginalValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _originalValuesFactory, this,
                entityType => new OriginalValuesFactoryFactory().Create(entityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> SidecarValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _sidecarValuesFactory, this,
                entityType => new SidecarValuesFactoryFactory().Create(entityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<ValueBuffer, ISnapshot> ShadowValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _shadowValuesFactory, this,
                entityType => new ShadowValuesFactoryFactory().Create(entityType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<ISnapshot> EmptyShadowValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _emptyShadowValuesFactory, this,
                entityType => new EmptyShadowValuesFactoryFactory().CreateEmpty(entityType));

        #endregion

        #region Service properties

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ServiceProperty AddServiceProperty(
            [NotNull] MemberInfo memberInfo,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource = ConfigurationSource.Explicit)
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ServiceProperty FindServiceProperty([NotNull] string name)
            => FindDeclaredServiceProperty(Check.NotEmpty(name, nameof(name))) ?? _baseType?.FindServiceProperty(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Property FindServiceProperty([NotNull] MemberInfo memberInfo)
            => FindProperty(memberInfo.GetSimpleMemberName());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ServiceProperty FindDeclaredServiceProperty([NotNull] string name)
            => _serviceProperties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
                ? property
                : null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindDerivedServiceProperties([NotNull] string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            return GetDerivedTypes().Select(et => et.FindDeclaredServiceProperty(propertyName)).Where(p => p != null);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindDerivedServicePropertiesInclusive([NotNull] string propertyName)
            => ToEnumerable(FindDeclaredServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindServicePropertiesInHierarchy([NotNull] string propertyName)
            => ToEnumerable(FindServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ServiceProperty GetOrAddServiceProperty([NotNull] MemberInfo memberInfo)
            => FindServiceProperty(memberInfo.GetSimpleMemberName()) ?? AddServiceProperty(memberInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> GetServiceProperties()
            => _baseType?.GetServiceProperties().Concat(_serviceProperties.Values) ?? _serviceProperties.Values;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> GetDeclaredServiceProperties()
            => _serviceProperties.Values;

        #endregion

        #region Ignore

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ConfigurationSource? FindIgnoredMemberConfigurationSource(string name)
        {
            var ignoredSource = FindDeclaredIgnoredMemberConfigurationSource(name);

            return BaseType == null ? ignoredSource : BaseType.FindIgnoredMemberConfigurationSource(name).Max(ignoredSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void OnTypeMemberIgnored(string name)
            => Model.ConventionDispatcher.OnEntityTypeMemberIgnored(Builder, name);

        #endregion

        #region Data

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IDictionary<string, object>> GetData(bool providerValues = false)
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
                            valueConverter = property.FindMapping()?.Converter
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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

        #region Explicit interface implementations

        IModel ITypeBase.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        IMutableModel IMutableTypeBase.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        IMutableModel IMutableEntityType.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        IEntityType IEntityType.BaseType
        {
            [DebuggerStepThrough]
            get => _baseType;
        }

        IMutableEntityType IMutableEntityType.BaseType
        {
            get => _baseType;
            set => HasBaseType((EntityType)value);
        }

        LambdaExpression IMutableEntityType.QueryFilter
        {
            get => QueryFilter;
            set => QueryFilter = value;
        }

        IEntityType IEntityType.DefiningEntityType
        {
            [DebuggerStepThrough]
            get => DefiningEntityType;
        }

        IMutableKey IMutableEntityType.SetPrimaryKey(IReadOnlyList<IMutableProperty> properties)
            => SetPrimaryKey(properties?.Cast<Property>().ToList());

        [DebuggerStepThrough]
        IKey IEntityType.FindPrimaryKey() => FindPrimaryKey();

        [DebuggerStepThrough]
        IMutableKey IMutableEntityType.FindPrimaryKey() => FindPrimaryKey();

        IMutableKey IMutableEntityType.AddKey(IReadOnlyList<IMutableProperty> properties)
            => AddKey(properties.Cast<Property>().ToList());

        [DebuggerStepThrough]
        IKey IEntityType.FindKey(IReadOnlyList<IProperty> properties) => FindKey(properties);

        [DebuggerStepThrough]
        IMutableKey IMutableEntityType.FindKey(IReadOnlyList<IProperty> properties) => FindKey(properties);

        IEnumerable<IKey> IEntityType.GetKeys() => GetKeys();
        IEnumerable<IMutableKey> IMutableEntityType.GetKeys() => GetKeys();
        IMutableKey IMutableEntityType.RemoveKey(IReadOnlyList<IProperty> properties) => RemoveKey(properties);

        IMutableForeignKey IMutableEntityType.AddForeignKey(
            IReadOnlyList<IMutableProperty> properties, IMutableKey principalKey, IMutableEntityType principalEntityType)
            => AddForeignKey(properties.Cast<Property>().ToList(), (Key)principalKey, (EntityType)principalEntityType);

        [DebuggerStepThrough]
        IMutableForeignKey IMutableEntityType.FindForeignKey(
            IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        [DebuggerStepThrough]
        IForeignKey IEntityType.FindForeignKey(IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        IEnumerable<IForeignKey> IEntityType.GetForeignKeys() => GetForeignKeys();
        IEnumerable<IMutableForeignKey> IMutableEntityType.GetForeignKeys() => GetForeignKeys();

        IMutableForeignKey IMutableEntityType.RemoveForeignKey(
            IReadOnlyList<IProperty> properties, IKey principalKey, IEntityType principalEntityType)
            => RemoveForeignKey(properties, principalKey, principalEntityType);

        IMutableIndex IMutableEntityType.AddIndex(IReadOnlyList<IMutableProperty> properties)
            => AddIndex(properties.Cast<Property>().ToList());

        [DebuggerStepThrough]
        IIndex IEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);

        [DebuggerStepThrough]
        IMutableIndex IMutableEntityType.FindIndex(IReadOnlyList<IProperty> properties) => FindIndex(properties);

        IEnumerable<IIndex> IEntityType.GetIndexes() => GetIndexes();
        IEnumerable<IMutableIndex> IMutableEntityType.GetIndexes() => GetIndexes();

        IMutableIndex IMutableEntityType.RemoveIndex(IReadOnlyList<IProperty> properties)
            => RemoveIndex(properties);

        IMutableProperty IMutableEntityType.AddProperty(string name, Type propertyType) => AddProperty(name, propertyType);

        IMutableProperty IMutableEntityType.AddIndexedProperty(string name, Type propertyType) => AddIndexedProperty(name, propertyType);

        [DebuggerStepThrough]
        IProperty IEntityType.FindProperty(string name) => FindProperty(name);

        [DebuggerStepThrough]
        IMutableProperty IMutableEntityType.FindProperty(string name) => FindProperty(name);

        [DebuggerStepThrough]
        IEnumerable<IProperty> IEntityType.GetProperties() => GetProperties();

        [DebuggerStepThrough]
        IEnumerable<IMutableProperty> IMutableEntityType.GetProperties() => GetProperties();

        IMutableProperty IMutableEntityType.RemoveProperty(string name) => RemoveProperty(name);

        IMutableServiceProperty IMutableEntityType.AddServiceProperty(MemberInfo memberInfo) => AddServiceProperty(memberInfo);

        [DebuggerStepThrough]
        IServiceProperty IEntityType.FindServiceProperty(string name) => FindServiceProperty(name);

        [DebuggerStepThrough]
        IMutableServiceProperty IMutableEntityType.FindServiceProperty(string name) => FindServiceProperty(name);

        IEnumerable<IServiceProperty> IEntityType.GetServiceProperties() => GetServiceProperties();
        IEnumerable<IMutableServiceProperty> IMutableEntityType.GetServiceProperties() => GetServiceProperties();
        IMutableServiceProperty IMutableEntityType.RemoveServiceProperty(string name) => RemoveServiceProperty(name);

        #endregion

        private static IEnumerable<T> ToEnumerable<T>(T element)
            where T : class
            => element == null ? Enumerable.Empty<T>() : new[] { element };

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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class Snapshot
        {
            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
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
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public virtual void Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
            {
                entityTypeBuilder.MergeAnnotationsFrom(EntityType);

                foreach (var ignoredMember in EntityType.GetIgnoredMembers())
                {
                    entityTypeBuilder.Ignore(ignoredMember, EntityType.FindDeclaredIgnoredMemberConfigurationSource(ignoredMember).Value);
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DebugView<EntityType> DebugView
            => new DebugView<EntityType>(this, m => m.ToDebugString(false));
    }
}
