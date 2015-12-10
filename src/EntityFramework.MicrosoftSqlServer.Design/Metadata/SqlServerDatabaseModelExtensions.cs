// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public static class SqlServerDatabaseModelExtensions
    {
        public static SqlServerColumnModelAnnotations SqlServer([NotNull] this ColumnModel column)
            => new SqlServerColumnModelAnnotations(column);

        public static SqlServerIndexModelAnnotations SqlServer([NotNull] this IndexModel index)
            => new SqlServerIndexModelAnnotations(index);
    }
}
