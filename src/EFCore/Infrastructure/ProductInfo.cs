// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Helper class for finding the version of Entity Framework Core being used.
/// </summary>
public static class ProductInfo
{
    /// <summary>
    ///     Gets the value of the <see cref="AssemblyInformationalVersionAttribute.InformationalVersion" />
    ///     for the EntityFrameworkCore assembly.
    /// </summary>
    /// <returns>The EF Core version being used.</returns>
    public static string GetVersion()
        => typeof(ProductInfo).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
}
