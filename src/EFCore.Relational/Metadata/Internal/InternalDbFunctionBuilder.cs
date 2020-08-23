// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalDbFunctionBuilder : AnnotatableBuilder<DbFunction, IConventionModelBuilder>, IConventionDbFunctionBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalDbFunctionBuilder([NotNull] DbFunction function, [NotNull] IConventionModelBuilder modelBuilder)
            : base(function, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionDbFunctionBuilder HasName([CanBeNull] string name, ConfigurationSource configurationSource)
        {
            if (CanSetName(name, configurationSource))
            {
                Metadata.SetName(name, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetName([CanBeNull] string name, ConfigurationSource configurationSource)
            => (name != "" || configurationSource == ConfigurationSource.Explicit)
                && (configurationSource.Overrides(Metadata.GetNameConfigurationSource())
                    || Metadata.Name == name);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionDbFunctionBuilder HasSchema([CanBeNull] string schema, ConfigurationSource configurationSource)
        {
            if (CanSetSchema(schema, configurationSource))
            {
                Metadata.SetSchema(schema, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetSchema([CanBeNull] string schema, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetSchemaConfigurationSource())
                || Metadata.Schema == schema;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionDbFunctionBuilder IsBuiltIn(bool builtIn, ConfigurationSource configurationSource)
        {
            if (CanSetIsBuiltIn(builtIn, configurationSource))
            {
                Metadata.SetIsBuiltIn(builtIn, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsBuiltIn(bool builtIn, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetIsBuiltInConfigurationSource())
                || Metadata.IsBuiltIn == builtIn;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionDbFunctionBuilder IsNullable(bool nullable, ConfigurationSource configurationSource)
        {
            if (CanSetIsNullable(nullable, configurationSource))
            {
                Metadata.SetIsNullable(nullable, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsNullable(bool nullable, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetIsNullableConfigurationSource())
                || Metadata.IsNullable == nullable;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionDbFunctionBuilder HasStoreType([CanBeNull] string storeType, ConfigurationSource configurationSource)
        {
            if (CanSetStoreType(storeType, configurationSource))
            {
                Metadata.SetStoreType(storeType, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetStoreType([CanBeNull] string storeType, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetStoreTypeConfigurationSource())
                || Metadata.StoreType == storeType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionDbFunctionBuilder HasTypeMapping(
            [CanBeNull] RelationalTypeMapping returnTypeMapping,
            ConfigurationSource configurationSource)
        {
            if (CanSetTypeMapping(returnTypeMapping, configurationSource))
            {
                Metadata.SetTypeMapping(returnTypeMapping, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetTypeMapping([CanBeNull] RelationalTypeMapping returnTypeMapping, ConfigurationSource configurationSource)
            => configurationSource.Overrides(Metadata.GetTypeMappingConfigurationSource())
                || Metadata.TypeMapping == returnTypeMapping;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IConventionDbFunctionBuilder HasTranslation(
            [CanBeNull] Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation,
            ConfigurationSource configurationSource)
        {
            if (CanSetTranslation(translation, configurationSource))
            {
                Metadata.SetTranslation(translation, configurationSource);
                return this;
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetTranslation(
            [CanBeNull] Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation,
            ConfigurationSource configurationSource)
            => (Metadata.IsScalar && !Metadata.IsAggregate || configurationSource == ConfigurationSource.Explicit)
                && (configurationSource.Overrides(Metadata.GetTranslationConfigurationSource())
                    || Metadata.Translation == translation);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalDbFunctionParameterBuilder HasParameter([NotNull] string name, ConfigurationSource configurationSource)
        {
            var parameter = Metadata.FindParameter(name);
            if (parameter == null)
            {
                throw new ArgumentException(
                    RelationalStrings.DbFunctionInvalidParameterName(name, Metadata.MethodInfo.DisplayName()));
            }

            return parameter.Builder;
        }

        IConventionDbFunction IConventionDbFunctionBuilder.Metadata
        {
            [DebuggerStepThrough]
            get => Metadata;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasName(string name, bool fromDataAnnotation)
            => HasName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunctionBuilder.CanSetName(string name, bool fromDataAnnotation)
            => CanSetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasSchema(string schema, bool fromDataAnnotation)
            => HasSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunctionBuilder.CanSetSchema(string schema, bool fromDataAnnotation)
            => CanSetSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.IsBuiltIn(bool builtIn, bool fromDataAnnotation)
            => IsBuiltIn(builtIn, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunctionBuilder.CanSetIsBuiltIn(bool builtIn, bool fromDataAnnotation)
            => CanSetIsBuiltIn(builtIn, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.IsNullable(bool nullable, bool fromDataAnnotation)
            => IsNullable(nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunctionBuilder.CanSetIsNullable(bool nullable, bool fromDataAnnotation)
            => CanSetIsNullable(nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasStoreType(string storeType, bool fromDataAnnotation)
            => HasStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunctionBuilder.CanSetStoreType(string storeType, bool fromDataAnnotation)
            => CanSetStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasTypeMapping(RelationalTypeMapping typeMapping, bool fromDataAnnotation)
            => HasTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunctionBuilder.CanSetTypeMapping(RelationalTypeMapping typeMapping, bool fromDataAnnotation)
            => CanSetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionDbFunctionBuilder IConventionDbFunctionBuilder.HasTranslation(
            Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation,
            bool fromDataAnnotation)
            => HasTranslation(translation, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunctionBuilder.CanSetTranslation(
            Func<IReadOnlyCollection<SqlExpression>, SqlExpression> translation,
            bool fromDataAnnotation)
            => CanSetTranslation(translation, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        IConventionDbFunctionParameterBuilder IConventionDbFunctionBuilder.HasParameter(string name, bool fromDataAnnotation)
            => HasParameter(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
