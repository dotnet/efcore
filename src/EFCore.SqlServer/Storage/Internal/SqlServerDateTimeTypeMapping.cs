// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient; // Note: Hard reference to SqlClient here.
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerDateTimeTypeMapping : DateTimeTypeMapping
    {
        private const string DateFormatConst = "'{0:yyyy-MM-dd}'";
        private const string SmallDateTimeFormatConst = "'{0:yyyy-MM-ddTHH:mm:ss}'";
        private const string DateTimeFormatConst = "'{0:yyyy-MM-ddTHH:mm:ss.fff}'";

        private readonly string[] _dateTime2Formats =
        {
            "'{0:yyyy-MM-ddTHH:mm:ss}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.fK}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.ffK}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.fffK}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.ffffK}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.fffffK}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.ffffffK}'",
            "'{0:yyyy-MM-ddTHH:mm:ss.fffffffK}'"
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerDateTimeTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null)
            : base(storeType, dbType)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected SqlServerDateTimeTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
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

            // Workaround for a SQLClient bug
            if (DbType == System.Data.DbType.Date)
            {
                ((SqlParameter)parameter).SqlDbType = SqlDbType.Date;
            }

            if (Size.HasValue
                && Size.Value != -1)
            {
                parameter.Size = Size.Value;
            }
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqlServerDateTimeTypeMapping(parameters);

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
                switch (StoreType)
                {
                    case "date":
                        return DateFormatConst;
                    case "datetime":
                        return DateTimeFormatConst;
                    case "smalldatetime":
                        return SmallDateTimeFormatConst;
                    default:
                        if (Size.HasValue)
                        {
                            var size = Size.Value;
                            if (size <= 7
                                && size >= 0)
                            {
                                return _dateTime2Formats[size];
                            }
                        }

                        return _dateTime2Formats[7];
                }
            }
        }
    }
}
