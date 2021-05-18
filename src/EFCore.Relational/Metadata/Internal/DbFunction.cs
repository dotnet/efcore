// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Builders.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
    public class DbFunction : ConventionAnnotatable, IMutableDbFunction, IConventionDbFunction, IRuntimeDbFunction
    {
        private readonly List<DbFunctionParameter> _parameters;
        private string? _schema;
        private string? _name;
        private bool _builtIn;
        private bool _nullable;
        private string? _storeType;
        private RelationalTypeMapping? _typeMapping;
        private Func<IReadOnlyList<SqlExpression>, SqlExpression>? _translation;
        private InternalDbFunctionBuilder? _builder;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _schemaConfigurationSource;
        private ConfigurationSource? _nameConfigurationSource;
        private ConfigurationSource? _builtInConfigurationSource;
        private ConfigurationSource? _nullableConfigurationSource;
        private ConfigurationSource? _storeTypeConfigurationSource;
        private ConfigurationSource? _typeMappingConfigurationSource;
        private ConfigurationSource? _translationConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DbFunction(
            MethodInfo methodInfo,
            IMutableModel model,
            ConfigurationSource configurationSource)
            : this(
                methodInfo.Name,
                methodInfo.ReturnType,
                methodInfo.GetParameters().Select(pi => (pi.Name!, pi.ParameterType)),
                model,
                configurationSource)
        {
            if (methodInfo.IsGenericMethod)
            {
                throw new ArgumentException(RelationalStrings.DbFunctionGenericMethodNotSupported(methodInfo.DisplayName()));
            }

            if (!methodInfo.IsStatic
                && !typeof(DbContext).IsAssignableFrom(methodInfo.DeclaringType))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                throw new ArgumentException(
                    RelationalStrings.DbFunctionInvalidInstanceType(
                        methodInfo.DisplayName(), methodInfo.DeclaringType!.ShortDisplayName()));
            }

            MethodInfo = methodInfo;

            ModelName = GetFunctionName(methodInfo, methodInfo.GetParameters());
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DbFunction(
            string name,
            Type returnType,
            IEnumerable<(string Name, Type Type)>? parameters,
            IMutableModel model,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));

            if (returnType == null
                || returnType == typeof(void))
            {
                throw new ArgumentException(
                    RelationalStrings.DbFunctionInvalidReturnType(name, returnType?.ShortDisplayName()));
            }

            IsScalar = !returnType.IsGenericType
                || returnType.GetGenericTypeDefinition() != typeof(IQueryable<>);
            IsAggregate = false;

            ModelName = name;
            ReturnType = returnType;
            Model = model;
            _configurationSource = configurationSource;
            _builder = new InternalDbFunctionBuilder(this, ((IConventionModel)model).Builder);
            _parameters = parameters == null
                ? new List<DbFunctionParameter>()
                : parameters
                    .Select(p => new DbFunctionParameter(this, p.Name, p.Type))
                    .ToList();

            if (IsScalar)
            {
                _nullable = true;
            }
        }

        private static string GetFunctionName(MethodInfo methodInfo, ParameterInfo[] parameters)
        {
            var builder = new StringBuilder();

            if (methodInfo.DeclaringType != null)
            {
                builder
                    .Append(methodInfo.DeclaringType.DisplayName())
                    .Append('.');
            }

            builder
                .Append(methodInfo.Name)
                .Append('(')
                .AppendJoin(',', parameters.Select(p => p.ParameterType.DisplayName()))
                .Append(')');

            return builder.ToString();
        }

        /// <inheritdoc />
        public virtual IMutableModel Model { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalDbFunctionBuilder Builder
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
        ///     Indicates whether the function is read-only.
        /// </summary>
        public override bool IsReadOnly => ((Annotatable)Model).IsReadOnly;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IDbFunction> GetDbFunctions(IReadOnlyModel model)
            => ((SortedDictionary<string, IDbFunction>?)model[RelationalAnnotationNames.DbFunctions])
                ?.Values
                ?? Enumerable.Empty<IDbFunction>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyDbFunction? FindDbFunction(IReadOnlyModel model, MethodInfo methodInfo)
            => model[RelationalAnnotationNames.DbFunctions] is SortedDictionary<string, IDbFunction> functions
                && functions.TryGetValue(GetFunctionName(methodInfo, methodInfo.GetParameters()), out var dbFunction)
                    ? dbFunction
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyDbFunction? FindDbFunction(IReadOnlyModel model, string name)
            => model[RelationalAnnotationNames.DbFunctions] is SortedDictionary<string, IDbFunction> functions
                && functions.TryGetValue(name, out var dbFunction)
                    ? dbFunction
                    : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static DbFunction AddDbFunction(
            IMutableModel model,
            MethodInfo methodInfo,
            ConfigurationSource configurationSource)
        {
            var function = new DbFunction(methodInfo, model, configurationSource);

            GetOrCreateFunctions(model).Add(function.ModelName, function);
            return function;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static DbFunction AddDbFunction(
            IMutableModel model,
            string name,
            Type returnType,
            ConfigurationSource configurationSource)
        {
            var function = new DbFunction(name, returnType, null, model, configurationSource);

            GetOrCreateFunctions(model).Add(name, function);
            return function;
        }

        private static SortedDictionary<string, IDbFunction> GetOrCreateFunctions(IMutableModel model)
            => (SortedDictionary<string, IDbFunction>)(
                model[RelationalAnnotationNames.DbFunctions] ??= new SortedDictionary<string, IDbFunction>());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static DbFunction? RemoveDbFunction(
            IMutableModel model,
            MethodInfo methodInfo)
        {
            if (model[RelationalAnnotationNames.DbFunctions] is SortedDictionary<string, IDbFunction> functions)
            {
                var name = GetFunctionName(methodInfo, methodInfo.GetParameters());
                if (functions.TryGetValue(name, out var function))
                {
                    var dbFunction = (DbFunction)function;
                    functions.Remove(name);
                    dbFunction.SetRemovedFromModel();

                    return dbFunction;
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
        public static DbFunction? RemoveDbFunction(
            IMutableModel model,
            string name)
        {
            if (model[RelationalAnnotationNames.DbFunctions] is SortedDictionary<string, IDbFunction> functions
                && functions.TryGetValue(name, out var function))
            {
                functions.Remove(name);
                ((DbFunction)function).SetRemovedFromModel();
            }

            return null;
        }

        /// <inheritdoc />
        public virtual string ModelName { get; }

        /// <inheritdoc />
        public virtual MethodInfo? MethodInfo { get; }

        /// <inheritdoc />
        public virtual Type ReturnType { get; }

        /// <inheritdoc />
        public virtual bool IsScalar { get; }

        /// <inheritdoc />
        public virtual bool IsAggregate { get; }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public virtual ConfigurationSource GetConfigurationSource()
            => _configurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = configurationSource.Max(_configurationSource);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? Schema
        {
            get => _schema ?? Model.GetDefaultSchema();
            set => SetSchema(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? SetSchema(string? schema, ConfigurationSource configurationSource)
        {
            EnsureMutable();

            _schema = schema;

            _schemaConfigurationSource = schema == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_schemaConfigurationSource);

            return schema;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetSchemaConfigurationSource()
            => _schemaConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Name
        {
            get => _name ?? MethodInfo?.Name ?? ModelName;
            set => SetName(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? SetName(string? name, ConfigurationSource configurationSource)
        {
            Check.NullButNotEmpty(name, nameof(name));

            EnsureMutable();

            _name = name;

            _nameConfigurationSource = name == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_nameConfigurationSource);

            return name;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetNameConfigurationSource()
            => _nameConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsBuiltIn
        {
            get => _builtIn;
            set => SetIsBuiltIn(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool SetIsBuiltIn(bool builtIn, ConfigurationSource configurationSource)
        {
            EnsureMutable();

            _builtIn = builtIn;
            _builtInConfigurationSource = configurationSource.Max(_builtInConfigurationSource);

            return builtIn;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsBuiltInConfigurationSource()
            => _builtInConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsNullable
        {
            get => _nullable;
            set => SetIsNullable(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool SetIsNullable(bool nullable, ConfigurationSource configurationSource)
        {
            EnsureMutable();

            if (!IsScalar)
            {
                throw new InvalidOperationException(RelationalStrings.NonScalarFunctionCannotBeNullable(Name));
            }

            _nullable = nullable;
            _nullableConfigurationSource = configurationSource.Max(_nullableConfigurationSource);

            return nullable;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsNullableConfigurationSource()
            => _nullableConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? StoreType
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
        public virtual string? SetStoreType(string? storeType, ConfigurationSource configurationSource)
        {
            EnsureMutable();

            _storeType = storeType;

            _storeTypeConfigurationSource = storeType == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_storeTypeConfigurationSource);

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual RelationalTypeMapping? TypeMapping
        {
            get => IsReadOnly && IsScalar
                    ? NonCapturingLazyInitializer.EnsureInitialized(ref _typeMapping, this, static dbFunction =>
                        {
                            var relationalTypeMappingSource =
                                (IRelationalTypeMappingSource)((IModel)dbFunction.Model).GetModelDependencies().TypeMappingSource;
                            return !string.IsNullOrEmpty(dbFunction._storeType)
                                        ? relationalTypeMappingSource.FindMapping(dbFunction._storeType)!
                                        : relationalTypeMappingSource.FindMapping(dbFunction.ReturnType)!;
                        })
                    : _typeMapping;
            set => SetTypeMapping(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual RelationalTypeMapping? SetTypeMapping(
            RelationalTypeMapping? typeMapping,
            ConfigurationSource configurationSource)
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
        public virtual Func<IReadOnlyList<SqlExpression>, SqlExpression>? Translation
        {
            get => _translation;
            set => SetTranslation(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<IReadOnlyList<SqlExpression>, SqlExpression>? SetTranslation(
            Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
            ConfigurationSource configurationSource)
        {
            EnsureMutable();

            if (translation != null
                && (!IsScalar || IsAggregate))
            {
                throw new InvalidOperationException(RelationalStrings.DbFunctionNonScalarCustomTranslation(MethodInfo?.DisplayName()));
            }

            _translation = translation;

            _translationConfigurationSource = translation == null
                ? (ConfigurationSource?)null
                : configurationSource.Max(_translationConfigurationSource);

            return translation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetTranslationConfigurationSource()
            => _translationConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyList<DbFunctionParameter> Parameters
        {
            [DebuggerStepThrough]
            get => _parameters;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DisallowNull]
        public virtual IStoreFunction? StoreFunction { get; set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString()
            => ((IDbFunction)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <inheritdoc />
        IConventionDbFunctionBuilder IConventionDbFunction.Builder
        {
            [DebuggerStepThrough]
            get => Builder;
        }

        /// <inheritdoc />
        IReadOnlyModel IReadOnlyDbFunction.Model
        {
            [DebuggerStepThrough]
            get => Model;
        }

        /// <inheritdoc />
        IConventionModel IConventionDbFunction.Model
        {
            [DebuggerStepThrough]
            get => (IConventionModel)Model;
        }

        /// <inheritdoc />
        IModel IDbFunction.Model
        {
            [DebuggerStepThrough]
            get => (IModel)Model;
        }

        /// <inheritdoc />
        IReadOnlyList<IReadOnlyDbFunctionParameter> IReadOnlyDbFunction.Parameters
        {
            [DebuggerStepThrough]
            get => _parameters;
        }

        /// <inheritdoc />
        IReadOnlyList<IConventionDbFunctionParameter> IConventionDbFunction.Parameters
        {
            [DebuggerStepThrough]
            get => _parameters;
        }

        /// <inheritdoc />
        IReadOnlyList<IMutableDbFunctionParameter> IMutableDbFunction.Parameters
        {
            [DebuggerStepThrough]
            get => _parameters;
        }

        /// <inheritdoc />
        IReadOnlyList<IDbFunctionParameter> IDbFunction.Parameters
        {
            [DebuggerStepThrough]
            get => _parameters;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DbFunctionParameter? FindParameter(string name)
            => Parameters.SingleOrDefault(p => p.Name == name);

        /// <inheritdoc />
        [DebuggerStepThrough]
        string? IConventionDbFunction.SetName(string? name, bool fromDataAnnotation)
            => SetName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        string? IConventionDbFunction.SetSchema(string? schema, bool fromDataAnnotation)
            => SetSchema(schema, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunction.SetIsBuiltIn(bool builtIn, bool fromDataAnnotation)
            => SetIsBuiltIn(builtIn, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        bool IConventionDbFunction.SetIsNullable(bool nullable, bool fromDataAnnotation)
            => SetIsNullable(nullable, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        string? IConventionDbFunction.SetStoreType(string? storeType, bool fromDataAnnotation)
            => SetStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        RelationalTypeMapping? IConventionDbFunction.SetTypeMapping(RelationalTypeMapping? returnTypeMapping, bool fromDataAnnotation)
            => SetTypeMapping(returnTypeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        [DebuggerStepThrough]
        Func<IReadOnlyList<SqlExpression>, SqlExpression>? IConventionDbFunction.SetTranslation(
            Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation,
            bool fromDataAnnotation)
            => SetTranslation(translation, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        IStoreFunction IDbFunction.StoreFunction
            => StoreFunction!; // Relational model creation ensures StoreFunction is populated

        IStoreFunction IRuntimeDbFunction.StoreFunction
        {
            get => StoreFunction!;
            set => StoreFunction = value;
        }
    }
}
