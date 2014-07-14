// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ConcurrencyModel
{
    public class Engine
    {
        public Engine()
        {
            // TODO: Remove once collection navigation property initializers are available
            Teams = new Collection<Team>();
            Gearboxes = new Collection<Gearbox>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public Location StorageLocation { get; set; }

        public int EngineSupplierId { get; set; }

        public virtual EngineSupplier EngineSupplier { get; set; }

        public virtual ICollection<Team> Teams { get; set; }

        public virtual ICollection<Gearbox> Gearboxes { get; set; } // Uni-directional
    }
}
