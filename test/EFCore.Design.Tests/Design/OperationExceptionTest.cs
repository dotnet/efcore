// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

public class OperationExceptionTest
{
    [ConditionalFact]
    public void OperationException_exposes_public_empty_constructor()
        => new OperationException();

    [ConditionalFact]
    public void OperationException_exposes_public_string_constructor()
        => Assert.Equal("Foo", new OperationException("Foo").Message);

    [ConditionalFact]
    public void OperationException_exposes_public_string_and_inner_exception_constructor()
    {
        var inner = new Exception();

        var ex = new OperationException("Foo", inner);

        Assert.Equal("Foo", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
