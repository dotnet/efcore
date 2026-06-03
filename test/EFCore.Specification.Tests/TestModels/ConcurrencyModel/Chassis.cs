// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public class Chassis
{
    public class ChassisProxy(
        ILazyLoader loader,
        int teamId,
        string name)
        : Chassis(loader, teamId, name), IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    private readonly ILazyLoader _loader;
    private Team _team;

    public Chassis()
    {
    }

    private Chassis(
        ILazyLoader loader,
        int teamId,
        string name)
    {
        _loader = loader;
        TeamId = teamId;
        Name = name;

        Assert.IsType<ChassisProxy>(this);
    }

    public int TeamId { get; set; }
    public string Name { get; set; }

    public virtual Team Team
    {
        get => _loader.Load(this, ref _team);
        set => _team = value;
    }
}
