// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage
{
    public interface IRelationalTypeMapper
    {
        RelationalTypeMapping MapPropertyType([NotNull] IProperty property);
        RelationalTypeMapping GetDefaultMapping([NotNull] Type clrType);
    }
}
