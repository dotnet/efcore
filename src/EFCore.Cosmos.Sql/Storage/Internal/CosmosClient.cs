// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Storage.Internal
{
    public class CosmosClient
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;

        // TODO: Do caching here.
        public CosmosClient(IDbContextOptions dbContextOptions,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
        {
            var options = dbContextOptions.Extensions.OfType<CosmosSqlDbOptionsExtension>().Single();

            DatabaseId = options.DatabaseName;
            DocumentClient = new DocumentClient(options.ServiceEndPoint, options.AuthKeyOrResourceToken);
            _commandLogger = commandLogger;
        }

        public virtual DocumentClient DocumentClient { get; }

        public virtual string DatabaseId { get; }

        public IEnumerator<Document> ExecuteSqlQuery(
            string collectionId,
            SqlQuerySpec sqlQuerySpec)
        {
            _commandLogger.ExecutingSqlQuery(sqlQuerySpec);

            return DocumentClient
                .CreateDocumentQuery<Document>(
                    UriFactory.CreateDocumentCollectionUri(DatabaseId, collectionId),
                    sqlQuerySpec)
                .GetEnumerator();
        }
    }
}
