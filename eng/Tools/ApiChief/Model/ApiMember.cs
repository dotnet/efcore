// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json.Serialization;

namespace ApiChief.Model;

internal sealed class ApiMember : IEquatable<ApiMember>
{
    [JsonPropertyOrder(0)]
    public string Member { get; set; } = string.Empty;

    [JsonPropertyOrder(1)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ApiStage Stage { get; set; }

    /// <summary>
    /// Gets or sets a constant value (for constant field) to ensure it doesn't change between releases.
    /// </summary>
    [JsonPropertyOrder(2)]
    public string? Value { get; set; }

    public bool Equals(ApiMember? other)
        => other != null && Member == other.Member && Value == other.Value;

    public override bool Equals(object? obj)
        => Equals(obj as ApiMember);

    public override int GetHashCode()
        => HashCode.Combine(Member, Value);
}
