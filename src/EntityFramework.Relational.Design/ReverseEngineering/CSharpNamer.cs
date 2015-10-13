// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class CSharpNamer<T>
    {
        private readonly Func<T, string> _nameGetter;
        protected readonly Dictionary<T, string> NameCache = new Dictionary<T, string>();

        public CSharpNamer([NotNull] Func<T, string> nameGetter)
        {
            Check.NotNull(nameGetter, nameof(nameGetter));

            _nameGetter = nameGetter;
        }

        public virtual string GetName([NotNull] T item)
        {
            Check.NotNull(item, nameof(item));

            if (NameCache.ContainsKey(item))
            {
                return NameCache[item];
            }

            var name = CSharpUtilities.Instance.GenerateCSharpIdentifier(_nameGetter(item), null);
            NameCache.Add(item, name);
            return name;
        }
    }
}
