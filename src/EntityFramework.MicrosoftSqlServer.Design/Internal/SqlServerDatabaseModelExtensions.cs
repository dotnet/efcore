// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Scaffolding.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public static class SqlServerDatabaseModelExtensions
    {
        public static SqlServerColumnModelAnnotations SqlServer([NotNull] this ColumnModel columnModel)
            => new SqlServerColumnModelAnnotations(Check.NotNull(columnModel, nameof(columnModel)));

        public static SqlServerIndexModelAnnotations SqlServer([NotNull] this IndexModel indexModel)
            => new SqlServerIndexModelAnnotations(Check.NotNull(indexModel, nameof(indexModel)));
    }
}
