// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels.ConcurrencyModel
{
    public class EngineSupplier
    {
        public EngineSupplier()
        {
            // TODO: Remove once collection navigation property initializers are available
            Engines = new Collection<Engine>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Engine> Engines { get; set; }
    }
}
