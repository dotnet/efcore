// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class EntityKey
    {
        // Note: For composite keys this will be some form of composite object
        public virtual object Value
        {
            get { return GetValue(); }
        }

        protected abstract object GetValue();
    }
}
