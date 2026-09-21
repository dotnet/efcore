// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Utilities;

internal static class ExceptionExtensions
{
    /// <summary>
    ///     Returns <see langword="true" /> if the exception should not be caught, e.g.
    ///     <see cref="OutOfMemoryException" />, <see cref="UnreachableException" />, or <see cref="OperationCanceledException" />.
    /// </summary>
    public static bool IsCritical(this Exception exception)
        => exception is OutOfMemoryException or UnreachableException or OperationCanceledException;
}
