// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public interface IForeignKeyRemovedConvention
    {
        void Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [NotNull] ForeignKey foreignKey);
    }
}
