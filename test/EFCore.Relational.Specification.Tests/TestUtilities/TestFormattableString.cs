﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestFormattableString : FormattableString
    {
        private readonly object[] _arguments;

        public TestFormattableString(string format, object[] arguments)
        {
            Format = format;
            _arguments = arguments;
        }

        public override object GetArgument(int index)
        {
            throw new NotImplementedException();
        }

        public override object[] GetArguments()
            => _arguments;

        public override string ToString(IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }

        public override int ArgumentCount { get; }
        public override string Format { get; }
    }
}
