// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The base interface for all Entity Framework interceptors that are registered as <see cref="ServiceLifetime.Singleton" />
///     services. This means a single instance is used by many <see cref="DbContext" /> instances.
///     The implementation must be thread-safe.
/// </summary>
public interface ISingletonInterceptor : IInterceptor;
