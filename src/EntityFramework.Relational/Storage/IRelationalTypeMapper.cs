// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalTypeMapper
    {
        RelationalTypeMapping GetMapping([NotNull] IProperty property);
        RelationalTypeMapping GetMapping([NotNull] Type clrType);
        RelationalTypeMapping GetMapping([NotNull] string typeName);
    }
}
