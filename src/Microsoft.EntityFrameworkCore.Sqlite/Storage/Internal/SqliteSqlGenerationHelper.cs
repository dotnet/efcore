// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.Data.Entity.Storage.Internal
{
    public class SqliteSqlGenerationHelper : RelationalSqlGenerationHelper
    {
        protected override string DateTimeFormat => @"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF";
        protected override string DateTimeOffsetFormat => @"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz";

        // TODO throw a logger warning that this call was improperly made. The SQLite provider should never specify a schema
        public override string DelimitIdentifier(string name, string schema) => base.DelimitIdentifier(name);

        protected override string GenerateLiteralValue(DateTime value)
            => $"'{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'";

        protected override string GenerateLiteralValue(DateTimeOffset value)
            => $"'{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'";
    }
}
