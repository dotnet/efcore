// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators
{
    /// <summary>
    ///     Represents plugin method call translators.
    /// </summary>
    public interface IMethodCallTranslatorPlugin
    {
        /// <summary>
        ///     Gets the method call translators.
        /// </summary>
        IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
