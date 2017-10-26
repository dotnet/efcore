// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class LanguageBasedSelector<T>
        where T : ILanguageBasedService
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected LanguageBasedSelector(IEnumerable<T> services)
            => Services = services;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IEnumerable<T> Services { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual T Select([CanBeNull] string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                language = "C#";
            }

            var legacyService = Services.LastOrDefault(s => s.Language == null);
            if (legacyService != null)
            {
                return legacyService;
            }

            var matches = Services.Where(s => string.Equals(s.Language, language, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0)
            {
                throw new OperationException(DesignStrings.NoLanguageService(language, typeof(T).ShortDisplayName()));
            }

            return matches.Last();
        }
    }
}
