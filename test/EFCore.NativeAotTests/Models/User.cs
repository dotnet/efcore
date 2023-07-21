// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.NativeAotTests.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
