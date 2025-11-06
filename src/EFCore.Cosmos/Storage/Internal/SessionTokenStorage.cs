// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    private readonly Dictionary<string, CompositeSessionToken> _containerSessionTokens;
    private readonly string _defaultContainerName;
    private readonly HashSet<string> _containerNames;
    private readonly SessionTokenManagementMode _mode;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SessionTokenStorage(string defaultContainerName, HashSet<string> containerNames, SessionTokenManagementMode mode)
    {
        Debug.Assert(containerNames.Contains(defaultContainerName));
        _defaultContainerName = defaultContainerName;
        _containerNames = containerNames;
        _mode = mode;

        _containerSessionTokens = containerNames.ToDictionary(x => x, x => new CompositeSessionToken());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetSessionTokens(IReadOnlyDictionary<string, string?> sessionTokens)
    {
        CheckMode();
        foreach (var sessionToken in sessionTokens)
        {
            if (!_containerNames.Contains(sessionToken.Key))
            {
                throw new InvalidOperationException("invalid container name");
            }

            _containerSessionTokens[sessionToken.Key] = new CompositeSessionToken(sessionToken.Value, true);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AppendSessionTokens(IReadOnlyDictionary<string, string> sessionTokens)
    {
        CheckMode();
        foreach (var sessionToken in sessionTokens)
        {
            if (!_containerNames.Contains(sessionToken.Key))
            {
                throw new InvalidOperationException("invalid container name");
            }

            _containerSessionTokens[sessionToken.Key].Add(sessionToken.Value, true);
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
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionToken, nameof(sessionToken));
        CheckMode();
        _containerSessionTokens[_defaultContainerName].Add(sessionToken, true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetDefaultContainerSessionToken(string? sessionToken)
    {
        CheckMode();
        _containerSessionTokens[_defaultContainerName] = new CompositeSessionToken(sessionToken, true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyDictionary<string, string?> GetTrackedTokens()
    {
        CheckMode();
        return _containerSessionTokens.ToDictionary(x => x.Key, x => x.Value.ConvertToString());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? GetDefaultContainerTrackedToken()
    {
        CheckMode();
        return _containerSessionTokens[_defaultContainerName].ConvertToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? GetSessionToken(string containerName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));

        if (_mode == SessionTokenManagementMode.FullyAutomatic)
        {
            return null;
        }

        var sessionToken = _containerSessionTokens[containerName];

        if (!sessionToken.IsSet && _mode == SessionTokenManagementMode.EnforcedManual)
        {
            throw new InvalidOperationException("No session token set for container while EnforcedManual");
        }

        return sessionToken.ConvertToString();
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

        if (_mode == SessionTokenManagementMode.FullyAutomatic)
        {
            return;
        }

        _containerSessionTokens[containerName].Add(sessionToken);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Clear()
    {
        foreach (var key in _containerSessionTokens.Keys)
        {
            _containerSessionTokens[key] = new CompositeSessionToken();
        }
    }

    private void CheckMode()
    {
        if (_mode == SessionTokenManagementMode.FullyAutomatic)
        {
            throw new InvalidOperationException("Can't use session tokens with FullyAutomatic");
        }
    }

    private sealed class CompositeSessionToken
    {
        private string? _string;
        private bool _isChanged;
        private readonly HashSet<string> _tokens = new();

        public CompositeSessionToken(string? token, bool isSet = false)
        {
            if (token != null)
            {
                Add(token);
            }
            IsSet = isSet;
        }

        public CompositeSessionToken()
        {
        }

        public bool IsSet { get; private set; }

        public void Add(string token, bool isSet = false)
        {
            IsSet = IsSet || isSet;

            if (token == null)
            {
                return;
            }

            foreach (var tokenPart in token.Split(','))
            {
                if (_tokens.Add(tokenPart))
                {
                    _isChanged = true;
                }
            }
        }

        public string? ConvertToString()
        {
            if (_isChanged)
            {
                _isChanged = false;
                _string = IsSet && _tokens.Count == 0 ? null : string.Join(",", _tokens);
            }

            return _string;
        }
    }
}
