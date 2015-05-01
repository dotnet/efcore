// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class EntityKey
    {
        public static readonly EntityKey InvalidEntityKey = new InvalidEntityKeySentinel();

        public virtual object Value => GetValue();

        protected abstract object GetValue();

        private sealed class InvalidEntityKeySentinel : EntityKey
        {
            protected override object GetValue() => null;

            public override int GetHashCode() => 0;
        }
    }
}
