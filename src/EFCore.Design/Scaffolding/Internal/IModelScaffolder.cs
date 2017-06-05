// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public interface IModelScaffolder
    {
        Task<ReverseEngineerFiles> GenerateAsync(
               [NotNull] string connectionString,
               [NotNull] TableSelectionSet tableSelectionSet,
               [NotNull] string projectPath,
               [CanBeNull] string outputPath,
               [NotNull] string rootNamespace,
               [CanBeNull] string contextName,
               bool useDataAnnotations,
               bool overwriteFiles,
               CancellationToken cancellationToken = default(CancellationToken));
    }
}
