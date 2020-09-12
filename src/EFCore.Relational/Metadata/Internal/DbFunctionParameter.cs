// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Builders.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DbFunctionParameter : ConventionAnnotatable, IMutableDbFunctionParameter, IConventionDbFunctionParameter
    {
        private string _storeType;
        private RelationalTypeMapping _typeMapping;
        private bool _propagatesNullability;

        private ConfigurationSource? _storeTypeConfigurationSource;
        private ConfigurationSource? _typeMappingConfigurationSource;
        private ConfigurationSource? _propagatesNullabilityConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DbFunctionParameter(
            [NotNull] DbFunction function,
            [NotNull] string name,
            [NotNull] Type clrType)
        {
            Check.NotNull(function, nameof(function));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));

            Name = name;
            Function = function;
            ClrType = clrType;
            Builder = new InternalDbFunctionParameterBuilder(this, function.Builder.ModelBuilder);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalDbFunctionParameterBuilder Builder { get; }

        /// <inheritdoc />
        IConventionDbFunctionParameterBuilder IConventionDbFunctionParameter.Builder
            => Builder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DbFunction Function { get; }

        /// <inheritdoc />
        IConventionDbFunction IConventionDbFunctionParameter.Function
        {
            [DebuggerStepThrough]
            get => Function;
        }

        /// <inheritdoc />
        IDbFunction IDbFunctionParameter.Function
        {
            [DebuggerStepThrough]
            get => Function;
        }

        /// <inheritdoc />
        IMutableDbFunction IMutableDbFunctionParameter.Function
        {
            [DebuggerStepThrough]
            get => Function;
        }

        /// <inheritdoc />
        public virtual string Name { get; }

        /// <inheritdoc />
        public virtual Type ClrType { get; }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public virtual ConfigurationSource GetConfigurationSource()
            => Function.GetConfigurationSource();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string StoreType
        {
            get => _storeType ?? TypeMapping?.StoreType;
            set => SetStoreType(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string SetStoreType([CanBeNull] string storeType, ConfigurationSource configurationSource)
        {
            _storeType = storeType;

            _storeTypeConfigurationSource = configurationSource.Max(_storeTypeConfigurationSource);

            return storeType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetStoreTypeConfigurationSource()
            => _storeTypeConfigurationSource;

        /// <inheritdoc />
        [DebuggerStepThrough]
        string IConventionDbFunctionParameter.SetStoreType(string storeType, bool fromDataAnnotation)
            => SetStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual RelationalTypeMapping TypeMapping
        {
            get => _typeMapping;
            set => SetTypeMapping(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual RelationalTypeMapping SetTypeMapping(
            [NotNull] RelationalTypeMapping typeMapping,
            ConfigurationSource configurationSource)
        {
            _typeMapping = typeMapping;
            _typeMappingConfigurationSource = configurationSource.Max(_typeMappingConfigurationSource);

            return typeMapping;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool PropagatesNullability
        {
            get => _propagatesNullability;
            set => SetPropagatesNullability(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool SetPropagatesNullability(bool propagatesNullability, ConfigurationSource configurationSource)
        {
            if (!Function.IsScalar)
            {
                new InvalidOperationException(RelationalStrings.NonScalarFunctionParameterCannotPropagatesNullability(Name, Function.Name));
            }

            _propagatesNullability = propagatesNullability;
            _propagatesNullabilityConfigurationSource = configurationSource.Max(_storeTypeConfigurationSource);

            return propagatesNullability;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetPropagatesNullabilityConfigurationSource()
            => _propagatesNullabilityConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetTypeMappingConfigurationSource()
            => _typeMappingConfigurationSource;

        /// <inheritdoc />
        [DebuggerStepThrough]
        RelationalTypeMapping IConventionDbFunctionParameter.SetTypeMapping(RelationalTypeMapping typeMapping, bool fromDataAnnotation)
            => SetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        public virtual IStoreFunctionParameter StoreFunctionParameter { get; [param: NotNull] set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => this.ToDebugString(MetadataDebugStringOptions.SingleLineDefault);
    }
}
