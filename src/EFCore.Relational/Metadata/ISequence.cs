// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface ISequence
    {
        string Name { get; }
        string Schema { get; }
        IModel Model { get; }
        long StartValue { get; }
        int IncrementBy { get; }
        long? MinValue { get; }
        long? MaxValue { get; }
        Type ClrType { get; }
        bool IsCyclic { get; }
    }
}
