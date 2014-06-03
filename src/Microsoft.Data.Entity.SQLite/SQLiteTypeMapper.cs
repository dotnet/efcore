// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteTypeMapper : RelationalTypeMapper
    {
        public override RelationalTypeMapping GetTypeMapping(
            string specifiedType, string storageName, Type propertyType, bool isKey, bool isConcurrencyToken)
        {
            propertyType = propertyType.UnwrapNullableType();
            if (propertyType.GetTypeInfo().IsEnum)
            {
                propertyType = Enum.GetUnderlyingType(propertyType);
            }

            var map = SQLiteTypeMap.FromClrType(propertyType);
            if (specifiedType != null)
            {
                map = SQLiteTypeMap.FromDeclaredType(specifiedType, map.SQLiteType);
            }

            // TODO: Leverage base implementation more
            return new RelationalTypeMapping(map.DeclaredTypes.First(), map.DbType);
        }
    }
}
