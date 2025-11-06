// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SessionTokenStorage : ISessionTokenStorage
{
    private bool _useSessionTokens = false;

    private Dictionary<string, CompositeSessionToken> _containerSessionTokens = new();
    private readonly string _defaultContainerName;
    private readonly SessionTokenManagementMode _mode;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SessionTokenStorage(string defaultContainerName, SessionTokenManagementMode mode)
    {
        Debug.Assert(mode != SessionTokenManagementMode.FullyAutomatic, $"Use {nameof(NullSessionTokenStorage)} instead.");

        _defaultContainerName = defaultContainerName;
        _mode = mode;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void SetSessionTokens(IReadOnlyDictionary<string, string> sessionTokens)
    {
        _useSessionTokens = true;
        _containerSessionTokens = sessionTokens.ToDictionary(x => x.Key, x => new CompositeSessionToken(x.Value));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public void AppendSessionTokens(IReadOnlyDictionary<string, string> sessionTokens)
    {
        _useSessionTokens = true;
        foreach (var sessionToken in sessionTokens)
        {
            TrackSessionToken(sessionToken.Key, sessionToken.Value);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AppendDefaultContainerSessionToken(string sessionToken)
    {
        _useSessionTokens = true;
        TrackSessionToken(_defaultContainerName, sessionToken);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetDefaultContainerSessionToken(string sessionToken)
    {
        _useSessionTokens = true;
        _containerSessionTokens[_defaultContainerName] = new CompositeSessionToken(sessionToken);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyDictionary<string, string> GetTrackedTokens() => _containerSessionTokens.ToDictionary(x => x.Key, x => x.Value.ConvertToString());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? GetDefaultContainerTrackedToken() => _containerSessionTokens.GetValueOrDefault(_defaultContainerName)?.ConvertToString();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? GetSessionToken(string containerName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));

        if (_mode == SessionTokenManagementMode.SemiAutomatic && !_useSessionTokens)
        {
            return null;
        }

        if (!_containerSessionTokens.TryGetValue(containerName, out var value))
        {
            if (_mode == SessionTokenManagementMode.EnforcedManual)
            {
                throw new InvalidOperationException();
            }

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
    public virtual void TrackSessionToken(string containerName, string sessionToken)
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
    {
        _useSessionTokens = false;
        _containerSessionTokens.Clear();
    }

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
