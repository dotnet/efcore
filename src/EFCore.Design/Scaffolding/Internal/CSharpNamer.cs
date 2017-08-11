// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CSharpNamer<T>
    {
        private readonly Func<T, string> _nameGetter;
        private readonly ICSharpUtilities _cSharpUtilities;
        private readonly Func<string, string> _singularizePluralizer;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected readonly Dictionary<T, string> NameCache = new Dictionary<T, string>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CSharpNamer(
            [NotNull] Func<T, string> nameGetter,
            [NotNull] ICSharpUtilities cSharpUtilities,
            [CanBeNull] Func<string, string> singularizePluralizer)
        {
            Check.NotNull(nameGetter, nameof(nameGetter));
            Check.NotNull(cSharpUtilities, nameof(cSharpUtilities));

            _nameGetter = nameGetter;
            _cSharpUtilities = cSharpUtilities;
            _singularizePluralizer = singularizePluralizer;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GetName([NotNull] T item)
        {
            Check.NotNull(item, nameof(item));

            if (NameCache.ContainsKey(item))
            {
                return NameCache[item];
            }

            var name = _cSharpUtilities.GenerateCSharpIdentifier(
                _nameGetter(item), existingIdentifiers: null, singularizePluralizer: _singularizePluralizer);
            NameCache.Add(item, name);
            return name;
        }
    }
}
