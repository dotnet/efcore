// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class NavigationAccessor
    {
        private readonly ThreadSafeLazyRef<IClrPropertyGetter> _getter;
        private readonly ThreadSafeLazyRef<IClrPropertySetter> _setter;

        public NavigationAccessor(
            [NotNull] Func<IClrPropertyGetter> getter,
            [NotNull] Func<IClrPropertySetter> setter)
        {
            Check.NotNull(getter, "getter");
            Check.NotNull(setter, "setter");

            _getter = new ThreadSafeLazyRef<IClrPropertyGetter>(getter);
            _setter = new ThreadSafeLazyRef<IClrPropertySetter>(setter);
        }

        public virtual IClrPropertyGetter Getter
        {
            get { return _getter.Value; }
        }

        public virtual IClrPropertySetter Setter
        {
            get { return _setter.Value; }
        }
    }
}
