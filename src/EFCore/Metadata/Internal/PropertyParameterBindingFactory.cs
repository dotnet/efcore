// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PropertyParameterBindingFactory : IPropertyParameterBindingFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ParameterBinding? FindParameter(
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
        var pascalized = char.ToUpperInvariant(parameterName[0]) + parameterName[1..];

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
