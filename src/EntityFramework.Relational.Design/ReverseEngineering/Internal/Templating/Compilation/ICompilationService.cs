// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating.Compilation
{
    public interface ICompilationService
    {
        CompilationResult Compile(
            [NotNull] IEnumerable<string> contents, [NotNull] List<MetadataReference> references);
    }
}
