// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a relational database function in a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public class RuntimeDbFunction : AnnotatableBase, IRuntimeDbFunction
{
    private readonly List<RuntimeDbFunctionParameter> _parameters = [];
    private readonly MethodInfo? _methodInfo;
    private readonly Type _returnType;
    private readonly bool _isScalar;
    private readonly bool _isAggregate;
    private readonly bool _isNullable;
    private readonly bool _isBuiltIn;
    private readonly string _storeName;
    private readonly string? _schema;
    private readonly string? _storeType;
    private readonly Func<IReadOnlyList<SqlExpression>, SqlExpression>? _translation;
    private RelationalTypeMapping? _typeMapping;
    private IStoreFunction? _storeFunction;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RuntimeDbFunction" /> class.
    /// </summary>
    /// <param name="modelName">The model name.</param>
    /// <param name="model">The model.</param>
    /// <param name="returnType">The return type.</param>
    /// <param name="storeName">The store name.</param>
    /// <param name="schema">The store schema.</param>
    /// <param name="storeType">The store type.</param>
    /// <param name="methodInfo">The mapped <see cref="MethodInfo" />.</param>
    /// <param name="scalar">Whether the return type is scalar.</param>
    /// <param name="aggregate">Whether the function is an aggregate.</param>
    /// <param name="nullable">Whether the function is nullable.</param>
    /// <param name="builtIn">Whether the function is built-in.</param>
    /// <param name="typeMapping">The type mapping for the return value.</param>
    /// <param name="translation">The function translation.</param>
    public RuntimeDbFunction(
        string modelName,
        RuntimeModel model,
        Type returnType,
        string storeName,
        string? schema = null,
        string? storeType = null,
        MethodInfo? methodInfo = null,
        bool scalar = false,
        bool aggregate = false,
        bool nullable = false,
        bool builtIn = false,
        RelationalTypeMapping? typeMapping = null,
        Func<IReadOnlyList<SqlExpression>, SqlExpression>? translation = null)
    {
        ModelName = modelName;
        Model = model;
        _returnType = returnType;
        _isScalar = scalar;
        _isAggregate = aggregate;
        _isNullable = nullable;
        _isBuiltIn = builtIn;
        _storeName = storeName;
        _schema = schema;
        _storeType = storeType;
        _methodInfo = methodInfo;
        _typeMapping = typeMapping;
        _translation = translation;
    }

    /// <summary>
    ///     Gets the model in which this function is defined.
    /// </summary>
    public virtual RuntimeModel Model { get; }

    /// <summary>
    ///     Gets the name of the function in the model.
    /// </summary>
    public virtual string ModelName { get; }

    /// <summary>
    ///     Gets or sets the type mapping for the function's return type.
    /// </summary>
    /// <returns>The type mapping.</returns>
    public virtual RelationalTypeMapping? TypeMapping
    {
        get => _isScalar
            ? NonCapturingLazyInitializer.EnsureInitialized(
                ref _typeMapping, this, static dbFunction =>
                {
                    if (!RuntimeFeature.IsDynamicCodeSupported)
                    {
                        throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel);
                    }

                    var relationalTypeMappingSource =
                        (IRelationalTypeMappingSource)((IModel)dbFunction.Model).GetModelDependencies().TypeMappingSource;
                    return !string.IsNullOrEmpty(dbFunction._storeType)
                        ? relationalTypeMappingSource.FindMapping(dbFunction._returnType, dbFunction._storeType)!
                        : relationalTypeMappingSource.FindMapping(dbFunction._returnType, dbFunction.Model)!;
                })
            : _typeMapping;
        set => _typeMapping = value;
    }

    /// <summary>
    ///     Adds a parameter to the function.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="clrType">The parameter type.</param>
    /// <param name="propagatesNullability">A value which indicates whether the parameter propagates nullability.</param>
    /// <param name="storeType">The store type of this parameter.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> for this parameter.</param>
    /// <returns>The new parameter.</returns>
    public virtual RuntimeDbFunctionParameter AddParameter(
        string name,
        Type clrType,
        bool propagatesNullability,
        string storeType,
        RelationalTypeMapping? typeMapping = null)
    {
        var runtimeFunctionParameter = new RuntimeDbFunctionParameter(
            this,
            name,
            clrType,
            propagatesNullability,
            storeType,
            typeMapping);

        _parameters.Add(runtimeFunctionParameter);
        return runtimeFunctionParameter;
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
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
    IReadOnlyModel IReadOnlyDbFunction.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    IModel IDbFunction.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    IReadOnlyList<IReadOnlyDbFunctionParameter> IReadOnlyDbFunction.Parameters
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
    MethodInfo? IReadOnlyDbFunction.MethodInfo
    {
        [DebuggerStepThrough]
        get => _methodInfo;
    }

    /// <inheritdoc />
    Type IReadOnlyDbFunction.ReturnType
    {
        [DebuggerStepThrough]
        get => _returnType;
    }

    /// <inheritdoc />
    bool IReadOnlyDbFunction.IsScalar
    {
        [DebuggerStepThrough]
        get => _isScalar;
    }

    /// <inheritdoc />
    bool IReadOnlyDbFunction.IsAggregate
    {
        [DebuggerStepThrough]
        get => _isAggregate;
    }

    /// <inheritdoc />
    bool IReadOnlyDbFunction.IsBuiltIn
    {
        [DebuggerStepThrough]
        get => _isBuiltIn;
    }

    /// <inheritdoc />
    bool IReadOnlyDbFunction.IsNullable
    {
        [DebuggerStepThrough]
        get => _isNullable;
    }

    /// <inheritdoc />
    IStoreFunction IDbFunction.StoreFunction
    {
        [DebuggerStepThrough]
        get => _storeFunction!;
    }

    IStoreFunction IRuntimeDbFunction.StoreFunction
    {
        get => _storeFunction!;
        set => _storeFunction = value;
    }

    /// <inheritdoc />
    string IReadOnlyDbFunction.Name
    {
        [DebuggerStepThrough]
        get => _storeName;
    }

    /// <inheritdoc />
    string? IReadOnlyDbFunction.Schema
    {
        [DebuggerStepThrough]
        get => _schema;
    }

    /// <inheritdoc />
    string? IReadOnlyDbFunction.StoreType
    {
        [DebuggerStepThrough]
        get => _storeType;
    }

    /// <inheritdoc />
    Func<IReadOnlyList<SqlExpression>, SqlExpression>? IReadOnlyDbFunction.Translation
    {
        [DebuggerStepThrough]
        get => _translation;
    }

    /// <inheritdoc />
    RelationalTypeMapping? IReadOnlyDbFunction.TypeMapping
    {
        [DebuggerStepThrough]
        get => TypeMapping;
    }
}
