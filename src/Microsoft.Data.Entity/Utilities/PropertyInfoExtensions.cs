// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;

namespace System.Reflection
{
    [DebuggerStepThrough]
    internal static class PropertyInfoExtensions
    {
        public static bool IsStatic(this PropertyInfo property)
        {
            return (property.GetMethod ?? property.SetMethod).IsStatic;
        }
    }
}
