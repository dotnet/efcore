// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Extension methods for <see cref="MethodInfo" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public static class MethodInfoExtensions
{
    private static readonly string EFTypeName = typeof(EF).FullName!;

    /// <summary>
    ///     Returns <see langword="true" /> if the given method is <see cref="EF.Property{TProperty}" />.
    /// </summary>
    /// <param name="methodInfo">The method.</param>
    /// <returns><see langword="true" /> if the method is <see cref="EF.Property{TProperty}" />; <see langword="false" /> otherwise.</returns>
    public static bool IsEFPropertyMethod(this MethodInfo methodInfo)
        => methodInfo.IsGenericMethod
            && (Equals(methodInfo.GetGenericMethodDefinition(), EF.PropertyMethod)
                // fallback to string comparison because MethodInfo.Equals is not
                // always true in .NET Native even if methods are the same
                || (methodInfo.Name == nameof(EF.Property)
                    && methodInfo.DeclaringType?.FullName == EFTypeName));

    /// <summary>
    ///     Returns <see langword="true" /> if the given method is <see cref="EF.Default{T}" />.
    /// </summary>
    /// <param name="methodInfo">The method.</param>
    /// <returns><see langword="true" /> if the method is <see cref="EF.Default{T}" />; <see langword="false" /> otherwise.</returns>
    public static bool IsEFDefaultMethod(this MethodInfo methodInfo)
        => methodInfo.IsGenericMethod
            && (Equals(methodInfo.GetGenericMethodDefinition(), EF.DefaultMethod)
                // fallback to string comparison because MethodInfo.Equals is not
                // always true in .NET Native even if methods are the same
                || (methodInfo.Name == nameof(EF.DefaultMethod)
                    && methodInfo.DeclaringType?.FullName == EFTypeName));
}
