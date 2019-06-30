// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class PropertyParameterBindingFactory : IPropertyParameterBindingFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ParameterBinding FindParameter(
            IEntityType entityType,
            Type parameterType,
            string parameterName)
        {
            var candidateNames = GetCandidatePropertyNames(parameterName);

            return entityType.GetProperties().Where(
                    p => p.ClrType == parameterType
                         && candidateNames.Any(c => c.Equals(p.Name, StringComparison.Ordinal)))
                .Select(p => new PropertyParameterBinding(p)).FirstOrDefault();
        }

        private static IList<string> GetCandidatePropertyNames(string parameterName)
        {
            var pascalized = char.ToUpperInvariant(parameterName[0]) + parameterName.Substring(1);

            return new List<string>
            {
                parameterName,
                pascalized,
                "_" + parameterName,
                "_" + pascalized,
                "m_" + parameterName,
                "m_" + pascalized
            };
        }
    }
}
