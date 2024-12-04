// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Data;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Json.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteHalfTypeMapping : RelationalTypeMapping
    {

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static SqliteHalfTypeMapping Default { get; } = new("REAL");

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteHalfTypeMapping(
            string storeType,
            DbType? dbType = System.Data.DbType.Single)
            : this(new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(Half), jsonValueReaderWriter: SqliteJsonHalfReaderWriter.Instance),
                storeType,
                dbType: dbType))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected SqliteHalfTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters) { }

        /// <inheritdoc/>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) => new SqliteHalfTypeMapping(parameters);
    }
}
