// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering
{
    public class CSharpUniqueNamer<T> : CSharpNamer<T>
    {
        private HashSet<string> _usedNames = new HashSet<string>();

        public CSharpUniqueNamer([NotNull] Func<T, string> nameGetter)
            :base(nameGetter)
        {
        }

        protected override string GenerateName([NotNull] T item)
        {
            var input = base.GenerateName(item);
            var name = input;
            var suffix = 1;

            while (_usedNames.Contains(name))
            {
                name = input + (suffix++);
            }

            _usedNames.Add(name);
            return name;
        }
    }
}
