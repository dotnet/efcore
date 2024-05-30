// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Utilities;

[DebuggerStepThrough]
internal static class Check
{
    [ContractAnnotation("value:null => halt")]
    [return: NotNull]
    public static T NotNull<T>(
        [NoEnumeration] [AllowNull] [NotNull] T value,
        [InvokerParameterName] [CallerArgumentExpression(nameof(value))] string parameterName = "")
        => value ?? throw new ArgumentNullException(parameterName);

    [ContractAnnotation("value:null => halt")]
    public static IReadOnlyList<T> NotEmpty<T>(
        [NotNull] IReadOnlyList<T>? value,
        [InvokerParameterName] [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        NotNull(value, parameterName);

        if (value.Count == 0)
        {
            throw new ArgumentException(AbstractionsStrings.CollectionArgumentIsEmpty(parameterName), parameterName);
        }

        return value;
    }

    [ContractAnnotation("value:null => halt")]
    public static string NotEmpty(
        [NotNull] string? value,
        [InvokerParameterName] [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }

        if (value.Trim().Length == 0)
        {
            throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName), parameterName);
        }

        return value;
    }

    public static string? NullButNotEmpty(
        string? value,
        [InvokerParameterName] [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        if (value is not null && value.Length == 0)
        {
            throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName), parameterName);
        }

        return value;
    }

    public static IReadOnlyList<T> HasNoNulls<T>(
        [NotNull] IReadOnlyList<T>? value,
        [InvokerParameterName] [CallerArgumentExpression(nameof(value))] string parameterName = "")
        where T : class
    {
        NotNull(value, parameterName);

        if (value.Any(e => e == null))
        {
            throw new ArgumentException(parameterName, parameterName);
        }

        return value;
    }

    public static IReadOnlyList<string> HasNoEmptyElements(
        [NotNull] IReadOnlyList<string>? value,
        [InvokerParameterName] [CallerArgumentExpression(nameof(value))] string parameterName = "")
    {
        NotNull(value, parameterName);

        if (value.Any(s => string.IsNullOrWhiteSpace(s)))
        {
            throw new ArgumentException(AbstractionsStrings.CollectionArgumentHasEmptyElements(parameterName), parameterName);
        }

        return value;
    }

    [Conditional("DEBUG")]
    public static void DebugAssert([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
        {
            throw new UnreachableException($"Check.DebugAssert failed: {message}");
        }
    }

    [Conditional("DEBUG")]
    [DoesNotReturn]
    public static void DebugFail(string message)
        => throw new UnreachableException($"Check.DebugFail failed: {message}");
}
