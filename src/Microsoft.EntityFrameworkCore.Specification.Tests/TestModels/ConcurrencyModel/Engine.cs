// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ConcurrencyModel
{
    public class Engine
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Location StorageLocation { get; set; }

        public int EngineSupplierId { get; set; }

        public virtual EngineSupplier EngineSupplier { get; set; }

        public virtual ICollection<Team> Teams { get; set; }

        public virtual ICollection<Gearbox> Gearboxes { get; set; } // Uni-directional
    }
}
