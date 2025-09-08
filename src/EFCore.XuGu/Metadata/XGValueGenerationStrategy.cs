// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    // ReSharper disable once SA1602
    public enum XGValueGenerationStrategy
    {
        /// <summary>
        /// TODO
        /// </summary>
        None,

        /// <summary>
        /// TODO
        /// </summary>
        IdentityColumn,

        /// <summary>
        /// TODO
        /// </summary>
        ComputedColumn // TODO: Remove this and only use .HasComputedColumnSql() instead in EF Core 5
    }
}
