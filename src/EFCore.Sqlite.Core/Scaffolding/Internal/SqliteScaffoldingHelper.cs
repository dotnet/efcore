// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class SqliteScaffoldingHelper : IScaffoldingHelper
    {
        private readonly ScaffoldingTypeMapper _scaffoldingTypeMapper;

        public SqliteScaffoldingHelper([NotNull] ScaffoldingTypeMapper scaffoldingTypeMapper)
        {
            Check.NotNull(scaffoldingTypeMapper, nameof(scaffoldingTypeMapper));

            _scaffoldingTypeMapper = scaffoldingTypeMapper;
        }

        public virtual string GetProviderOptionsBuilder(string connectionString)
        {
            return $"{nameof(SqliteDbContextOptionsBuilderExtensions.UseSqlite)}({GenerateVerbatimStringLiteral(connectionString)});";
        }

        public virtual TypeScaffoldingInfo GetTypeScaffoldingInfo(ColumnModel columnModel)
        {
            if (columnModel.StoreType == null)
            {
                return null;
            }

            var typeScaffoldingInfo = _scaffoldingTypeMapper.FindMapping(columnModel.StoreType, keyOrIndex: false, rowVersion: false);

            if (columnModel.StoreType == "")
            {
                return new TypeScaffoldingInfo(
                    typeScaffoldingInfo.ClrType,
                    inferred: true,
                    scaffoldUnicode: typeScaffoldingInfo.ScaffoldUnicode,
                    scaffoldMaxLength: typeScaffoldingInfo.ScaffoldMaxLength);
            }

            return typeScaffoldingInfo;
        }

        private static string GenerateVerbatimStringLiteral(string value) => "@\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
