// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking
{
    internal abstract class EntityKeyFactory
    {
        public abstract EntityKey Create([NotNull] object entity);
    }
}
