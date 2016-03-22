// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class SqliteFullAnnotationNames : RelationalFullAnnotationNames
    {
        protected SqliteFullAnnotationNames(string prefix)
            : base(prefix)
        {
            Autoincrement = "Autoincrement";
            InlinePrimaryKey = "InlinePrimaryKey";
            InlinePrimaryKeyName = "InlinePrimaryKeyName";
        }

        public new static SqliteFullAnnotationNames Instance { get; } = new SqliteFullAnnotationNames(SqliteAnnotationNames.Prefix);

        public readonly string Autoincrement;
        public readonly string InlinePrimaryKey;
        public readonly string InlinePrimaryKeyName;
    }
}
