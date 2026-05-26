// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiChief.Model;

public sealed class ApiType : IEquatable<ApiType>
{
    [JsonIgnore]
    public string FullTypeName { get; set; } = string.Empty;

    [JsonPropertyOrder(0)]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyOrder(1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ApiStage Stage { get; set; }

    [JsonPropertyOrder(2)]
    public ISet<ApiMember>? Methods;

    [JsonPropertyOrder(3)]
    public ISet<ApiMember>? Fields;

    [JsonPropertyOrder(4)]
    public ISet<ApiMember>? Properties;

    [JsonPropertyOrder(5)]
    public ApiType? Additions { get; set; }

    [JsonPropertyOrder(6)]
    public ApiType? Removals { get; set; }

    [JsonIgnore]
    public bool IsNew { get; set; }

    [JsonIgnore]
    public bool IsRemoved { get; set; }

    /// <summary>
    /// The baseline declaration when only the type header changed (e.g. an interface was added or removed).
    /// Not serialized; only used by the in-memory delta to surface header-only modifications.
    /// </summary>
    [JsonIgnore]
    public string? PreviousType { get; set; }

    /// <summary>
    /// Stable identity for matching baseline and current types. Strips the base/interface list and generic
    /// constraints so that changes to those are reported as modifications instead of a removed-and-re-added type.
    /// </summary>
    [JsonIgnore]
    public string Identity => GetIdentity(Type);

    private static string GetIdentity(string type)
    {
        if (string.IsNullOrEmpty(type))
        {
            return string.Empty;
        }

        var endIdx = type.Length;

        var whereIdx = type.IndexOf(" where ", StringComparison.Ordinal);
        if (whereIdx >= 0)
        {
            endIdx = whereIdx;
        }

        var colonIdx = type.IndexOf(" : ", StringComparison.Ordinal);
        if (colonIdx >= 0 && colonIdx < endIdx)
        {
            endIdx = colonIdx;
        }

        return type[..endIdx].TrimEnd();
    }

    public bool Equals(ApiType? other) => other != null && Identity == other.Identity;
    public override bool Equals(object? obj) => Equals(obj as ApiType);
    public override int GetHashCode() => Identity.GetHashCode();
}
