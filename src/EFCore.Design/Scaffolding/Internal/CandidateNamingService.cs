// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CandidateNamingService : ICandidateNamingService
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateCandidateIdentifier(DatabaseTable originalTable)
        {
            return GenerateCandidateIdentifier(originalTable.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateCandidateIdentifier(DatabaseColumn originalColumn)
        {
            return GenerateCandidateIdentifier(originalColumn.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GetDependentEndCandidateNavigationPropertyName(IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            var candidateName = FindCandidateNavigationName(foreignKey.Properties);

            return !string.IsNullOrEmpty(candidateName) ? candidateName : foreignKey.PrincipalEntityType.ShortName();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GetPrincipalEndCandidateNavigationPropertyName(
            IForeignKey foreignKey,
            string dependentEndNavigationPropertyName)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotEmpty(dependentEndNavigationPropertyName, nameof(dependentEndNavigationPropertyName));

            var allForeignKeysBetweenDependentAndPrincipal =
                foreignKey.PrincipalEntityType?
                    .GetReferencingForeignKeys()
                    .Where(fk => foreignKey.DeclaringEntityType == fk.DeclaringEntityType);

            return allForeignKeysBetweenDependentAndPrincipal?.Count() > 1
                ? foreignKey.DeclaringEntityType.ShortName()
                       + dependentEndNavigationPropertyName
                : foreignKey.DeclaringEntityType.ShortName();
        }

        private static string GenerateCandidateIdentifier(string originalIdentifier)
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

        private static string FindCandidateNavigationName(IEnumerable<IProperty> properties)
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

            return StripId(candidateName);
        }

        private static string FindCommonPrefix(string firstName, IEnumerable<string> propertyNames)
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

        private static string StripId(string commonPrefix)
        {
            return commonPrefix.Length > 2
                && commonPrefix.EndsWith("id", StringComparison.OrdinalIgnoreCase)
                ? commonPrefix.Substring(0, commonPrefix.Length - 2)
                : commonPrefix;
        }
    }
}
