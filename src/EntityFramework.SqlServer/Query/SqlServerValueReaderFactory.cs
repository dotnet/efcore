// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.SqlServer.Query
{
    public class SqlServerValueReaderFactory : IRelationalValueReaderFactory
    {
        public virtual IValueReader CreateValueReader(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            return new RelationalObjectArrayValueReader(dataReader);
        }
    }
}
