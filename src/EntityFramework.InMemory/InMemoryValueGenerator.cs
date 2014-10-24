// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryValueGenerator : SimpleValueGenerator
    {
        private long _current;

        public override void Next(StateEntry stateEntry, IProperty property)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            stateEntry[property] = Convert.ChangeType(Interlocked.Increment(ref _current), property.PropertyType.UnwrapNullableType());
        }
    }
}
