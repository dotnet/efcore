// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Tools;

internal class WrappedXunitException : XunitException
{
    public WrappedXunitException(WrappedException ex)
        : base(ex.ToString(), "(See error message)")
    {
    }
}
