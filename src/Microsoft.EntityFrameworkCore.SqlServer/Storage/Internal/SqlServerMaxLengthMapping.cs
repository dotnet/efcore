// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class SqlServerMaxLengthMapping : RelationalTypeMapping
    {
        private readonly int _maxSpecificSize;

        public SqlServerMaxLengthMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            DbType? dbType = null)
            : this(storeType, clrType, dbType, unicode: false, size: null)
        {
        }

        public SqlServerMaxLengthMapping(
            [NotNull] string storeType,
            [NotNull] Type clrType,
            DbType? dbType,
            bool unicode,
            int? size,
            bool hasNonDefaultUnicode = false,
            bool hasNonDefaultSize = false)
            : base(storeType, clrType, dbType, unicode, size, hasNonDefaultUnicode, hasNonDefaultSize)
        {
            _maxSpecificSize =
                (dbType == System.Data.DbType.AnsiString)
                || (dbType == System.Data.DbType.AnsiStringFixedLength)
                || (dbType == System.Data.DbType.Binary)
                    ? 8000
                    : 4000;
        }

        protected override void ConfigureParameter(DbParameter parameter)
        {
            // For strings and byte arrays, set the max length to 8000 bytes if the data will
            // fit so as to avoid query cache fragmentation by setting lots of differet Size
            // values otherwise always set to -1 (unbounded) to avoid SQL client size inference.

            var length = (parameter.Value as string)?.Length ?? (parameter.Value as byte[])?.Length;

            parameter.Size = length != null && length <= _maxSpecificSize
                ? _maxSpecificSize
                : -1;
        }
    }
}
