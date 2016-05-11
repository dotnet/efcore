// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Storage
{
    public interface IByteArrayRelationalTypeMapper
    {
        RelationalTypeMapping FindMapping(bool rowVersion, bool keyOrIndex, int? size);
    }
}
