// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IRelationalPropertyAnnotations
    {
        string ColumnName { get; }
        string ColumnType { get; }
        string DefaultValueSql { get; }
        string ComputedColumnSql { get; }
        object DefaultValue { get; }
    }
}
