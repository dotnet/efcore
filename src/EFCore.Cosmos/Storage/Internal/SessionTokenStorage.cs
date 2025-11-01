// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SessionTokenStorage : ISessionTokenStorage
{
    private readonly Dictionary<string, CompositeSessionToken> _containerSessionTokens = new();
    private readonly string _defaultContainerName;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SessionTokenStorage(DbContext dbContext)
    {
        _defaultContainerName = (string)dbContext.Model.GetAnnotation(CosmosAnnotationNames.ContainerName).Value!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? GetSessionToken() => GetSessionToken(_defaultContainerName);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AppendSessionToken(string sessionToken)
        => AppendSessionToken(_defaultContainerName, sessionToken);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? GetSessionToken(string containerName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));

        if (!_containerSessionTokens.TryGetValue(containerName, out var value))
        {
            return null;
        }

        return value.ConvertToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AppendSessionTokens(IReadOnlyDictionary<string, string> sessionTokens)
    {
        ArgumentNullException.ThrowIfNull(sessionTokens, nameof(sessionTokens));
        foreach (var (containerName, sessionToken) in sessionTokens)
        {
            AppendSessionToken(containerName, sessionToken);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AppendSessionToken(string containerName, string sessionToken)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sessionToken, nameof(sessionToken));

        ref var compositeSessionToken = ref CollectionsMarshal.GetValueRefOrAddDefault(_containerSessionTokens, containerName, out var exists);
        if (!exists)
        {
            compositeSessionToken = new(sessionToken);
        }
        else
        {
            compositeSessionToken!.Add(sessionToken);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Clear()
        => _containerSessionTokens.Clear();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyDictionary<string, string> ToDictionary() => _containerSessionTokens.ToDictionary(x => x.Key, x => x.Value.ConvertToString());

    private sealed class CompositeSessionToken
    {
        private string? _string;
        private bool _isChanged;
        private readonly HashSet<string> _tokens = new();

        public CompositeSessionToken(string token)
            => Add(token);

        public void Add(string token)
        {
            foreach (var tokenPart in token.Split(','))
            {
                if (_tokens.Add(tokenPart))
                {
                    _isChanged = true;
                }
            }
        }

        public string ConvertToString()
        {
            if (_isChanged)
            {
                _isChanged = false;
                _string = string.Join(",", _tokens);
            }

            return _string!;
        }
    }
}
