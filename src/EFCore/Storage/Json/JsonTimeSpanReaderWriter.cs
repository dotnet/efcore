// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="TimeSpan" /> values.
/// </summary>
public sealed class JsonTimeSpanReaderWriter : JsonValueReaderWriter<TimeSpan>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonTimeSpanReaderWriter Instance { get; } = new();

    private JsonTimeSpanReaderWriter()
    {
    }

    /// <inheritdoc />
    public override TimeSpan FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => TimeSpan.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TimeSpan value)
        => writer.WriteStringValue(value.ToString("g", CultureInfo.InvariantCulture));

    private readonly Expression<Func<JsonTimeSpanReaderWriter>> _instanceLambda = () => Instance;

    /// <inheritdoc />
    [Experimental(EFDiagnostics.PrecompiledQueryExperimental)]
    public override Expression ConstructorExpression => _instanceLambda.Body;
}
