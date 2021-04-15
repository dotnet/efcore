// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
    public class EntityType : TypeBase, IMutableEntityType, IConventionEntityType, IRuntimeEntityType
    {
        private const string DynamicProxyGenAssemblyName = "DynamicProxyGenAssembly2";

        private readonly SortedSet<ForeignKey> _foreignKeys
            = new(ForeignKeyComparer.Instance);

        private readonly SortedDictionary<string, Navigation> _navigations
            = new(StringComparer.Ordinal);

        private readonly SortedDictionary<string, SkipNavigation> _skipNavigations
            = new(StringComparer.Ordinal);

        private readonly SortedDictionary<IReadOnlyList<IReadOnlyProperty>, Index> _unnamedIndexes
            = new(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, Index> _namedIndexes
            = new(StringComparer.Ordinal);

        private readonly SortedDictionary<string, Property> _properties;

        private readonly SortedDictionary<IReadOnlyList<IReadOnlyProperty>, Key> _keys
            = new(PropertyListComparer.Instance);

        private readonly SortedDictionary<string, ServiceProperty> _serviceProperties
            = new(StringComparer.Ordinal);

        private List<object>? _data;
        private Key? _primaryKey;
        private bool? _isKeyless;
        private EntityType? _baseType;
        private ChangeTrackingStrategy? _changeTrackingStrategy;
        private InternalEntityTypeBuilder? _builder;

        private ConfigurationSource? _primaryKeyConfigurationSource;
        private ConfigurationSource? _isKeylessConfigurationSource;
        private ConfigurationSource? _baseTypeConfigurationSource;
        private ConfigurationSource? _changeTrackingStrategyConfigurationSource;
        private ConfigurationSource? _constructorBindingConfigurationSource;
        private ConfigurationSource? _serviceOnlyConstructorBindingConfigurationSource;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private PropertyCounts? _counts;

        // _serviceOnlyConstructorBinding needs to be set as well whenever _constructorBinding is set
        private InstantiationBinding? _constructorBinding;
        private InstantiationBinding? _serviceOnlyConstructorBinding;

        private Func<InternalEntityEntry, ISnapshot>? _relationshipSnapshotFactory;
        private Func<InternalEntityEntry, ISnapshot>? _originalValuesFactory;
        private Func<InternalEntityEntry, ISnapshot>? _temporaryValuesFactory;
        private Func<InternalEntityEntry, ISnapshot>? _storeGeneratedValuesFactory;
        private Func<ValueBuffer, ISnapshot>? _shadowValuesFactory;
        private Func<ISnapshot>? _emptyShadowValuesFactory;
        private Func<MaterializationContext, object>? _instanceFactory;
        private IProperty[]? _foreignKeyProperties;
        private IProperty[]? _valueGeneratingProperties;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityType(string name, Model model, ConfigurationSource configurationSource)
            : base(name, Model.DefaultPropertyBagType, model, configurationSource)
        {
            _properties = new SortedDictionary<string, Property>(new PropertyNameComparer(this));
            _builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityType(Type type, Model model, ConfigurationSource configurationSource)
            : base(type, model, configurationSource)
        {
            if (!type.IsValidEntityType())
            {
                throw new ArgumentException(CoreStrings.InvalidEntityType(type));
            }

            if (DynamicProxyGenAssemblyName.Equals(
                type.Assembly.GetName().Name, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    CoreStrings.AddingProxyTypeAsEntityType(type.FullName));
            }

            _properties = new SortedDictionary<string, Property>(new PropertyNameComparer(this));
            _builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityType(string name, Type type, Model model, ConfigurationSource configurationSource)
            : base(name, type, model, configurationSource)
        {
            if (!type.IsValidEntityType())
            {
                throw new ArgumentException(CoreStrings.InvalidEntityType(type));
            }

            if (DynamicProxyGenAssemblyName.Equals(
                type.Assembly.GetName().Name, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    CoreStrings.AddingProxyTypeAsEntityType(type.FullName));
            }

            _properties = new SortedDictionary<string, Property>(new PropertyNameComparer(this));
            _builder = new InternalEntityTypeBuilder(this, model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalEntityTypeBuilder Builder
        {
            [DebuggerStepThrough] get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsInModel
            => _builder is not null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetRemovedFromModel()
            => _builder = null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? BaseType
            => _baseType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsKeyless
        {
            get => RootType()._isKeyless ?? false;
            set => SetIsKeyless(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        private string DisplayName()
            => ((IReadOnlyEntityType)this).DisplayName();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? SetIsKeyless(bool? keyless, ConfigurationSource configurationSource)
        {
            EnsureMutable();

            if (_isKeyless == keyless)
            {
                UpdateIsKeylessConfigurationSource(configurationSource);
                return keyless;
            }

            if (keyless == true)
            {
                if (_baseType != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DerivedEntityTypeHasNoKey(DisplayName(), RootType().DisplayName()));
                }

                if (_keys.Count != 0)
                {
                    throw new InvalidOperationException(
                        CoreStrings.KeylessTypeExistingKey(
                            DisplayName(), _keys.First().Value.Properties.Format()));
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

            return keyless;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsKeylessConfigurationSource()
            => _isKeylessConfigurationSource;

        private void UpdateIsKeylessConfigurationSource(ConfigurationSource configurationSource)
            => _isKeylessConfigurationSource = configurationSource.Max(_isKeylessConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType? SetBaseType(EntityType? newBaseType, ConfigurationSource configurationSource)
        {
            EnsureMutable();
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");

            if (_baseType == newBaseType)
            {
                UpdateBaseTypeConfigurationSource(configurationSource);
                newBaseType?.UpdateConfigurationSource(configurationSource);
                return newBaseType;
            }

            var originalBaseType = _baseType;

            _baseType?._directlyDerivedTypes.Remove(this);
            _baseType = null;

            if (newBaseType != null)
            {
                if (!newBaseType.ClrType.IsAssignableFrom(ClrType))
                {
                    throw new InvalidOperationException(
                        CoreStrings.NotAssignableClrBaseType(
                            DisplayName(), newBaseType.DisplayName(), ClrType.ShortDisplayName(),
                            newBaseType.ClrType.ShortDisplayName()));
                }

                if (newBaseType.InheritsFrom(this))
                {
                    throw new InvalidOperationException(CoreStrings.CircularInheritance(DisplayName(), newBaseType.DisplayName()));
                }

                if (_keys.Count > 0)
                {
                    throw new InvalidOperationException(CoreStrings.DerivedEntityCannotHaveKeys(DisplayName()));
                }

                if (IsKeyless)
                {
                    throw new InvalidOperationException(CoreStrings.DerivedEntityCannotBeKeyless(DisplayName()));
                }

                var conflictingMember = newBaseType.GetMembers()
                    .Select(p => p.Name)
                    .SelectMany(FindMembersInHierarchy)
                    .FirstOrDefault();

                if (conflictingMember != null)
                {
                    var baseProperty = newBaseType.FindMembersInHierarchy(conflictingMember.Name).Single();
                    throw new InvalidOperationException(
                        CoreStrings.DuplicatePropertiesOnBase(
                            DisplayName(),
                            newBaseType.DisplayName(),
                            ((IReadOnlyTypeBase)conflictingMember.DeclaringType).DisplayName(),
                            conflictingMember.Name,
                            ((IReadOnlyTypeBase)baseProperty.DeclaringType).DisplayName(),
                            baseProperty.Name));
                }

                _baseType = newBaseType;
                _baseType._directlyDerivedTypes.Add(this);
            }

            UpdateBaseTypeConfigurationSource(configurationSource);
            newBaseType?.UpdateConfigurationSource(configurationSource);

            return (EntityType?)Model.ConventionDispatcher.OnEntityTypeBaseTypeChanged(Builder, newBaseType, originalBaseType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void OnTypeRemoved()
        {
            if (_foreignKeys.Count > 0)
            {
                foreach (var foreignKey in GetDeclaredForeignKeys().ToList())
                {
                    if (foreignKey.PrincipalEntityType != this)
                    {
                        RemoveForeignKey(foreignKey);
                    }
                }
            }

            if (_skipNavigations.Count > 0)
            {
                foreach (var skipNavigation in GetDeclaredSkipNavigations().ToList())
                {
                    if (skipNavigation.TargetEntityType != this)
                    {
                        RemoveSkipNavigation(skipNavigation);
                    }
                }
            }

            _builder = null;
            _baseType?._directlyDerivedTypes.Remove(this);

            Model.ConventionDispatcher.OnEntityTypeRemoved(Model.Builder, this);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetBaseTypeConfigurationSource()
            => _baseTypeConfigurationSource;

        [DebuggerStepThrough]
        private void UpdateBaseTypeConfigurationSource(ConfigurationSource configurationSource)
            => _baseTypeConfigurationSource = configurationSource.Max(_baseTypeConfigurationSource);

        private readonly SortedSet<EntityType> _directlyDerivedTypes = new(EntityTypeFullNameComparer.Instance);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        // Note this is ISet because there is no suitable readonly interface in the profiles we are using
        [DebuggerStepThrough]
        public virtual ISet<EntityType> GetDirectlyDerivedTypes()
            => _directlyDerivedTypes;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<EntityType> GetDerivedTypes()
        {
            if (_directlyDerivedTypes.Count == 0)
            {
                return Enumerable.Empty<EntityType>();
            }

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
        [DebuggerStepThrough]
        public virtual IEnumerable<EntityType> GetDerivedTypesInclusive()
            => _directlyDerivedTypes.Count == 0
                ? new[] { this }
                : new[] { this }.Concat(GetDerivedTypes());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual IEnumerable<ForeignKey> GetForeignKeysInHierarchy()
            => _directlyDerivedTypes.Count == 0
                ? GetForeignKeys()
                : GetForeignKeys().Concat(GetDerivedForeignKeys());

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
        public virtual EntityType RootType()
            => (EntityType)((IReadOnlyEntityType)this).GetRootType();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => ((IReadOnlyEntityType)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override IConventionAnnotation? OnAnnotationSet(
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
            => Model.ConventionDispatcher.OnEntityTypeAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<PropertyBase> GetMembers()
            => GetProperties().Cast<PropertyBase>()
                .Concat(GetServiceProperties())
                .Concat(GetNavigations())
                .Concat(GetSkipNavigations());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<PropertyBase> GetDeclaredMembers()
            => GetDeclaredProperties().Cast<PropertyBase>()
                .Concat(GetDeclaredServiceProperties())
                .Concat(GetDeclaredNavigations())
                .Concat(GetDeclaredSkipNavigations());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<PropertyBase> FindMembersInHierarchy(string name)
            => FindPropertiesInHierarchy(name).Cast<PropertyBase>()
                .Concat(FindServicePropertiesInHierarchy(name))
                .Concat(FindNavigationsInHierarchy(name))
                .Concat(FindSkipNavigationsInHierarchy(name));

        #region Primary and Candidate Keys
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? SetPrimaryKey(Property? property, ConfigurationSource configurationSource)
            => SetPrimaryKey(
                property == null
                    ? null
                    : new[] { property }, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? SetPrimaryKey(
            IReadOnlyList<Property>? properties,
            ConfigurationSource configurationSource)
        {
            EnsureMutable();
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");

            if (_baseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(DisplayName(), RootType().DisplayName()));
            }

            var oldPrimaryKey = _primaryKey;
            if (oldPrimaryKey == null && (properties is null || properties.Count == 0))
            {
                return null;
            }

            Key? newKey = null;
            if (properties?.Count > 0)
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

            if (properties?.Count > 0 && newKey != null)
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

            return (Key?)Model.ConventionDispatcher.OnPrimaryKeyChanged(Builder, newKey, oldPrimaryKey);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? FindPrimaryKey()
            => _baseType?.FindPrimaryKey() ?? _primaryKey;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? FindPrimaryKey(IReadOnlyList<Property>? properties)
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
        public virtual ConfigurationSource? GetPrimaryKeyConfigurationSource()
            => _primaryKeyConfigurationSource;

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
        public virtual Key? AddKey(Property property, ConfigurationSource configurationSource)
            => AddKey(
                new[] { property }, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? AddKey(
            IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            EnsureMutable();

            if (_baseType != null)
            {
                throw new InvalidOperationException(CoreStrings.DerivedEntityTypeKey(DisplayName(), _baseType.DisplayName()));
            }

            if (IsKeyless)
            {
                throw new InvalidOperationException(CoreStrings.KeylessTypeWithKey(properties.Format(), DisplayName()));
            }

            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                for (var j = i + 1; j < properties.Count; j++)
                {
                    if (property == properties[j])
                    {
                        throw new InvalidOperationException(CoreStrings.DuplicatePropertyInKey(properties.Format(), property.Name));
                    }
                }

                if (FindProperty(property.Name) != property
                    || !property.IsInModel)
                {
                    throw new InvalidOperationException(CoreStrings.KeyPropertiesWrongEntity(properties.Format(), DisplayName()));
                }

                if (property.IsNullable)
                {
                    throw new InvalidOperationException(CoreStrings.NullableKey(DisplayName(), property.Name));
                }
            }

            var key = FindKey(properties);
            if (key != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateKey(
                        properties.Format(), DisplayName(), key.DeclaringEntityType.DisplayName()));
            }

            key = new Key(properties, configurationSource);
            _keys.Add(properties, key);

            foreach (var property in properties)
            {
                if (property.Keys == null)
                {
                    property.Keys = new List<Key> { key };
                }
                else
                {
                    property.Keys.Add(key);
                }
            }

            return (Key?)Model.ConventionDispatcher.OnKeyAdded(key.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? FindKey(IReadOnlyProperty property)
            => FindKey(new[] { property });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? FindKey(IReadOnlyList<IReadOnlyProperty> properties)
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
        public virtual IEnumerable<Key> GetDeclaredKeys()
            => _keys.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? FindDeclaredKey(IReadOnlyList<IReadOnlyProperty> properties)
            => _keys.TryGetValue(Check.NotEmpty(properties, nameof(properties)), out var key)
                ? key
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Key? RemoveKey(IReadOnlyList<IReadOnlyProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            var wrongEntityTypeProperty = properties.FirstOrDefault(p => !p.DeclaringEntityType.IsAssignableFrom(this));
            if (wrongEntityTypeProperty != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.KeyWrongType(
                        properties.Format(), DisplayName(), wrongEntityTypeProperty.DeclaringEntityType.DisplayName()));
            }

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
        public virtual Key? RemoveKey(Key key)
        {
            Check.NotNull(key, nameof(key));
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
            EnsureMutable();

            if (key.DeclaringEntityType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.KeyWrongType(key.Properties.Format(), DisplayName(), key.DeclaringEntityType.DisplayName()));
            }

            CheckKeyNotInUse(key);

            if (_primaryKey == key)
            {
                SetPrimaryKey((IReadOnlyList<Property>?)null, ConfigurationSource.Explicit);
                _primaryKeyConfigurationSource = null;
            }

            var removed = _keys.Remove(key.Properties);
            Check.DebugAssert(removed, "removed is false");
            key.SetRemovedFromModel();

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

            return (Key?)Model.ConventionDispatcher.OnKeyRemoved(Builder, key);
        }

        private void CheckKeyNotInUse(Key key)
        {
            var foreignKey = key.GetReferencingForeignKeys().FirstOrDefault();
            if (foreignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.KeyInUse(
                        key.Properties.Format(),
                        DisplayName(),
                        foreignKey.Properties.Format(),
                        foreignKey.DeclaringEntityType.DisplayName()));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Key> GetKeys()
            => _baseType?.GetKeys().Concat(_keys.Values) ?? _keys.Values;
        #endregion

        #region Foreign Keys
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey? AddForeignKey(
            Property property,
            Key principalKey,
            EntityType principalEntityType,
            ConfigurationSource? componentConfigurationSource,
            ConfigurationSource configurationSource)
            => AddForeignKey(
                new[] { property }, principalKey, principalEntityType, componentConfigurationSource, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey? AddForeignKey(
            IReadOnlyList<Property> properties,
            Key principalKey,
            EntityType principalEntityType,
            ConfigurationSource? componentConfigurationSource,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));
            EnsureMutable();

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

            return (ForeignKey?)Model.ConventionDispatcher.OnForeignKeyAdded(foreignKey.Builder)?.Metadata;
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
            Check.DebugAssert(removed, "removed is false");

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

            removed = foreignKey.PrincipalKey.ReferencingForeignKeys!.Remove(foreignKey);
            Check.DebugAssert(removed, "removed is false");
            removed = foreignKey.PrincipalEntityType.DeclaredReferencingForeignKeys!.Remove(foreignKey);
            Check.DebugAssert(removed, "removed is false");
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
            Check.DebugAssert(added, "added is false");

            foreach (var property in foreignKey.Properties)
            {
                if (property.ForeignKeys == null)
                {
                    property.ForeignKeys = new List<ForeignKey> { foreignKey };
                }
                else
                {
                    property.ForeignKeys.Add(foreignKey);
                }
            }

            var principalKey = foreignKey.PrincipalKey;
            if (principalKey.ReferencingForeignKeys == null)
            {
                principalKey.ReferencingForeignKeys = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance) { foreignKey };
            }
            else
            {
                added = principalKey.ReferencingForeignKeys.Add(foreignKey);
                Check.DebugAssert(added, "added is false");
            }

            var principalEntityType = foreignKey.PrincipalEntityType;
            if (principalEntityType.DeclaredReferencingForeignKeys == null)
            {
                principalEntityType.DeclaredReferencingForeignKeys = new SortedSet<ForeignKey>(ForeignKeyComparer.Instance) { foreignKey };
            }
            else
            {
                added = principalEntityType.DeclaredReferencingForeignKeys.Add(foreignKey);
                Check.DebugAssert(added, "added is false");
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeys(IReadOnlyProperty property)
            => FindForeignKeys(new[] { property });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        {
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(properties, nameof(properties));

            return _baseType != null
                ? _foreignKeys.Count == 0
                    ? _baseType.FindForeignKeys(properties)
                    : _baseType.FindForeignKeys(properties).Concat(FindDeclaredForeignKeys(properties))
                : FindDeclaredForeignKeys(properties);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey? FindForeignKey(
            IReadOnlyProperty property,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => FindForeignKey(
                new[] { property }, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey? FindForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
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
        public virtual ForeignKey? FindOwnership()
        {
            foreach (var foreignKey in GetForeignKeys())
            {
                if (foreignKey.IsOwnership)
                {
                    return foreignKey;
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
        public virtual ForeignKey? FindDeclaredOwnership()
        {
            foreach (var foreignKey in _foreignKeys)
            {
                if (foreignKey.IsOwnership)
                {
                    return foreignKey;
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
        public virtual IEnumerable<ForeignKey> GetDeclaredForeignKeys()
            => _foreignKeys;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDerivedForeignKeys()
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<ForeignKey>()
                : GetDerivedTypes().SelectMany(et => et._foreignKeys);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetForeignKeys()
            => _baseType != null
                ? _foreignKeys.Count == 0
                    ? _baseType.GetForeignKeys()
                    : _baseType.GetForeignKeys().Concat(_foreignKeys)
                : _foreignKeys;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
        {
            Check.NotEmpty(properties, nameof(properties));

            return _foreignKeys.Count == 0
                ? Enumerable.Empty<ForeignKey>()
                : _foreignKeys.Where(fk => PropertyListComparer.Instance.Equals(fk.Properties, properties));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey? FindDeclaredForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.NotNull(principalKey, nameof(principalKey));
            Check.NotNull(principalEntityType, nameof(principalEntityType));

            if (_foreignKeys.Count == 0)
            {
                return null;
            }

            foreach (var fk in FindDeclaredForeignKeys(properties))
            {
                if (PropertyListComparer.Instance.Equals(fk.PrincipalKey.Properties, principalKey.Properties)
                    && fk.PrincipalEntityType == principalEntityType)
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
            IReadOnlyList<IReadOnlyProperty> properties)
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<ForeignKey>()
                : GetDerivedTypes().SelectMany(et => et.FindDeclaredForeignKeys(properties));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindDerivedForeignKeys(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<ForeignKey>()
                : (IEnumerable<ForeignKey>)GetDerivedTypes()
                    .Select(et => et.FindDeclaredForeignKey(properties, principalKey, principalEntityType))
                    .Where(fk => fk != null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            IReadOnlyList<IReadOnlyProperty> properties)
            => _directlyDerivedTypes.Count == 0
                ? FindForeignKeys(properties)
                : FindForeignKeys(properties).Concat(FindDerivedForeignKeys(properties));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> FindForeignKeysInHierarchy(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindForeignKey(properties, principalKey, principalEntityType))
                : ToEnumerable(FindForeignKey(properties, principalKey, principalEntityType))
                    .Concat(FindDerivedForeignKeys(properties, principalKey, principalEntityType));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ForeignKey? RemoveForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
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
        public virtual ForeignKey? RemoveForeignKey(ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
            EnsureMutable();

            if (foreignKey.DeclaringEntityType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.ForeignKeyWrongType(
                        foreignKey.Properties.Format(),
                        foreignKey.PrincipalKey.Properties.Format(),
                        foreignKey.PrincipalEntityType.DisplayName(),
                        DisplayName(),
                        foreignKey.DeclaringEntityType.DisplayName()));
            }

            var referencingSkipNavigation = foreignKey.ReferencingSkipNavigations?.FirstOrDefault();
            if (referencingSkipNavigation != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ForeignKeyInUseSkipNavigation(
                        foreignKey.Properties.Format(),
                        DisplayName(),
                        referencingSkipNavigation.Name,
                        referencingSkipNavigation.DeclaringEntityType.DisplayName()));
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

            foreignKey.SetRemovedFromModel();

            if (foreignKey.DependentToPrincipal != null)
            {
                foreignKey.DependentToPrincipal.SetRemovedFromModel();
                Model.ConventionDispatcher.OnNavigationRemoved(
                    Builder,
                    foreignKey.PrincipalEntityType.Builder,
                    foreignKey.DependentToPrincipal.Name,
                    foreignKey.DependentToPrincipal.GetIdentifyingMemberInfo());
            }

            if (foreignKey.PrincipalToDependent != null)
            {
                foreignKey.PrincipalToDependent.SetRemovedFromModel();
                Model.ConventionDispatcher.OnNavigationRemoved(
                    foreignKey.PrincipalEntityType.Builder,
                    Builder,
                    foreignKey.PrincipalToDependent.Name,
                    foreignKey.PrincipalToDependent.GetIdentifyingMemberInfo());
            }

            return (ForeignKey?)Model.ConventionDispatcher.OnForeignKeyRemoved(Builder, foreignKey);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetReferencingForeignKeys()
            => _baseType != null
                ? (DeclaredReferencingForeignKeys?.Count ?? 0) == 0
                    ? _baseType.GetReferencingForeignKeys()
                    : _baseType.GetReferencingForeignKeys().Concat(GetDeclaredReferencingForeignKeys())
                : GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetDeclaredReferencingForeignKeys()
            => DeclaredReferencingForeignKeys ?? Enumerable.Empty<ForeignKey>();

        private SortedSet<ForeignKey>? DeclaredReferencingForeignKeys { get; set; }
        #endregion

        #region Navigations
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation AddNavigation(
            string name,
            ForeignKey foreignKey,
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
            MemberInfo navigationMember,
            ForeignKey foreignKey,
            bool pointsToPrincipal)
        {
            Check.NotNull(navigationMember, nameof(navigationMember));
            Check.NotNull(foreignKey, nameof(foreignKey));

            return AddNavigation(new MemberIdentity(navigationMember), foreignKey, pointsToPrincipal);
        }

        private Navigation AddNavigation(MemberIdentity navigationMember, ForeignKey foreignKey, bool pointsToPrincipal)
        {
            EnsureMutable();

            var name = navigationMember.Name!;
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
                        name, DisplayName(), duplicateNavigation.DeclaringEntityType.DisplayName()));
            }

            var duplicateProperty = FindMembersInHierarchy(name).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, DisplayName(), ((IReadOnlyTypeBase)duplicateProperty.DeclaringType).DisplayName()));
            }

            Check.DebugAssert(
                !GetNavigations().Any(n => n.ForeignKey == foreignKey && n.IsOnDependent == pointsToPrincipal),
                "There is another navigation corresponding to the same foreign key and pointing in the same direction.");

            Check.DebugAssert(
                (pointsToPrincipal ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType) == this,
                "EntityType mismatch");

            var memberInfo = navigationMember.MemberInfo;
            if (memberInfo != null)
            {
                ValidateClrMember(name, memberInfo);
            }
            else
            {
                memberInfo = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
            }

            if (memberInfo != null)
            {
                Navigation.IsCompatible(
                    name,
                    memberInfo,
                    this,
                    pointsToPrincipal ? foreignKey.PrincipalEntityType : foreignKey.DeclaringEntityType,
                    !pointsToPrincipal && !foreignKey.IsUnique,
                    shouldThrow: true);
            }

            var navigation = new Navigation(name, memberInfo as PropertyInfo, memberInfo as FieldInfo, foreignKey);

            _navigations.Add(name, navigation);

            return navigation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation? FindNavigation(string name)
            => (Navigation?)((IReadOnlyEntityType)this).FindNavigation(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation? FindNavigation(MemberInfo memberInfo)
            => (Navigation?)((IReadOnlyEntityType)this).FindNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation? FindDeclaredNavigation(string name)
            => _navigations.TryGetValue(Check.NotEmpty(name, nameof(name)), out var navigation)
                ? navigation
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDeclaredNavigations()
            => _navigations.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> GetDerivedNavigations()
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<Navigation>()
                : GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> FindDerivedNavigations(string name)
        {
            Check.NotNull(name, nameof(name));

            return _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<Navigation>()
                : (IEnumerable<Navigation>)GetDerivedTypes().Select(et => et.FindDeclaredNavigation(name)).Where(n => n != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Navigation> FindNavigationsInHierarchy(string name)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindNavigation(name))
                : ToEnumerable(FindNavigation(name)).Concat(FindDerivedNavigations(name));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Navigation? RemoveNavigation(string name)
        {
            Check.NotEmpty(name, nameof(name));
            EnsureMutable();

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
            => _baseType != null
                ? _navigations.Count == 0 ? _baseType.GetNavigations() : _baseType.GetNavigations().Concat(_navigations.Values)
                : _navigations.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SkipNavigation? AddSkipNavigation(
            string name,
            MemberInfo? memberInfo,
            EntityType targetEntityType,
            bool collection,
            bool onDependent,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(targetEntityType, nameof(targetEntityType));
            EnsureMutable();

            var duplicateProperty = FindMembersInHierarchy(name).FirstOrDefault();
            if (duplicateProperty != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, DisplayName(), ((IReadOnlyTypeBase)duplicateProperty.DeclaringType).DisplayName()));
            }

            if (memberInfo != null)
            {
                ValidateClrMember(name, memberInfo);
            }
            else
            {
                memberInfo = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
            }

            if (memberInfo != null)
            {
                Navigation.IsCompatible(
                    name,
                    memberInfo,
                    this,
                    targetEntityType,
                    collection,
                    shouldThrow: true);
            }

            var skipNavigation = new SkipNavigation(
                name,
                memberInfo as PropertyInfo,
                memberInfo as FieldInfo,
                this,
                targetEntityType,
                collection,
                onDependent,
                configurationSource);

            _skipNavigations.Add(name, skipNavigation);

            if (targetEntityType.DeclaredReferencingSkipNavigations == null)
            {
                targetEntityType.DeclaredReferencingSkipNavigations =
                    new SortedSet<SkipNavigation>(SkipNavigationComparer.Instance) { skipNavigation };
            }
            else
            {
                var added = targetEntityType.DeclaredReferencingSkipNavigations.Add(skipNavigation);
                Check.DebugAssert(added, "added is false");
            }

            return (SkipNavigation?)Model.ConventionDispatcher.OnSkipNavigationAdded(skipNavigation.Builder)?.Metadata;
        }

        private Type? ValidateClrMember(string name, MemberInfo memberInfo, bool throwOnNameMismatch = true)
        {
            if (name != memberInfo.GetSimpleMemberName())
            {
                if (memberInfo != FindIndexerPropertyInfo())
                {
                    if (throwOnNameMismatch)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PropertyWrongName(
                                name,
                                DisplayName(),
                                memberInfo.GetSimpleMemberName()));
                    }

                    return memberInfo.GetMemberType();
                }

                var clashingMemberInfo = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
                if (clashingMemberInfo != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyClashingNonIndexer(
                            name,
                            DisplayName()));
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
        public virtual SkipNavigation? FindSkipNavigation(string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindDeclaredSkipNavigation(name) ?? _baseType?.FindSkipNavigation(name);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SkipNavigation? FindSkipNavigation(MemberInfo memberInfo)
            => FindSkipNavigation(Check.NotNull(memberInfo, nameof(memberInfo)).GetSimpleMemberName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SkipNavigation? FindDeclaredSkipNavigation(string name)
            => _skipNavigations.TryGetValue(Check.NotEmpty(name, nameof(name)), out var navigation)
                ? navigation
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> GetDeclaredSkipNavigations()
            => _skipNavigations.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> GetDerivedSkipNavigations()
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<SkipNavigation>()
                : GetDerivedTypes().SelectMany(et => et.GetDeclaredSkipNavigations());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> FindDerivedSkipNavigations(string name)
        {
            Check.NotNull(name, nameof(name));

            return _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<SkipNavigation>()
                : (IEnumerable<SkipNavigation>)GetDerivedTypes().Select(et => et.FindDeclaredSkipNavigation(name)).Where(n => n != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> FindDerivedSkipNavigationsInclusive(string name)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindDeclaredSkipNavigation(name))
                : ToEnumerable(FindDeclaredSkipNavigation(name)).Concat(FindDerivedSkipNavigations(name));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> FindSkipNavigationsInHierarchy(string name)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindSkipNavigation(name))
                : ToEnumerable(FindSkipNavigation(name)).Concat(FindDerivedSkipNavigations(name));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SkipNavigation? RemoveSkipNavigation(string name)
        {
            Check.NotEmpty(name, nameof(name));

            var navigation = FindDeclaredSkipNavigation(name);
            return navigation == null ? null : RemoveSkipNavigation(navigation);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SkipNavigation? RemoveSkipNavigation(SkipNavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
            EnsureMutable();

            if (navigation.DeclaringEntityType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.SkipNavigationWrongType(
                        navigation.Name, DisplayName(), navigation.DeclaringEntityType.DisplayName()));
            }

            if (navigation.Inverse?.Inverse == navigation)
            {
                throw new InvalidOperationException(
                    CoreStrings.SkipNavigationInUseBySkipNavigation(
                        navigation.DeclaringEntityType.DisplayName(),
                        navigation.Name,
                        navigation.Inverse.DeclaringEntityType.DisplayName(),
                        navigation.Inverse.Name));
            }

            var removed = _skipNavigations.Remove(navigation.Name);
            Check.DebugAssert(removed, "Expected the navigation to be removed");

            removed = navigation.ForeignKey is ForeignKey foreignKey
                ? foreignKey.ReferencingSkipNavigations!.Remove(navigation)
                : true;
            Check.DebugAssert(removed, "removed is false");

            removed = navigation.TargetEntityType.DeclaredReferencingSkipNavigations!.Remove(navigation);
            Check.DebugAssert(removed, "removed is false");

            navigation.SetRemovedFromModel();

            return (SkipNavigation?)Model.ConventionDispatcher.OnSkipNavigationRemoved(Builder, navigation);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> GetSkipNavigations()
            => _baseType != null
                ? _skipNavigations.Count == 0
                    ? _baseType.GetSkipNavigations()
                    : _baseType.GetSkipNavigations().Concat(_skipNavigations.Values)
                : _skipNavigations.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> GetReferencingSkipNavigations()
            => _baseType != null
                ? (DeclaredReferencingSkipNavigations?.Count ?? 0) == 0
                    ? _baseType.GetReferencingSkipNavigations()
                    : _baseType.GetReferencingSkipNavigations().Concat(GetDeclaredReferencingSkipNavigations())
                : GetDeclaredReferencingSkipNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> GetDeclaredReferencingSkipNavigations()
            => DeclaredReferencingSkipNavigations ?? Enumerable.Empty<SkipNavigation>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<SkipNavigation> GetDerivedReferencingSkipNavigations()
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<SkipNavigation>()
                : GetDerivedTypes().SelectMany(et => et.GetDeclaredReferencingSkipNavigations());

        private SortedSet<SkipNavigation>? DeclaredReferencingSkipNavigations { get; set; }
        #endregion

        #region Indexes
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? AddIndex(
            Property property,
            ConfigurationSource configurationSource)
            => AddIndex(new[] { property }, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? AddIndex(
            Property property,
            string name,
            ConfigurationSource configurationSource)
            => AddIndex(new[] { property }, name, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? AddIndex(
            IReadOnlyList<Property> properties,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            EnsureMutable();

            CheckIndexProperties(properties);

            var duplicateIndex = FindIndexesInHierarchy(properties).FirstOrDefault();
            if (duplicateIndex != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateIndex(properties.Format(), DisplayName(), duplicateIndex.DeclaringEntityType.DisplayName()));
            }

            var index = new Index(properties, this, configurationSource);
            _unnamedIndexes.Add(properties, index);

            UpdatePropertyIndexes(properties, index);

            return (Index?)Model.ConventionDispatcher.OnIndexAdded(index.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? AddIndex(
            IReadOnlyList<Property> properties,
            string name,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotEmpty(name, nameof(name));
            EnsureMutable();

            CheckIndexProperties(properties);

            var duplicateIndex = FindIndexesInHierarchy(name).FirstOrDefault();
            if (duplicateIndex != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.DuplicateNamedIndex(
                        name,
                        properties.Format(),
                        DisplayName(),
                        duplicateIndex.DeclaringEntityType.DisplayName()));
            }

            var index = new Index(properties, name, this, configurationSource);
            _namedIndexes.Add(name, index);

            UpdatePropertyIndexes(properties, index);

            return (Index?)Model.ConventionDispatcher.OnIndexAdded(index.Builder)?.Metadata;
        }

        private void CheckIndexProperties(IReadOnlyList<Property> properties)
        {
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                for (var j = i + 1; j < properties.Count; j++)
                {
                    if (property == properties[j])
                    {
                        throw new InvalidOperationException(CoreStrings.DuplicatePropertyInIndex(properties.Format(), property.Name));
                    }
                }

                if (FindProperty(property.Name) != property
                    || !property.IsInModel)
                {
                    throw new InvalidOperationException(CoreStrings.IndexPropertiesWrongEntity(properties.Format(), DisplayName()));
                }
            }
        }

        private void UpdatePropertyIndexes(IReadOnlyList<Property> properties, Index index)
        {
            foreach (var property in properties)
            {
                if (property.Indexes == null)
                {
                    property.Indexes = new List<Index> { index };
                }
                else
                {
                    property.Indexes.Add(index);
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? FindIndex(IReadOnlyProperty property)
            => FindIndex(new[] { property });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
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
        public virtual Index? FindIndex(string name)
        {
            Check.NotEmpty(name, nameof(name));

            return FindDeclaredIndex(name) ?? _baseType?.FindIndex(name);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> GetDeclaredIndexes()
            => _namedIndexes.Count == 0
                ? _unnamedIndexes.Values
                : _unnamedIndexes.Values.Concat(_namedIndexes.Values);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> GetDerivedIndexes()
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<Index>()
                : GetDerivedTypes().SelectMany(et => et.GetDeclaredIndexes());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? FindDeclaredIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => _unnamedIndexes.TryGetValue(Check.NotEmpty(properties, nameof(properties)), out var index)
                ? index
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? FindDeclaredIndex(string name)
            => _namedIndexes.TryGetValue(Check.NotEmpty(name, nameof(name)), out var index)
                ? index
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> FindDerivedIndexes(IReadOnlyList<IReadOnlyProperty> properties)
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<Index>()
                : (IEnumerable<Index>)GetDerivedTypes().Select(et => et.FindDeclaredIndex(properties)).Where(i => i != null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> FindDerivedIndexes(string name)
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<Index>()
                : (IEnumerable<Index>)GetDerivedTypes()
                    .Select(et => et.FindDeclaredIndex(Check.NotEmpty(name, nameof(name))))
                    .Where(i => i != null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> FindIndexesInHierarchy(IReadOnlyList<IReadOnlyProperty> properties)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindIndex(properties))
                : ToEnumerable(FindIndex(properties)).Concat(FindDerivedIndexes(properties));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> FindIndexesInHierarchy(string name)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindIndex(Check.NotEmpty(name, nameof(name))))
                : ToEnumerable(FindIndex(Check.NotEmpty(name, nameof(name)))).Concat(FindDerivedIndexes(name));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Index? RemoveIndex(IReadOnlyList<IReadOnlyProperty> properties)
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
        public virtual Index? RemoveIndex(string name)
        {
            Check.NotEmpty(name, nameof(name));

            var index = FindDeclaredIndex(name);
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
        public virtual Index? RemoveIndex(Index index)
        {
            Check.NotNull(index, nameof(index));
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
            EnsureMutable();

            if (index.Name == null)
            {
                if (!_unnamedIndexes.Remove(index.Properties))
                {
                    throw new InvalidOperationException(
                        CoreStrings.IndexWrongType(index.Properties.Format(), DisplayName(), index.DeclaringEntityType.DisplayName()));
                }
            }
            else
            {
                if (!_namedIndexes.Remove(index.Name))
                {
                    throw new InvalidOperationException(
                        CoreStrings.NamedIndexWrongType(index.Name, DisplayName()));
                }
            }

            index.SetRemovedFromModel();

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

            return (Index?)Model.ConventionDispatcher.OnIndexRemoved(Builder, index);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> GetIndexes()
            => _baseType != null
                ? _namedIndexes.Count == 0 && _unnamedIndexes.Count == 0
                    ? _baseType.GetIndexes()
                    : _baseType.GetIndexes().Concat(GetDeclaredIndexes())
                : GetDeclaredIndexes();
        #endregion

        #region Properties
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property? AddProperty(
            string name,
            Type propertyType,
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(propertyType, nameof(propertyType));

            return AddProperty(
                name,
                propertyType,
                ClrType.GetMembersInHierarchy(name).FirstOrDefault(),
                typeConfigurationSource,
                configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property? AddProperty(
            MemberInfo memberInfo,
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
        public virtual Property? AddProperty(
            string name,
            ConfigurationSource configurationSource)
        {
            var clrMember = ClrType.GetMembersInHierarchy(name).FirstOrDefault();
            if (clrMember == null)
            {
                throw new InvalidOperationException(CoreStrings.NoPropertyType(name, DisplayName()));
            }

            return AddProperty(clrMember, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property? AddProperty(
            string name,
            Type propertyType,
            MemberInfo? memberInfo,
            ConfigurationSource? typeConfigurationSource,
            ConfigurationSource configurationSource)
        {
            Check.NotNull(name, nameof(name));
            Check.NotNull(propertyType, nameof(propertyType));
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
            EnsureMutable();

            var conflictingMember = FindMembersInHierarchy(name).FirstOrDefault();
            if (conflictingMember != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, DisplayName(),
                        ((IReadOnlyTypeBase)conflictingMember.DeclaringType).DisplayName()));
            }

            if (memberInfo != null)
            {
                propertyType = ValidateClrMember(name, memberInfo, typeConfigurationSource != null)
                    ?? propertyType;

                if (memberInfo.DeclaringType?.IsAssignableFrom(ClrType) != true)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyWrongEntityClrType(
                            memberInfo.Name, DisplayName(), memberInfo.DeclaringType?.ShortDisplayName()));
                }
            }
            else if (IsPropertyBag)
            {
                memberInfo = FindIndexerPropertyInfo();
            }
            else
            {
                Check.DebugAssert(
                    ClrType.GetMembersInHierarchy(name).FirstOrDefault() == null,
                    "MemberInfo not supplied for non-shadow property");
            }

            if (memberInfo != null
                && propertyType != memberInfo.GetMemberType()
                && memberInfo != FindIndexerPropertyInfo())
            {
                if (typeConfigurationSource != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyWrongClrType(
                            name,
                            DisplayName(),
                            memberInfo.GetMemberType().ShortDisplayName(),
                            propertyType.ShortDisplayName()));
                }

                propertyType = memberInfo.GetMemberType();
            }

            var property = new Property(
                name, propertyType, memberInfo as PropertyInfo, memberInfo as FieldInfo, this,
                configurationSource, typeConfigurationSource);

            _properties.Add(property.Name, property);

            return (Property?)Model.ConventionDispatcher.OnPropertyAdded(property.Builder)?.Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property? FindProperty(string name)
            => FindDeclaredProperty(Check.NotEmpty(name, nameof(name))) ?? _baseType?.FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property? FindDeclaredProperty(string name)
            => _properties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
                ? property
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> GetDeclaredProperties()
            => _properties.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> GetDerivedProperties()
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<Property>()
                : GetDerivedTypes().SelectMany(et => et.GetDeclaredProperties());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> FindDerivedProperties(string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            return _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<Property>()
                : (IEnumerable<Property>)GetDerivedTypes().Select(et => et.FindDeclaredProperty(propertyName)).Where(p => p != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> FindDerivedPropertiesInclusive(string propertyName)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindDeclaredProperty(propertyName))
                : ToEnumerable(FindDeclaredProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Property> FindPropertiesInHierarchy(string propertyName)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindProperty(propertyName))
                : ToEnumerable(FindProperty(propertyName)).Concat(FindDerivedProperties(propertyName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<Property>? FindProperties(IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(propertyNames, nameof(propertyNames));

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
        public virtual Property? RemoveProperty(string name)
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
        public virtual Property? RemoveProperty(Property property)
        {
            Check.NotNull(property, nameof(property));
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
            EnsureMutable();

            if (property.DeclaringEntityType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyWrongType(
                        property.Name,
                        DisplayName(),
                        property.DeclaringEntityType.DisplayName()));
            }

            CheckPropertyNotInUse(property);

            var removed = _properties.Remove(property.Name);
            Check.DebugAssert(removed, "removed is false");

            property.SetRemovedFromModel();

            return (Property?)Model.ConventionDispatcher.OnPropertyRemoved(Builder, property);
        }

        private void CheckPropertyNotInUse(Property property)
        {
            var containingKey = property.Keys?.FirstOrDefault();
            if (containingKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseKey(property.Name, DisplayName(), containingKey.Properties.Format()));
            }

            var containingForeignKey = property.ForeignKeys?.FirstOrDefault();
            if (containingForeignKey != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseForeignKey(
                        property.Name, DisplayName(),
                        containingForeignKey.Properties.Format(), containingForeignKey.DeclaringEntityType.DisplayName()));
            }

            var containingIndex = property.Indexes?.FirstOrDefault();
            if (containingIndex != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyInUseIndex(
                        property.Name, DisplayName(),
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
            => _baseType != null
                ? _baseType.GetProperties().Concat(_properties.Values)
                : _properties.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertyCounts Counts
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _counts, this, static entityType =>
                {
                    entityType.EnsureReadOnly();
                    return entityType.CalculateCounts();
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> RelationshipSnapshotFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _relationshipSnapshotFactory, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();
                    return new RelationshipSnapshotFactoryFactory().Create(entityType);
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> OriginalValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _originalValuesFactory, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();
                    return new OriginalValuesFactoryFactory().Create(entityType);
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> StoreGeneratedValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _storeGeneratedValuesFactory, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();
                    return new SidecarValuesFactoryFactory().Create(entityType);
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<InternalEntityEntry, ISnapshot> TemporaryValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _temporaryValuesFactory, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();
                    return new TemporaryValuesFactoryFactory().Create(entityType);
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<ValueBuffer, ISnapshot> ShadowValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _shadowValuesFactory, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();
                    return new ShadowValuesFactoryFactory().Create(entityType);
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<ISnapshot> EmptyShadowValuesFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _emptyShadowValuesFactory, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();
                    return new EmptyShadowValuesFactoryFactory().CreateEmpty(entityType);
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<MaterializationContext, object> InstanceFactory
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _instanceFactory, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();

                    var binding = entityType.ServiceOnlyConstructorBinding;
                    if (binding == null)
                    {
                        var _ = entityType.ConstructorBinding;
                        binding = entityType.ServiceOnlyConstructorBinding;
                        if (binding == null)
                        {
                            throw new InvalidOperationException(CoreStrings.NoParameterlessConstructor(entityType.DisplayName()));
                        }
                    }

                    var contextParam = Expression.Parameter(typeof(MaterializationContext), "mc");

                    return Expression.Lambda<Func<MaterializationContext, object>>(
                            binding.CreateConstructorExpression(
                                new ParameterBindingInfo(entityType, contextParam)),
                            contextParam)
                        .Compile();
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<IProperty> ForeignKeyProperties
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _foreignKeyProperties, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();

                    return entityType.GetProperties().Where(p => p.IsForeignKey()).ToArray();
                });

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<IProperty> ValueGeneratingProperties
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _valueGeneratingProperties, this,
                static entityType =>
                {
                    entityType.EnsureReadOnly();

                    return entityType.GetProperties().Where(p => p.RequiresValueGenerator()).ToArray();
                });
        #endregion

        #region Service properties
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty AddServiceProperty(
            MemberInfo memberInfo,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            ConfigurationSource configurationSource)
        {
            Check.NotNull(memberInfo, nameof(memberInfo));
            EnsureMutable();

            var name = memberInfo.GetSimpleMemberName();
            var duplicateMember = FindMembersInHierarchy(name).FirstOrDefault();
            if (duplicateMember != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConflictingPropertyOrNavigation(
                        name, DisplayName(),
                        ((IReadOnlyTypeBase)duplicateMember.DeclaringType).DisplayName()));
            }

            ValidateClrMember(name, memberInfo, false);

            var serviceProperty = new ServiceProperty(
                name,
                memberInfo as PropertyInfo,
                memberInfo as FieldInfo,
                this,
                configurationSource);

            _serviceProperties[serviceProperty.Name] = serviceProperty;

            return serviceProperty;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty? FindServiceProperty(string name)
            => FindDeclaredServiceProperty(Check.NotEmpty(name, nameof(name))) ?? _baseType?.FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property? FindServiceProperty(MemberInfo memberInfo)
            => FindProperty(memberInfo.GetSimpleMemberName());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty? FindDeclaredServiceProperty(string name)
            => _serviceProperties.TryGetValue(Check.NotEmpty(name, nameof(name)), out var property)
                ? property
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindDerivedServiceProperties(string propertyName)
        {
            Check.NotNull(propertyName, nameof(propertyName));

            return _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<ServiceProperty>()
                : (IEnumerable<ServiceProperty>)GetDerivedTypes()
                    .Select(et => et.FindDeclaredServiceProperty(propertyName))
                    .Where(p => p != null);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindDerivedServicePropertiesInclusive(string propertyName)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindDeclaredServiceProperty(propertyName))
                : ToEnumerable(FindDeclaredServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> FindServicePropertiesInHierarchy(string propertyName)
            => _directlyDerivedTypes.Count == 0
                ? ToEnumerable(FindServiceProperty(propertyName))
                : ToEnumerable(FindServiceProperty(propertyName)).Concat(FindDerivedServiceProperties(propertyName));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty? RemoveServiceProperty(string name)
        {
            Check.NotEmpty(name, nameof(name));

            var property = FindServiceProperty(name);
            return property == null
                ? null
                : RemoveServiceProperty(property);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ServiceProperty RemoveServiceProperty(ServiceProperty property)
        {
            Check.NotNull(property, nameof(property));
            Check.DebugAssert(IsInModel, "The entity type has been removed from the model");
            EnsureMutable();

            if (property.DeclaringEntityType != this)
            {
                throw new InvalidOperationException(
                    CoreStrings.PropertyWrongType(
                        property.Name,
                        DisplayName(),
                        property.DeclaringEntityType.DisplayName()));
            }

            var removed = _serviceProperties.Remove(property.Name);
            Check.DebugAssert(removed, "removed is false");

            property.SetRemovedFromModel();

            return property;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> GetServiceProperties()
            => _baseType != null
                ? _serviceProperties.Count == 0
                    ? _baseType.GetServiceProperties()
                    : _baseType.GetServiceProperties().Concat(_serviceProperties.Values)
                : _serviceProperties.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> GetDeclaredServiceProperties()
            => _serviceProperties.Values;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ServiceProperty> GetDerivedServiceProperties()
            => _directlyDerivedTypes.Count == 0
                ? Enumerable.Empty<ServiceProperty>()
                : GetDerivedTypes().SelectMany(et => et.GetDeclaredServiceProperties());
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
        public override string? OnTypeMemberIgnored(string name)
            => Model.ConventionDispatcher.OnEntityTypeMemberIgnored(Builder, name);
        #endregion

        #region Data
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IDictionary<string, object?>> GetSeedData(bool providerValues = false)
        {
            if (_data == null
                || _data.Count == 0)
            {
                return Enumerable.Empty<IDictionary<string, object?>>();
            }

            var data = new List<Dictionary<string, object?>>();
            var valueConverters = new Dictionary<string, ValueConverter?>(StringComparer.Ordinal);
            var properties = GetProperties()
                .Concat<IReadOnlyPropertyBase>(GetNavigations())
                .Concat(GetSkipNavigations())
                .ToDictionary(p => p.Name);
            foreach (var rawSeed in _data)
            {
                var seed = new Dictionary<string, object?>(StringComparer.Ordinal);
                data.Add(seed);
                var type = rawSeed.GetType();

                if (ClrType.IsAssignableFrom(type))
                {
                    // non-anonymous type
                    foreach (var propertyBase in properties.Values)
                    {
                        ValueConverter? valueConverter = null;
                        if (providerValues
                            && propertyBase is IReadOnlyProperty property
                            && !valueConverters.TryGetValue(propertyBase.Name, out valueConverter))
                        {
                            valueConverter = property.GetTypeMapping().Converter;
                            valueConverters[propertyBase.Name] = valueConverter;
                        }

                        object? value = null;
                        switch (propertyBase.GetIdentifyingMemberInfo())
                        {
                            case PropertyInfo propertyInfo:
                                if (propertyBase.IsIndexerProperty())
                                {
                                    try
                                    {
                                        value = propertyInfo.GetValue(rawSeed, new[] { propertyBase.Name });
                                    }
                                    catch (Exception)
                                    {
                                        // Swallow if the property value is not set on the seed data
                                    }
                                }
                                else
                                {
                                    value = propertyInfo.GetValue(rawSeed);
                                }

                                break;
                            case FieldInfo fieldInfo:
                                value = fieldInfo.GetValue(rawSeed);
                                break;
                            case null:
                                continue;
                        }

                        seed[propertyBase.Name] = valueConverter == null
                            ? value
                            : valueConverter.ConvertToProvider(value);
                    }
                }
                else
                {
                    // anonymous type
                    foreach (var memberInfo in type.GetMembersInHierarchy())
                    {
                        if (!properties.TryGetValue(memberInfo.GetSimpleMemberName(), out var propertyBase))
                        {
                            continue;
                        }

                        ValueConverter? valueConverter = null;
                        if (providerValues
                            && !valueConverters.TryGetValue(propertyBase.Name, out valueConverter))
                        {
                            if (propertyBase is IReadOnlyProperty property)
                            {
                                valueConverter = property.GetTypeMapping().Converter;
                            }

                            valueConverters[propertyBase.Name] = valueConverter;
                        }

                        // All memberInfos are PropertyInfo in anonymous type
                        var value = ((PropertyInfo)memberInfo).GetValue(rawSeed);

                        seed[propertyBase.Name] = valueConverter == null
                            ? value
                            : valueConverter.ConvertToProvider(value);
                    }
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
        public virtual void AddData(IEnumerable<object> data)
        {
            EnsureMutable();

            _data ??= new List<object>();

            foreach (var entity in data)
            {
                if (ClrType != entity.GetType()
                    && ClrType.IsAssignableFrom(entity.GetType()))
                {
                    throw new InvalidOperationException(
                        CoreStrings.SeedDatumDerivedType(
                            DisplayName(), entity.GetType().ShortDisplayName()));
                }

                _data.Add(entity);
            }
        }
        #endregion

        #region Other
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ChangeTrackingStrategy GetChangeTrackingStrategy()
            => _changeTrackingStrategy ?? Model.GetChangeTrackingStrategy();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ChangeTrackingStrategy? SetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            ConfigurationSource configurationSource)
        {
            EnsureMutable();

            if (changeTrackingStrategy != null)
            {
                var requireFullNotifications =
                    (bool?)Model[CoreAnnotationNames.FullChangeTrackingNotificationsRequiredAnnotation] == true;
                var errorMessage = CheckChangeTrackingStrategy(this, changeTrackingStrategy.Value, requireFullNotifications);
                if (errorMessage != null)
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }

            _changeTrackingStrategy = changeTrackingStrategy;

            _changeTrackingStrategyConfigurationSource = _changeTrackingStrategy == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_changeTrackingStrategyConfigurationSource);

            return changeTrackingStrategy;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string? CheckChangeTrackingStrategy(
            IReadOnlyEntityType entityType,
            ChangeTrackingStrategy value,
            bool requireFullNotifications)
        {
            if (requireFullNotifications)
            {
                if (value != ChangeTrackingStrategy.ChangingAndChangedNotifications
                    && value != ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
                {
                    return CoreStrings.FullChangeTrackingRequired(
                        entityType.DisplayName(), value, nameof(ChangeTrackingStrategy.ChangingAndChangedNotifications),
                        nameof(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues));
                }
            }
            else
            {
                if (value != ChangeTrackingStrategy.Snapshot
                    && !typeof(INotifyPropertyChanged).IsAssignableFrom(entityType.ClrType))
                {
                    return CoreStrings.ChangeTrackingInterfaceMissing(entityType.DisplayName(), value, nameof(INotifyPropertyChanged));
                }

                if ((value == ChangeTrackingStrategy.ChangingAndChangedNotifications
                        || value == ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
                    && !typeof(INotifyPropertyChanging).IsAssignableFrom(entityType.ClrType))
                {
                    return CoreStrings.ChangeTrackingInterfaceMissing(entityType.DisplayName(), value, nameof(INotifyPropertyChanging));
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
        public virtual ConfigurationSource? GetChangeTrackingStrategyConfigurationSource()
            => _changeTrackingStrategyConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual LambdaExpression? SetQueryFilter(LambdaExpression? queryFilter, ConfigurationSource configurationSource)
        {
            var errorMessage = CheckQueryFilter(queryFilter);
            if (errorMessage != null)
            {
                throw new InvalidOperationException(errorMessage);
            }

            return (LambdaExpression?)SetOrRemoveAnnotation(CoreAnnotationNames.QueryFilter, queryFilter, configurationSource)?.Value;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? CheckQueryFilter(LambdaExpression? queryFilter)
        {
            if (queryFilter != null
                && (queryFilter.Parameters.Count != 1
                    || queryFilter.Parameters[0].Type != ClrType
                    || queryFilter.ReturnType != typeof(bool)))
            {
                return CoreStrings.BadFilterExpression(queryFilter, DisplayName(), ClrType);
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual LambdaExpression? GetQueryFilter()
            => (LambdaExpression?)this[CoreAnnotationNames.QueryFilter];

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetQueryFilterConfigurationSource()
            => FindAnnotation(CoreAnnotationNames.QueryFilter)?.GetConfigurationSource();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [Obsolete]
        public virtual LambdaExpression? SetDefiningQuery(LambdaExpression? definingQuery, ConfigurationSource configurationSource)
            => (LambdaExpression?)SetOrRemoveAnnotation(CoreAnnotationNames.DefiningQuery, definingQuery, configurationSource)?.Value;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Property? SetDiscriminatorProperty(Property? property, ConfigurationSource configurationSource)
        {
            CheckDiscriminatorProperty(property);

            if (((property == null && BaseType == null)
                || (property != null && !property.ClrType.IsInstanceOfType(((IReadOnlyEntityType)this).GetDiscriminatorValue()))))
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

            return ((string?)SetAnnotation(CoreAnnotationNames.DiscriminatorProperty, property?.Name, configurationSource)?.Value)
                == property?.Name
                    ? property
                    : (Property?)((IReadOnlyEntityType)this).FindDiscriminatorProperty();
        }

        private void CheckDiscriminatorProperty(Property? property)
        {
            if (property != null)
            {
                if (BaseType != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DiscriminatorPropertyMustBeOnRoot(DisplayName()));
                }

                if (property.DeclaringEntityType != this)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DiscriminatorPropertyNotFound(property.Name, DisplayName()));
                }
            }
        }

        /// <summary>
        ///     Returns the name of the property that will be used for storing a discriminator value.
        /// </summary>
        /// <returns> The name of the property that will be used for storing a discriminator value. </returns>
        public virtual string? GetDiscriminatorPropertyName()
        {
            if (BaseType != null)
            {
                return ((IReadOnlyEntityType)this).GetRootType().GetDiscriminatorPropertyName();
            }

            return (string?)this[CoreAnnotationNames.DiscriminatorProperty];
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static object? CheckDiscriminatorValue(IReadOnlyEntityType entityType, object? value)
        {
            if (value is null)
            {
                return value;
            }

            var discriminatorProperty = entityType.FindDiscriminatorProperty();
            if (discriminatorProperty is null)
            {
                throw new InvalidOperationException(
                    CoreStrings.NoDiscriminatorForValue(entityType.DisplayName(), entityType.GetRootType().DisplayName()));
            }

            if (!discriminatorProperty.ClrType.IsAssignableFrom(value.GetType()))
            {
                throw new InvalidOperationException(
                    CoreStrings.DiscriminatorValueIncompatible(value, discriminatorProperty.Name, discriminatorProperty.ClrType));
            }

            return value;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual ConfigurationSource? GetDiscriminatorPropertyConfigurationSource()
            => FindAnnotation(CoreAnnotationNames.DiscriminatorProperty)?.GetConfigurationSource();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsImplicitlyCreatedJoinEntityType
            => GetConfigurationSource() == ConfigurationSource.Convention
                && ClrType == Model.DefaultPropertyBagType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InstantiationBinding? ConstructorBinding
        {
            get
                => IsReadOnly && !ClrType.IsAbstract
                    ? NonCapturingLazyInitializer.EnsureInitialized(
                        ref _constructorBinding, this, static entityType =>
                        {
                            ((IModel)entityType.Model).GetModelDependencies().ConstructorBindingFactory.GetBindings(
                                (IReadOnlyEntityType)entityType,
                                out entityType._constructorBinding,
                                out entityType._serviceOnlyConstructorBinding);
                        })
                    : _constructorBinding;

            set => SetConstructorBinding(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InstantiationBinding? SetConstructorBinding(
            InstantiationBinding? constructorBinding,
            ConfigurationSource configurationSource)
        {
            EnsureMutable();

            _constructorBinding = constructorBinding;

            if (_constructorBinding == null)
            {
                _constructorBindingConfigurationSource = null;
            }
            else
            {
                UpdateConstructorBindingConfigurationSource(configurationSource);
            }

            return constructorBinding;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetConstructorBindingConfigurationSource()
            => _constructorBindingConfigurationSource;

        private void UpdateConstructorBindingConfigurationSource(ConfigurationSource configurationSource)
            => _constructorBindingConfigurationSource = configurationSource.Max(_constructorBindingConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InstantiationBinding? ServiceOnlyConstructorBinding
        {
            get => _serviceOnlyConstructorBinding;
            set => SetServiceOnlyConstructorBinding(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InstantiationBinding? SetServiceOnlyConstructorBinding(
            InstantiationBinding? constructorBinding,
            ConfigurationSource configurationSource)
        {
            EnsureMutable();

            _serviceOnlyConstructorBinding = constructorBinding;

            if (_serviceOnlyConstructorBinding == null)
            {
                _serviceOnlyConstructorBindingConfigurationSource = null;
            }
            else
            {
                UpdateServiceOnlyConstructorBindingConfigurationSource(configurationSource);
            }

            return constructorBinding;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetServiceOnlyConstructorBindingConfigurationSource()
            => _serviceOnlyConstructorBindingConfigurationSource;

        private void UpdateServiceOnlyConstructorBindingConfigurationSource(ConfigurationSource configurationSource)
            => _serviceOnlyConstructorBindingConfigurationSource =
                configurationSource.Max(_serviceOnlyConstructorBindingConfigurationSource);
        #endregion

        #region Explicit interface implementations
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityTypeBuilder IConventionEntityType.Builder
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionAnnotatableBuilder IConventionAnnotatable.Builder
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IReadOnlyModel IReadOnlyTypeBase.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableModel IMutableTypeBase.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableModel IMutableEntityType.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionModel IConventionEntityType.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IModel ITypeBase.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IReadOnlyEntityType? IReadOnlyEntityType.BaseType
        {
            [DebuggerStepThrough]
            get => _baseType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType? IMutableEntityType.BaseType
        {
            get => _baseType;
            set => SetBaseType((EntityType?)value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType? IConventionEntityType.BaseType
        {
            [DebuggerStepThrough]
            get => BaseType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType? IEntityType.BaseType
        {
            [DebuggerStepThrough]
            get => BaseType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IMutableEntityType.SetDiscriminatorProperty(IReadOnlyProperty? property)
            => SetDiscriminatorProperty((Property?)property, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionProperty? IConventionEntityType.SetDiscriminatorProperty(
            IReadOnlyProperty? property,
            bool fromDataAnnotation)
            => SetDiscriminatorProperty(
                (Property?)property,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IMutableEntityType.SetChangeTrackingStrategy(ChangeTrackingStrategy? changeTrackingStrategy)
            => SetChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        ChangeTrackingStrategy? IConventionEntityType.SetChangeTrackingStrategy(
            ChangeTrackingStrategy? changeTrackingStrategy,
            bool fromDataAnnotation)
            => SetChangeTrackingStrategy(
                changeTrackingStrategy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        void IMutableEntityType.SetQueryFilter(LambdaExpression? queryFilter)
            => SetQueryFilter(queryFilter, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        LambdaExpression? IConventionEntityType.SetQueryFilter(LambdaExpression? queryFilter, bool fromDataAnnotation)
            => SetQueryFilter(queryFilter, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDerivedTypes()
            => GetDerivedTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyEntityType> IReadOnlyEntityType.GetDirectlyDerivedTypes()
            => GetDirectlyDerivedTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IEntityType> IEntityType.GetDirectlyDerivedTypes()
            => GetDirectlyDerivedTypes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType? IConventionEntityType.SetBaseType(IConventionEntityType? entityType, bool fromDataAnnotation)
            => SetBaseType(
                (EntityType?)entityType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        bool? IConventionEntityType.SetIsKeyless(bool? keyless, bool fromDataAnnotation)
            => SetIsKeyless(keyless, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableKey? IMutableEntityType.SetPrimaryKey(IReadOnlyList<IMutableProperty>? properties)
            => SetPrimaryKey(properties?.Cast<Property>().ToList(), ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionKey? IConventionEntityType.SetPrimaryKey(IReadOnlyList<IConventionProperty>? properties, bool fromDataAnnotation)
            => SetPrimaryKey(
                properties?.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyKey? IReadOnlyEntityType.FindPrimaryKey()
            => FindPrimaryKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableKey? IMutableEntityType.FindPrimaryKey()
            => FindPrimaryKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionKey? IConventionEntityType.FindPrimaryKey()
            => FindPrimaryKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IKey? IEntityType.FindPrimaryKey()
            => FindPrimaryKey();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableKey IMutableEntityType.AddKey(IReadOnlyList<IMutableProperty> properties)
            => AddKey(properties.Cast<Property>().ToList(), ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionKey? IConventionEntityType.AddKey(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => AddKey(
                properties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyKey? IReadOnlyEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
            => FindKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableKey? IMutableEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
            => FindKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionKey? IConventionEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
            => FindKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IKey? IEntityType.FindKey(IReadOnlyList<IReadOnlyProperty> properties)
            => FindKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetDeclaredKeys()
            => GetDeclaredKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IKey> IEntityType.GetDeclaredKeys()
            => GetDeclaredKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyKey> IReadOnlyEntityType.GetKeys()
            => GetKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IMutableKey> IMutableEntityType.GetKeys()
            => GetKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IConventionKey> IConventionEntityType.GetKeys()
            => GetKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IKey> IEntityType.GetKeys()
            => GetKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableKey? IMutableEntityType.RemoveKey(IReadOnlyList<IReadOnlyProperty> properties)
            => RemoveKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionKey? IConventionEntityType.RemoveKey(IReadOnlyList<IReadOnlyProperty> properties)
            => RemoveKey(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableKey? IMutableEntityType.RemoveKey(IReadOnlyKey key)
            => RemoveKey((Key)key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionKey? IConventionEntityType.RemoveKey(IReadOnlyKey key)
            => RemoveKey((Key)key);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableForeignKey IMutableEntityType.AddForeignKey(
            IReadOnlyList<IMutableProperty> properties,
            IMutableKey principalKey,
            IMutableEntityType principalEntityType)
            => AddForeignKey(
                properties.Cast<Property>().ToList(),
                (Key)principalKey,
                (EntityType)principalEntityType,
                ConfigurationSource.Explicit,
                ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKey? IConventionEntityType.AddForeignKey(
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
        [DebuggerStepThrough]
        IReadOnlyForeignKey? IReadOnlyEntityType.FindForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableForeignKey? IMutableEntityType.FindForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKey? IConventionEntityType.FindForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IForeignKey? IEntityType.FindForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IReadOnlyKey principalKey,
            IReadOnlyEntityType principalEntityType)
            => FindForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => FindForeignKeys(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IForeignKey> IEntityType.FindForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => FindForeignKeys(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => FindDeclaredForeignKeys(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IForeignKey> IEntityType.FindDeclaredForeignKeys(IReadOnlyList<IReadOnlyProperty> properties)
            => FindDeclaredForeignKeys(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetForeignKeys()
            => GetForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IMutableForeignKey> IMutableEntityType.GetForeignKeys()
            => GetForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IConventionForeignKey> IConventionEntityType.GetForeignKeys()
            => GetForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IForeignKey> IEntityType.GetForeignKeys()
            => GetForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredForeignKeys()
            => GetDeclaredForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IForeignKey> IEntityType.GetDeclaredForeignKeys()
            => GetDeclaredForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDerivedForeignKeys()
            => GetDerivedForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IForeignKey> IEntityType.GetDerivedForeignKeys()
            => GetDerivedForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetDeclaredReferencingForeignKeys()
            => GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IForeignKey> IEntityType.GetDeclaredReferencingForeignKeys()
            => GetDeclaredReferencingForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyForeignKey> IReadOnlyEntityType.GetReferencingForeignKeys()
            => GetReferencingForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IForeignKey> IEntityType.GetReferencingForeignKeys()
            => GetReferencingForeignKeys();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKey? IConventionEntityType.RemoveForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IConventionKey principalKey,
            IConventionEntityType principalEntityType)
            => RemoveForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableForeignKey? IMutableEntityType.RemoveForeignKey(
            IReadOnlyList<IReadOnlyProperty> properties,
            IMutableKey principalKey,
            IMutableEntityType principalEntityType)
            => RemoveForeignKey(properties, principalKey, principalEntityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableForeignKey? IMutableEntityType.RemoveForeignKey(IReadOnlyForeignKey foreignKey)
            => RemoveForeignKey((ForeignKey)foreignKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionForeignKey? IConventionEntityType.RemoveForeignKey(IReadOnlyForeignKey foreignKey)
            => RemoveForeignKey((ForeignKey)foreignKey);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetDeclaredNavigations()
            => GetDeclaredNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<INavigation> IEntityType.GetDeclaredNavigations()
            => GetDeclaredNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyNavigation? IReadOnlyEntityType.FindDeclaredNavigation(string name)
            => FindDeclaredNavigation(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        INavigation? IEntityType.FindDeclaredNavigation(string name)
            => FindDeclaredNavigation(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetDerivedNavigations()
            => GetDerivedNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyNavigation> IReadOnlyEntityType.GetNavigations()
            => GetNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<INavigation> IEntityType.GetNavigations()
            => GetNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableSkipNavigation IMutableEntityType.AddSkipNavigation(
            string name,
            MemberInfo? memberInfo,
            IMutableEntityType targetEntityType,
            bool collection,
            bool onDependent)
            => AddSkipNavigation(
                name, memberInfo, (EntityType)targetEntityType, collection, onDependent,
                ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionSkipNavigation? IConventionEntityType.AddSkipNavigation(
            string name,
            MemberInfo? memberInfo,
            IConventionEntityType targetEntityType,
            bool collection,
            bool onDependent,
            bool fromDataAnnotation)
            => AddSkipNavigation(
                name, memberInfo, (EntityType)targetEntityType, collection, onDependent,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlySkipNavigation? IReadOnlyEntityType.FindSkipNavigation(MemberInfo memberInfo)
            => FindSkipNavigation(memberInfo);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlySkipNavigation? IReadOnlyEntityType.FindSkipNavigation(string name)
            => FindSkipNavigation(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableSkipNavigation? IMutableEntityType.FindSkipNavigation(string name)
            => FindSkipNavigation(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionSkipNavigation? IConventionEntityType.FindSkipNavigation(string name)
            => FindSkipNavigation(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        ISkipNavigation? IEntityType.FindSkipNavigation(string name)
            => FindSkipNavigation(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlySkipNavigation? IReadOnlyEntityType.FindDeclaredSkipNavigation(string name)
            => FindDeclaredSkipNavigation(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetDeclaredSkipNavigations()
            => GetDeclaredSkipNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetDerivedSkipNavigations()
            => GetDerivedSkipNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlySkipNavigation> IReadOnlyEntityType.GetSkipNavigations()
            => GetSkipNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IMutableSkipNavigation> IMutableEntityType.GetSkipNavigations()
            => GetSkipNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IConventionSkipNavigation> IConventionEntityType.GetSkipNavigations()
            => GetSkipNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<ISkipNavigation> IEntityType.GetSkipNavigations()
            => GetSkipNavigations();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableSkipNavigation? IMutableEntityType.RemoveSkipNavigation(IReadOnlySkipNavigation navigation)
            => RemoveSkipNavigation((SkipNavigation)navigation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionSkipNavigation? IConventionEntityType.RemoveSkipNavigation(IReadOnlySkipNavigation navigation)
            => RemoveSkipNavigation((SkipNavigation)navigation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableIndex IMutableEntityType.AddIndex(IReadOnlyList<IMutableProperty> properties)
            => AddIndex(properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(), ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableIndex IMutableEntityType.AddIndex(IReadOnlyList<IMutableProperty> properties, string name)
            => AddIndex(properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(), name, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndex? IConventionEntityType.AddIndex(IReadOnlyList<IConventionProperty> properties, bool fromDataAnnotation)
            => AddIndex(
                properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndex? IConventionEntityType.AddIndex(
            IReadOnlyList<IConventionProperty> properties,
            string name,
            bool fromDataAnnotation)
            => AddIndex(
                properties as IReadOnlyList<Property> ?? properties.Cast<Property>().ToList(),
                name,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyIndex? IReadOnlyEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => FindIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableIndex? IMutableEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => FindIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndex? IConventionEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => FindIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IIndex? IEntityType.FindIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => FindIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyIndex? IReadOnlyEntityType.FindIndex(string name)
            => FindIndex(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableIndex? IMutableEntityType.FindIndex(string name)
            => FindIndex(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndex? IConventionEntityType.FindIndex(string name)
            => FindIndex(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IIndex? IEntityType.FindIndex(string name)
            => FindIndex(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDeclaredIndexes()
            => GetDeclaredIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IIndex> IEntityType.GetDeclaredIndexes()
            => GetDeclaredIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetDerivedIndexes()
            => GetDerivedIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IIndex> IEntityType.GetDerivedIndexes()
            => GetDerivedIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyIndex> IReadOnlyEntityType.GetIndexes()
            => GetIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IMutableIndex> IMutableEntityType.GetIndexes()
            => GetIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IConventionIndex> IConventionEntityType.GetIndexes()
            => GetIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IIndex> IEntityType.GetIndexes()
            => GetIndexes();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndex? IConventionEntityType.RemoveIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => RemoveIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableIndex? IMutableEntityType.RemoveIndex(IReadOnlyList<IReadOnlyProperty> properties)
            => RemoveIndex(properties);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableIndex? IMutableEntityType.RemoveIndex(IReadOnlyIndex index)
            => RemoveIndex((Index)index);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionIndex? IConventionEntityType.RemoveIndex(IReadOnlyIndex index)
            => RemoveIndex((Index)index);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableProperty IMutableEntityType.AddProperty(string name)
            => AddProperty(name, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionProperty? IConventionEntityType.AddProperty(string name, bool fromDataAnnotation)
            => AddProperty(
                name,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableProperty IMutableEntityType.AddProperty(string name, Type propertyType)
            => AddProperty(
                name,
                propertyType,
                ConfigurationSource.Explicit,
                ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionProperty? IConventionEntityType.AddProperty(
            string name,
            Type propertyType,
            bool setTypeConfigurationSource,
            bool fromDataAnnotation)
            => AddProperty(
                name,
                propertyType,
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
        [DebuggerStepThrough]
        IMutableProperty IMutableEntityType.AddProperty(string name, Type propertyType, MemberInfo? memberInfo)
            => AddProperty(
                name, propertyType, memberInfo ?? ClrType.GetMembersInHierarchy(name).FirstOrDefault(),
                ConfigurationSource.Explicit, ConfigurationSource.Explicit)!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionProperty? IConventionEntityType.AddProperty(
            string name,
            Type propertyType,
            MemberInfo? memberInfo,
            bool setTypeConfigurationSource,
            bool fromDataAnnotation)
            => AddProperty(
                name,
                propertyType,
                memberInfo ?? ClrType.GetMembersInHierarchy(name).FirstOrDefault(),
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
        [DebuggerStepThrough]
        IReadOnlyProperty? IReadOnlyEntityType.FindDeclaredProperty(string name)
            => FindDeclaredProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IProperty? IEntityType.FindDeclaredProperty(string name)
            => FindDeclaredProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyList<IReadOnlyProperty>? IReadOnlyEntityType.FindProperties(IReadOnlyList<string> propertyNames)
            => FindProperties(propertyNames);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyProperty? IReadOnlyEntityType.FindProperty(string name)
            => FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableProperty? IMutableEntityType.FindProperty(string name)
            => FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionProperty? IConventionEntityType.FindProperty(string name)
            => FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IProperty? IEntityType.FindProperty(string name)
            => FindProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyProperty> IReadOnlyEntityType.GetDeclaredProperties()
            => GetDeclaredProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IProperty> IEntityType.GetDeclaredProperties()
            => GetDeclaredProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyProperty> IReadOnlyEntityType.GetDerivedProperties()
            => GetDerivedProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyProperty> IReadOnlyEntityType.GetProperties()
            => GetProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IMutableProperty> IMutableEntityType.GetProperties()
            => GetProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IConventionProperty> IConventionEntityType.GetProperties()
            => GetProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IProperty> IEntityType.GetProperties()
            => GetProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IProperty> IEntityType.GetForeignKeyProperties()
            => ForeignKeyProperties;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IProperty> IEntityType.GetValueGeneratingProperties()
            => ValueGeneratingProperties;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableProperty? IMutableEntityType.RemoveProperty(string name)
            => RemoveProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionProperty? IConventionEntityType.RemoveProperty(string name)
            => RemoveProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableProperty? IMutableEntityType.RemoveProperty(IReadOnlyProperty property)
            => RemoveProperty((Property)property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionProperty? IConventionEntityType.RemoveProperty(IReadOnlyProperty property)
            => RemoveProperty((Property)property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableServiceProperty IMutableEntityType.AddServiceProperty(MemberInfo memberInfo)
            => AddServiceProperty(memberInfo, ConfigurationSource.Explicit);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionServiceProperty IConventionEntityType.AddServiceProperty(MemberInfo memberInfo, bool fromDataAnnotation)
            => AddServiceProperty(memberInfo, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IReadOnlyServiceProperty? IReadOnlyEntityType.FindServiceProperty(string name)
            => FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableServiceProperty? IMutableEntityType.FindServiceProperty(string name)
            => FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionServiceProperty? IConventionEntityType.FindServiceProperty(string name)
            => FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IServiceProperty? IEntityType.FindServiceProperty(string name)
            => FindServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetDeclaredServiceProperties()
            => GetDeclaredServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IServiceProperty> IEntityType.GetDeclaredServiceProperties()
            => GetDeclaredServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetDerivedServiceProperties()
            => GetDerivedServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IReadOnlyServiceProperty> IReadOnlyEntityType.GetServiceProperties()
            => GetServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IMutableServiceProperty> IMutableEntityType.GetServiceProperties()
            => GetServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IConventionServiceProperty> IConventionEntityType.GetServiceProperties()
            => GetServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IEnumerable<IServiceProperty> IEntityType.GetServiceProperties()
            => GetServiceProperties();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableServiceProperty? IMutableEntityType.RemoveServiceProperty(IReadOnlyServiceProperty property)
            => RemoveServiceProperty((ServiceProperty)property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionServiceProperty? IConventionEntityType.RemoveServiceProperty(IReadOnlyServiceProperty property)
            => RemoveServiceProperty((ServiceProperty)property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IMutableServiceProperty? IMutableEntityType.RemoveServiceProperty(string name)
            => RemoveServiceProperty(name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        IConventionServiceProperty? IConventionEntityType.RemoveServiceProperty(string name)
            => RemoveServiceProperty(name);
        #endregion

        private static IEnumerable<T> ToEnumerable<T>(T? element)
            where T : class
            => element == null
                ? Enumerable.Empty<T>()
                : new[] { element };

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
                EntityType entityType,
                PropertiesSnapshot? properties,
                List<InternalIndexBuilder>? indexes,
                List<(InternalKeyBuilder, ConfigurationSource?)>? keys,
                List<RelationshipSnapshot>? relationships,
                List<InternalSkipNavigationBuilder>? skipNavigations,
                List<InternalServicePropertyBuilder>? serviceProperties)
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

                SkipNavigations = skipNavigations;
                ServiceProperties = serviceProperties;
            }

            private EntityType EntityType { [DebuggerStepThrough] get; }
            private PropertiesSnapshot Properties { [DebuggerStepThrough] get; }
            private List<InternalSkipNavigationBuilder>? SkipNavigations { [DebuggerStepThrough] get; }
            private List<InternalServicePropertyBuilder>? ServiceProperties { [DebuggerStepThrough] get; }

            /// <summary>
            ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
            ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
            ///     any release. You should only use it directly in your code with extreme caution and knowing that
            ///     doing so can result in application failures when updating to a new Entity Framework Core release.
            /// </summary>
            public virtual void Attach(InternalEntityTypeBuilder entityTypeBuilder)
            {
                entityTypeBuilder.MergeAnnotationsFrom(EntityType);

                foreach (var ignoredMember in EntityType.GetIgnoredMembers())
                {
                    entityTypeBuilder.Ignore(ignoredMember, EntityType.FindDeclaredIgnoredConfigurationSource(ignoredMember)!.Value);
                }

                if (EntityType._baseTypeConfigurationSource != null)
                {
                    var baseType = EntityType._baseType;
                    if (baseType?.IsInModel == false)
                    {
                        baseType = EntityType.Model.FindActualEntityType(baseType);
                    }

                    entityTypeBuilder.Metadata.SetBaseType(baseType, EntityType._baseTypeConfigurationSource.Value);
                }

                if (EntityType._isKeylessConfigurationSource != null)
                {
                    entityTypeBuilder.Metadata.SetIsKeyless(EntityType.IsKeyless, EntityType._isKeylessConfigurationSource.Value);
                }

                if (EntityType._changeTrackingStrategyConfigurationSource != null)
                {
                    entityTypeBuilder.Metadata.SetChangeTrackingStrategy(
                        EntityType.GetChangeTrackingStrategy(), EntityType._changeTrackingStrategyConfigurationSource.Value);
                }

                if (ServiceProperties != null)
                {
                    foreach (var detachedServiceProperty in ServiceProperties)
                    {
                        detachedServiceProperty.Attach(entityTypeBuilder);
                    }
                }

                Properties.Attach(entityTypeBuilder);

                if (SkipNavigations != null)
                {
                    foreach (var detachedSkipNavigation in SkipNavigations)
                    {
                        detachedSkipNavigation.Attach();
                    }
                }

                if (EntityType._constructorBindingConfigurationSource != null)
                {
                    entityTypeBuilder.Metadata.SetConstructorBinding(
                        Create(EntityType.ConstructorBinding, entityTypeBuilder.Metadata),
                        EntityType._constructorBindingConfigurationSource.Value);
                }

                if (EntityType._serviceOnlyConstructorBindingConfigurationSource != null)
                {
                    entityTypeBuilder.Metadata.SetServiceOnlyConstructorBinding(
                        Create(EntityType.ServiceOnlyConstructorBinding, entityTypeBuilder.Metadata),
                        EntityType._serviceOnlyConstructorBindingConfigurationSource.Value);
                }

                var rawData = EntityType._data;
                if (rawData != null)
                {
                    entityTypeBuilder.Metadata.AddData(rawData);
                }
            }

            private InstantiationBinding? Create(InstantiationBinding? instantiationBinding, EntityType entityType)
                => instantiationBinding?.With(instantiationBinding.ParameterBindings.Select(binding => Create(binding, entityType)).ToList());

            private ParameterBinding Create(ParameterBinding parameterBinding, EntityType entityType)
                => parameterBinding.With(parameterBinding.ConsumedProperties.Select(property =>
                (entityType.FindProperty(property.Name)
                    ?? entityType.FindServiceProperty(property.Name)
                    ?? entityType.FindNavigation(property.Name)
                    ?? (IPropertyBase?)entityType.FindSkipNavigation(property.Name))!).ToArray());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new(
                () => ((IReadOnlyEntityType)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IReadOnlyEntityType)this).ToDebugString(MetadataDebugStringOptions.LongDefault));
    }
}
