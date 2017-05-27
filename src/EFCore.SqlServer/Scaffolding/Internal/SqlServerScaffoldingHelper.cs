// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class SqlServerScaffoldingHelper : IScaffoldingHelper
    {
        private readonly ScaffoldingTypeMapper _scaffoldingTypeMapper;

        public SqlServerScaffoldingHelper([NotNull] ScaffoldingTypeMapper scaffoldingTypeMapper)
        {
            Check.NotNull(scaffoldingTypeMapper, nameof(scaffoldingTypeMapper));

            _scaffoldingTypeMapper = scaffoldingTypeMapper;
        }

        public virtual string GetProviderOptionsBuilder(string connectionString)
        {
            return $"{nameof(SqlServerDbContextOptionsExtensions.UseSqlServer)}({CSharpUtilities.Instance.GenerateVerbatimStringLiteral(connectionString)});";
        }

        public virtual TypeScaffoldingInfo GetTypeScaffoldingInfo(ColumnModel columnModel)
        {
            if (columnModel.DataType == null)
            {
                return null;
            }

            string underlyingDataType = null;
            columnModel.Table.Database.SqlServer().TypeAliases?.TryGetValue(
                SchemaQualifiedKey(columnModel.DataType, columnModel.SqlServer().DataTypeSchemaName), out underlyingDataType);

            var dataType = underlyingDataType ?? (columnModel.DataType + (columnModel.MaxLength.HasValue ? $"({columnModel.MaxLength.Value})" : ""));

            var typeScaffoldingInfo = _scaffoldingTypeMapper.FindMapping(dataType, keyOrIndex: false, rowVersion: false);

            if (underlyingDataType != null)
            {
                return new TypeScaffoldingInfo(
                    typeScaffoldingInfo.ClrType,
                    inferred: false,
                    scaffoldUnicode: typeScaffoldingInfo.ScaffoldUnicode,
                    scaffoldMaxLength: typeScaffoldingInfo.ScaffoldMaxLength);
            }

            return typeScaffoldingInfo;
        }

        private static string SchemaQualifiedKey(string name, string schema = null) => "[" + (schema ?? "") + "].[" + name + "]";
    }
}
