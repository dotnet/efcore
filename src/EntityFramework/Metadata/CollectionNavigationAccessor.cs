// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class CollectionNavigationAccessor : NavigationAccessor
    {
        private readonly ThreadSafeLazyRef<IClrCollectionAccessor> _collectionAccessor;

        public CollectionNavigationAccessor(
            [NotNull] Func<IClrPropertyGetter> getter,
            [NotNull] Func<IClrPropertySetter> setter,
            [NotNull] Func<IClrCollectionAccessor> collectionAccessor)
            : base(getter, setter)
        {
            Check.NotNull(collectionAccessor, "collectionAccessor");

            _collectionAccessor = new ThreadSafeLazyRef<IClrCollectionAccessor>(collectionAccessor);
        }

        public virtual IClrCollectionAccessor Collection
        {
            get { return _collectionAccessor.Value; }
        }
    }
}
