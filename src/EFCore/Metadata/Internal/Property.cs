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
using Microsoft.EntityFrameworkCore.Storage;
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
        private CoreTypeMapping _typeMapping;

        private ConfigurationSource? _typeConfigurationSource;
        private ConfigurationSource? _isNullableConfigurationSource;
        private ConfigurationSource? _isConcurrencyTokenConfigurationSource;
        private ConfigurationSource? _valueGeneratedConfigurationSource;
        private ConfigurationSource? _typeMappingConfigurationSource;

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
            : base(name, propertyInfo, fieldInfo, configurationSource)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DeclaringEntityType = declaringEntityType;
            ClrType = clrType;
            _typeConfigurationSource = typeConfigurationSource;

            Builder = new InternalPropertyBuilder(this, declaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override TypeBase DeclaringType
        {
            [DebuggerStepThrough]
            get => DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type ClrType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalPropertyBuilder Builder { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetTypeConfigurationSource()
            => _typeConfigurationSource;

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
        public virtual bool? SetIsNullable(bool? nullable, ConfigurationSource configurationSource)
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

                return nullable;
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

            _isNullableConfigurationSource = configurationSource.Max(_isNullableConfigurationSource);

            _isNullable = nullable;

            if (isChanging)
            {
                return DeclaringEntityType.Model.ConventionDispatcher.OnPropertyNullableChanged(Builder);
            }

            return nullable;
        }

        private bool DefaultIsNullable
            => ClrType.IsNullableType();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsNullableConfigurationSource()
            => _isNullableConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override FieldInfo OnFieldInfoSet(FieldInfo newFieldInfo, FieldInfo oldFieldInfo)
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
        public virtual ValueGenerated? SetValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            _valueGenerated = valueGenerated;

            _valueGeneratedConfigurationSource = valueGenerated == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_valueGeneratedConfigurationSource);

            return valueGenerated;
        }

        private static ValueGenerated DefaultValueGenerated
            => ValueGenerated.Never;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetValueGeneratedConfigurationSource()
            => _valueGeneratedConfigurationSource;

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
        public virtual bool? SetIsConcurrencyToken(bool? concurrencyToken, ConfigurationSource configurationSource)
        {
            if (IsConcurrencyToken != concurrencyToken)
            {
                _isConcurrencyToken = concurrencyToken;
            }

            _isConcurrencyTokenConfigurationSource = concurrencyToken == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_isConcurrencyTokenConfigurationSource);

            return concurrencyToken;
        }

        private static bool DefaultIsConcurrencyToken
            => false;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsConcurrencyTokenConfigurationSource()
            => _isConcurrencyTokenConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int? SetMaxLength(int? maxLength, ConfigurationSource configurationSource)
        {
            if (maxLength != null
                && maxLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.MaxLength, maxLength, configurationSource);

            return maxLength;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool? SetIsUnicode(bool? unicode, ConfigurationSource configurationSource)
        {
            this.SetOrRemoveAnnotation(CoreAnnotationNames.Unicode, unicode, configurationSource);

            return unicode;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int? SetPrecision(int? precision, ConfigurationSource configurationSource)
        {
            if (precision != null && precision < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(precision));
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.Precision, precision, configurationSource);

            return precision;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int? SetScale(int? scale, ConfigurationSource configurationSource)
        {
            if (scale != null && scale < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scale));
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.Scale, scale, configurationSource);

            return scale;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertySaveBehavior? SetBeforeSaveBehavior(
            PropertySaveBehavior? beforeSaveBehavior,
            ConfigurationSource configurationSource)
        {
            this.SetOrRemoveAnnotation(CoreAnnotationNames.BeforeSaveBehavior, beforeSaveBehavior, configurationSource);

            return beforeSaveBehavior;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual PropertySaveBehavior? SetAfterSaveBehavior(
            PropertySaveBehavior? afterSaveBehavior,
            ConfigurationSource configurationSource)
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

            return afterSaveBehavior;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string CheckAfterSaveBehavior(PropertySaveBehavior behavior)
            => behavior != PropertySaveBehavior.Throw
                && this.IsKey()
                    ? CoreStrings.KeyPropertyMustBeReadOnly(Name, DeclaringEntityType.DisplayName())
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<IProperty, IEntityType, ValueGenerator> SetValueGeneratorFactory(
            [CanBeNull] Func<IProperty, IEntityType, ValueGenerator> factory,
            ConfigurationSource configurationSource)
        {
            SetAnnotation(CoreAnnotationNames.ValueGeneratorFactory, factory, configurationSource);

            return factory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ValueConverter SetValueConverter(
            [CanBeNull] ValueConverter converter,
            ConfigurationSource configurationSource)
        {
            var errorString = CheckValueConverter(converter);
            if (errorString != null)
            {
                throw new InvalidOperationException(errorString);
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.ValueConverter, converter, configurationSource);

            return converter;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string CheckValueConverter([CanBeNull] ValueConverter converter)
            => converter != null
                && converter.ModelClrType.UnwrapNullableType() != ClrType.UnwrapNullableType()
                    ? CoreStrings.ConverterPropertyMismatch(
                        converter.ModelClrType.ShortDisplayName(),
                        DeclaringEntityType.DisplayName(),
                        Name,
                        ClrType.ShortDisplayName())
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type SetProviderClrType([CanBeNull] Type providerClrType, ConfigurationSource configurationSource)
        {
            this.SetOrRemoveAnnotation(CoreAnnotationNames.ProviderClrType, providerClrType, configurationSource);

            return providerClrType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual CoreTypeMapping TypeMapping
        {
            get => _typeMapping;
            [param: NotNull]
            set => SetTypeMapping(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual CoreTypeMapping SetTypeMapping([NotNull] CoreTypeMapping typeMapping, ConfigurationSource configurationSource)
        {
            _typeMapping = typeMapping;
            _typeMappingConfigurationSource = typeMapping == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_typeMappingConfigurationSource);

            return typeMapping;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetTypeMappingConfigurationSource()
            => _typeMappingConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ValueComparer SetValueComparer([CanBeNull] ValueComparer comparer, ConfigurationSource configurationSource)
        {
            var errorString = CheckValueComparer(comparer);
            if (errorString != null)
            {
                throw new InvalidOperationException(errorString);
            }

            this.SetOrRemoveAnnotation(CoreAnnotationNames.ValueComparer, comparer, configurationSource);

            return comparer;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string CheckValueComparer([CanBeNull] ValueComparer comparer)
            => comparer != null
                && comparer.Type != ClrType
                    ? CoreStrings.ComparerPropertyMismatch(
                        comparer.Type.ShortDisplayName(),
                        DeclaringEntityType.DisplayName(),
                        Name,
                        ClrType.ShortDisplayName())
                    : null;

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
        public virtual IEnumerable<Key> GetContainingKeys()
            => Keys ?? Enumerable.Empty<Key>();

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
        public virtual IEnumerable<ForeignKey> GetContainingForeignKeys()
            => ForeignKeys ?? Enumerable.Empty<ForeignKey>();

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
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation)
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
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DebugView DebugView
            => new DebugView(
                () => this.ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => this.ToDebugString(MetadataDebugStringOptions.LongDefault));

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
        IEntityType IProperty.DeclaringEntityType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IMutableEntityType IMutableProperty.DeclaringEntityType
        {
            [DebuggerStepThrough] get => DeclaringEntityType;
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
        bool? IConventionProperty.SetIsNullable(bool? nullable, bool fromDataAnnotation)
            => SetIsNullable(
                nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        ValueGenerated? IConventionProperty.SetValueGenerated(ValueGenerated? valueGenerated, bool fromDataAnnotation)
            => SetValueGenerated(
                valueGenerated, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool? IConventionProperty.SetIsConcurrencyToken(bool? concurrencyToken, bool fromDataAnnotation)
            => SetIsConcurrencyToken(
                concurrencyToken, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
