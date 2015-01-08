// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class EntityKey
    {
        public static readonly EntityKey NullEntityKey = new NullEntityKeySentinel();

        public virtual object Value => GetValue();

        protected abstract object GetValue();

        private sealed class NullEntityKeySentinel : EntityKey
        {
            protected override object GetValue()
            {
                return null;
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }
    }
}
