// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    internal static class TypeExtensions
    {
        public static string GetNestedName(this Type type)
        {
            return type.FullName.Replace("+", ".");
        }
    }
}
