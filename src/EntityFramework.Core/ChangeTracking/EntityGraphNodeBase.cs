// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class EntityGraphNodeBase<TNode>
        where TNode : EntityGraphNodeBase<TNode>
    {
        protected EntityGraphNodeBase(
            [NotNull] InternalEntityEntry internalEntityEntry, 
            [CanBeNull] INavigation inboundNavigation)
        {
            Check.NotNull(internalEntityEntry, nameof(internalEntityEntry));

            Entry = internalEntityEntry;
            InboundNavigation = inboundNavigation;
        }

        public virtual INavigation InboundNavigation { get; }

        public virtual InternalEntityEntry Entry { get; }

        public virtual object NodeState { get; [param: CanBeNull] set; }

        public abstract TNode CreateNode(
            [NotNull] TNode currentNode, 
            [NotNull] InternalEntityEntry internalEntityEntry, 
            [NotNull] INavigation reachedVia);
    }
}
