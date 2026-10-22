// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public class Gearbox
{
    public class GearboxProxy(
        int id,
        string name) : Gearbox(id, name), IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public Gearbox()
    {
    }

    private Gearbox(int id, string name)
    {
        Id = id;
        Name = name;

        Assert.IsType<GearboxProxy>(this);
    }

    public int Id { get; set; }
    public string Name { get; set; }
}
