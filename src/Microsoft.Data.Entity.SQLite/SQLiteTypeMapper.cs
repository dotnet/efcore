// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteTypeMapper : RelationalTypeMapper
    {
        public override RelationalTypeMapping GetTypeMapping(
            string specifiedType, string storageName, Type propertyType, bool isKey, bool isConcurrencyToken)
        {
            // TODO: This is a hacky implementation for getting the DbType to use.

            var baseMapping = base.GetTypeMapping(specifiedType, storageName, propertyType, isKey, isConcurrencyToken);

            if (specifiedType != null)
            {
                return new RelationalTypeMapping(specifiedType, baseMapping.StoreType);
            }

            var types = SQLiteTypeMap.FromClrType(Nullable.GetUnderlyingType(propertyType) ?? propertyType)
                .DeclaredTypes;
            Contract.Assert(types.Any(), "types is empty.");

            return new RelationalTypeMapping(types.First(), baseMapping.StoreType);
        }
    }
}
