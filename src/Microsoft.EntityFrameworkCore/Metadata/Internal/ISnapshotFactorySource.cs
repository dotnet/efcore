// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public interface ISnapshotFactorySource
    {
        Func<IUpdateEntry, ISnapshot> OriginalValuesFactory { get; }
        Func<IUpdateEntry, ISnapshot> RelationshipSnapshotFactory { get; }
        Func<ValueBuffer, ISnapshot> ShadowValuesFactory { get; }
        Func<ISnapshot> EmptyShadowValuesFactory { get; }
    }
}
