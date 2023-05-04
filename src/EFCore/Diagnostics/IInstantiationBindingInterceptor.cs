// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="ISingletonInterceptor" /> used to modify the <see cref="InstantiationBinding" /> used when creating
///     entity instances.
/// </summary>
/// <remarks>
///     <see cref="InstantiationBinding" /> instances define how to create an entity instance through the binding of EF model properties
///     to, for example, constructor parameters or parameters of a factory method. This is then built into the expression tree which is
///     compiled into a delegate used to materialize entity instances.
/// </remarks>
public interface IInstantiationBindingInterceptor : ISingletonInterceptor
{
    /// <summary>
    ///     Returns a new <see cref="InstantiationBinding" /> for the given entity type, potentially modified from the given binding.
    /// </summary>
    /// <param name="interceptionData">Contextual information about the binding.</param>
    /// <param name="binding">The current binding.</param>
    /// <returns>A new binding.</returns>
    InstantiationBinding ModifyBinding(InstantiationBindingInterceptionData interceptionData, InstantiationBinding binding);
}
