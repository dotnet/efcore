// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType DeclaringEntityType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TypeBase DeclaringType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type ClrType { [DebuggerStepThrough] get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Builder
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
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            var oldConfigurationSource = _configurationSource;
            _configurationSource = _configurationSource.Max(configurationSource);
            return _configurationSource != oldConfigurationSource;
        }

        // Needed for a workaround before reference counting is implemented
        // Issue #214
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetTypeConfigurationSource() => _typeConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateTypeConfigurationSource(ConfigurationSource configurationSource)
            => _typeConfigurationSource = _typeConfigurationSource.Max(configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsNullable
        {
            get => _isNullable ?? DefaultIsNullable;
            set => SetIsNullable(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                    throw new InvalidOperationException(
                        CoreStrings.CannotBeNullable(Name, DeclaringEntityType.DisplayName(), ClrType.ShortDisplayName()));
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsNullableConfigurationSource() => _isNullableConfigurationSource;

        private void UpdateIsNullableConfigurationSource(ConfigurationSource configurationSource)
            => _isNullableConfigurationSource = configurationSource.Max(_isNullableConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void OnFieldInfoSet(FieldInfo newFieldInfo, FieldInfo oldFieldInfo)
            => DeclaringEntityType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, newFieldInfo, oldFieldInfo);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ValueGenerated ValueGenerated
        {
            get => _valueGenerated ?? DefaultValueGenerated;
            set => SetValueGenerated(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetValueGeneratedConfigurationSource() => _valueGeneratedConfigurationSource;

        private void UpdateValueGeneratedConfigurationSource(ConfigurationSource configurationSource)
            => _valueGeneratedConfigurationSource = configurationSource.Max(_valueGeneratedConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsConcurrencyToken
        {
            get => _isConcurrencyToken ?? DefaultIsConcurrencyToken;
            set => SetIsConcurrencyToken(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetIsConcurrencyToken(bool? concurrencyToken, ConfigurationSource configurationSource)
        {
            if (IsConcurrencyToken != concurrencyToken)
            {
                _isConcurrencyToken = concurrencyToken;
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsConcurrencyTokenConfigurationSource() => _isConcurrencyTokenConfigurationSource;

        private void UpdateIsConcurrencyTokenConfigurationSource(ConfigurationSource configurationSource)
            => _isConcurrencyTokenConfigurationSource = configurationSource.Max(_isConcurrencyTokenConfigurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetMaxLength(int? maxLength, ConfigurationSource configurationSource)
        {
            if (maxLength != null
                && maxLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.MaxLength, maxLength, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetIsUnicode(bool? unicode, ConfigurationSource configurationSource)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.Unicode, unicode, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetBeforeSaveBehavior(PropertySaveBehavior? beforeSaveBehavior, ConfigurationSource configurationSource)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.BeforeSaveBehavior, beforeSaveBehavior, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetValueGeneratorFactory(
            [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> factory,
            ConfigurationSource configurationSource)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.ValueGeneratorFactory, factory, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetValueConverter([CanBeNull] ValueConverter converter, ConfigurationSource configurationSource)
        {
            var errorString = CheckValueConverter(converter);
            if (errorString != null)
            {
                throw new InvalidOperationException(errorString);
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.ValueConverter, converter, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string CheckValueConverter([CanBeNull] ValueConverter converter)
        {
            if (converter != null
                && converter.ModelClrType.UnwrapNullableType() != ClrType.UnwrapNullableType())
            {
                return CoreStrings.ConverterPropertyMismatch(
                    converter.ModelClrType.ShortDisplayName(),
                    DeclaringEntityType.DisplayName(),
                    Name,
                    ClrType.ShortDisplayName());
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetProviderClrType([CanBeNull] Type providerClrType, ConfigurationSource configurationSource)
            => this.SetOrRemoveAnnotation(CoreAnnotationNames.ProviderClrType, providerClrType, configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetValueComparer([CanBeNull] ValueComparer comparer, ConfigurationSource configurationSource)
        {
            var errorString = CheckValueComparer(comparer);
            if (errorString != null)
            {
                throw new InvalidOperationException(errorString);
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.ValueComparer, comparer, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetKeyValueComparer([CanBeNull] ValueComparer comparer, ConfigurationSource configurationSource)
        {
            var errorString = CheckValueComparer(comparer);
            if (errorString != null)
            {
                throw new InvalidOperationException(errorString);
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.KeyValueComparer, comparer, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetStructuralValueComparer([CanBeNull] ValueComparer comparer, ConfigurationSource configurationSource)
        {
            var errorString = CheckValueComparer(comparer);
            if (errorString != null)
            {
                throw new InvalidOperationException(errorString);
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.StructuralValueComparer, comparer, configurationSource);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string CheckValueComparer(ValueComparer comparer)
        {
            if (comparer != null
                && comparer.Type != ClrType)
            {
                return CoreStrings.ComparerPropertyMismatch(
                    comparer.Type.ShortDisplayName(),
                    DeclaringEntityType.DisplayName(),
                    Name,
                    ClrType.ShortDisplayName());
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetContainingForeignKeys()
            => ForeignKeys ?? Enumerable.Empty<ForeignKey>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Key> GetContainingKeys()
            => Keys ?? Enumerable.Empty<Key>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<Index> GetContainingIndexes()
            => Indexes ?? Enumerable.Empty<Index>();

        /// <summary>
        ///     Runs the conventions when an annotation was set or removed.
        /// </summary>
        /// <param name="name"> The key of the set annotation. </param>
        /// <param name="annotation"> The annotation set. </param>
        /// <param name="oldAnnotation"> The old annotation. </param>
        /// <returns> The annotation that was set. </returns>
        protected override IConventionAnnotation OnAnnotationSet(
            string name, IConventionAnnotation annotation, IConventionAnnotation oldAnnotation)
            => DeclaringType.Model.ConventionDispatcher.OnPropertyAnnotationChanged(Builder, name, annotation, oldAnnotation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string Format([NotNull] IEnumerable<string> properties)
            => "{"
               + string.Join(
                   ", ",
                   properties.Select(p => string.IsNullOrEmpty(p) ? "" : "'" + p + "'"))
               + "}";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IEntityType IProperty.DeclaringEntityType => DeclaringEntityType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableProperty.DeclaringEntityType => DeclaringEntityType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IKey PrimaryKey { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual List<Key> Keys { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual List<ForeignKey> ForeignKeys { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual List<Index> Indexes { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString() => this.ToDebugString();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView<Property> DebugView
            => new DebugView<Property>(this, m => m.ToDebugString(false));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionPropertyBuilder IConventionProperty.Builder
        {
            [DebuggerStepThrough] get => Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionEntityType IConventionProperty.DeclaringEntityType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionProperty.SetIsNullable(bool? nullable, bool fromDataAnnotation)
            => SetIsNullable(
                nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionProperty.SetValueGenerated(ValueGenerated? valueGenerated, bool fromDataAnnotation)
            => SetValueGenerated(
                valueGenerated, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionProperty.SetIsConcurrencyToken(bool? concurrencyToken, bool fromDataAnnotation)
            => SetIsConcurrencyToken(
                concurrencyToken, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
