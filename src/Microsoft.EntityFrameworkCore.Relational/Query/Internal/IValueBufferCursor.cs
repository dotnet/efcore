// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query.Internal
{
    public interface IValueBufferCursor
    {
        ValueBuffer Current { get; }

        void BufferAll();
        Task BufferAllAsync(CancellationToken cancellationToken);
    }
}
