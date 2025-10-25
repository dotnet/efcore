// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage;

/// <summary>
/// Defines methods for managing session tokens used in a <see cref="DbContext"/>.
/// </summary>
public interface ISessionTokenStorage
{
    /// <summary>
    ///     Appends or merges the session token specified to any composite already stored in the storage for the default container.
    /// </summary>
    /// <param name="sessionToken">The session token to append or merge.</param>
    void AppendSessionToken(string sessionToken);

    /// <summary>
    ///     Appends or merges the session token specified to any composite already stored in the storage for the specified container.
    /// </summary>
    /// <param name="containerName">The name of the container to append the session token for.</param>
    /// <param name="sessionToken">The session token to append or merge.</param>
    void AppendSessionToken(string containerName, string sessionToken);

    /// <summary>
    ///     Gets the composite session token for the default container.
    /// </summary>
    /// <returns>The composite session token, or <see langword="null" /> if none is stored.</returns>
    string? GetSessionToken();

    /// <summary>
    ///     Gets the composite session token for the specified container.
    /// </summary>
    /// <param name="containerName">The name of the container to get the session token for.</param>
    /// <returns>The composite session token, or <see langword="null" /> if none is stored.</returns>
    string? GetSessionToken(string containerName);

    /// <summary>
    ///     Overwrites the session token for the container.
    /// </summary>
    /// <param name="containerName">The name of the container to set the session token for.</param>
    /// <param name="sessionToken">The session token to set, or <see langword="null" /> to clear any stored token.</param>
    void SetSessionToken(string containerName, string? sessionToken);

    /// <summary>
    ///     Overwrites the session token for the default container.
    /// </summary>
    /// <param name="sessionToken">The session token to set, or <see langword="null" /> to clear any stored token.</param>
    void SetSessionToken(string? sessionToken);

    /// <summary>
    ///     Clears all stored session tokens.
    /// </summary>
    void Clear();
}
