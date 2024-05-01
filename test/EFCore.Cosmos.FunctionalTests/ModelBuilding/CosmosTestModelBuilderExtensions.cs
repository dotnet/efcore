// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

#nullable disable

public static class CosmosTestModelBuilderExtensions
{
    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> HasPartitionKey<TEntity, TProperty>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.HasPartitionKey(propertyExpression);
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                var names = propertyExpression.GetMemberAccessList().Select(e => e.GetSimpleMemberName()).ToList();
                nonGenericBuilder.Instance.HasPartitionKey(names.FirstOrDefault(), names.Count > 1 ? names.Skip(1).ToArray() : []);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> HasPartitionKey<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        params string[] additionalPropertyNames)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.HasPartitionKey(name, additionalPropertyNames);
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasPartitionKey(name, additionalPropertyNames);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPropertyBuilder<TProperty> ToJsonProperty<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        string name)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.ToJsonProperty(name);
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToJsonProperty(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> ToJsonProperty<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        string name)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.ToJsonProperty(name);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToJsonProperty(name);
                break;
        }

        return builder;
    }
}
