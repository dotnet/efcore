// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Service dependencies parameter class for <see cref="RelationalLiftableConstantFactory" />
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Do not construct instances of this class directly from either provider or application code as the
///         constructor signature may change as new dependencies are added. Instead, use this type in
///         your constructor so that an instance will be created and injected automatically by the
///         dependency injection container. To create an instance with some dependent services replaced,
///         first resolve the object from the dependency injection container, then replace selected
///         services using the C# 'with' operator. Do not call the constructor at any point in this process.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
/// </remarks>
[Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
public sealed record RelationalLiftableConstantExpressionDependencies
{
}
