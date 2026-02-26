// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <inheritdoc/>
public class NullSessionTokenStorage : ISessionTokenStorage
{
    /// <inheritdoc/>
    public void AppendDefaultContainerSessionToken(string sessionToken) { }

    /// <inheritdoc/>
    public void AppendSessionTokens(IReadOnlyDictionary<string, string> sessionTokens) {}

    /// <inheritdoc/>
    public void Clear() {}

    /// <inheritdoc/>
    public string? GetDefaultContainerTrackedToken() => null;

    /// <inheritdoc/>
    public string? GetSessionToken(string containerName) => null;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string?> GetTrackedTokens() => null!;

    /// <inheritdoc/>
    public void TrackSessionToken(string containerName, string sessionToken) {}

    /// <inheritdoc/>
    public void SetDefaultContainerSessionToken(string sessionToken) {}

    /// <inheritdoc/>
    public void SetSessionTokens(IReadOnlyDictionary<string, string?> sessionTokens) {}
}
