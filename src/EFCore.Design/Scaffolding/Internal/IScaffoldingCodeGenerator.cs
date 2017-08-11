// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public interface IScaffoldingCodeGenerator
    {
        string FileExtension { get; }
        IFileService FileService { get; }

        IList<string> GetExistingFilePaths(
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            [NotNull] IEnumerable<IEntityType> entityTypes);

        IList<string> GetReadOnlyFilePaths(
            [NotNull] string outputPath,
            [NotNull] string dbContextClassName,
            [NotNull] IEnumerable<IEntityType> entityTypes);

        ReverseEngineerFiles WriteCode(
            [NotNull] IModel model,
            [NotNull] string outputPath,
            [NotNull] string @namespace,
            [NotNull] string contextName,
            [NotNull] string connectionString,
            bool dataAnnotations);
    }
}
