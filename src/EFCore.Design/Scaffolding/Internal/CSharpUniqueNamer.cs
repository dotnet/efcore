// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CSharpUniqueNamer<T> : CSharpNamer<T>
    {
        private readonly HashSet<string> _usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CSharpUniqueNamer(
            [NotNull] Func<T, string> nameGetter,
            [NotNull] ICSharpUtilities cSharpUtilities,
            [CanBeNull] Func<string, string> singularizePluralizer)
            : this(nameGetter, null, cSharpUtilities, singularizePluralizer)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CSharpUniqueNamer(
            [NotNull] Func<T, string> nameGetter,
            [CanBeNull] IEnumerable<string> usedNames,
            [NotNull] ICSharpUtilities cSharpUtilities,
            [CanBeNull] Func<string, string> singularizePluralizer)
            : base(nameGetter, cSharpUtilities, singularizePluralizer)
        {
            if (usedNames != null)
            {
                foreach (var name in usedNames)
                {
                    _usedNames.Add(name);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetName(T item)
        {
            if (NameCache.ContainsKey(item))
            {
                return base.GetName(item);
            }

            var input = base.GetName(item);
            var name = input;
            var suffix = 1;

            while (_usedNames.Contains(name))
            {
                name = input + suffix++;
            }

            _usedNames.Add(name);
            NameCache[item] = name;

            return name;
        }
    }
}
