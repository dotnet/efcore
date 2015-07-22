// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;

namespace Microsoft.Data.Entity.Relational.Design.Templating
{
    public interface ITemplating
    {
        Task<TemplateResult> RunTemplateAsync(
            [NotNull] string content,
            [NotNull] dynamic templateModel,
            [NotNull] IDatabaseMetadataModelProvider provider,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
