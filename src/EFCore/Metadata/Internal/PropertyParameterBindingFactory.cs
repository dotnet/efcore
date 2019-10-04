// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PropertyParameterBindingFactory : IPropertyParameterBindingFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ParameterBinding TryBindParameter(
            IMutableEntityType entityType,
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
