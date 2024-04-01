// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class ModelAsserter
{
    protected ModelAsserter()
    {
    }

    public static ModelAsserter Instance { get; } = new();

    public virtual void AssertEqual(
        IReadOnlyModel expected,
        IReadOnlyModel actual,
        bool compareAnnotations = true)
        => AssertEqual(
            expected,
            actual,
            compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
            compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
            compareMemberAnnotations: compareAnnotations);

    public virtual void AssertEqual(
        IReadOnlyModel expected,
        IReadOnlyModel actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareMemberAnnotations)
    {
        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        var designTime = expected is Model && actual is Model;

        Assert.Multiple(
            () => Assert.Equal(expected.ModelId, actual.ModelId),
            () => Assert.Equal(expected.GetProductVersion(), actual.GetProductVersion()),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetProductVersion(), actual.GetProductVersion());
                }
            },
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetPropertyAccessMode(), actual.GetPropertyAccessMode());
                }
            },
            () => AssertEqual(expected.GetEntityTypes(), actual.GetEntityTypes(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlyEntityType> expectedEntityTypes,
        IEnumerable<IReadOnlyEntityType> actualEntityTypes,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedEntityTypes = expectedEntityTypes.OrderBy(p => p.Name);
            actualEntityTypes = actualEntityTypes.OrderBy(p => p.Name);
        }
        else
        {
            expectedEntityTypes = expectedEntityTypes.Select(x => x);
        }

        Assert.Equal(expectedEntityTypes, actualEntityTypes,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false,
                    compareMemberAnnotations: compareAnnotations));
    }

    public virtual bool AssertEqual(IReadOnlyEntityType? expected, IReadOnlyEntityType? actual,
        IEnumerable<IAnnotation> expectedAnnotations, IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false, bool compareMemberAnnotations = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        var designTime = expected is EntityType && actual is EntityType;

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.ClrType, actual.ClrType),
            () => Assert.Equal(expected.HasSharedClrType, actual.HasSharedClrType),
            () => Assert.Equal(expected.IsPropertyBag, actual.IsPropertyBag),
            () => Assert.Equal(expected.GetQueryFilter(), actual.GetQueryFilter()),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetNavigationAccessMode(), actual.GetNavigationAccessMode());
                }
            },
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetPropertyAccessMode(), actual.GetPropertyAccessMode());
                }
            },
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetSeedData(), actual.GetSeedData());
                }
            },
            () => Assert.Equal(expected.GetChangeTrackingStrategy(), actual.GetChangeTrackingStrategy()),
            () => Assert.Equal(expected.GetDiscriminatorPropertyName(), actual.GetDiscriminatorPropertyName()),
            () => Assert.Equal(expected.GetDiscriminatorValue(), actual.GetDiscriminatorValue()),
            () => Assert.Equal(expected.GetIsDiscriminatorMappingComplete(), actual.GetIsDiscriminatorMappingComplete()),
            () => AssertEqual(expected.GetProperties(), actual.GetProperties(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () => AssertEqual(expected.GetServiceProperties(), actual.GetServiceProperties(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () => AssertEqual(expected.GetSkipNavigations(), actual.GetSkipNavigations(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () => AssertEqual(expected.GetForeignKeys(), actual.GetForeignKeys(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () => AssertEqual(expected.GetKeys(), actual.GetKeys(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () => AssertEqual(expected.GetIndexes(), actual.GetIndexes(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () => AssertEqual(expected.GetComplexProperties(), actual.GetComplexProperties(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.BaseType?.Name, actual.BaseType?.Name);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.GetReferencingForeignKeys(), actual.GetReferencingForeignKeys());
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual bool AssertEqual(IReadOnlyComplexType expected, IReadOnlyComplexType actual,
        IEnumerable<IAnnotation> expectedAnnotations, IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        var designTime = expected is ComplexType && actual is ComplexType;

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.ClrType, actual.ClrType),
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.GetPropertyAccessMode(), actual.GetPropertyAccessMode());
                }
            },
            () => Assert.Equal(expected.GetChangeTrackingStrategy(), actual.GetChangeTrackingStrategy()),
            () => AssertEqual(expected.GetProperties(), actual.GetProperties(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () => AssertEqual(expected.GetComplexProperties(), actual.GetComplexProperties(),
                assertOrder: true, compareAnnotations: compareMemberAnnotations),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.ContainingEntityType.Name, actual.ContainingEntityType.Name);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlyComplexProperty> expectedProperties,
        IEnumerable<IReadOnlyComplexProperty> actualProperties,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedProperties = expectedProperties.OrderBy(p => p.Name);
            actualProperties = actualProperties.OrderBy(p => p.Name);
        }
        else
        {
            expectedProperties = expectedProperties.Select(x => x);
        }

        Assert.Equal(expectedProperties, actualProperties,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false,
                    compareMemberAnnotations: compareAnnotations));
    }

    public virtual bool AssertEqual(IReadOnlyComplexProperty? expected, IReadOnlyComplexProperty? actual,
        IEnumerable<IAnnotation> expectedAnnotations, IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.ClrType, actual.ClrType),
            () => Assert.Equal(expected.FieldInfo, actual.FieldInfo),
            () => Assert.Equal(expected.GetIdentifyingMemberInfo(), actual.GetIdentifyingMemberInfo()),
            () => Assert.Equal(expected.IsShadowProperty(), actual.IsShadowProperty()),
            () => Assert.Equal(expected.IsNullable, actual.IsNullable),
            () => Assert.Equal(expected.Sentinel, actual.Sentinel),
            () => Assert.Equal(expected.GetPropertyAccessMode(), actual.GetPropertyAccessMode()),
            () => AssertEqual(expected.ComplexType, actual.ComplexType,
                compareMemberAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                compareMemberAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                compareBackreferences: false,
                compareMemberAnnotations: compareMemberAnnotations),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.DeclaringType.Name, actual.DeclaringType.Name);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlyProperty> expectedProperties,
        IEnumerable<IReadOnlyProperty> actualProperties,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedProperties = expectedProperties.OrderBy(p => p.Name);
            actualProperties = actualProperties.OrderBy(p => p.Name);
        }
        else
        {
            expectedProperties = expectedProperties.Select(x => x);
        }

        Assert.Equal(expectedProperties, actualProperties,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false,
                    compareAnnotations));
    }

    public virtual bool AssertEqual(
        IReadOnlyProperty? expected,
        IReadOnlyProperty? actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.ClrType, actual.ClrType),
            () => Assert.Equal(expected.FieldInfo?.Name, actual.FieldInfo?.Name),
            () => Assert.Equal(expected.GetIdentifyingMemberInfo()?.Name, actual.GetIdentifyingMemberInfo()?.Name),
            () => Assert.Equal(expected.IsShadowProperty(), actual.IsShadowProperty()),
            () => Assert.Equal(expected.IsNullable, actual.IsNullable),
            () => Assert.Equal(expected.IsConcurrencyToken, actual.IsConcurrencyToken),
            () => Assert.Equal(expected.Sentinel, actual.Sentinel),
            () => Assert.Equal(expected.ValueGenerated, actual.ValueGenerated),
            () => Assert.Equal(expected.GetPropertyAccessMode(), actual.GetPropertyAccessMode()),
            () => Assert.Equal(expected.GetBeforeSaveBehavior(), actual.GetBeforeSaveBehavior()),
            () => Assert.Equal(expected.GetAfterSaveBehavior(), actual.GetAfterSaveBehavior()),
            () =>
            {
                if (actual.Name != (actual.DeclaringType as IEntityType)?.GetDiscriminatorPropertyName())
                {
                    Assert.Equal(expected.GetMaxLength(), actual.GetMaxLength());
                }
            },
            () => Assert.Equal(expected.GetPrecision(), actual.GetPrecision()),
            () => Assert.Equal(expected.GetScale(), actual.GetScale()),
            () => Assert.Equal(expected.IsUnicode(), actual.IsUnicode()),
            () => Assert.Equal(expected.GetProviderClrType(), actual.GetProviderClrType()),
            () =>
            {
                var actualConverter = actual.GetValueConverter() ?? actual.FindTypeMapping()?.Converter;
                if ((expected.GetValueConverter() ?? expected.FindTypeMapping()?.Converter) == null)
                {
                    Assert.Null(actualConverter);
                }
                else
                {
                    Assert.NotNull(actualConverter);
                }
            },
            () =>
            {
                var actualComparer = actual.GetValueComparer() ?? actual.FindTypeMapping()?.Comparer;
                if ((expected.GetValueComparer() ?? expected.FindTypeMapping()?.Comparer) == null)
                {
                    Assert.Null(actualComparer);
                }
                else
                {
                    Assert.NotNull(actualComparer);
                }
            },
            () => Assert.Equal(expected.IsKey(), actual.IsKey()),
            () => Assert.Equal(expected.IsForeignKey(), actual.IsForeignKey()),
            () => Assert.Equal(expected.IsIndex(), actual.IsIndex()),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.DeclaringType.Name, actual.DeclaringType.Name);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.GetContainingForeignKeys(), actual.GetContainingForeignKeys(), ForeignKeyComparer.Instance);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.GetContainingIndexes(), actual.GetContainingIndexes(), IndexComparer.Instance);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.GetContainingKeys(), actual.GetContainingKeys(), KeyComparer.Instance);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlyServiceProperty> expectedProperties,
        IEnumerable<IReadOnlyServiceProperty> actualProperties,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedProperties = expectedProperties.OrderBy(p => p.Name);
            actualProperties = actualProperties.OrderBy(p => p.Name);
        }
        else
        {
            expectedProperties = expectedProperties.Select(x => x);
        }

        Assert.Equal(expectedProperties, actualProperties,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false));
    }

    public virtual bool AssertEqual(IReadOnlyServiceProperty? expected, IReadOnlyServiceProperty? actual,
        IEnumerable<IAnnotation> expectedAnnotations, IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.ClrType, actual.ClrType),
            () => Assert.Equal(expected.FieldInfo, actual.FieldInfo),
            () => Assert.Equal(expected.GetIdentifyingMemberInfo(), actual.GetIdentifyingMemberInfo()),
            () => Assert.Equal(expected.IsShadowProperty(), actual.IsShadowProperty()),
            () => Assert.Equal(expected.Sentinel, actual.Sentinel),
            () => Assert.Equal(expected.GetPropertyAccessMode(), actual.GetPropertyAccessMode()),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.DeclaringType.Name, actual.DeclaringType.Name);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlyNavigation> expectedNavigations,
        IEnumerable<IReadOnlyNavigation> actualNavigations,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedNavigations = expectedNavigations.OrderBy(p => p.Name);
            actualNavigations = actualNavigations.OrderBy(p => p.Name);
        }
        else
        {
            expectedNavigations = expectedNavigations.Select(x => x);
        }

        Assert.Equal(expectedNavigations, actualNavigations,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false));
    }

    public virtual bool AssertEqual(IReadOnlyNavigation? expected, IReadOnlyNavigation? actual,
        IEnumerable<IAnnotation> expectedAnnotations, IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.ClrType, actual.ClrType),
            () => Assert.Equal(expected.FieldInfo, actual.FieldInfo),
            () => Assert.Equal(expected.GetIdentifyingMemberInfo(), actual.GetIdentifyingMemberInfo()),
            () => Assert.Equal(expected.IsShadowProperty(), actual.IsShadowProperty()),
            () => Assert.Equal(expected.IsCollection, actual.IsCollection),
            () => Assert.Equal(expected.IsEagerLoaded, actual.IsEagerLoaded),
            () => Assert.Equal(expected.LazyLoadingEnabled, actual.LazyLoadingEnabled),
            () => Assert.Equal(expected.Sentinel, actual.Sentinel),
            () => Assert.Equal(expected.GetPropertyAccessMode(), actual.GetPropertyAccessMode()),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.DeclaringType.Name, actual.DeclaringType.Name);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.ForeignKey, actual.ForeignKey, ForeignKeyComparer.Instance);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.Inverse?.Name, actual.Inverse?.Name);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlySkipNavigation> expectedNavigations,
        IEnumerable<IReadOnlySkipNavigation> actualNavigations,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedNavigations = expectedNavigations.OrderBy(p => p.Name);
            actualNavigations = actualNavigations.OrderBy(p => p.Name);
        }
        else
        {
            expectedNavigations = expectedNavigations.Select(x => x);
        }

        Assert.Equal(expectedNavigations, actualNavigations,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false));
    }

    public virtual bool AssertEqual(IReadOnlySkipNavigation expected, IReadOnlySkipNavigation actual,
        IEnumerable<IAnnotation> expectedAnnotations, IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () => Assert.Equal(expected.Name, actual.Name),
            () => Assert.Equal(expected.ClrType, actual.ClrType),
            () => Assert.Equal(expected.FieldInfo, actual.FieldInfo),
            () => Assert.Equal(expected.GetIdentifyingMemberInfo(), actual.GetIdentifyingMemberInfo()),
            () => Assert.Equal(expected.IsShadowProperty(), actual.IsShadowProperty()),
            () => Assert.Equal(expected.IsCollection, actual.IsCollection),
            () => Assert.Equal(expected.IsEagerLoaded, actual.IsEagerLoaded),
            () => Assert.Equal(expected.LazyLoadingEnabled, actual.LazyLoadingEnabled),
            () => Assert.Equal(expected.Sentinel, actual.Sentinel),
            () => Assert.Equal(expected.GetPropertyAccessMode(), actual.GetPropertyAccessMode()),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.DeclaringType.Name, actual.DeclaringType.Name);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.ForeignKey!, actual.ForeignKey!, ForeignKeyComparer.Instance);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.Inverse?.Name, actual.Inverse?.Name);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlyKey> expectedKeys,
        IEnumerable<IReadOnlyKey> actualKeys,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedKeys = expectedKeys.Order(KeyComparer.Instance);
            actualKeys = actualKeys.Order(KeyComparer.Instance);
        }
        else
        {
            expectedKeys = expectedKeys.Select(x => x);
        }

        Assert.Equal(expectedKeys, actualKeys,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false));
    }

    public virtual bool AssertEqual(
        IReadOnlyKey expected,
        IReadOnlyKey actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected, actual, KeyComparer.Instance);
                }
                else
                {
                    Assert.Equal(expected.Properties, actual.Properties, PropertyListComparer.Instance);
                }
            },
            () => Assert.Equal(expected.IsPrimaryKey(), actual.IsPrimaryKey()),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.DeclaringEntityType.Name, actual.DeclaringEntityType.Name);
                }
            },
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.GetReferencingForeignKeys().ToList(), actual.GetReferencingForeignKeys(), ForeignKeyComparer.Instance);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlyForeignKey> expectedForeignKey,
        IEnumerable<IReadOnlyForeignKey> actualForeignKey,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedForeignKey = expectedForeignKey.Order(ForeignKeyComparer.Instance);
            actualForeignKey = actualForeignKey.Order(ForeignKeyComparer.Instance);
        }
        else
        {
            expectedForeignKey = expectedForeignKey.Select(x => x);
        }

        Assert.Equal(expectedForeignKey, actualForeignKey,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false,
                    compareMemberAnnotations: compareAnnotations));
    }

    public virtual bool AssertEqual(
        IReadOnlyForeignKey expected,
        IReadOnlyForeignKey actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        Assert.Multiple(
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected, actual, ForeignKeyComparer.Instance);
                }
                else
                {
                    Assert.Equal(expected.Properties, actual.Properties, PropertyListComparer.Instance);
                    Assert.Equal(expected.PrincipalKey.Properties, actual.PrincipalKey.Properties, PropertyListComparer.Instance);
                }
            },
            () => Assert.Equal(expected.IsRequired, actual.IsRequired),
            () => Assert.Equal(expected.IsRequiredDependent, actual.IsRequiredDependent),
            () => Assert.Equal(expected.IsUnique, actual.IsUnique),
            () => Assert.Equal(expected.DeleteBehavior, actual.DeleteBehavior),
            () => AssertEqual(expected.DependentToPrincipal, actual.DependentToPrincipal,
                compareMemberAnnotations
                    ? expected.DependentToPrincipal?.GetAnnotations() ?? Enumerable.Empty<IAnnotation>()
                    : Enumerable.Empty<IAnnotation>(),
                compareMemberAnnotations
                    ? actual.DependentToPrincipal?.GetAnnotations() ?? Enumerable.Empty<IAnnotation>()
                    : Enumerable.Empty<IAnnotation>(),
                compareBackreferences: true),
            () => AssertEqual(expected.PrincipalToDependent, actual.PrincipalToDependent,
                compareMemberAnnotations
                    ? expected.PrincipalToDependent?.GetAnnotations() ?? Enumerable.Empty<IAnnotation>()
                    : Enumerable.Empty<IAnnotation>(),
                compareMemberAnnotations
                    ? actual.PrincipalToDependent?.GetAnnotations() ?? Enumerable.Empty<IAnnotation>()
                    : Enumerable.Empty<IAnnotation>(),
                compareBackreferences: true),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.DeclaringEntityType.Name, actual.DeclaringEntityType.Name);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual void AssertEqual(
        IEnumerable<IReadOnlyIndex> expectedIndex,
        IEnumerable<IReadOnlyIndex> actualIndex,
        bool assertOrder = false,
        bool compareAnnotations = false)
    {
        if (!assertOrder)
        {
            expectedIndex = expectedIndex.Order(IndexComparer.Instance);
            actualIndex = actualIndex.Order(IndexComparer.Instance);
        }
        else
        {
            expectedIndex = expectedIndex.Select(x => x);
        }

        Assert.Equal(expectedIndex, actualIndex,
            (expected, actual) =>
                AssertEqual(
                    expected,
                    actual,
                    compareAnnotations ? expected.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareAnnotations ? actual.GetAnnotations() : Enumerable.Empty<IAnnotation>(),
                    compareBackreferences: false));
    }

    public virtual bool AssertEqual(
        IReadOnlyIndex expected,
        IReadOnlyIndex actual,
        IEnumerable<IAnnotation> expectedAnnotations,
        IEnumerable<IAnnotation> actualAnnotations,
        bool compareBackreferences = false,
        bool compareMemberAnnotations = false)
    {
        if (expected == null)
        {
            Assert.Null(actual);

            return true;
        }

        Assert.NotNull(actual);

        expectedAnnotations = expectedAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));
        actualAnnotations = actualAnnotations.Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name));

        var designTime = expected is Metadata.Internal.Index && actual is Metadata.Internal.Index;

        Assert.Multiple(
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected, actual, IndexComparer.Instance);
                }
                else
                {
                    Assert.Equal(expected.Properties, actual.Properties, PropertyListComparer.Instance);
                }
            },
            () =>
            {
                if (designTime)
                {
                    Assert.Equal(expected.IsDescending, actual.IsDescending);
                }
            },
            () => Assert.Equal(expected.IsUnique, actual.IsUnique),
            () => Assert.Equal(expected.Name, actual.Name),
            () =>
            {
                if (compareBackreferences)
                {
                    Assert.Equal(expected.DeclaringEntityType.Name, actual.DeclaringEntityType.Name);
                }
            },
            () => Assert.Equal(expectedAnnotations, actualAnnotations, TestAnnotationComparer.Instance));

        return true;
    }

    public virtual IReadOnlyModel Clone(IReadOnlyModel model)
    {
        IMutableModel modelClone = new Model(model.ModelId);
        modelClone.SetChangeTrackingStrategy(model.GetChangeTrackingStrategy());

        if (model.GetProductVersion() is string productVersion)
        {
            modelClone.SetProductVersion(productVersion);
        }

        modelClone.SetPropertyAccessMode(model.GetPropertyAccessMode());
        modelClone.AddAnnotations(model.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));

        var clonedEntityTypes = new Dictionary<IReadOnlyEntityType, IMutableEntityType>();
        foreach (var entityType in ((IModel)model).GetEntityTypesInHierarchicalOrder())
        {
            var clonedEntityType = entityType.HasSharedClrType
                ? modelClone.AddEntityType(entityType.Name, entityType.ClrType)
                : modelClone.AddEntityType(entityType.ClrType);

            Copy(entityType, clonedEntityType);
            clonedEntityTypes.Add(entityType, clonedEntityType);
        }

        foreach (var clonedEntityType in clonedEntityTypes)
        {
            var targetEntityType = clonedEntityType.Value;
            foreach (var foreignKey in clonedEntityType.Key.GetDeclaredForeignKeys())
            {
                var targetPrincipalEntityType = targetEntityType.Model.FindEntityType(foreignKey.PrincipalEntityType.Name)!;
                var clonedForeignKey = targetEntityType.AddForeignKey(
                    foreignKey.Properties.Select(p => targetEntityType.FindProperty(p.Name)!).ToList(),
                    targetPrincipalEntityType.FindKey(
                        foreignKey.PrincipalKey.Properties.Select(p => targetPrincipalEntityType.FindProperty(p.Name)!).ToList())!,
                    targetPrincipalEntityType);
                Copy(foreignKey, clonedForeignKey);
            }
        }

        foreach (var clonedEntityType in clonedEntityTypes)
        {
            foreach (var skipNavigation in clonedEntityType.Key.GetDeclaredSkipNavigations())
            {
                var targetEntityType = clonedEntityType.Value;
                var otherEntityType = targetEntityType.Model.FindEntityType(skipNavigation.TargetEntityType.Name)!;
                Copy(skipNavigation, clonedEntityType.Value.AddSkipNavigation(
                    skipNavigation.Name,
                    skipNavigation.GetIdentifyingMemberInfo(),
                    otherEntityType,
                    skipNavigation.IsCollection,
                    skipNavigation.IsOnDependent));
            }
        }

        return modelClone;
    }

    protected virtual void Copy(IReadOnlyEntityType sourceEntityType, IMutableEntityType targetEntityType)
    {
        if (sourceEntityType.BaseType != null)
        {
            targetEntityType.BaseType = targetEntityType.Model.FindEntityType(sourceEntityType.BaseType.Name);
        }

        targetEntityType.SetQueryFilter(sourceEntityType.GetQueryFilter());
        targetEntityType.AddData(sourceEntityType.GetSeedData());
        targetEntityType.SetPropertyAccessMode(sourceEntityType.GetPropertyAccessMode());
        targetEntityType.SetChangeTrackingStrategy(sourceEntityType.GetChangeTrackingStrategy());
        targetEntityType.SetDiscriminatorMappingComplete(sourceEntityType.GetIsDiscriminatorMappingComplete());
        targetEntityType.SetDiscriminatorValue(sourceEntityType.GetDiscriminatorValue());

        foreach (var property in sourceEntityType.GetDeclaredProperties())
        {
            var targetProperty = property.IsShadowProperty()
                ? targetEntityType.AddProperty(property.Name, property.ClrType)
                : targetEntityType.AddProperty(property.Name, property.ClrType, property.GetIdentifyingMemberInfo()!);
            Copy(property, targetProperty);
        }

        if (sourceEntityType.BaseType == null
            && sourceEntityType.GetDiscriminatorPropertyName() is string discriminatorPropertyName)
        {
            targetEntityType.SetDiscriminatorProperty(
                targetEntityType.FindProperty(discriminatorPropertyName)!);
        }

        foreach (var property in sourceEntityType.GetDeclaredComplexProperties())
        {
            Copy(property, targetEntityType.AddComplexProperty(
                property.Name,
                property.ClrType,
                property.ComplexType.ClrType,
                property.ComplexType.Name,
                collection: property.IsCollection));
        }

        foreach (var property in sourceEntityType.GetDeclaredServiceProperties())
        {
            Copy(property, targetEntityType.AddServiceProperty(
                property.GetIdentifyingMemberInfo()!, property.ClrType));
        }

        foreach (var key in sourceEntityType.GetDeclaredKeys())
        {
            Copy(key, targetEntityType.AddKey(
                key.Properties.Select(p => targetEntityType.FindProperty(p.Name)!).ToList()));
        }

        foreach (var index in sourceEntityType.GetDeclaredIndexes())
        {
            var targetProperties = index.Properties.Select(p => targetEntityType.FindProperty(p.Name)!).ToList();
            var clonedIndex = index.Name == null
                ? targetEntityType.AddIndex(targetProperties)
                : targetEntityType.AddIndex(targetProperties, index.Name);
            Copy(index, clonedIndex);
        }

        targetEntityType.AddAnnotations(sourceEntityType.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
    }

    protected virtual void Copy(IReadOnlyProperty sourceProperty, IMutableProperty targetProperty)
    {
        if (sourceProperty.FieldInfo is FieldInfo fieldInfo)
        {
            targetProperty.FieldInfo = fieldInfo;
        }
        targetProperty.IsNullable = sourceProperty.IsNullable;
        targetProperty.IsConcurrencyToken = sourceProperty.IsConcurrencyToken;
        targetProperty.Sentinel = sourceProperty.Sentinel;
        targetProperty.ValueGenerated = sourceProperty.ValueGenerated;
        targetProperty.SetPropertyAccessMode(sourceProperty.GetPropertyAccessMode());
        targetProperty.SetBeforeSaveBehavior(sourceProperty.GetBeforeSaveBehavior());
        targetProperty.SetAfterSaveBehavior(sourceProperty.GetAfterSaveBehavior());
        targetProperty.SetMaxLength(sourceProperty.GetMaxLength());
        targetProperty.SetPrecision(sourceProperty.GetPrecision());
        targetProperty.SetScale(sourceProperty.GetScale());
        targetProperty.SetIsUnicode(sourceProperty.IsUnicode());
        targetProperty.SetProviderClrType(sourceProperty.GetProviderClrType());
        targetProperty.SetValueConverter(sourceProperty.GetValueConverter());
        targetProperty.AddAnnotations(sourceProperty.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
    }

    protected virtual void Copy(IReadOnlyServiceProperty sourceProperty, IMutableServiceProperty targetProperty)
    {
        if (sourceProperty.FieldInfo is FieldInfo fieldInfo)
        {
            targetProperty.FieldInfo = fieldInfo;
        }
        targetProperty.SetPropertyAccessMode(sourceProperty.GetPropertyAccessMode());
        targetProperty.AddAnnotations(sourceProperty.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
    }

    protected virtual void Copy(IReadOnlyComplexProperty sourceProperty, IMutableComplexProperty targetProperty)
    {
        if (sourceProperty.FieldInfo is FieldInfo fieldInfo)
        {
            targetProperty.FieldInfo = fieldInfo;
        }
        targetProperty.IsNullable = sourceProperty.IsNullable;
        targetProperty.SetPropertyAccessMode(sourceProperty.GetPropertyAccessMode());
        targetProperty.AddAnnotations(sourceProperty.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
        Copy(sourceProperty.ComplexType, targetProperty.ComplexType);
    }

    protected virtual void Copy(IReadOnlyComplexType sourceComplexType, IMutableComplexType targetComplexType)
    {
        foreach (var property in sourceComplexType.GetDeclaredProperties())
        {
            var targetProperty = property.IsShadowProperty()
                ? targetComplexType.AddProperty(property.Name, property.ClrType)
                : targetComplexType.AddProperty(property.Name, property.ClrType, property.GetIdentifyingMemberInfo()!);
            Copy(property, targetProperty);
        }

        foreach (var property in sourceComplexType.GetDeclaredComplexProperties())
        {
            Copy(property, targetComplexType.AddComplexProperty(
                property.Name,
                property.ClrType,
                property.ComplexType.ClrType,
                property.ComplexType.Name,
                collection: property.IsCollection));
        }

        targetComplexType.AddAnnotations(sourceComplexType.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
    }

    protected virtual void Copy(IReadOnlyKey sourceKey, IMutableKey targetKey)
    {
        if (sourceKey.IsPrimaryKey())
        {
            targetKey.DeclaringEntityType.SetPrimaryKey(targetKey.Properties);
        }

        targetKey.AddAnnotations(sourceKey.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
    }

    protected virtual void Copy(IReadOnlyIndex sourceIndex, IMutableIndex targetIndex)
    {
        targetIndex.IsDescending = sourceIndex.IsDescending;
        targetIndex.IsUnique = sourceIndex.IsUnique;
        targetIndex.AddAnnotations(sourceIndex.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
    }

    protected virtual void Copy(IReadOnlyForeignKey sourceForeignKey, IMutableForeignKey targetForeignKey)
    {
        targetForeignKey.IsUnique = sourceForeignKey.IsUnique;
        targetForeignKey.IsRequired = sourceForeignKey.IsRequired;
        targetForeignKey.IsRequiredDependent = sourceForeignKey.IsRequiredDependent;
        targetForeignKey.DeleteBehavior = sourceForeignKey.DeleteBehavior;

        if (sourceForeignKey.DependentToPrincipal != null)
        {
            var clonedNavigation = sourceForeignKey.DependentToPrincipal.IsShadowProperty()
                ? targetForeignKey.SetDependentToPrincipal(sourceForeignKey.DependentToPrincipal.Name)
                : targetForeignKey.SetDependentToPrincipal(sourceForeignKey.DependentToPrincipal.GetIdentifyingMemberInfo());
            Copy(sourceForeignKey.DependentToPrincipal, clonedNavigation!);
        }

        if (sourceForeignKey.PrincipalToDependent != null)
        {
            var clonedNavigation = sourceForeignKey.PrincipalToDependent.IsShadowProperty()
                ? targetForeignKey.SetPrincipalToDependent(sourceForeignKey.PrincipalToDependent.Name)
                : targetForeignKey.SetPrincipalToDependent(sourceForeignKey.PrincipalToDependent.GetIdentifyingMemberInfo());
            Copy(sourceForeignKey.PrincipalToDependent, clonedNavigation!);
        }

        targetForeignKey.AddAnnotations(sourceForeignKey.GetAnnotations()
            .Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
    }

    protected virtual void Copy(IReadOnlyNavigation sourceNavigation, IMutableNavigation targetNavigation)
    {
        if (sourceNavigation.FieldInfo is FieldInfo fieldInfo)
        {
            targetNavigation.FieldInfo = fieldInfo;
        }

        targetNavigation.SetPropertyAccessMode(sourceNavigation.GetPropertyAccessMode());
        targetNavigation.SetIsEagerLoaded(sourceNavigation.IsEagerLoaded);
        targetNavigation.SetLazyLoadingEnabled(sourceNavigation.LazyLoadingEnabled);
        targetNavigation.AddAnnotations(sourceNavigation.GetAnnotations()
            .Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
    }

    protected virtual void Copy(IReadOnlySkipNavigation sourceNavigation, IMutableSkipNavigation targetNavigation)
    {
        if (sourceNavigation.FieldInfo is FieldInfo fieldInfo)
        {
            targetNavigation.FieldInfo = fieldInfo;
        }

        targetNavigation.SetPropertyAccessMode(sourceNavigation.GetPropertyAccessMode());
        targetNavigation.SetIsEagerLoaded(sourceNavigation.IsEagerLoaded);
        targetNavigation.SetLazyLoadingEnabled(sourceNavigation.LazyLoadingEnabled);
        targetNavigation.AddAnnotations(sourceNavigation.GetAnnotations().Where(a => !CoreAnnotationNames.AllNames.Contains(a.Name)));
        if (sourceNavigation.ForeignKey != null)
        {
            var targetDependentType = targetNavigation.DeclaringEntityType.Model.FindEntityType(
                sourceNavigation.ForeignKey.DeclaringEntityType.Name)!;
            var targetPrincipalType = targetNavigation.DeclaringEntityType.Model.FindEntityType(
                sourceNavigation.ForeignKey.PrincipalEntityType.Name)!;
            var targetKey = targetPrincipalType.FindKey(
                sourceNavigation.ForeignKey.PrincipalKey.Properties.Select(p => targetPrincipalType.FindProperty(p.Name)!).ToList())!;
            var targetForeignKey = targetDependentType.FindForeignKey(
                sourceNavigation.ForeignKey.Properties.Select(p => targetDependentType.FindProperty(p.Name)!).ToList(),
                targetKey,
                targetPrincipalType)!;
            targetNavigation.SetForeignKey(targetForeignKey);
        }

        if (sourceNavigation.Inverse != null)
        {
            var targetEntityType = targetNavigation.DeclaringEntityType.Model.FindEntityType(
                sourceNavigation.Inverse.DeclaringEntityType.Name)!;
            targetNavigation.SetInverse(
                targetEntityType.FindSkipNavigation(sourceNavigation.Inverse.Name));
        }
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
