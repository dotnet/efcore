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
            return $"{nameof(SqliteDbContextOptionsBuilderExtensions.UseSqlite)}({CSharpUtilities.Instance.GenerateVerbatimStringLiteral(connectionString)});";
        }

        public virtual TypeScaffoldingInfo GetTypeScaffoldingInfo(ColumnModel columnModel)
        {
            if (columnModel.DataType == null)
            {
                return null;
            }

            var dataType = columnModel.DataType + (columnModel.MaxLength.HasValue ? $"({columnModel.MaxLength.Value})" : "");

            var typeScaffoldingInfo = _scaffoldingTypeMapper.FindMapping(dataType, keyOrIndex: false, rowVersion: false);

            if (columnModel.DataType == "")
            {
                return new TypeScaffoldingInfo(
                    typeScaffoldingInfo.ClrType,
                    inferred: true,
                    scaffoldUnicode: typeScaffoldingInfo.ScaffoldUnicode,
                    scaffoldMaxLength: typeScaffoldingInfo.ScaffoldMaxLength);
            }

            return typeScaffoldingInfo;
        }
    }
}
