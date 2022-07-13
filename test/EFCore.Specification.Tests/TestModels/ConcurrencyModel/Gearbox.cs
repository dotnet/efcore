// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class Gearbox
{
    public class GearboxProxy : Gearbox, IF1Proxy
    {
        public GearboxProxy(
            int id,
            string name)
            : base(id, name)
        {
        }

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
