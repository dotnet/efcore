// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class DocumentDbDatabase : Database
    {
        private readonly IDocumentDbClientService _documentDbClientService;

        public DocumentDbDatabase([NotNull] DatabaseDependencies dependencies,
            IDocumentDbClientService documentDbClientService)
            : base(dependencies)
        {
            _documentDbClientService = documentDbClientService;
        }

        public override int SaveChanges(IReadOnlyList<IUpdateEntry> entries)
        {
            return SaveChangesAsync(entries).GetAwaiter().GetResult();
        }

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries, CancellationToken cancellationToken = default)
        {
            return _documentDbClientService.SaveChangesAsync(entries, cancellationToken);
        }
    }
}
