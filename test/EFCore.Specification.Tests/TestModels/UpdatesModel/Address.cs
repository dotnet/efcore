// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

namespace Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

public class Address
{
    public string City { get; set; } = null!;
    public Country Country { get; set; }
    public int? ZipCode { get; set; }
}
