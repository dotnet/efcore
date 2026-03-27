// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public class Engine
{
    public class EngineProxy(
        ILazyLoader loader,
        int id,
        string name) : Engine(loader, id, name), IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    private readonly ILazyLoader _loader;
    private EngineSupplier _engineSupplier;
    private ICollection<Team> _teams;
    private ICollection<Gearbox> _gearboxes;

    public Engine()
    {
    }

    public Engine(ILazyLoader loader, int id, string name)
    {
        _loader = loader;
        Id = id;
        Name = name;

        Assert.IsType<EngineProxy>(this);
    }

    public int Id { get; set; }
    public string Name { get; set; }

    public Location StorageLocation { get; set; }

    public string EngineSupplierId { get; set; }

    public virtual EngineSupplier EngineSupplier
    {
        get => _loader.Load(this, ref _engineSupplier);
        set => _engineSupplier = value;
    }

    public virtual ICollection<Team> Teams
    {
        get => _loader.Load(this, ref _teams);
        set => _teams = value;
    }

    public virtual ICollection<Gearbox> Gearboxes
    {
        get => _loader.Load(this, ref _gearboxes);
        set => _gearboxes = value;
    }
}
