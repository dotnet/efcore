// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     Represents plugin member translators.
    /// </summary>
    public interface IMemberTranslatorPlugin
    {
        /// <summary>
        ///     Gets the member translators.
        /// </summary>
        IEnumerable<IMemberTranslator> Translators { get; }
    }
}
