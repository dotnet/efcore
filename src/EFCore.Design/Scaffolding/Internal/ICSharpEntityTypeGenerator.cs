// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public interface ICSharpEntityTypeGenerator
    {
        string WriteCode([NotNull] IEntityType entityType, [NotNull] string @namespace, bool useDataAnnotations);
    }
}
