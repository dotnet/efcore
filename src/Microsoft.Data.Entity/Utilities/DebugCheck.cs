// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Utilities
{
    using System.Diagnostics;

    [DebuggerStepThrough]
    internal static class DebugCheck
    {
        [Conditional("DEBUG")]
        public static void NotNull(object value)
        {
            Debug.Assert(value != null);
        }

        [Conditional("DEBUG")]
        public static void NotEmpty(string value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(value));
        }
    }
}
