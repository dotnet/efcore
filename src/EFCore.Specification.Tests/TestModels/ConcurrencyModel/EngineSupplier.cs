// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class EngineSupplier
    {
        private readonly ILazyLoader _loader;
        private ICollection<Engine> _engines;

        public EngineSupplier()
        {
        }

        private EngineSupplier(ILazyLoader loader, int id, string name)
        {
            _loader = loader;
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Engine> Engines
        {
            get => _loader.Load(this, ref _engines);
            set => _engines = value;
        }
    }
}
