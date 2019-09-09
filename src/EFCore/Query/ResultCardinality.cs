// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
    public enum ResultCardinality
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        Enumerable,
        Single,
        SingleOrDefault
#pragma warning restore SA1602 // Enumeration items should be documented
    }
}
