// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class PropertyParameterBindingFactory : ParameterBindingFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ParameterBinding TryBindParameter(IMutableEntityType entityType, ParameterInfo parameter)
        {
            var candidateNames = GetCandidatePropertyNames(parameter);

            return entityType.GetProperties().Where(
                p => p.ClrType == parameter.ParameterType
                     && candidateNames.Any(c => c.Equals(p.Name, StringComparison.Ordinal)))
                .Select(p => new PropertyParameterBinding(p)).FirstOrDefault();
        }

        private static IList<string> GetCandidatePropertyNames([NotNull] ParameterInfo parameter)
        {
            var name = parameter.Name;
            var pascalized = char.ToUpperInvariant(name[0]) + name.Substring(1);

            return new List<string>
            {
                name,
                pascalized,
                "_" + name,
                "_" + pascalized,
                "m_" + name,
                "m_" + pascalized
            };
        }
    }
}
