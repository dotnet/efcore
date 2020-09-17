// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteStringTypeMapping : StringTypeMapping
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteStringTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        public SqliteStringTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null,
            bool unicode = false,
            int? size = null)
            : base(storeType, dbType, unicode, size)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteStringTypeMapping" /> class.
        /// </summary>
        /// <param name="parameters"> Parameter object for <see cref="RelationalTypeMapping" />. </param>
        protected SqliteStringTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters"> The parameters for this mapping. </param>
        /// <returns> The newly created mapping. </returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqliteStringTypeMapping(parameters);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
        {
            var stringValue = (string)value;
            var builder = new StringBuilder();

            var start = 0;
            int i;
            int length;
            var concatenated = false;
            var openApostrophe = false;
            for (i = 0; i < stringValue.Length; i++)
            {
                var lineFeed = stringValue[i] == '\n';
                var carriageReturn = stringValue[i] == '\r';
                var apostrophe = stringValue[i] == '\'';
                if (lineFeed || carriageReturn || apostrophe)
                {
                    length = i - start;
                    if (length != 0)
                    {
                        if (!openApostrophe)
                        {
                            if (builder.Length != 0)
                            {
                                builder.Append(" || ");
                                concatenated = true;
                            }

                            builder.Append('\'');
                            openApostrophe = true;
                        }

                        builder.Append(stringValue.AsSpan().Slice(start, length));
                    }

                    if (lineFeed || carriageReturn)
                    {
                        if (openApostrophe)
                        {
                            builder.Append('\'');
                            openApostrophe = false;
                        }

                        if (builder.Length != 0)
                        {
                            builder.Append(" || ");
                            concatenated = true;
                        }

                        builder
                            .Append("CHAR(")
                            .Append(lineFeed ? "10" : "13")
                            .Append(')');

                    }
                    else if (apostrophe)
                    {
                        if (!openApostrophe)
                        {
                            if (builder.Length != 0)
                            {
                                builder.Append(" || ");
                                concatenated = true;
                            }

                            builder.Append("'");
                            openApostrophe = true;
                        }

                        builder.Append("''");
                    }

                    start = i + 1;
                }
            }

            length = i - start;
            if (length != 0)
            {
                if (!openApostrophe)
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(" || ");
                        concatenated = true;
                    }

                    builder.Append('\'');
                    openApostrophe = true;
                }

                builder.Append(stringValue.AsSpan().Slice(start, length));
            }

            if (openApostrophe)
            {
                builder.Append('\'');
            }

            if (concatenated)
            {
                builder
                    .Insert(0, '(')
                    .Append(')');
            }

            if (builder.Length == 0)
            {
                builder.Append("''");
            }

            return builder.ToString();
        }
    }
}
