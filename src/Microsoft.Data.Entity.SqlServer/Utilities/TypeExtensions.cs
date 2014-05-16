// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace System
{
    [DebuggerStepThrough]
    internal static class TypeExtensions
    {
        // TODO: Share code with core
        public static bool IsInteger(this Type type)
        {
            return type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(short)
                   || type == typeof(byte);
        }
    }
}
