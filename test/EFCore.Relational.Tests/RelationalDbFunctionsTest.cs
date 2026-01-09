// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class RelationalDbFunctionsTest
{
    [ConditionalFact]
    public void Collate_on_client_throws()
        => Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(RelationalDbFunctionsExtensions.Collate)),
            Assert.Throws<InvalidOperationException>(() => EF.Functions.Collate("abc", "Latin1_General_CI_AS")).Message);

    [ConditionalFact]
    public void Least_on_client_throws()
        => Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(RelationalDbFunctionsExtensions.Least)),
            Assert.Throws<InvalidOperationException>(() => EF.Functions.Least(1, 2, 3)).Message);

    [ConditionalFact]
    public void Greatest_on_client_throws()
        => Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(RelationalDbFunctionsExtensions.Greatest)),
            Assert.Throws<InvalidOperationException>(() => EF.Functions.Greatest(1, 2, 3)).Message);

    [ConditionalFact]
    public void JsonExists_on_client_throws()
        => Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(RelationalDbFunctionsExtensions.JsonExists)),
            Assert.Throws<InvalidOperationException>(() => EF.Functions.JsonExists("{\"key\": 1}", "$.key")).Message);
}
