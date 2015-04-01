// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Sqlite.Query
{
    public class SqliteValueReaderFactory : IRelationalValueReaderFactory
    {
        public virtual IValueReader CreateValueReader(DbDataReader dataReader) => new RelationalTypedValueReader(dataReader);
    }
}
