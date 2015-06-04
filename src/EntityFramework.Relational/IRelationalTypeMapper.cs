// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.Relational
{
    public interface IRelationalTypeMapper
    {
        RelationalTypeMapping MapPropertyType([NotNull] IProperty property);
        RelationalTypeMapping MapSequenceType([NotNull] ISequence sequence);
    }
}
