// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A parameter object passed to <see cref="IInstantiationBindingInterceptor" /> methods.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
/// </remarks>
public readonly struct InstantiationBindingInterceptionData
{
    /// <summary>
    ///     Constructs the parameter object.
    /// </summary>
    /// <param name="typeBase">The entity type for which the binding is being used.</param>
    [EntityFrameworkInternal]
    [UsedImplicitly]
    public InstantiationBindingInterceptionData(ITypeBase typeBase)
    {
        TypeBase = typeBase;
    }

    /// <summary>
    ///     The entity type for which the binding is being used.
    /// </summary>
    public ITypeBase TypeBase { get; }
}
