// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license. 


using System.Runtime.CompilerServices;
using static Microsoft.EntityFrameworkCore.MergeOptionFeature.NorthwindMergeOptionFeatureContext;

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class RefreshFromDb_ComplexTypes_SqlServer_Test : IClassFixture<NorthwindMergeOptionFeatureFixture>
{
    private readonly NorthwindMergeOptionFeatureFixture _fixture;

    public RefreshFromDb_ComplexTypes_SqlServer_Test(
        NorthwindMergeOptionFeatureFixture fx) => _fixture = fx;

    /// <summary>
    /// Tests the behavior of refreshing collection owned types from the database.
    /// </summary>
    [Fact]
    public async Task Test_CollectionOwnedTypes()
    {
        using var ctx = _fixture.CreateContext();
        // Include is redundant for owned properties - they are loaded automatically
        var product = await ctx.ComplexProducts.OrderBy(c => c.Id).FirstAsync();
        var originalReviewCount = product.Reviews.Count;

        // Simulate external change to collection owned type
        var newReview = new ComplexReviewEntity
        {
            Rating = 5,
            Comment = "Great product!"
        };
        await ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO [ComplexProductReview] ([ProductId], [Rating], [Comment]) VALUES ({0}, {1}, {2})",
            product.Id, newReview.Rating, newReview.Comment);

        // For owned entities, we need to reload the entire owner entity
        // because owned entities cannot be tracked without their owner
        await ctx.Entry(product).ReloadAsync();

        // Assert
        Assert.Equal(originalReviewCount + 1, product.Reviews.Count);
        Assert.Contains(product.Reviews, r => r.Comment == "Great product!");
    }

    ///// <summary>
    ///// test for non-collection owned types refresh from database
    ///// </summary>
    //[Fact]
    //public async Task Test_NonCollectionOwnedTypes()
    //{
    //    using var ctx = _fixture.CreateContext();

    //    var product = await ctx.Products.OrderBy(c => c.Id).FirstAsync();
    //    var originalName = product.Details.Name;

    //    try
    //    {
    //        // Simulate external change to non-collection owned type
    //        var newName = "Updated Product Name";
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "UPDATE [Products] SET [Details_Name] = {0} WHERE [Id] = {1}",
    //            newName, product.Id);

    //        // Refresh the entity
    //        await ctx.Entry(product).ReloadAsync();

    //        // Assert
    //        Assert.Equal(newName, product.Details.Name);
    //    }
    //    finally
    //    {
    //        // Cleanup
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "UPDATE [Products] SET [Details_Name] = {0} WHERE [Id] = {1}",
    //            originalName, product.Id);
    //    }
    //}


    ///// <summary>
    ///// test collection complex properties refresh from database
    ///// </summary>
    //[Fact]
    //public async Task Test_CollectionComplexProperties()
    //{
    //    using var ctx = _fixture.CreateContext();

    //    // Addresses is an owned collection, not a complex collection
    //    var customer = await ctx.Customers.OrderBy(c => c.Id).AsNoTracking().FirstAsync();
    //    var originalAddressCount = customer.Addresses.Count;

    //    try
    //    {
    //        // Simulate external change to owned collection
    //        var newAddress = new Address { Street = "123 New St", City = "New City", PostalCode = "12345" };
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "INSERT INTO [CustomerAddress] ([CustomerId], [Street], [City], [PostalCode]) VALUES ({0}, {1}, {2}, {3})",
    //            customer.Id, newAddress.Street, newAddress.City, newAddress.PostalCode);

    //        // For owned entities, reload the entire entity to avoid duplicates
    //        var addresses = ctx.Entry<Customer>(customer).Collection(c => c.Addresses);
    //        addresses.IsLoaded = false;
    //        await addresses.LoadAsync(LoadOptions.ForceIdentityResolution);

    //        // Assert
    //        Assert.Equal(originalAddressCount + 1, customer.Addresses.Count);
    //        Assert.Contains(customer.Addresses, a => a.Street == "123 New St");
    //    }
    //    finally
    //    {
    //        // Cleanup
    //        await ctx.Database.ExecuteSqlRawAsync(
    //            "DELETE FROM [CustomerAddress] WHERE [Street] = {0}",
    //            "123 New St");
    //    }
    //}

    //[Fact]
    //public async Task Test_NonCollectionComplexProperties()
    //{
    //    using var ctx = _fixture.CreateContext();

    //    var customer = await ctx.Customers.OrderBy(c => c.Id).FirstAsync();
    //    var originalContactPhone = customer.Contact.Phone;

    //    // Simulate external change to non-collection complex property
    //    var newPhone = "555-0199";
    //    await ctx.Database.ExecuteSqlRawAsync(
    //        "UPDATE [Customers] SET [Contact_Phone] = {0} WHERE [Id] = {1}",
    //        newPhone, customer.Id);

    //    // Refresh the entity
    //    await ctx.Entry(customer).ReloadAsync();

    //    // Assert
    //    Assert.Equal(newPhone, customer.Contact.Phone);

    //    // Cleanup
    //    await ctx.Database.ExecuteSqlRawAsync(
    //        "UPDATE [Customers] SET [Contact_Phone] = {0} WHERE [Id] = {1}",
    //        originalContactPhone, customer.Id);
    //}


}
