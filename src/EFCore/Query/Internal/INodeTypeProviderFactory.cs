// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Linq.Parsing.Structure;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     Creates <see cref="INodeTypeProvider" /> instances for use by the query compiler.
    /// </summary>
    public interface INodeTypeProviderFactory
    {
        /// <summary>
        ///     Creates a <see cref="INodeTypeProvider" />.
        /// </summary>
        /// <returns>The <see cref="INodeTypeProvider" />.</returns>
        INodeTypeProvider Create();

        /// <summary>
        ///     Registers methods to be used with the <see cref="INodeTypeProvider" />.
        /// </summary>
        /// <param name="methods">The methods to register.</param>
        /// <param name="nodeType">The node type for these methods.</param>
        void RegisterMethods([NotNull] IEnumerable<MethodInfo> methods, [NotNull] Type nodeType);
    }
}
