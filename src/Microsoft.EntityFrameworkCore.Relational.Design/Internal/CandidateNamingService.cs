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
    public class CandidateNamingService
    {
        /// <summary>
        /// Generates a candidate identifier from an original identifier using the following rules.
        /// Split the original identifier into words using as word separator either a change from
        /// lower to upper case or a non-letter, non-digit character. Then ignore any non-letter, non-digit
        /// characters. Then upper-case the first character of each word and lower-case the remaining
        /// characters. Then concatenate the words back together into a single candidate identifier.
        /// </summary>
        /// <param name="originalIdentifier"> the original identifier </param>
        /// <returns> the candidate identifier </returns>
        public virtual string GenerateCandidateIdentifier([NotNull] string originalIdentifier)
        {
            Check.NotEmpty(originalIdentifier, nameof(originalIdentifier));

            var candidateStringBuilder = new StringBuilder();
            var previousLetterCharInWordIsLowerCase = false;
            var isFirstCharacterInWord = true;
            foreach (char c in originalIdentifier)
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

        public virtual string GetDependentEndCandidateNavigationPropertyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            var candidateName = FindCandidateNavigationName(foreignKey.Properties);

            if (!string.IsNullOrEmpty(candidateName))
            {
                return candidateName;
            }

            return foreignKey.PrincipalEntityType.DisplayName();
        }

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
                return foreignKey.DeclaringEntityType.DisplayName()
                       + dependentEndNavigationPropertyName;
            }

            return foreignKey.DeclaringEntityType.DisplayName();
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
