// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    public class ModelUtilities
    {
        public virtual string GetDependentEndCandidateNavigationPropertyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            var candidateName = StripId(
                FindCommonPrefix(foreignKey.Properties.Select(p => p.Name)));

            if (!string.IsNullOrEmpty(candidateName))
            {
                return candidateName;
            }

            return foreignKey.PrincipalEntityType.DisplayName();
        }

        public virtual string GetPrincipalEndCandidateNavigationPropertyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, nameof(foreignKey));

            return foreignKey.DeclaringEntityType.DisplayName();
        }

        private string FindCommonPrefix(IEnumerable<string> stringsEnumerable)
        {
            if (stringsEnumerable.Count() == 0)
            {
                return string.Empty;
            }

            if (stringsEnumerable.Count() == 1)
            {
                return stringsEnumerable.Single();
            }

            var prefixLength = 0;
            var firstString = stringsEnumerable.First();
            foreach (var c in firstString)
            {
                foreach (var s in stringsEnumerable)
                {
                    if (s.Length <= prefixLength
                        || s[prefixLength] != c)
                    {
                        return firstString.Substring(0, prefixLength);
                    }
                }

                prefixLength++;
            }

            return firstString;
        }

        private string StripId(string identifier)
        {
            if (identifier.EndsWith("_id", StringComparison.OrdinalIgnoreCase))
            {
                return identifier.Substring(0, identifier.Length - 3);
            }

            if (identifier.EndsWith("Id", StringComparison.Ordinal)
                || identifier.EndsWith("ID", StringComparison.Ordinal))
            {
                return identifier.Substring(0, identifier.Length - 2);
            }

            return identifier;
        }
    }
}
