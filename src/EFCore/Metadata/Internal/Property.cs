// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Property : PropertyBase, IMutableProperty, IConventionProperty
    {
        private bool? _isConcurrencyToken;
        private bool? _isNullable;
        private ValueGenerated? _valueGenerated;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _typeConfigurationSource;
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
        public override TypeBase DeclaringType
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
        public virtual InternalPropertyBuilder Builder
        {
            [DebuggerStepThrough] get;
            [DebuggerStepThrough]
            [param: CanBeNull]
            set;
        }

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
        public virtual void SetIsNullable(bool? nullable, ConfigurationSource configurationSource)
        {
            var isChanging = (nullable ?? DefaultIsNullable) != IsNullable;
            if (nullable == null)
            {
                _isNullable = null;
                _isNullableConfigurationSource = null;
                if (isChanging)
                {
                    DeclaringEntityType.Model.ConventionDispatcher.OnPropertyNullableChanged(Builder);
                }

                return;
            }

            if (nullable.Value)
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
        public virtual void SetBeforeSaveBehavior(PropertySaveBehavior? beforeSaveBehavior, ConfigurationSource configurationSource)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.BeforeSaveBehavior, beforeSaveBehavior, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetAfterSaveBehavior(PropertySaveBehavior? afterSaveBehavior, ConfigurationSource configurationSource)
        {
            if (afterSaveBehavior != null)
            {
                var errorMessage = CheckAfterSaveBehavior(afterSaveBehavior.Value);
                if (errorMessage != null)
                {
                    throw new InvalidOperationException(errorMessage);
                }
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.AfterSaveBehavior, afterSaveBehavior, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string CheckAfterSaveBehavior(PropertySaveBehavior behavior)
        {
            if (behavior != PropertySaveBehavior.Throw
                && this.IsKey())
            {
                return CoreStrings.KeyPropertyMustBeReadOnly(Name, DeclaringEntityType.DisplayName());
            }

            return null;
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
        public virtual void SetIsConcurrencyToken(bool? concurrencyToken, ConfigurationSource configurationSource)
        {
            if (IsConcurrencyToken != concurrencyToken)
            {
                _isConcurrencyToken = concurrencyToken;

                PropertyMetadataChanged();
            }

            if (concurrencyToken == null)
            {
                _isConcurrencyTokenConfigurationSource = null;
            }
            else
            {
                UpdateIsConcurrencyTokenConfigurationSource(configurationSource);
            }
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
        public static string Format([NotNull] IEnumerable<string> properties)
            => "{"
               + string.Join(
                   ", ",
                   properties.Select(p => string.IsNullOrEmpty(p) ? "" : "'" + p + "'"))
               + "}";

        IEntityType IProperty.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableProperty.DeclaringEntityType => DeclaringEntityType;

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
                    property.IsShadowProperty()
                    || (entityType.HasClrType()
                        && ((property.PropertyInfo != null
                             && entityType.GetRuntimeProperties().ContainsKey(property.Name))
                            || (property.FieldInfo != null
                                && entityType.GetRuntimeFields().ContainsKey(property.Name)))));
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

        IConventionEntityType IConventionProperty.DeclaringEntityType => DeclaringEntityType;

        void IConventionProperty.SetIsNullable(bool? nullable, bool fromDataAnnotation)
            => SetIsNullable(
                nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        void IConventionProperty.SetValueGenerated(ValueGenerated? valueGenerated, bool fromDataAnnotation)
            => SetValueGenerated(
                valueGenerated, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        void IConventionProperty.IsConcurrencyToken(bool? concurrencyToken, bool fromDataAnnotation)
            => SetIsConcurrencyToken(
                concurrencyToken, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

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
