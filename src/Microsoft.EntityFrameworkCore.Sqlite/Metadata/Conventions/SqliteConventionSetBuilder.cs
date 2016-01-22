// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class SqliteConventionSetBuilder : RelationalConventionSetBuilder
    {
        public SqliteConventionSetBuilder([NotNull] IRelationalTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        public static ConventionSet Build()
            => new SqliteConventionSetBuilder(new SqliteTypeMapper())
                .AddConventions(new CoreConventionSetBuilder().CreateConventionSet());
    }
}
