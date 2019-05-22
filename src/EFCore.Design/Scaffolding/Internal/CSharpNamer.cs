// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CSharpNamer<T>
    {
        private readonly Func<T, string> _nameGetter;
        private readonly ICSharpUtilities _cSharpUtilities;
        private readonly Func<string, string> _singularizePluralizer;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected readonly Dictionary<T, string> NameCache = new Dictionary<T, string>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
