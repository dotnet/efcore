// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestFormattableString(string format, object?[] arguments) : FormattableString
{
    public override object GetArgument(int index)
        => throw new NotImplementedException();

    public override object?[] GetArguments()
        => arguments;

    public override string ToString(IFormatProvider? formatProvider)
        => throw new NotImplementedException();

    public override int ArgumentCount { get; }
    public override string Format { get; } = format;
}
