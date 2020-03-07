// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
