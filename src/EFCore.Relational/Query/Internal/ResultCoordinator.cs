// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ResultCoordinator
    {
        public ResultCoordinator()
        {
            ResultContext = new ResultContext();
        }

        public virtual ResultContext ResultContext { get; }
        public virtual bool ResultReady { get; set; }
        public virtual bool? HasNext { get; set; }
        public virtual IList<CollectionMaterializationContext> Collections { get; } = new List<CollectionMaterializationContext>();

        public virtual void SetCollectionMaterializationContext(
            int collectionId, CollectionMaterializationContext collectionMaterializationContext)
        {
            while (Collections.Count <= collectionId)
            {
                Collections.Add(null);
            }

            Collections[collectionId] = collectionMaterializationContext;
        }
    }
}
