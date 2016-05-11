// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IRelationalTypeMapper
    {
        RelationalTypeMapping FindMapping([NotNull] IProperty property);
        RelationalTypeMapping FindMapping([NotNull] Type clrType);

        RelationalTypeMapping FindMapping([NotNull] string storeType);
        void ValidateTypeName([NotNull] string storeType);

        IByteArrayRelationalTypeMapper ByteArrayMapper { get; }
        IStringRelationalTypeMapper StringMapper { get; }
    }
}
