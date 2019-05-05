// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         Creates <see cref="INodeTypeProvider" /> instances for use by the query compiler
    ///         based on a <see cref="MethodInfoBasedNodeTypeRegistry" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class MethodInfoBasedNodeTypeRegistryFactory : INodeTypeProviderFactory
    {
        private static readonly object _syncLock = new object();

        private static readonly MethodNameBasedNodeTypeRegistry _methodNameBasedNodeTypeRegistry
            = MethodNameBasedNodeTypeRegistry.CreateFromRelinqAssembly();

        private readonly MethodInfoBasedNodeTypeRegistry _methodInfoBasedNodeTypeRegistry;
        private volatile INodeTypeProvider[] _nodeTypeProviders;

        /// <summary>
        ///     Creates a new <see cref="MethodInfoBasedNodeTypeRegistryFactory" /> that will use the given
        ///     <see cref="MethodInfoBasedNodeTypeRegistry" />
        /// </summary>
        /// <param name="methodInfoBasedNodeTypeRegistry">The registry to use./></param>
        public MethodInfoBasedNodeTypeRegistryFactory(
            [NotNull] MethodInfoBasedNodeTypeRegistry methodInfoBasedNodeTypeRegistry)
        {
            Check.NotNull(methodInfoBasedNodeTypeRegistry, nameof(methodInfoBasedNodeTypeRegistry));

            _methodInfoBasedNodeTypeRegistry = methodInfoBasedNodeTypeRegistry;

            _methodInfoBasedNodeTypeRegistry
                .Register(ToOrderedEnumerableExpressionNode.SupportedMethods, typeof(ToOrderedEnumerableExpressionNode));
            _methodInfoBasedNodeTypeRegistry
                .Register(ToOrderedQueryableExpressionNode.SupportedMethods, typeof(ToOrderedQueryableExpressionNode));
            _methodInfoBasedNodeTypeRegistry
                .Register(TrackingExpressionNode.SupportedMethods, typeof(TrackingExpressionNode));
            _methodInfoBasedNodeTypeRegistry
                .Register(TagExpressionNode.SupportedMethods, typeof(TagExpressionNode));
            _methodInfoBasedNodeTypeRegistry
                .Register(IgnoreQueryFiltersExpressionNode.SupportedMethods, typeof(IgnoreQueryFiltersExpressionNode));
            _methodInfoBasedNodeTypeRegistry
                .Register(IncludeExpressionNode.SupportedMethods, typeof(IncludeExpressionNode));
            _methodInfoBasedNodeTypeRegistry
                .Register(StringIncludeExpressionNode.SupportedMethods, typeof(StringIncludeExpressionNode));
            _methodInfoBasedNodeTypeRegistry
                .Register(ThenIncludeExpressionNode.SupportedMethods, typeof(ThenIncludeExpressionNode));

            _nodeTypeProviders = new INodeTypeProvider[] { _methodInfoBasedNodeTypeRegistry, _methodNameBasedNodeTypeRegistry };
        }

        /// <summary>
        ///     Registers methods to be used with the <see cref="INodeTypeProvider" />.
        /// </summary>
        /// <param name="methods">The methods to register.</param>
        /// <param name="nodeType">The node type for these methods.</param>
        public virtual void RegisterMethods(IEnumerable<MethodInfo> methods, Type nodeType)
        {
            Check.NotNull(methods, nameof(methods));
            Check.NotNull(nodeType, nameof(nodeType));

            lock (_syncLock)
            {
                _methodInfoBasedNodeTypeRegistry.Register(methods, nodeType);
                _nodeTypeProviders = new INodeTypeProvider[] { _methodInfoBasedNodeTypeRegistry, _methodNameBasedNodeTypeRegistry };
            }
        }

        /// <summary>
        ///     Creates a <see cref="INodeTypeProvider" />.
        /// </summary>
        /// <returns>The <see cref="INodeTypeProvider" />.</returns>
        public virtual INodeTypeProvider Create() => new CompoundNodeTypeProvider(_nodeTypeProviders);
    }
}
