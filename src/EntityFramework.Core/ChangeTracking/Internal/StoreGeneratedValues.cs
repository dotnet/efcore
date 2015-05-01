// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class StoreGeneratedValues : DictionarySidecar
    {
        public StoreGeneratedValues([NotNull] InternalEntityEntry entry, [NotNull] IEnumerable<IProperty> properties)
            : base(entry, properties)
        {
        }

        public override string Name => WellKnownNames.StoreGeneratedValues;

        public override bool TransparentRead => true;

        public override bool TransparentWrite => true;

        public override bool AutoCommit => true;
    }
}
