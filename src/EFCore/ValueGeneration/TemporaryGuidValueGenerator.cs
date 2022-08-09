// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ValueGeneration;

/// <summary>
///     Generates <see cref="Guid" /> values using <see cref="Guid.NewGuid()" />.
///     The generated values are temporary, meaning they will be replaced by database
///     generated values when the entity is saved.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information and examples.
/// </remarks>
public class TemporaryGuidValueGenerator : GuidValueGenerator
{
    /// <summary>
    ///     Gets a value to be assigned to a property.
    /// </summary>
    /// <returns>The value to be assigned to a property.</returns>
    public override bool GeneratesTemporaryValues
        => true;
}
