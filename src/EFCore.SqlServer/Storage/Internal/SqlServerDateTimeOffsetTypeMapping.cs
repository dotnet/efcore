// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
    {
        // Note: this array will be accessed using the precision as an index
        // so the order of the entries in this array is important
        private readonly string[] _dateTimeOffsetFormats =
        {
            "'{0:yyyy-MM-ddTHH:mm:sszzz}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.fzzz}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.ffzzz}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.fffzzz}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.ffffzzz}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.fffffzzz}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.ffffffzzz}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.fffffffzzz}'"
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerDateTimeOffsetTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = System.Data.DbType.DateTimeOffset)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(DateTimeOffset)),
                    storeType,
                    StoreTypePostfix.Precision,
                    dbType))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected SqlServerDateTimeOffsetTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqlServerDateTimeOffsetTypeMapping(parameters);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string SqlLiteralFormatString
        {
            get
            {
                if (Precision.HasValue)
                {
                    var precision = Precision.Value;
                    if (precision <= 7
                        && precision >= 0)
                    {
                        return _dateTimeOffsetFormats[precision];
                    }
                }

                return _dateTimeOffsetFormats[7];
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            if (Size.HasValue
                && Size.Value != -1)
            {
                parameter.Size = Size.Value;
            }

            if (Precision.HasValue)
            {
                parameter.Precision = unchecked((byte)Precision.Value);
            }
        }
    }
}
