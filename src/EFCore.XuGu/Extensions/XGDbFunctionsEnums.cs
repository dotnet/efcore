// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore
{
    public enum XGMatchSearchMode
    {
        /// <summary>
        /// Perform a natural language search for a string against a text collection.
        /// </summary>
        NaturalLanguage = 0,

        /// <summary>
        /// Perform a natural language search with query expansion for a string against a text collection.
        /// </summary>
        NaturalLanguageWithQueryExpansion = 1,

        /// <summary>
        /// Perform a boolean search for a string against a text collection.
        /// </summary>
        Boolean = 2,
    }
}
