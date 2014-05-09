// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteSqlGenerator : SqlGenerator
    {
        public override IEnumerable<KeyValuePair<Column, string>> CreateWhereConditionsForStoreGeneratedKeys(
            Column[] storeGeneratedKeyColumns)
        {
            return from k in storeGeneratedKeyColumns
                   where k.ValueGenerationStrategy == StoreValueGenerationStrategy.Identity
                   select new KeyValuePair<Column, string>(k, "last_insert_rowid()");
        }
    }
}
