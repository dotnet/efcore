// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class JsonQueryDbFunctionsRelationalTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : JsonQueryRelationalFixture, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task JsonExists_With_ConstantValue(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => EF.Functions.JsonExists("{\"Name\": \"Test\"}", "$.Name") == true),
            ss => ss.Set<JsonEntityBasic>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task JsonExists_With_StringJsonProperty(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityStringConversion>().Where(x => EF.Functions.JsonExists(x.StringJsonValue, "$.Name") == true),
            ss => ss.Set<JsonEntityStringConversion>().Where(x => HasJsonProperty(x.StringJsonValue, "Name") == true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task JsonExists_With_StringConversionJsonProperty(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityStringConversion>().Where(x => EF.Functions.JsonExists(x.ReferenceRoot, "$.Name") == true),
            ss => ss.Set<JsonEntityStringConversion>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task JsonExists_With_OwnedJsonProperty(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<JsonEntityBasic>().Where(x => EF.Functions.JsonExists(x.OwnedReferenceRoot, "$.Name") == true),
            ss => ss.Set<JsonEntityBasic>());

    private static bool? HasJsonProperty(string jsonString, string propertyName)
    {
        if (jsonString is null)
            return null;

        try
        {
            using var document = JsonDocument.Parse(jsonString);
            return document.RootElement.TryGetProperty(propertyName, out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
