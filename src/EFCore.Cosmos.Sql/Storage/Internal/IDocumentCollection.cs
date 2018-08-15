// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public interface IDocumentCollection
    {
        Task SaveAsync(IUpdateEntry entry, CancellationToken cancellationToken);
    }
}
