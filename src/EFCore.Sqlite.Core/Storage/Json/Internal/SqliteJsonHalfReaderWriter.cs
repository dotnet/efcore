// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Json.Internal
{
    /// <summary>
    ///     The Sqlite-specific JsonValueReaderWrite for Half. Generates a string representation instead of a JSON number, in order to match
    ///     our SQLite non-JSON representation.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public sealed class SqliteJsonHalfReaderWriter : JsonValueReaderWriter<Half>
    {
        private const string HalfFormatConst = "{0:0.0###}";

        private static readonly PropertyInfo InstanceProperty = typeof(SqliteJsonHalfReaderWriter).GetProperty(nameof(Instance))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static SqliteJsonHalfReaderWriter Instance { get; } = new();

        private SqliteJsonHalfReaderWriter()
        {

        }

        /// <inheritdoc/>
        public override Expression ConstructorExpression
            => Expression.Property(null, InstanceProperty);

        /// <inheritdoc/>
        public override Half FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null) =>
            Half.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public override void ToJsonTyped(Utf8JsonWriter writer, Half value) =>
            writer.WriteStringValue(string.Format(CultureInfo.InvariantCulture, HalfFormatConst, value));
    }
}
