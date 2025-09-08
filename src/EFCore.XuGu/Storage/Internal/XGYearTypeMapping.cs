// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGYearTypeMapping : XGTypeMapping
    {
        public static XGYearTypeMapping Default { get; } = new("year");

        public XGYearTypeMapping([NotNull] string storeType)
            : base(
                storeType,
                typeof(short),
                XGDbType.TinyInt,
                System.Data.DbType.Int16,
                jsonValueReaderWriter: JsonInt16ReaderWriter.Instance)
        {
        }

        protected XGYearTypeMapping(RelationalTypeMappingParameters parameters, XGDbType xgDbType)
            : base(parameters, xgDbType)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGYearTypeMapping(parameters, XGDbType);
    }
}
