namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class InversePropertyAttributeConvention : NavigationAttributeConventionBase<InversePropertyAttribute>
{
    private IConventionForeignKeyBuilder? ConfigureInverseNavigation(
        IConventionEntityTypeBuilder entityTypeBuilder,
        MemberInfo navigationMemberInfo,
        IConventionEntityTypeBuilder targetEntityTypeBuilder,
        InversePropertyAttribute attribute)
    {
        var entityType = entityTypeBuilder.Metadata;
        var targetEntityType = targetEntityTypeBuilder.Metadata;
        var inverseNavigationPropertyInfo = targetEntityType.GetRuntimeProperties().Values
            .Single(p => string.Equals(p.GetSimpleMemberName(), attribute.Property, StringComparison.Ordinal));

        var referencingNavigations = AddInverseNavigation(
            entityType,
            navigationMemberInfo,
            targetEntityType,
            inverseNavigationPropertyInfo);

        if (FindAmbiguousInverse(navigationMemberInfo, entityType, referencingNavigations) is { } ambiguousInverse)
        {
            entityTypeBuilder.Ignore(navigationMemberInfo.GetSimpleMemberName(), fromDataAnnotation: true);
            ambiguousInverse.Item2.Builder.Ignore(
                ambiguousInverse.Item1.GetSimpleMemberName(),
                fromDataAnnotation: true);
            targetEntityTypeBuilder.Ignore(
                inverseNavigationPropertyInfo.GetSimpleMemberName(),
                fromDataAnnotation: true);

            return null;
        }

        return targetEntityTypeBuilder.HasRelationship(
            entityType,
            inverseNavigationPropertyInfo,
            navigationMemberInfo,
            fromDataAnnotation: true);
    }

    private static (MemberInfo, IConventionEntityType)? FindAmbiguousInverse(
        MemberInfo navigation,
        IConventionEntityType entityType,
        List<(MemberInfo Inverse, IConventionEntityType InverseEntityType)> referencingNavigations)
    {
        foreach (var candidate in referencingNavigations)
        {
            var candidateEntityType = FindActualEntityType(candidate.InverseEntityType);
            if (candidateEntityType is not null
                && !candidateEntityType.Builder.IsIgnored(candidate.Inverse.GetSimpleMemberName(), fromDataAnnotation: true)
                && (!candidate.Inverse.IsSameAs(navigation)
                    || (!entityType.IsAssignableFrom(candidateEntityType)
                        && !candidateEntityType.IsAssignableFrom(entityType))))
            {
                return candidate;
            }
        }

        return null;
    }

    private static List<(MemberInfo, IConventionEntityType)> AddInverseNavigation(
        IConventionEntityType entityType,
        MemberInfo navigation,
        IConventionEntityType targetEntityType,
        MemberInfo inverseNavigation)
    {
        var inverseNavigations = GetInverseNavigations(targetEntityType) ?? [];
        SetInverseNavigations(targetEntityType.Builder, inverseNavigations);

        if (!inverseNavigations.TryGetValue(inverseNavigation.Name, out var inverseTuple))
        {
            inverseTuple = (inverseNavigation, []);
            inverseNavigations[inverseNavigation.Name] = inverseTuple;
        }

        inverseTuple.References.Add((navigation, entityType));
        return inverseTuple.References;
    }
}