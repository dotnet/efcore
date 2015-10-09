// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class CSharpNamer<T>
    {
        private readonly Func<T, string> _nameGetter;
        private Dictionary<T, string> _nameCache = new Dictionary<T, string>();

        public CSharpNamer([NotNull] Func<T, string> nameGetter)
        {
            Check.NotNull(nameGetter, nameof(nameGetter));

            _nameGetter = nameGetter;
        }

        public virtual string GetName([NotNull] T item)
        {
            Check.NotNull(item, nameof(item));

            if (_nameCache.ContainsKey(item))
            {
                return _nameCache[item];
            }

            var name = CSharpUtilities.Instance.GenerateCSharpIdentifier(GenerateName(item), null);
            _nameCache.Add(item, name);
            return name;
        }

        protected virtual string GenerateName([NotNull] T item)
            => _nameGetter(item);
    }
}
