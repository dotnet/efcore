// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.Data.Entity.Utilities
{
    [DebuggerStepThrough]
    internal static class DebugCheck
    {
        [Conditional("DEBUG")]
        public static void NotNull([ValidatedNotNull] object value)
        {
            Debug.Assert(value != null);
        }

        [Conditional("DEBUG")]
        public static void NotEmpty([ValidatedNotNull] string value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(value));
        }
    }
}
