// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CandidateNamingService
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateCandidateIdentifier([NotNull] string originalIdentifier)
        {
            Check.NotEmpty(originalIdentifier, nameof(originalIdentifier));

            var candidateStringBuilder = new StringBuilder();
            var previousLetterCharInWordIsLowerCase = false;
            var isFirstCharacterInWord = true;
            foreach (var c in originalIdentifier)
            {
                var isNotLetterOrDigit = !char.IsLetterOrDigit(c);
                if (isNotLetterOrDigit
                    || (previousLetterCharInWordIsLowerCase && char.IsUpper(c)))
                {
                    isFirstCharacterInWord = true;
                    previousLetterCharInWordIsLowerCase = false;
                    if (isNotLetterOrDigit)
                    {
                        continue;
                    }
                }

                candidateStringBuilder.Append(
                    isFirstCharacterInWord ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
                isFirstCharacterInWord = false;
                if (char.IsLower(c))
                {
                    previousLetterCharInWordIsLowerCase = true;
                }
            }

            return candidateStringBuilder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GetDependentEndCandidateNavigationPropertyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            var candidateName = FindCandidateNavigationName(foreignKey.Properties);

            if (!string.IsNullOrEmpty(candidateName))
            {
                return candidateName;
            }

            return foreignKey.PrincipalEntityType.ShortName();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GetPrincipalEndCandidateNavigationPropertyName(
            [NotNull] IForeignKey foreignKey,
            [NotNull] string dependentEndNavigationPropertyName)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotEmpty(dependentEndNavigationPropertyName, nameof(dependentEndNavigationPropertyName));

            var allForeignKeysBetweenDependentAndPrincipal =
                foreignKey.PrincipalEntityType?
                    .GetReferencingForeignKeys()
                    .Where(fk => foreignKey.DeclaringEntityType == fk.DeclaringEntityType);

            if (allForeignKeysBetweenDependentAndPrincipal != null
                && allForeignKeysBetweenDependentAndPrincipal.Count() > 1)
            {
                return foreignKey.DeclaringEntityType.ShortName()
                       + dependentEndNavigationPropertyName;
            }

            return foreignKey.DeclaringEntityType.ShortName();
        }

        private string FindCandidateNavigationName(IEnumerable<IProperty> properties)
        {
            if (!properties.Any())
            {
                return string.Empty;
            }

            var candidateName = string.Empty;
            var firstProperty = properties.First();
            if (properties.Count() == 1)
            {
                candidateName = firstProperty.Name;
            }
            else
            {
                candidateName = FindCommonPrefix(firstProperty.Name, properties.Select(p => p.Name));
            }

            return StripId(candidateName, properties);
        }

        private string FindCommonPrefix(string firstName, IEnumerable<string> propertyNames)
        {
            var prefixLength = 0;
            foreach (var c in firstName)
            {
                foreach (var s in propertyNames)
                {
                    if (s.Length <= prefixLength
                        || s[prefixLength] != c)
                    {
                        return firstName.Substring(0, prefixLength);
                    }
                }

                prefixLength++;
            }

            return firstName.Substring(0, prefixLength);
        }

        private string StripId(string commonPrefix, IEnumerable<IProperty> properties)
        {
            if (commonPrefix.Length > 2
                && commonPrefix.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                return commonPrefix.Substring(0, commonPrefix.Length - 2);
            }

            return commonPrefix;
        }
    }
}
