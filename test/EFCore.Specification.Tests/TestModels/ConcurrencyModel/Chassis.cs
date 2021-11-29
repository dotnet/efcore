// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class Chassis
{
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
    }

    public int TeamId { get; set; }
    public string Name { get; set; }

    public virtual Team Team
    {
        get => _loader.Load(this, ref _team);
        set => _team = value;
    }
}
