// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public class EngineSupplier
{
    public class EngineSupplierProxy(
        ILazyLoader loader,
        string name) : EngineSupplier(loader, name), IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    private readonly ILazyLoader _loader;
    private ICollection<Engine> _engines;

    public EngineSupplier()
    {
    }

    private EngineSupplier(ILazyLoader loader, string name)
    {
        _loader = loader;
        Name = name;

        Assert.IsType<EngineSupplierProxy>(this);
    }

    public string Name { get; set; }

    public virtual ICollection<Engine> Engines
    {
        get => _loader.Load(this, ref _engines);
        set => _engines = value;
    }
}
