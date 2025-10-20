// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
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
    private readonly Dictionary<string, CompositeSessionToken> _containerSessionTokens;
    private readonly string _defaultContainerName;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SessionTokenStorage(DbContext dbContext)
    {
        var defaultContainerName = (string)dbContext.Model.GetAnnotation(CosmosAnnotationNames.ContainerName).Value!;
        var containerNames = (HashSet<string>)dbContext.Model.GetAnnotation(CosmosAnnotationNames.ContainerNames).Value!;

        _defaultContainerName = defaultContainerName;
        _containerSessionTokens = containerNames.ToDictionary(containerName => containerName, _ => new CompositeSessionToken());
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? GetSessionToken()
        => GetSessionToken(_defaultContainerName);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetSessionToken(string? sessionToken)
        => SetSessionToken(_defaultContainerName, sessionToken);

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
            throw new ArgumentException(CosmosStrings.ContainerNameDoesNotExist(containerName), nameof(containerName));
        }

        return value.ConvertToString();
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

        if (!_containerSessionTokens.TryGetValue(containerName, out var compositeSessionToken))
        {
            throw new ArgumentException(CosmosStrings.ContainerNameDoesNotExist(containerName), nameof(containerName));
        }

        ParseAndMerge(compositeSessionToken, sessionToken);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetSessionToken(string containerName, string? sessionToken)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));
        if (sessionToken is not null && string.IsNullOrWhiteSpace(sessionToken))
        {
            // @TODO: Exception messages in this file.
            throw new ArgumentException("sessionToken cannot be whitespace.", sessionToken);
        }

        ref var value = ref CollectionsMarshal.GetValueRefOrNullRef(_containerSessionTokens, containerName);

        if (Unsafe.IsNullRef(ref value))
        {
            throw new ArgumentException(CosmosStrings.ContainerNameDoesNotExist(containerName), nameof(containerName));
        }

        value = new CompositeSessionToken();

        if (sessionToken is null)
        {
            return;
        }

        ParseAndMerge(value, sessionToken);
    }

    private void ParseAndMerge(CompositeSessionToken compositeSessionToken, string sessionToken)
    {
        var parts = sessionToken.Split(',');
        foreach (var part in parts)
        {
            var index = part.IndexOf(':');
            if (index == -1)
            {
                throw new ArgumentException("CosmosStrings.InvalidSessionToken(sessionToken)", nameof(sessionToken));
            }

            var pkRangeId = sessionToken.Substring(0, index);
            var vector = sessionToken.Substring(index + 1);
            if (!VectorSessionToken.TryCreate(vector, out var vectorSessionToken))
            {
                throw new ArgumentException("CosmosStrings.InvalidSessionToken(sessionToken)", nameof(sessionToken));
            }

            compositeSessionToken.Merge(pkRangeId, vectorSessionToken);
        }
    }

    private sealed class CompositeSessionToken
    {
        private string? _string;
        private bool _isChanged;
        public Dictionary<string, VectorSessionToken> Tokens { get; } = new();

        public void Merge(string pkRangeId, VectorSessionToken token)
        {
            ref var existing = ref CollectionsMarshal.GetValueRefOrAddDefault(Tokens, pkRangeId, out var exists);
            if (exists)
            {
                existing = existing!.Merge(token);
            }
            else
            {
                existing = token;
            }

            _isChanged = true;
        }

        public string? ConvertToString()
        {
            if (_isChanged)
            {
                _isChanged = false;
                _string = string.Join(",", Tokens.Select(kvp => $"{kvp.Key}:{kvp.Value.ConvertToString()}"));
            }

            return _string;
        }
    }


    /// <see cref="Azure.Documents.VectorSessionToken"/>
    private sealed class VectorSessionToken : IEquatable<VectorSessionToken>
    {
        private static readonly IReadOnlyDictionary<uint, long> DefaultLocalLsnByRegion = new Dictionary<uint, long>(0);

        private readonly string sessionToken;

        private readonly long version;

        private readonly long globalLsn;

        private readonly IReadOnlyDictionary<uint, long> localLsnByRegion;

        private static readonly bool isFalseProgressMergeDisabled = string.Equals(Environment.GetEnvironmentVariable("AZURE_COSMOS_SESSION_TOKEN_FALSE_PROGRESS_MERGE_DISABLED"), "true", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public long LSN => globalLsn;

        private VectorSessionToken(long version, long globalLsn, IReadOnlyDictionary<uint, long> localLsnByRegion, string? sessionToken = null)
        {
            this.version = version;
            this.globalLsn = globalLsn;
            this.localLsnByRegion = localLsnByRegion;
            if (sessionToken != null)
            {
                this.sessionToken = sessionToken;
                return;
            }

            string? text = null;
            if (localLsnByRegion.Any())
            {
                text = string.Join("#", localLsnByRegion.Select((KeyValuePair<uint, long> kvp) => string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", kvp.Key, '=', kvp.Value)));
            }

            if (string.IsNullOrEmpty(text))
            {
                this.sessionToken = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", this.version, "#", this.globalLsn);
                return;
            }

            this.sessionToken = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}", this.version, "#", this.globalLsn, "#", text);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public VectorSessionToken(VectorSessionToken other, long globalLSN)
            : this(other.version, globalLSN, other.localLsnByRegion)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool TryCreate(string sessionToken, [NotNullWhen(true)] out VectorSessionToken? parsedSessionToken)
        {
            parsedSessionToken = null;
            if (TryParseSessionToken(sessionToken, out var num, out var num2, out var readOnlyDictionary))
            {
                parsedSessionToken = new VectorSessionToken(num, num2, readOnlyDictionary, sessionToken);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool Equals(VectorSessionToken? obj)
        {
            if (!(obj is VectorSessionToken vectorSessionToken))
            {
                return false;
            }

            if (version == vectorSessionToken.version && globalLsn == vectorSessionToken.globalLsn)
            {
                return AreRegionProgressEqual(vectorSessionToken.localLsnByRegion);
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool IsValid(VectorSessionToken otherSessionToken)
        {
            if (!(otherSessionToken is VectorSessionToken vectorSessionToken))
            {
                throw new ArgumentNullException("otherSessionToken");
            }

            if (isFalseProgressMergeDisabled)
            {
                if (vectorSessionToken.version < version || vectorSessionToken.globalLsn < globalLsn)
                {
                    return false;
                }
            }
            else if (vectorSessionToken.version < version || (vectorSessionToken.version == version && vectorSessionToken.globalLsn < globalLsn))
            {
                return false;
            }

            if (vectorSessionToken.version == version && vectorSessionToken.localLsnByRegion.Count != localLsnByRegion.Count)
            {
                throw new InvalidOperationException("string.Format(CultureInfo.InvariantCulture, RMResources.InvalidRegionsInSessionToken, sessionToken, vectorSessionToken.sessionToken)");
            }

            foreach (KeyValuePair<uint, long> item in vectorSessionToken.localLsnByRegion)
            {
                uint key = item.Key;
                long value = item.Value;
                long value2 = -1L;
                if (!localLsnByRegion.TryGetValue(key, out value2))
                {
                    if (version == vectorSessionToken.version)
                    {
                        throw new InvalidOperationException("string.Format(CultureInfo.InvariantCulture, RMResources.InvalidRegionsInSessionToken, sessionToken, vectorSessionToken.sessionToken)");
                    }
                }
                else if (value < value2)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public VectorSessionToken Merge(VectorSessionToken vectorSessionToken)
        {
            if (version == vectorSessionToken.version && localLsnByRegion.Count != vectorSessionToken.localLsnByRegion.Count)
            {
                throw new InvalidOperationException("string.Format(CultureInfo.InvariantCulture, RMResources.InvalidRegionsInSessionToken, sessionToken, vectorSessionToken.sessionToken)");
            }

            if (version >= vectorSessionToken.version && globalLsn > vectorSessionToken.globalLsn)
            {
                if (AreAllLocalLsnByRegionsGreaterThanOrEqual(this, vectorSessionToken))
                {
                    return this;
                }
            }
            else if (vectorSessionToken.version >= version && vectorSessionToken.globalLsn >= globalLsn && AreAllLocalLsnByRegionsGreaterThanOrEqual(vectorSessionToken, this))
            {
                return vectorSessionToken;
            }

            VectorSessionToken vectorSessionToken2;
            VectorSessionToken vectorSessionToken3;
            if (version < vectorSessionToken.version)
            {
                vectorSessionToken2 = this;
                vectorSessionToken3 = vectorSessionToken;
            }
            else
            {
                vectorSessionToken2 = vectorSessionToken;
                vectorSessionToken3 = this;
            }

            Dictionary<uint, long> dictionary = new Dictionary<uint, long>(vectorSessionToken3.localLsnByRegion.Count);
            foreach (KeyValuePair<uint, long> item in vectorSessionToken3.localLsnByRegion)
            {
                uint key = item.Key;
                long value = item.Value;
                long value2 = -1L;
                if (vectorSessionToken2.localLsnByRegion.TryGetValue(key, out value2))
                {
                    dictionary[key] = Math.Max(value, value2);
                    continue;
                }

                if (version == vectorSessionToken.version)
                {
                    throw new InvalidOperationException("string.Format(CultureInfo.InvariantCulture, RMResources.InvalidRegionsInSessionToken, sessionToken, vectorSessionToken.sessionToken)");
                }

                dictionary[key] = value;
            }

            long num = Math.Max(version, vectorSessionToken.version);
            long num2 = ((version == vectorSessionToken.version || isFalseProgressMergeDisabled) ? Math.Max(globalLsn, vectorSessionToken.globalLsn) : vectorSessionToken3.globalLsn);
            return new VectorSessionToken(num, num2, dictionary);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public string ConvertToString()
        {
            return sessionToken;
        }

        private bool AreRegionProgressEqual(IReadOnlyDictionary<uint, long> other)
        {
            if (localLsnByRegion.Count != other.Count)
            {
                return false;
            }

            foreach (KeyValuePair<uint, long> item in localLsnByRegion)
            {
                uint key = item.Key;
                long value = item.Value;
                if (other.TryGetValue(key, out var value2) && value != value2)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreAllLocalLsnByRegionsGreaterThanOrEqual(VectorSessionToken higherToken, VectorSessionToken lowerToken)
        {
            if (higherToken.localLsnByRegion.Count != lowerToken.localLsnByRegion.Count)
            {
                return false;
            }

            if (!higherToken.localLsnByRegion.Any())
            {
                return true;
            }

            foreach (KeyValuePair<uint, long> item in higherToken.localLsnByRegion)
            {
                uint key = item.Key;
                long value = item.Value;
                if (lowerToken.localLsnByRegion.TryGetValue(key, out var value2))
                {
                    if (value2 > value)
                    {
                        return false;
                    }

                    continue;
                }

                return false;
            }

            return true;
        }

        private static bool TryParseSessionToken(string sessionToken, out long version, out long globalLsn, [NotNullWhen(true)] out IReadOnlyDictionary<uint, long>? localLsnByRegion)
        {
            version = 0L;
            localLsnByRegion = null;
            globalLsn = -1L;
            if (string.IsNullOrEmpty(sessionToken))
            {
                return false;
            }

            int index = 0;
            if (!TryParseLongSegment(sessionToken, ref index, out version))
            {
                return false;
            }

            if (index >= sessionToken.Length)
            {
                return false;
            }

            if (!TryParseLongSegment(sessionToken, ref index, out globalLsn))
            {
                return false;
            }

            if (index >= sessionToken.Length)
            {
                localLsnByRegion = DefaultLocalLsnByRegion;
                return true;
            }

            Dictionary<uint, long> dictionary = new Dictionary<uint, long>();
            while (index < sessionToken.Length)
            {
                if (!TryParseUintTillRegionProgressSeparator(sessionToken, ref index, out var value))
                {
                    return false;
                }

                if (!TryParseLongSegment(sessionToken, ref index, out var value2))
                {
                    return false;
                }

                dictionary[value] = value2;
            }

            localLsnByRegion = dictionary;
            return true;
        }

        private static bool TryParseUintTillRegionProgressSeparator(string input, ref int index, out uint value)
        {
            value = 0u;
            if (index >= input.Length)
            {
                return false;
            }

            long num = 0L;
            while (index < input.Length)
            {
                char c = input[index];
                if (c >= '0' && c <= '9')
                {
                    num = num * 10 + (c - 48);
                    index++;
                    continue;
                }

                if (c == '=')
                {
                    index++;
                    break;
                }

                return false;
            }

            if (num > uint.MaxValue || num < 0)
            {
                return false;
            }

            value = (uint)num;
            return true;
        }

        private static bool TryParseLongSegment(string input, ref int index, out long value)
        {
            value = 0L;
            if (index >= input.Length)
            {
                return false;
            }

            bool flag = false;
            if (input[index] == '-')
            {
                index++;
                flag = true;
            }

            while (index < input.Length)
            {
                char c = input[index];
                if (c >= '0' && c <= '9')
                {
                    value = value * 10 + (c - 48);
                    index++;
                    continue;
                }

                if (c == '#')
                {
                    index++;
                    break;
                }

                return false;
            }

            if (flag)
            {
                value *= -1L;
            }

            return true;
        }
    }

}
