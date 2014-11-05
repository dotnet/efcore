// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteDatabaseBuilder : DatabaseBuilder
    {
        public SQLiteDatabaseBuilder([NotNull] SQLiteTypeMapper typeMapper)
            : base(typeMapper)
        {
        }

        public virtual new SQLiteTypeMapper TypeMapper
        {
            get { return (SQLiteTypeMapper)base.TypeMapper; }
        }

        protected override Sequence BuildSequence(IProperty property)
        {
            return null;
        }
    }
}
