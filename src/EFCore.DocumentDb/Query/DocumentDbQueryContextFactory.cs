// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DocumentDbQueryContextFactory : QueryContextFactory
    {
        private readonly IDocumentDbClientService _documentDbClientService;

        public DocumentDbQueryContextFactory([NotNull] QueryContextDependencies dependencies,
            IDocumentDbClientService documentDbClientService)
            : base(dependencies)
        {
            _documentDbClientService = documentDbClientService;
        }

        public override QueryContext Create()
        {
            return new DocumentDbQueryContext(Dependencies, CreateQueryBuffer, _documentDbClientService);
        }
    }
}
