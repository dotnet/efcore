// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Property : PropertyBase, IMutableProperty
    {
        private bool? _isConcurrencyToken;
        private bool? _isNullable;
        private ValueGenerated? _valueGenerated;
        private PropertySaveBehavior? _beforeSaveBehavior;
        private PropertySaveBehavior? _afterSaveBehavior;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _typeConfigurationSource;
        private ConfigurationSource? _beforeSaveBehaviorConfigurationSource;
        private ConfigurationSource? _afterSaveBehaviorConfigurationSource;
        private ConfigurationSource? _isNullableConfigurationSource;
        private ConfigurationSource? _isConcurrencyTokenConfigurationSource;
        private ConfigurationSource? _valueGeneratedConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Property(
            [NotNull] string name,
            [NotNull] Type clrType,
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] FieldInfo fieldInfo,
            [NotNull] EntityType declaringEntityType,
            ConfigurationSource configurationSource,
            ConfigurationSource? typeConfigurationSource)
            : base(name, propertyInfo, fieldInfo)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DeclaringEntityType = declaringEntityType;
            ClrType = clrType;
            _configurationSource = configurationSource;
            _typeConfigurationSource = typeConfigurationSource;

            Builder = new InternalPropertyBuilder(this, declaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void PropertyMetadataChanged() => DeclaringType.PropertyMetadataChanged();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual EntityType DeclaringType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ClrType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Builder { [DebuggerStepThrough] get; [DebuggerStepThrough] [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        // Needed for a workaround before reference counting is implemented
        // Issue #214
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetTypeConfigurationSource() => _typeConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateTypeConfigurationSource(ConfigurationSource configurationSource)
            => _typeConfigurationSource = _typeConfigurationSource.Max(configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsNullable
        {
            get => _isNullable ?? DefaultIsNullable;
            set => SetIsNullable(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsNullable(bool nullable, ConfigurationSource configurationSource)
        {
            if (nullable)
            {
                if (!ClrType.IsNullableType())
                {
                    throw new InvalidOperationException(CoreStrings.CannotBeNullable(Name, DeclaringEntityType.DisplayName(), ClrType.ShortDisplayName()));
                }

                if (Keys != null)
                {
                    throw new InvalidOperationException(CoreStrings.CannotBeNullablePK(Name, DeclaringEntityType.DisplayName()));
                }
            }

            UpdateIsNullableConfigurationSource(configurationSource);

            var isChanging = IsNullable != nullable;
            _isNullable = nullable;
            if (isChanging)
            {
                DeclaringEntityType.Model.ConventionDispatcher.OnPropertyNullableChanged(Builder);
            }
        }

        private bool DefaultIsNullable => ClrType.IsNullableType();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsNullableConfigurationSource() => _isNullableConfigurationSource;

        private void UpdateIsNullableConfigurationSource(ConfigurationSource configurationSource)
            => _isNullableConfigurationSource = configurationSource.Max(_isNullableConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void OnFieldInfoSet(FieldInfo oldFieldInfo)
            => DeclaringEntityType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, oldFieldInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueGenerated ValueGenerated
        {
            get => _valueGenerated ?? DefaultValueGenerated;
            set => SetValueGenerated(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            if (valueGenerated != null
                && valueGenerated != ValueGenerated.Never
                && this.IsKey())
            {
                var inheritedFk = GetContainingForeignKeys().FirstOrDefault(fk => fk.DeclaringEntityType.BaseType != null);
                if (inheritedFk != null)
                {
                    var key = GetContainingKeys().First();
                    throw new InvalidOperationException(
                        CoreStrings.ForeignKeyPropertyInKey(
                            Name,
                            inheritedFk.DeclaringEntityType.DisplayName(),
                            Format(key.Properties),
                            key.DeclaringEntityType.DisplayName()));
                }
            }

            _valueGenerated = valueGenerated;

            if (valueGenerated == null)
            {
                _valueGeneratedConfigurationSource = null;
            }
            else
            {
                UpdateValueGeneratedConfigurationSource(configurationSource);
            }
        }

        private static ValueGenerated DefaultValueGenerated => ValueGenerated.Never;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetValueGeneratedConfigurationSource() => _valueGeneratedConfigurationSource;

        private void UpdateValueGeneratedConfigurationSource(ConfigurationSource configurationSource)
            => _valueGeneratedConfigurationSource = configurationSource.Max(_valueGeneratedConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertySaveBehavior BeforeSaveBehavior
        {
            get => _beforeSaveBehavior ?? DefaultBeforeSaveBehavior;
            set => SetBeforeSaveBehavior(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetBeforeSaveBehavior(PropertySaveBehavior? beforeSaveBehavior, ConfigurationSource configurationSource)
        {
            if (BeforeSaveBehavior != beforeSaveBehavior)
            {
                _beforeSaveBehavior = beforeSaveBehavior;
                PropertyMetadataChanged();
            }
            else
            {
                _beforeSaveBehavior = beforeSaveBehavior;
            }

            UpdateBeforeSaveBehaviorConfigurationSource(configurationSource);
        }

        private PropertySaveBehavior DefaultBeforeSaveBehavior
            => ValueGenerated == ValueGenerated.OnAddOrUpdate
                ? PropertySaveBehavior.Ignore
                : PropertySaveBehavior.Save;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetBeforeSaveBehaviorConfigurationSource() => _beforeSaveBehaviorConfigurationSource;

        private void UpdateBeforeSaveBehaviorConfigurationSource(ConfigurationSource configurationSource)
            => _beforeSaveBehaviorConfigurationSource = configurationSource.Max(_beforeSaveBehaviorConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertySaveBehavior AfterSaveBehavior
        {
            get => _afterSaveBehavior ?? DefaultAfterSaveBehavior;
            set => SetAfterSaveBehavior(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetAfterSaveBehavior(PropertySaveBehavior? afterSaveBehavior, ConfigurationSource configurationSource)
        {
            if (afterSaveBehavior != PropertySaveBehavior.Throw
                && Keys != null)
            {
                throw new InvalidOperationException(CoreStrings.KeyPropertyMustBeReadOnly(Name, DeclaringEntityType.DisplayName()));
            }

            if (AfterSaveBehavior != afterSaveBehavior)
            {
                _afterSaveBehavior = afterSaveBehavior;
                PropertyMetadataChanged();
            }
            else
            {
                _afterSaveBehavior = afterSaveBehavior;
            }

            UpdateAfterSaveBehaviorConfigurationSource(configurationSource);
        }

        private PropertySaveBehavior DefaultAfterSaveBehavior
            => Keys != null
                ? PropertySaveBehavior.Throw
                : ValueGenerated.ForUpdate()
                    ? PropertySaveBehavior.Ignore
                    : PropertySaveBehavior.Save;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetAfterSaveBehaviorConfigurationSource() => _afterSaveBehaviorConfigurationSource;

        private void UpdateAfterSaveBehaviorConfigurationSource(ConfigurationSource configurationSource)
            => _afterSaveBehaviorConfigurationSource = configurationSource.Max(_afterSaveBehaviorConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete("Use BeforeSaveBehavior instead.")]
        public virtual bool IsReadOnlyBeforeSave
        {
            get => BeforeSaveBehavior == PropertySaveBehavior.Throw;
            set
            {
                if (value)
                {
                    BeforeSaveBehavior = PropertySaveBehavior.Throw;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete("Use AfterSaveBehavior instead.")]
        public virtual bool IsReadOnlyAfterSave
        {
            get => AfterSaveBehavior == PropertySaveBehavior.Throw;
            set
            {
                if (value)
                {
                    AfterSaveBehavior = PropertySaveBehavior.Throw;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsConcurrencyToken
        {
            get => _isConcurrencyToken ?? DefaultIsConcurrencyToken;
            set => SetIsConcurrencyToken(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsConcurrencyToken(bool concurrencyToken, ConfigurationSource configurationSource)
        {
            if (IsConcurrencyToken != concurrencyToken)
            {
                _isConcurrencyToken = concurrencyToken;

                PropertyMetadataChanged();
            }
            UpdateIsConcurrencyTokenConfigurationSource(configurationSource);
        }

        private static bool DefaultIsConcurrencyToken => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsConcurrencyTokenConfigurationSource() => _isConcurrencyTokenConfigurationSource;

        private void UpdateIsConcurrencyTokenConfigurationSource(ConfigurationSource configurationSource)
            => _isConcurrencyTokenConfigurationSource = configurationSource.Max(_isConcurrencyTokenConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete("Use BeforeSaveBehavior or AfterSaveBehavior instead.")]
        public virtual bool IsStoreGeneratedAlways
        {
            get => AfterSaveBehavior == PropertySaveBehavior.Ignore || BeforeSaveBehavior == PropertySaveBehavior.Ignore;
            set
            {
                if (value)
                {
                    BeforeSaveBehavior = PropertySaveBehavior.Ignore;
                    AfterSaveBehavior = PropertySaveBehavior.Ignore;
                }
                else
                {
                    BeforeSaveBehavior = PropertySaveBehavior.Save;
                    AfterSaveBehavior = PropertySaveBehavior.Save;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetContainingForeignKeys()
            => ((IProperty)this).GetContainingForeignKeys().Cast<ForeignKey>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Key> GetContainingKeys()
            => ((IProperty)this).GetContainingKeys().Cast<Key>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetContainingIndexes()
            => ((IProperty)this).GetContainingIndexes().Cast<Index>();

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override Annotation OnAnnotationSet(string name, Annotation annotation, Annotation oldAnnotation)
            => DeclaringType.Model.ConventionDispatcher.OnPropertyAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string Format([NotNull] IEnumerable<IPropertyBase> properties, bool includeTypes = false)
            => "{"
               + string.Join(
                   ", ",
                   properties.Select(
                       p => "'" + p.Name + "'" + (includeTypes ? " : " + p.ClrType.DisplayName(fullName: false) : "")))
               + "}";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string Format([NotNull] IEnumerable<string> properties)
            => "{"
               + string.Join(
                   ", ",
                   properties.Select(p => string.IsNullOrEmpty(p) ? "" : "'" + p + "'"))
               + "}";

        IEntityType IProperty.DeclaringEntityType => DeclaringEntityType;
        ITypeBase IPropertyBase.DeclaringType => DeclaringType;
        IMutableEntityType IMutableProperty.DeclaringEntityType => DeclaringEntityType;
        IMutableTypeBase IMutablePropertyBase.DeclaringType => DeclaringType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool AreCompatible([NotNull] IReadOnlyList<Property> properties, [NotNull] EntityType entityType)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(entityType, nameof(entityType));

            return properties.All(
                property =>
                    property.IsShadowProperty
                    || (entityType.HasClrType()
                        && ((property.PropertyInfo != null
                             && entityType.ClrType.GetRuntimeProperties().FirstOrDefault(p => p.Name == property.Name) != null)
                            || (property.FieldInfo != null
                                && entityType.ClrType.GetFieldInfo(property.Name) != null))));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IKey PrimaryKey { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<IKey> Keys { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<IForeignKey> ForeignKeys { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual List<IIndex> Indexes { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string ToString() => this.ToDebugString();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DebugView<Property> DebugView
            => new DebugView<Property>(this, m => m.ToDebugString(false));
    }
}
