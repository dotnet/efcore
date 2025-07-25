// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json
{
    /// <summary>
    ///     Reads and writes JSON for Half type values.
    /// </summary>
    public sealed class JsonHalfReaderWriter : JsonValueReaderWriter<Half>
    {
        private const string HalfFormatConst = "{0:0.0###}";

        private static readonly PropertyInfo InstanceProperty = typeof(JsonHalfReaderWriter).GetProperty(nameof(Instance))!;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static JsonHalfReaderWriter Instance { get; } = new();

        private JsonHalfReaderWriter()
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
