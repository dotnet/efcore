// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Rodney
{
    public string Id { get; set; } = null!;

    [ConcurrencyCheck]
    public DateTime Concurrency { get; set; }
}
