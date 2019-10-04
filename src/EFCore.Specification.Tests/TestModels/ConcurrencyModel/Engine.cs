// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class Engine
    {
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
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public Location StorageLocation { get; set; }

        public int EngineSupplierId { get; set; }

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
}
