// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CollectionMaterializationContext
    {
        public CollectionMaterializationContext(
            [NotNull] object parent,
            [NotNull] object collection,
            [NotNull] object[] parentIdentifier,
            [NotNull] object[] outerIdentifier)
        {
            Parent = parent;
            Collection = collection;
            ParentIdentifier = parentIdentifier;
            OuterIdentifier = outerIdentifier;
            ResultContext = new ResultContext();
        }

        public virtual ResultContext ResultContext { get; }
        public virtual object Parent { get; }
        public virtual object Collection { get; }
        public virtual object[] ParentIdentifier { get; }
        public virtual object[] OuterIdentifier { get; }
        public virtual object[] SelfIdentifier { get; private set; }

        public virtual void UpdateSelfIdentifier([NotNull] object[] selfIdentifier)
        {
            SelfIdentifier = selfIdentifier;
        }
    }
}
