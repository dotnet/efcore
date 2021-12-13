// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CandidateNamingService : ICandidateNamingService
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GenerateCandidateIdentifier(DatabaseTable originalTable)
        => GenerateCandidateIdentifier(originalTable.Name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GenerateCandidateIdentifier(DatabaseColumn originalColumn)
        => GenerateCandidateIdentifier(originalColumn.Name);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetDependentEndCandidateNavigationPropertyName(IReadOnlyForeignKey foreignKey)
    {
        var candidateName = FindCandidateNavigationName(foreignKey.Properties);

        return !string.IsNullOrEmpty(candidateName) ? candidateName : foreignKey.PrincipalEntityType.ShortName();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetPrincipalEndCandidateNavigationPropertyName(
        IReadOnlyForeignKey foreignKey,
        string dependentEndNavigationPropertyName)
    {
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

    private static string FindCandidateNavigationName(IEnumerable<IReadOnlyProperty> properties)
    {
        var count = properties.Count();
        if (count == 0)
        {
            return string.Empty;
        }

        var firstProperty = properties.First();
        return StripId(
            count == 1
                ? firstProperty.Name
                : FindCommonPrefix(firstProperty.Name, properties.Select(p => p.Name)));
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
                    return firstName[..prefixLength];
                }
            }

            prefixLength++;
        }

        return firstName[..prefixLength];
    }

    private static string StripId(string commonPrefix)
    {
        if (commonPrefix.Length < 3
            || !commonPrefix.EndsWith("id", StringComparison.OrdinalIgnoreCase))
        {
            return commonPrefix;
        }

        int i;
        for (i = commonPrefix.Length - 3; i >= 0; i--)
        {
            if (char.IsLetterOrDigit(commonPrefix[i]))
            {
                break;
            }
        }

        return i != 0
            ? commonPrefix[..(i + 1)]
            : commonPrefix;
    }
}
