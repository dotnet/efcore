// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DefaultMethodInfoBasedNodeTypeRegistryFactory : MethodInfoBasedNodeTypeRegistryFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DefaultMethodInfoBasedNodeTypeRegistryFactory()
            : base(MethodInfoBasedNodeTypeRegistry.CreateFromRelinqAssembly())
        {
        }
    }
}
