// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a function parameter.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public class RuntimeDbFunctionParameter : AnnotatableBase, IRuntimeDbFunctionParameter
{
    private readonly string _name;
    private readonly Type _clrType;
    private readonly bool _propagatesNullability;
    private readonly string _storeType;
    private IStoreFunctionParameter? _storeFunctionParameter;
    private RelationalTypeMapping? _typeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public RuntimeDbFunctionParameter(
        RuntimeDbFunction function,
        string name,
        Type clrType,
        bool propagatesNullability,
        string storeType,
        RelationalTypeMapping? typeMapping)
    {
        _name = name;
        Function = function;
        _clrType = clrType;
        _propagatesNullability = propagatesNullability;
        _storeType = storeType;
        _typeMapping = typeMapping;
    }

    /// <summary>
    ///     Gets the name of the function in the database.
    /// </summary>
    public virtual string Name
    {
        [DebuggerStepThrough]
        get => _name;
    }

    /// <summary>
    ///     Gets the function to which this parameter belongs.
    /// </summary>
    public virtual RuntimeDbFunction Function { get; }

    /// <summary>
    ///     Gets or sets the type mapping for this parameter.
    /// </summary>
    /// <returns>The type mapping.</returns>
    public virtual RelationalTypeMapping? TypeMapping
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _typeMapping, this, static parameter =>
            {
                if (!RuntimeFeature.IsDynamicCodeSupported)
                {
                    throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel);
                }

                var relationalTypeMappingSource =
                    (IRelationalTypeMappingSource)((IModel)parameter.Function.Model).GetModelDependencies().TypeMappingSource;
                return relationalTypeMappingSource.FindMapping(parameter._storeType)!;
            });

        set => _typeMapping = value;
    }

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => ((IDbFunctionParameter)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual DebugView DebugView
        => new(
            () => ((IDbFunctionParameter)this).ToDebugString(),
            () => ((IDbFunctionParameter)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

    /// <inheritdoc />
    IReadOnlyDbFunction IReadOnlyDbFunctionParameter.Function
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
    IStoreFunctionParameter IDbFunctionParameter.StoreFunctionParameter
    {
        [DebuggerStepThrough]
        get => _storeFunctionParameter!;
    }

    IStoreFunctionParameter IRuntimeDbFunctionParameter.StoreFunctionParameter
    {
        get => _storeFunctionParameter!;
        set => _storeFunctionParameter = value;
    }

    /// <inheritdoc />
    Type IReadOnlyDbFunctionParameter.ClrType
    {
        [DebuggerStepThrough]
        get => _clrType;
    }

    /// <inheritdoc />
    string? IReadOnlyDbFunctionParameter.StoreType
    {
        [DebuggerStepThrough]
        get => _storeType;
    }

    /// <inheritdoc />
    string IDbFunctionParameter.StoreType
    {
        [DebuggerStepThrough]
        get => _storeType;
    }

    /// <inheritdoc />
    bool IReadOnlyDbFunctionParameter.PropagatesNullability
    {
        [DebuggerStepThrough]
        get => _propagatesNullability;
    }

    /// <inheritdoc />
    RelationalTypeMapping? IReadOnlyDbFunctionParameter.TypeMapping
    {
        [DebuggerStepThrough]
        get => TypeMapping;
    }
}
