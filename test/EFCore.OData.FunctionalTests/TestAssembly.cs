﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    internal sealed class TestAssembly : Assembly
    {
        readonly Type[] _types;

        public TestAssembly(params Type[] types)
        {
            _types = types;
        }

        public override Type[] GetTypes()
        {
            return _types;
        }
    }
}
