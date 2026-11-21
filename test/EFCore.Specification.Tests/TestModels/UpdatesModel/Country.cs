// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public enum Country
{
#pragma warning disable SA1317 // Preserve the country's correct spelling.
    Türkiye,
#pragma warning restore SA1317
    Czechia,
    Montenegro,
    Serbia,
    Eswatini
}
