// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     <para>
///         An arbitrary piece of metadata that can be stored on an object that implements <see cref="IConventionAnnotatable" />.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionAnnotation : IAnnotation
{
    /// <summary>
    ///     Returns the configuration source for this annotation.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource GetConfigurationSource();
}
