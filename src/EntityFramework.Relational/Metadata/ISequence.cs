// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Metadata
{
    public interface ISequence
    {
        string Name { get; }
        string Schema { get; }
        long StartValue { get; }
        int IncrementBy { get; }
        long? MinValue { get; }
        long? MaxValue { get; }
        Type Type { get; }
        IModel Model { get; }
        bool Cycle { get; }
    }
}
