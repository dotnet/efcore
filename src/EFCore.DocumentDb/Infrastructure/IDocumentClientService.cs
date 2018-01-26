// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.Update;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public interface IDocumentDbClientService
    {
        DocumentClient Client { get; }
        string DatabaseId { get; }

        IEnumerator<Document> ExecuteQuery(
            string collectionId,
            SqlQuerySpec sqlQuerySpec);
        Task<int> SaveChangesAsync(IReadOnlyList<IUpdateEntry> entries, CancellationToken cancellationToken);
    }
}
