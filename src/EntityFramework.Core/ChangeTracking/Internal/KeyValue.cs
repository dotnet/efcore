// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class KeyValue : IKeyValue
    {
        public static readonly KeyValue InvalidKeyValue = new InvalidKeyValueSentinel();

        private KeyValue()
        {
        }

        protected KeyValue([NotNull] IKey key)
        {
            Key = key;
        }

        public virtual IKey Key { get; }

        public virtual object Value => GetValue();

        protected abstract object GetValue();

        private sealed class InvalidKeyValueSentinel : KeyValue
        {
            protected override object GetValue() => null;

            public override int GetHashCode() => 0;
        }
    }
}
