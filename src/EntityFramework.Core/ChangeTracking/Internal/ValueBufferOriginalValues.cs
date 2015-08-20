// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class ValueBufferOriginalValues : ValueBufferSidecar
    {
        public ValueBufferOriginalValues([NotNull] InternalEntityEntry entry, ValueBuffer values)
            : base(entry, values)
        {
        }

        public override string Name => WellKnownNames.OriginalValues;

        public override bool TransparentRead => false;

        public override bool TransparentWrite => false;

        public override bool AutoCommit => false;
    }
}
