// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

public class Officer : Gear
{
    public Officer()
    {
        Reports = new List<Gear>();
    }

    // 1 - many self reference
    public virtual ICollection<Gear> Reports { get; set; }
}
