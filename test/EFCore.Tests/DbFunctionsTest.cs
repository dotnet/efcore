// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class DbFunctionsTest
{
    [ConditionalFact]
    public void Like_on_client_throws()
        => Assert.Equal(
            CoreStrings.FunctionOnClient(nameof(DbFunctionsExtensions.Like)),
            Assert.Throws<InvalidOperationException>(
                () => EF.Functions.Like("abc", "abc")).Message);
}
