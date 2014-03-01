// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class PropertyEntry<TEntity, TProperty> : PropertyEntry
    {
        public PropertyEntry([NotNull] StateEntry stateEntry, [NotNull] string name)
            : base(stateEntry, name)
        {
        }
    }
}
