// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Classifies CLR members of a type during model building for relational providers, honoring an explicitly
///     configured store type so that such members are mapped as scalar properties rather than property bags.
/// </summary>
/// <remarks>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
/// </remarks>
public class RelationalMemberClassifier : MemberClassifier
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationalMemberClassifier" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public RelationalMemberClassifier(MemberClassifierDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override Type? FindCandidateNavigationPropertyType(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out bool? shouldBeOwned)
    {
        // A member with an explicitly configured store type (e.g. [Column(TypeName = "jsonb")]) is meant to be
        // mapped to a column, so it should not be detected as a navigation to a property-bag entity type even when
        // no type mapping is found for it (e.g. Dictionary<string, object>). See issue #26903.
        if (useAttributes
            && HasExplicitColumnType(memberInfo))
        {
            shouldBeOwned = null;
            return null;
        }

        return base.FindCandidateNavigationPropertyType(memberInfo, model, useAttributes, out shouldBeOwned);
    }

    /// <inheritdoc />
    public override bool IsCandidatePrimitiveProperty(
        MemberInfo memberInfo,
        IConventionModel model,
        bool useAttributes,
        out CoreTypeMapping? typeMapping)
    {
        if (base.IsCandidatePrimitiveProperty(memberInfo, model, useAttributes, out typeMapping))
        {
            return true;
        }

        // When a store type is explicitly configured, the member should be discovered as a property even if no type
        // mapping is found for the CLR type. The provider then either maps it (e.g. Npgsql maps Dictionary<string,
        // object> to jsonb) or model validation reports that the type cannot be mapped. See issue #26903.
        if (useAttributes
            && memberInfo.IsCandidateProperty()
            && HasExplicitColumnType(memberInfo))
        {
            typeMapping = null;
            return true;
        }

        return false;
    }

    private static bool HasExplicitColumnType(MemberInfo memberInfo)
        => !string.IsNullOrWhiteSpace(memberInfo.GetCustomAttribute<ColumnAttribute>(inherit: true)?.TypeName);
}
