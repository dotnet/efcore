// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonOwnedCustomNameBranch
{
    [JsonPropertyName("CustomDate")]
    public DateTime Date { get; set; }

    [JsonPropertyName("CustomFraction")]
    public double Fraction { get; set; }
}
