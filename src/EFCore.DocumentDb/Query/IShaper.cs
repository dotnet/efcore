// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public interface IShaper<out T>
    {
        T Shape(QueryContext queryContext, ValueBuffer valueBuffer);
    }
}
