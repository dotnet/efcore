// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DocumentDbQueryContext : QueryContext
    {
        private readonly IDocumentDbClientService _documentDbClientService;

        public DocumentDbQueryContext(
            [NotNull] QueryContextDependencies dependencies,
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            IDocumentDbClientService documentDbClientService)
            : base(dependencies, queryBufferFactory)
        {
            _documentDbClientService = documentDbClientService;
        }

        public IEnumerator<Document> ExecuteQuery(
            string collectionId,
            SqlQuerySpec sqlQuerySpec)
        {
            return _documentDbClientService.ExecuteQuery(collectionId, sqlQuerySpec);
        }
    }
}
