// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

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
    private IStoreFunction? _storeFunction;

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

        ModelName = GetFunctionName(methodInfo);
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
            ? []
            : parameters
                .Select(p => new DbFunctionParameter(this, p.Name, p.Type))
                .ToList();

        if (IsScalar)
        {
            _nullable = true;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string GetFunctionName(MethodInfo methodInfo)
    {
        var parameters = methodInfo.GetParameters();
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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IMutableModel Model { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalDbFunctionBuilder Builder
    {
        [DebuggerStepThrough]
        get => _builder ?? throw new InvalidOperationException(CoreStrings.ObjectRemovedFromModel(ModelName));
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
    public override bool IsReadOnly
        => ((Annotatable)Model).IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IEnumerable<IDbFunction> GetDbFunctions(IReadOnlyModel model)
        => ((Dictionary<string, IDbFunction>?)model[RelationalAnnotationNames.DbFunctions])
            ?.OrderBy(t => t.Key).Select(t => t.Value)
            ?? Enumerable.Empty<IDbFunction>();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IDbFunction? FindDbFunction(IReadOnlyModel model, MethodInfo methodInfo)
        => model[RelationalAnnotationNames.DbFunctions] is Dictionary<string, IDbFunction> functions
            && functions.TryGetValue(GetFunctionName(methodInfo), out var dbFunction)
                ? dbFunction
                : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IDbFunction? FindDbFunction(IReadOnlyModel model, string name)
        => model[RelationalAnnotationNames.DbFunctions] is Dictionary<string, IDbFunction> functions
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

    private static Dictionary<string, IDbFunction> GetOrCreateFunctions(IMutableModel model)
        => (Dictionary<string, IDbFunction>)(
            model[RelationalAnnotationNames.DbFunctions] ??= new Dictionary<string, IDbFunction>(StringComparer.Ordinal));

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
        if (model[RelationalAnnotationNames.DbFunctions] is Dictionary<string, IDbFunction> functions)
        {
            var name = GetFunctionName(methodInfo);
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
        if (model[RelationalAnnotationNames.DbFunctions] is Dictionary<string, IDbFunction> functions
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

        _schemaConfigurationSource = configurationSource.Max(_schemaConfigurationSource);

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
        EnsureMutable();

        _name = name;

        _nameConfigurationSource = configurationSource.Max(_nameConfigurationSource);

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
            ? null
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
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _typeMapping, this, static dbFunction =>
                {
                    var relationalTypeMappingSource =
                        (IRelationalTypeMappingSource)((IModel)dbFunction.Model).GetModelDependencies().TypeMappingSource;
                    return !string.IsNullOrEmpty(dbFunction._storeType)
                        ? relationalTypeMappingSource.FindMapping(dbFunction.ReturnType, dbFunction._storeType)!
                        : relationalTypeMappingSource.FindMapping(dbFunction.ReturnType, (IModel)dbFunction.Model)!;
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
            ? null
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
            ? null
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
    public virtual DbFunctionParameter? FindParameter(string name)
        => Parameters.SingleOrDefault(p => p.Name == name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IDbFunction)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IDbFunction)this).ToDebugString(),
            () => ((IDbFunction)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

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
        => _storeFunction!; // Relational model creation ensures StoreFunction is populated

    IStoreFunction IRuntimeDbFunction.StoreFunction
    {
        get => _storeFunction!;
        set => _storeFunction = value;
    }
}
