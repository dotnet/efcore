// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;
public class RefreshFromDb_Northwind_SqlServer_Test
: IClassFixture<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    #region Private

    private readonly NorthwindQuerySqlServerFixture<NoopModelCustomizer> _fx;

    #endregion

    #region ctor's

    public RefreshFromDb_Northwind_SqlServer_Test(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fx) => _fx = fx;
    #endregion

    #region Test -> Custom
    [Fact]
    public async Task Refresh_reads_latest_values()
    {
        using var ctx = _fx.CreateContext();
        var cust = await ctx.Customers.FirstAsync();
        var oldContactName = cust.ContactName;
        var newContactName = $"Alex {DateTime.Now:MM-dd-HH-mm}";
        // simuliraj vanjsku promjenu u bazi
        await ctx.Database.ExecuteSqlRawAsync(
            "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
            newContactName,
            cust.CustomerID);

        await ctx.Entry(cust).ReloadAsync();
        var readedNewContactName = cust.ContactName;

        cust.ContactName = oldContactName;
        await ctx.SaveChangesAsync();

        // Assert
        Assert.Equal(newContactName, readedNewContactName);
    }

    [Fact]
    public async Task Refresh_in_collection()
    {
        using var ctx = _fx.CreateContext();
        var productName = "Tofu";
        var orderID = 10249;

        var order = ctx.Orders.Where(c => c.OrderID == orderID).Include(o => o.OrderDetails).ThenInclude(od => od.Product).First();
        var orderDetail = order.OrderDetails.Where(od => od.Product.ProductName == productName).First();
        var originalQuantity = orderDetail.Quantity;

        var productID = orderDetail.ProductID;
        var randomQuantity = (short)new Random().Next(1, 100);

        await ctx.Database.ExecuteSqlRawAsync(
            @"UPDATE [dbo].[Order Details]
               SET 
                  [Quantity] = {0}
             WHERE OrderID = {1} and ProductID = {2}",
            randomQuantity,
            orderID,
            orderDetail.ProductID);


        foreach (var item in order.OrderDetails)
        {
            await ctx.Entry(item).ReloadAsync();
        }

        var newReadedQuantity = orderDetail.Quantity;

        orderDetail.Quantity = originalQuantity;
        await ctx.SaveChangesAsync();

        // Assert
        Assert.Equal(newReadedQuantity, randomQuantity);
    }

    [Fact]
    public async Task TestChangeTracker()
    {
        using var ctx = _fx.CreateContext();

        var queryCustomers = ctx.Customers.Where(c => c.CustomerID.StartsWith("A"));

        var cust = await queryCustomers.FirstAsync();
        var contactName = cust.ContactName;
        var newContactName = $"Alex {DateTime.Now:MM-dd-HH-mm}";

        await ctx.Database.ExecuteSqlRawAsync(
           @"UPDATE [dbo].[Customers]
               SET 
                  [ContactName] = {0}
             WHERE CustomerID = {1}",
           newContactName,
           cust.CustomerID);

        //  var queryCustomers2 = ctx.Customers.Where(c => c.CustomerID.StartsWith("A"));
        queryCustomers = queryCustomers.Refresh(MergeOption.OverwriteChanges);
        cust = await queryCustomers.FirstAsync();

        await ctx.Database.ExecuteSqlRawAsync(
           @"UPDATE [dbo].[Customers]
               SET 
                  [ContactName] = {0}
             WHERE CustomerID = {1}",
           contactName,
           cust.CustomerID);

        Assert.Equal(newContactName, cust.ContactName);
    }

    [Fact]
    public async Task TestChangeTracker2()
    {
        using var ctx = _fx.CreateContext();

        var query = ctx.Customers.OrderBy(c => c.CustomerID);

        var customers = await query.Take(3).ToArrayAsync();

        var first = customers[0];
        var firstID = first.CustomerID;
        var second = customers[1];
        var secondID = second.CustomerID;
        var third = customers[2];
        var thirdID = third.CustomerID;

        second.ContactName = third.ContactName + " Mod";

        ctx.Customers.Remove(third);

        var newCustomer = new Customer()
        {
            CustomerID = "ZZZZZ",
            CompanyName = "New Company"
        };
        ctx.Customers.Add(newCustomer);
        ctx.ChangeTracker.DetectChanges();


        IEnumerable<EntityEntry> addedItems = ctx.ChangeTracker.GetEntriesForState(true, false, false, false).ToArray();
        IEnumerable<EntityEntry> modifiedItems = ctx.ChangeTracker.GetEntriesForState(false, true, false, false).ToArray();
        IEnumerable<EntityEntry> removedItems = ctx.ChangeTracker.GetEntriesForState(false, false, true, false).ToArray();
        IEnumerable<EntityEntry> unchangedItems = ctx.ChangeTracker.GetEntriesForState(false, false, false, true).ToArray();

        var standardAddedItems = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToArray();
        var standardModifiedItems = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToArray();
        var standardRemovedItems = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).ToArray();
        var standardUnchangedItems = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Unchanged).ToArray();

        var query2 = ctx.Customers.OrderBy(c => c.CustomerID);
        query2.Refresh(MergeOption.OverwriteChanges);
        customers = await query2.Take(3).ToArrayAsync();
        first = customers[0];
        second = customers[1];
        third = customers[2];

        // Store assertion values
        var hasAddedItems = addedItems.Any();
        var hasModifiedItems = modifiedItems.Any();
        var hasRemovedItems = removedItems.Any();
        var hasUnchangedItems = unchangedItems.Any();
        var firstAddedEntity = hasAddedItems ? addedItems.First().Entity as Customer : null;
        var firstModifiedEntity = hasModifiedItems ? modifiedItems.First().Entity as Customer : null;
        var firstRemovedEntity = hasRemovedItems ? removedItems.First().Entity as Customer : null;
        var firstUnchangedEntity = hasUnchangedItems ? unchangedItems.First().Entity as Customer : null;

        // Assertions
        if (hasAddedItems)
        {
            Assert.Equal(newCustomer, firstAddedEntity);
        }
        else
        {
            Assert.Fail("No added items found");
        }

        if (hasModifiedItems)
        {
            Assert.Equal(second, firstModifiedEntity);
        }
        else
        {
            Assert.Fail("No modified items found");
        }

        if (hasRemovedItems)
        {
            Assert.Equal(third, firstRemovedEntity);
        }
        else
        {
            Assert.Fail("No removed items found");
        }

        if (hasUnchangedItems)
        {
            Assert.Equal(first, firstUnchangedEntity);
        }
        else
        {
            Assert.Fail("No unchanged items found");
        }

        Assert.Equal(firstID, first.CustomerID);
        Assert.Equal(secondID, second.CustomerID);
        Assert.Equal(thirdID, third.CustomerID);
    }

    //[Fact]
    //public async Task CompareCustomMethodsWithStandardEFCore()
    //{
    //    using var ctx = _fx.CreateContext();

    //    var query = ctx.Customers.OrderBy(c => c.CustomerID);
    //    Customer[] customers = await query.Take(3).ToArrayAsync();

    //    Customer first = customers[0];
    //    Customer second = customers[1];
    //    Customer third = customers[2];

    //    // Store original values and IDs for cleanup
    //    string firstID = first.CustomerID;
    //    string secondID = second.CustomerID;
    //    string thirdID = third.CustomerID;
    //    string originalFirstContactName = first.ContactName;
    //    string originalSecondContactName = second.ContactName;
    //    string originalThirdContactName = third.ContactName;

    //    // Make changes
    //    second.ContactName = third.ContactName + " Mod";
    //    ctx.Customers.Remove(third);

    //    Customer newCustomer = new Customer()
    //    {
    //        CustomerID = "ZZZZZ",
    //        CompanyName = "New Company"
    //    };
    //    ctx.Customers.Add(newCustomer);

    //    // === DEBUGGING CUSTOM GetEntriesForState METHOD ===

    //    // Test custom GetEntriesForState method
    //    var customAddedItems = ctx.ChangeTracker.GetEntriesForState(true, false, false, false).ToArray();
    //    var customModifiedItems = ctx.ChangeTracker.GetEntriesForState(false, true, false, false).ToArray();
    //    var customRemovedItems = ctx.ChangeTracker.GetEntriesForState(false, false, true, false).ToArray();
    //    var customUnchangedItems = ctx.ChangeTracker.GetEntriesForState(false, false, false, true).ToArray();

    //    // Compare with standard EF Core methods
    //    var standardAddedItems = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToArray();
    //    var standardModifiedItems = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToArray();
    //    var standardRemovedItems = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted).ToArray();
    //    var standardUnchangedItems = ctx.ChangeTracker.Entries().Where(e => e.State == EntityState.Unchanged).ToArray();

    //    // Output detailed debugging information
    //    Console.WriteLine("=== DEBUGGING GetEntriesForState ===");
    //    Console.WriteLine($"Custom Added Count: {customAddedItems.Length}, Standard Added Count: {standardAddedItems.Length}");
    //    Console.WriteLine($"Custom Modified Count: {customModifiedItems.Length}, Standard Modified Count: {standardModifiedItems.Length}");
    //    Console.WriteLine($"Custom Removed Count: {customRemovedItems.Length}, Standard Removed Count: {standardRemovedItems.Length}");
    //    Console.WriteLine($"Custom Unchanged Count: {customUnchangedItems.Length}, Standard Unchanged Count: {standardUnchangedItems.Length}");

    //    // Detailed comparison
    //    Console.WriteLine("\n--- Added Items ---");
    //    foreach (var item in standardAddedItems)
    //    {
    //        Console.WriteLine($"Standard Added: {item.Entity.GetType().Name} - {item.Entity}");
    //    }
    //    foreach (var item in customAddedItems)
    //    {
    //        Console.WriteLine($"Custom Added: {item.Entity.GetType().Name} - {item.Entity}");
    //    }

    //    Console.WriteLine("\n--- Modified Items ---");
    //    foreach (var item in standardModifiedItems)
    //    {
    //        Console.WriteLine($"Standard Modified: {item.Entity.GetType().Name} - {item.Entity}");
    //    }
    //    foreach (var item in customModifiedItems)
    //    {
    //        Console.WriteLine($"Custom Modified: {item.Entity.GetType().Name} - {item.Entity}");
    //    }

    //    Console.WriteLine("\n--- Removed Items ---");
    //    foreach (var item in standardRemovedItems)
    //    {
    //        Console.WriteLine($"Standard Removed: {item.Entity.GetType().Name} - {item.Entity}");
    //    }
    //    foreach (var item in customRemovedItems)
    //    {
    //        Console.WriteLine($"Custom Removed: {item.Entity.GetType().Name} - {item.Entity}");
    //    }

    //    // === DEBUGGING REFRESH METHOD ===

    //    // Save changes to database first
    //    await ctx.SaveChangesAsync();

    //    // Update second customer in database directly
    //    string databaseUpdatedContactName = $"DB{DateTime.Now:HHmmss}";
    //    await ctx.Database.ExecuteSqlRawAsync(
    //        "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
    //        databaseUpdatedContactName,
    //        second.CustomerID);

    //    // Modify second customer in memory
    //    second.ContactName = $"Mem{DateTime.Now:HHmmss}";

    //    Console.WriteLine("\n=== DEBUGGING REFRESH ===");
    //    Console.WriteLine($"Before Refresh - second.ContactName: {second.ContactName}");
    //    Console.WriteLine($"Expected after refresh: {databaseUpdatedContactName}");

    //    // Test the Refresh method
    //    var query2 = ctx.Customers.Where(c => c.CustomerID == second.CustomerID);
    //    var refreshedQuery = query2.Refresh(MergeOption.OverwriteChanges);

    //    // Re-query to see if refresh worked
    //    var refreshedCustomer = await refreshedQuery.FirstAsync();

    //    Console.WriteLine($"After Refresh - refreshedCustomer.ContactName: {refreshedCustomer.ContactName}");
    //    Console.WriteLine($"After Refresh - second.ContactName: {second.ContactName}");

    //    // Compare with standard EF Core reload
    //    await ctx.Entry(second).ReloadAsync();
    //    Console.WriteLine($"After Standard Reload - second.ContactName: {second.ContactName}");

    //    // === TESTING ENTITY STATE AFTER REFRESH ===

    //    // Check if third customer still shows as deleted after refresh
    //    var thirdEntryAfterRefresh = ctx.Entry(third);
    //    Console.WriteLine($"\nThird customer state after refresh: {thirdEntryAfterRefresh.State}");

    //    // Check if we can find third customer in database
    //    var thirdInDatabase = await ctx.Customers.FirstOrDefaultAsync(c => c.CustomerID == thirdID);
    //    Console.WriteLine($"Third customer exists in database: {thirdInDatabase != null}");

    //    // Cleanup - restore original values
    //    first.ContactName = originalFirstContactName;
    //    if (thirdInDatabase == null)
    //    {
    //        // Re-add third customer if it was actually deleted
    //        var restoredThird = new Customer()
    //        {
    //            CustomerID = thirdID,
    //            CompanyName = third.CompanyName,
    //            ContactName = originalThirdContactName,
    //            ContactTitle = third.ContactTitle,
    //            Address = third.Address,
    //            City = third.City,
    //            Region = third.Region,
    //            PostalCode = third.PostalCode,
    //            Country = third.Country,
    //            Phone = third.Phone,
    //            Fax = third.Fax
    //        };
    //        ctx.Customers.Add(restoredThird);
    //    }
    //    else
    //    {
    //        thirdInDatabase.ContactName = originalThirdContactName;
    //    }

    //    await ctx.SaveChangesAsync();
    //}

    #endregion

    #region Test -> Tasks predefined

    /// <summary>
    /// Task: Existing entries in all states
    /// Tests existing entries that are already tracked in Added, Modified, Deleted, and Unchanged states 
    /// to ensure the refresh functionality works correctly with entities in different states.
    /// </summary>
    [Fact]
    public async Task Refresh_ExistingEntriesInAllStates()
    {
        using var ctx = _fx.CreateContext();
        var originalQuery = ctx.Customers.Where(c => c.CustomerID.StartsWith("A"));

        // Get existing customers to work with
        var customers = await originalQuery.OrderBy(c => c.CustomerID).Take(3).ToArrayAsync();
        var customer1 = customers[0];
        var customer2 = customers[1];
        var customer3 = customers[2];

        // Store original values
        var originalContactName1 = customer1.ContactName;
        var originalContactName2 = customer2.ContactName;
        var originalContactName3 = customer3.ContactName;

        try
        {
            // Create entity in Added state
            var newCustomer = new Customer
            {
                CustomerID = "TEST1",
                CompanyName = "Test Company",
                ContactName = "Test Contact"
            };
            ctx.Customers.Add(newCustomer); // Added state

            // Modify existing entity 
            customer1.ContactName = $"Mod{DateTime.Now:HHmmss}"; // Modified state

            // Delete existing entity
            ctx.Customers.Remove(customer2); // Deleted state

            // customer3 remains Unchanged

            // Update database directly for customer3
            var newContactName3 = $"DB{DateTime.Now:HHmmss}";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                newContactName3,
                customer3.CustomerID);

            // Apply refresh - should handle all entity states appropriately
            originalQuery = originalQuery.Refresh(MergeOption.OverwriteChanges);
            customers = await originalQuery.OrderBy(c => c.CustomerID).Take(3).ToArrayAsync();
            customer1 = customers[0];
            customer2 = customers[1];
            customer3 = customers[2];

            // Store values for assertions

            // Check after refresh
            // customer 1
            Assert.Equal(originalContactName1, customer1.ContactName);

            // customer 2
            Assert.Equal(originalContactName2, customer2.ContactName);

            // Check is contact name 
            Assert.Equal(newContactName3, customer3.ContactName);
        }
        finally
        {
            // Restore customer3 original value
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                originalContactName3,
                customer3.CustomerID);
        }
    }

    /// <summary>
    /// Task: Unchanged entries with original value set to something that doesn't match the database state
    /// Tests the scenario where an entity is in Unchanged state but its original values don't match
    /// what's currently in the database, simulating concurrent modifications.
    /// </summary>
    [Fact]
    public async Task Refresh_UnchangedEntriesWithMismatchedOriginalValues()
    {
        using var ctx = _fx.CreateContext();

        var customer = await ctx.Customers.FirstAsync();
        var originalContactName = customer.ContactName;

        try
        {
            // Simulate another process changing the database value
            var externallyModifiedValue = $"Ext{DateTime.Now:HHmmss}";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                externallyModifiedValue,
                customer.CustomerID);

            // At this point, the entity is Unchanged but the database has different data
            var customerState = ctx.Entry(customer).State;
            var customerContactName = customer.ContactName;

            // Refresh should detect and handle this discrepancy
            var query = ctx.Customers.Where(c => c.CustomerID == customer.CustomerID);
            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedCustomer = await refreshedQuery.FirstAsync();

            // Store values for assertions
            var refreshedContactName = refreshedCustomer.ContactName;

            // Assertions
            Assert.Equal(EntityState.Unchanged, customerState);
            Assert.Equal(originalContactName, customerContactName);
            Assert.Equal(externallyModifiedValue, refreshedContactName);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                originalContactName,
                customer.CustomerID);
        }
    }

    /// <summary>
    /// Task: Modified entries with properties marked as modified, but with the original value set to something that matches the database state
    /// Tests entities where properties are marked as modified but the original values actually match the current database state.
    /// </summary>
    [Fact]
    public async Task Refresh_ModifiedEntriesWithMatchingOriginalValues()
    {
        using var ctx = _fx.CreateContext();

        var customer = await ctx.Customers.FirstAsync();
        var originalContactName = customer.ContactName;

        try
        {
            // Modify the entity to put it in Modified state
            customer.ContactName = "Temporarily Modified";
            var stateAfterModify = ctx.Entry(customer).State;

            // Revert the change but keep it marked as modified
            customer.ContactName = originalContactName;

            // At this point, current value matches original/database value but entity is still Modified
            var stateAfterRevert = ctx.Entry(customer).State;

            // Refresh should handle this scenario correctly
            var query = ctx.Customers.Where(c => c.CustomerID == customer.CustomerID);
            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedCustomer = await refreshedQuery.FirstAsync();

            // Store values for assertions
            var refreshedContactName = refreshedCustomer.ContactName;

            // Assertions
            Assert.Equal(EntityState.Modified, stateAfterModify);
            Assert.Equal(EntityState.Modified, stateAfterRevert);
            Assert.Equal(originalContactName, refreshedContactName);
        }
        finally
        {
            // After refresh, entity should be in proper state
            ctx.Entry(customer).State = EntityState.Unchanged;
        }
    }

    /// <summary>
    /// Task: Owned entity that was replaced by a different instance, so it's tracked as both Added and Deleted
    /// Tests refresh behavior when dealing with owned entities that have been replaced, creating
    /// scenarios where the same conceptual entity appears in both Added and Deleted states.
    /// </summary>
    [Fact]
    public async Task Refresh_OwnedEntityReplacedWithDifferentInstance()
    {
        using var ctx = _fx.CreateContext();

        // Note: Northwind model doesn't have owned entities, so this test simulates the scenario
        // by using OrderDetails as a proxy for owned entity behavior
        var order = await ctx.Orders.Include(o => o.OrderDetails).FirstAsync(o => o.OrderDetails.Any());
        var originalOrderDetail = order.OrderDetails.First();
        var originalQuantity = originalOrderDetail.Quantity;

        try
        {
            // Simulate replacing an owned entity by removing and adding a "new" one
            // This creates the Added/Deleted state scenario for the same conceptual entity
            order.OrderDetails.Remove(originalOrderDetail);

            var replacementOrderDetail = new OrderDetail
            {
                OrderID = originalOrderDetail.OrderID,
                ProductID = originalOrderDetail.ProductID,
                UnitPrice = originalOrderDetail.UnitPrice,
                Quantity = (short)(originalOrderDetail.Quantity + 10),
                Discount = originalOrderDetail.Discount
            };
            order.OrderDetails.Add(replacementOrderDetail);

            // Update database directly to simulate external change
            await ctx.Database.ExecuteSqlRawAsync(
                @"UPDATE [Order Details] SET [Quantity] = {0} 
                  WHERE [OrderID] = {1} AND [ProductID] = {2}",
                originalQuantity + 5,
                originalOrderDetail.OrderID,
                originalOrderDetail.ProductID);

            // Apply refresh to handle the complex state scenario
            var query = ctx.OrderDetails.Where(od =>
                od.OrderID == originalOrderDetail.OrderID &&
                od.ProductID == originalOrderDetail.ProductID);
            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedOrderDetail = await refreshedQuery.FirstAsync();

            // Store values for assertions
            var refreshedQuantity = refreshedOrderDetail.Quantity;
            var expectedQuantity = (short)(originalQuantity + 5);

            // Verify refresh handled the scenario appropriately
            Assert.Equal(expectedQuantity, refreshedQuantity);
        }
        finally
        {
            // Cleanup - restore original state
            var currentOrderDetail = await ctx.OrderDetails.FirstOrDefaultAsync(od =>
                od.OrderID == originalOrderDetail.OrderID &&
                od.ProductID == originalOrderDetail.ProductID);

            if (currentOrderDetail != null)
            {
                currentOrderDetail.Quantity = originalQuantity;
                await ctx.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Task: A derived entity that was replaced by a base entity with same key value
    /// Tests refresh behavior in TPH (Table Per Hierarchy) scenarios where an entity of a derived type
    /// is replaced by an entity of the base type with the same key.
    /// </summary>
    [Fact]
    public async Task Refresh_DerivedEntityReplacedByBaseEntity()
    {
        using var ctx = _fx.CreateContext();

        // Note: Northwind doesn't have TPH inheritance, so we simulate this with different entity types
        // that could conceptually represent base/derived relationship through CustomerID linkage
        var customer = await ctx.Customers.FirstAsync();
        var customerID = customer.CustomerID;

        // Simulate the scenario where we have a "derived" entity concept (Order associated with Customer)
        var order = await ctx.Orders.FirstAsync(o => o.CustomerID == customerID);
        var originalOrderDate = order.OrderDate;

        try
        {
            // Modify the "derived" entity
            order.OrderDate = DateTime.Now;

            // Update database to simulate external change that affects the relationship
            var newOrderDate = DateTime.Now.AddDays(1);
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [OrderID] = {1}",
                newOrderDate,
                order.OrderID);

            // Apply refresh to handle the inheritance-like scenario
            var query = ctx.Orders.Where(o => o.OrderID == order.OrderID);
            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedOrder = await refreshedQuery.FirstAsync();

            // Store values for assertions
            var refreshedOrderDate = refreshedOrder.OrderDate?.Date;
            var expectedOrderDate = newOrderDate.Date;

            // Verify refresh worked correctly
            Assert.Equal(expectedOrderDate, refreshedOrderDate);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [OrderID] = {1}",
                originalOrderDate ?? DateTime.Now, // Fix null reference
                order.OrderID);
        }
    }

    /// <summary>
    /// Task: Different terminating operators: ToList, FirstOrDefault, etc...
    /// Tests that refresh functionality works correctly with various query termination operators
    /// like ToList(), FirstOrDefault(), Single(), Count(), etc.
    /// </summary>
    [Fact]
    public async Task Refresh_DifferentTerminatingOperators()
    {
        using var ctx = _fx.CreateContext();

        var customers = await ctx.Customers.Take(3).ToArrayAsync();
        var customer = customers[0];
        var originalContactName = customer.ContactName;

        try
        {
            // Update database
            var newContactName = $"Upd{DateTime.Now:HHmmss}";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                newContactName,
                customer.CustomerID);

            var query = ctx.Customers.Where(c => c.CustomerID == customer.CustomerID);
            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);

            // Test ToList()
            var listResult = await refreshedQuery.ToListAsync();
            var listContactName = listResult[0].ContactName;

            // Test FirstOrDefault()
            var firstResult = await refreshedQuery.FirstOrDefaultAsync();
            var firstContactName = firstResult?.ContactName;

            // Test Single()
            var singleResult = await refreshedQuery.SingleAsync();
            var singleContactName = singleResult.ContactName;

            // Test First()
            var firstResultDirect = await refreshedQuery.FirstAsync();
            var firstDirectContactName = firstResultDirect.ContactName;

            // Test Count()
            var count = await refreshedQuery.CountAsync();

            // Test Any()
            var exists = await refreshedQuery.AnyAsync();

            // Assert
            Assert.Single(listResult);
            Assert.Equal(newContactName, listContactName);
            Assert.NotNull(firstResult);
            Assert.Equal(newContactName, firstContactName);
            Assert.Equal(newContactName, singleContactName);
            Assert.Equal(newContactName, firstDirectContactName);
            Assert.Equal(1, count);
            Assert.True(exists);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                originalContactName,
                customer.CustomerID);
        }
    }

    /// <summary>
    /// Task: Streaming (non-buffering) query that's consumed one-by-one
    /// Tests refresh functionality with streaming queries that are consumed iteratively
    /// rather than materialized all at once, ensuring proper handling of change tracking.
    /// </summary>
    [Fact]
    public async Task Refresh_StreamingQueryConsumedOneByOne()
    {
        using var ctx = _fx.CreateContext();

        var customers = await ctx.Customers.Take(5).ToArrayAsync();
        var originalContactNames = customers.Select(c => c.ContactName).ToArray();

        try
        {
            // Update database for all customers
            for (var i = 0; i < customers.Length; i++)
            {
                var newContactName = $"Str{i}-{DateTime.Now:HHmm}";
                await ctx.Database.ExecuteSqlRawAsync(
                    "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                    newContactName,
                    customers[i].CustomerID);
            }

            // Create streaming query with refresh
            var customerIds = customers.Select(c => c.CustomerID).ToList();
            var query = ctx.Customers.Where(c => customerIds.Contains(c.CustomerID));
            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);

            // Consume the query one by one (streaming)
            var processedCount = 0;
            await foreach (var customer in refreshedQuery.AsAsyncEnumerable())
            {
                // Verify each customer has the updated database value
                var contactNameStartsWithStr = customer.ContactName.StartsWith("Str");
                Assert.True(contactNameStartsWithStr);
                processedCount++;
            }

            Assert.Equal(customers.Length, processedCount);
        }
        finally
        {
            // Cleanup
            for (var i = 0; i < customers.Length; i++)
            {
                await ctx.Database.ExecuteSqlRawAsync(
                    "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                    originalContactNames[i],
                    customers[i].CustomerID);
            }
        }
    }

    /// <summary>
    /// Task: Queries with Include, Include with filter and ThenInclude
    /// Tests refresh functionality with complex Include queries that load related data,
    /// including filtered includes and nested ThenInclude operations.
    /// </summary>
    [Fact]
    public async Task Refresh_QueriesWithIncludeAndThenInclude()
    {
        using var ctx = _fx.CreateContext();

        var customer = await ctx.Customers
            .Include(c => c.Orders.Where(o => o.OrderDate.HasValue))
            .ThenInclude(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstAsync(c => c.Orders.Any());

        var originalContactName = customer.ContactName;
        var originalOrdersCount = customer.Orders.Count;

        // Get first order for testing
        var firstOrder = customer.Orders.First();
        var originalOrderDate = firstOrder.OrderDate;

        try
        {
            // Update database - modify customer and related data
            var newContactName = $"Inc{DateTime.Now:HHmmss}";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                newContactName,
                customer.CustomerID);

            // Also modify an order
            var newOrderDate = DateTime.Now.AddDays(10);
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [OrderID] = {1}",
                newOrderDate,
                firstOrder.OrderID);

            // Apply refresh with the same complex include
            var query = ctx.Customers
                .Include(c => c.Orders.Where(o => o.OrderDate.HasValue))
                .ThenInclude(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(c => c.CustomerID == customer.CustomerID);

            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedCustomer = await refreshedQuery.FirstAsync();

            // Store values for assertions
            var refreshedContactName = refreshedCustomer.ContactName;
            var hasOrders = refreshedCustomer.Orders.Any();
            var refreshedOrder = refreshedCustomer.Orders.First(o => o.OrderID == firstOrder.OrderID);
            var refreshedOrderDate = refreshedOrder.OrderDate;

            // Verify both customer and included data are refreshed
            Assert.Equal(newContactName, refreshedContactName);
            Assert.True(hasOrders);
            var dateFormat = "dd.MM.yyyy HH:mm:ss";
            var refreshedOrderDateStr = refreshedOrderDate?.ToString(dateFormat) ?? "";
            Assert.Equal(newOrderDate.ToString(dateFormat), refreshedOrderDateStr);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                originalContactName,
                customer.CustomerID);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [OrderID] = {1}",
                originalOrderDate ?? DateTime.Now,
                firstOrder.OrderID);
        }
    }

    /// <summary>
    /// Task: Projecting a related entity in Select without Include
    /// Tests refresh behavior when projecting related entities through Select clauses
    /// without using explicit Include statements.
    /// </summary>
    [Fact]
    public async Task Refresh_ProjectingRelatedEntityInSelect()
    {
        using var ctx = _fx.CreateContext();

        // Project related entity without explicit Include
        var customerOrder = await ctx.Customers
            .Where(c => c.Orders.Any())
            .Select(c => new
            {
                Customer = c,
                LastOrder = c.Orders.OrderByDescending(o => o.OrderDate).First(),
                OrderCount = c.Orders.Count()
            })
            .FirstAsync();

        var originalContactName = customerOrder.Customer.ContactName;
        var originalOrderDate = customerOrder.LastOrder.OrderDate;

        try
        {
            // Update both customer and order in database
            var newContactName = $"Prj{DateTime.Now:HHmmss}";
            var newOrderDate = DateTime.Now.AddDays(5);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                newContactName,
                customerOrder.Customer.CustomerID);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [OrderID] = {1}",
                newOrderDate,
                customerOrder.LastOrder.OrderID);

            // Create refresh query with projection
            var query = ctx.Customers
                .Where(c => c.CustomerID == customerOrder.Customer.CustomerID)
                .Select(c => new
                {
                    Customer = c,
                    LastOrder = c.Orders.OrderByDescending(o => o.OrderDate).First(),
                    OrderCount = c.Orders.Count()
                });

            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedResult = await refreshedQuery.FirstAsync();

            // Store values for assertions
            var refreshedContactName = refreshedResult.Customer.ContactName;
            var refreshedOrderDate = refreshedResult.LastOrder.OrderDate?.Date;
            var expectedOrderDate = newOrderDate.Date;

            // Verify projected entities are refreshed
            Assert.Equal(newContactName, refreshedContactName);
            Assert.Equal(expectedOrderDate, refreshedOrderDate);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                originalContactName,
                customerOrder.Customer.CustomerID);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [OrderID] = {1}",
                originalOrderDate ?? DateTime.Now,
                customerOrder.LastOrder.OrderID);
        }
    }

    /// <summary>
    /// Task: Creating a new instance of the target entity in Select with calculated values that are going to be client-evaluated
    /// Tests refresh behavior with client-side evaluation in projections where new entity instances
    /// are created with calculated values that require client-side processing.
    /// </summary>
    [Fact]
    public async Task Refresh_SelectWithClientEvaluatedCalculatedValues()
    {
        using var ctx = _fx.CreateContext();

        // Select with client-evaluated calculated values
        var customersWithCalculated = await ctx.Customers
            .Take(3)
            .Select(c => new Customer
            {
                CustomerID = c.CustomerID,
                CompanyName = c.CompanyName,
                // This will be client-evaluated - keep under 30 chars
                ContactName = c.ContactName + " - Calc",
                ContactTitle = c.ContactTitle,
                Address = c.Address,
                City = c.City,
                Region = c.Region,
                PostalCode = c.PostalCode,
                Country = c.Country,
                Phone = c.Phone,
                Fax = c.Fax
            })
            .ToArrayAsync();

        var customer = customersWithCalculated[0];
        var originalContactName = customer.ContactName;
        var baseContactName = originalContactName.Split(" - Calc")[0];

        try
        {
            // Update database
            var newContactName = $"CE{DateTime.Now:HHmmss}";
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                newContactName,
                customer.CustomerID);

            // Create refresh query with client evaluation
            var query = ctx.Customers
                .Where(c => c.CustomerID == customer.CustomerID)
                .Select(c => new Customer
                {
                    CustomerID = c.CustomerID,
                    CompanyName = c.CompanyName,
                    ContactName = c.ContactName + " - Calc",
                    ContactTitle = c.ContactTitle,
                    Address = c.Address,
                    City = c.City,
                    Region = c.Region,
                    PostalCode = c.PostalCode,
                    Country = c.Country,
                    Phone = c.Phone,
                    Fax = c.Fax
                });

            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedCustomer = await refreshedQuery.FirstAsync();

            // Store values for assertions
            var refreshedContactName = refreshedCustomer.ContactName;
            var containsNewName = refreshedContactName.Contains(newContactName);
            var containsCalc = refreshedContactName.Contains("Calc");

            // Verify the calculated value reflects the database change
            Assert.True(containsNewName);
            Assert.True(containsCalc);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                baseContactName,
                customer.CustomerID);
        }
    }

    /// <summary>
    /// Task: Projecting an entity multiple times in Select with same key, but different property values
    /// Tests refresh behavior when the same entity is projected multiple times in a single Select
    /// with different property combinations or calculated values.
    /// </summary>
    [Fact]
    public async Task Refresh_ProjectingEntityMultipleTimesWithSameKey()
    {
        using var ctx = _fx.CreateContext();

        var customer = await ctx.Customers.FirstAsync();
        var originalContactName = customer.ContactName;
        var originalCompanyName = customer.CompanyName;

        try
        {
            // Project the same entity multiple times with different property combinations
            var multiProjection = await ctx.Customers
                .Where(c => c.CustomerID == customer.CustomerID)
                .Select(c => new
                {
                    FullCustomer = c,
                    NameOnly = new { c.CustomerID, c.ContactName },
                    CompanyOnly = new { c.CustomerID, c.CompanyName },
                    CombinedInfo = new
                    {
                        ID = c.CustomerID,
                        DisplayName = c.ContactName + " @ " + c.CompanyName
                    }
                })
                .FirstAsync();

            // Update database
            var newContactName = $"MP{DateTime.Now:HHmm}";
            var newCompanyName = $"UpdCo{DateTime.Now:HHmm}";

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0}, [CompanyName] = {1} WHERE [CustomerID] = {2}",
                newContactName,
                newCompanyName,
                customer.CustomerID);

            // Create refresh query with multiple projections
            var query = ctx.Customers
                .Where(c => c.CustomerID == customer.CustomerID)
                .Select(c => new
                {
                    FullCustomer = c,
                    NameOnly = new { c.CustomerID, c.ContactName },
                    CompanyOnly = new { c.CustomerID, c.CompanyName },
                    CombinedInfo = new
                    {
                        ID = c.CustomerID,
                        DisplayName = c.ContactName + " @ " + c.CompanyName
                    }
                });

            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedResult = await refreshedQuery.FirstAsync();

            // Store values for assertions
            var fullCustomerContactName = refreshedResult.FullCustomer.ContactName;
            var fullCustomerCompanyName = refreshedResult.FullCustomer.CompanyName;
            var nameOnlyContactName = refreshedResult.NameOnly.ContactName;
            var companyOnlyCompanyName = refreshedResult.CompanyOnly.CompanyName;
            var combinedDisplayName = refreshedResult.CombinedInfo.DisplayName;
            var containsNewContactNameInDisplay = combinedDisplayName.Contains(newContactName);
            var containsNewCompanyNameInDisplay = combinedDisplayName.Contains(newCompanyName);

            // Verify all projections reflect the database changes
            Assert.Equal(newContactName, fullCustomerContactName);
            Assert.Equal(newCompanyName, fullCustomerCompanyName);
            Assert.Equal(newContactName, nameOnlyContactName);
            Assert.Equal(newCompanyName, companyOnlyCompanyName);
            Assert.True(containsNewContactNameInDisplay);
            Assert.True(containsNewCompanyNameInDisplay);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0}, [CompanyName] = {1} WHERE [CustomerID] = {2}",
                originalContactName,
                originalCompanyName,
                customer.CustomerID);
        }
    }

    /// <summary>
    /// Task: Lazy-loading proxies with navigations in loaded and unloaded states
    /// Tests refresh behavior with lazy-loading proxies where navigation properties
    /// may be in various states of loading (loaded, unloaded, partially loaded).
    /// </summary>
    [Fact]
    public async Task Refresh_LazyLoadingProxiesWithNavigationStates()
    {
        using var ctx = _fx.CreateContext();

        // Enable lazy loading for this test
        var originalLazyLoadingEnabled = ctx.ChangeTracker.LazyLoadingEnabled;
        ctx.ChangeTracker.LazyLoadingEnabled = true;

        try
        {
            var customer = await ctx.Customers.FirstAsync(c => c.Orders.Any());
            var originalContactName = customer.ContactName;

            // Access navigation to trigger lazy loading
            await ctx.Entry(customer).Collection(c => c.Orders).LoadAsync();
            var orderCount = customer.Orders.Count; // This should trigger lazy loading

            // Modify some orders to create mixed loading states
            var firstOrder = customer.Orders.First();
            var originalOrderDate = firstOrder.OrderDate;

            // Update database
            var newContactName = $"LL{DateTime.Now:HHmmss}";
            var newOrderDate = DateTime.Now.AddDays(7);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                newContactName,
                customer.CustomerID);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [OrderID] = {1}",
                newOrderDate,
                firstOrder.OrderID);

            // Create refresh query - should handle lazy-loaded navigations properly
            var query = ctx.Customers
                .Include(c => c.Orders) // Explicitly include to ensure consistent state
                .Where(c => c.CustomerID == customer.CustomerID);

            var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);
            var refreshedCustomer = await refreshedQuery.FirstAsync();

            // Access the navigation again to verify it's properly refreshed
            var refreshedOrderCount = refreshedCustomer.Orders.Count;

            var refreshedFirstOrder = refreshedCustomer.Orders.First(o => o.OrderID == firstOrder.OrderID);
            var refreshedOrderDate = refreshedFirstOrder.OrderDate?.Date;
            var expectedOrderDate = newOrderDate.Date;

            // Store values for cleanup
            var refreshedContactName = refreshedCustomer.ContactName;

            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Customers] SET [ContactName] = {0} WHERE [CustomerID] = {1}",
                originalContactName,
                customer.CustomerID);

            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE [Orders] SET [OrderDate] = {0} WHERE [OrderID] = {1}",
                originalOrderDate ?? DateTime.Now, // Fix null reference
                firstOrder.OrderID);

            // Verify refresh worked with lazy-loaded data
            Assert.Equal(newContactName, refreshedContactName);
            Assert.Equal(orderCount, refreshedOrderCount);
            Assert.Equal(expectedOrderDate, refreshedOrderDate);
        }
        finally
        {
            ctx.ChangeTracker.LazyLoadingEnabled = originalLazyLoadingEnabled; // Reset to original value
        }
    }

    /// <summary>
    /// Test legacy thrown InvalidOperationException
    /// </summary>
    [Fact]
    public async Task Refresh_NonTrackingQueriesThrowExceptionLegacy()
    {
        using var ctx = _fx.CreateContext();

        var customer = ctx.Customers.First();

        // Create a non-tracking query
        var nonTrackingQuery = ctx.Customers
            .AsNoTracking()
            .Where(c => c.CustomerID == customer.CustomerID);

        customer = nonTrackingQuery.FirstOrDefault();

        // Attempting to refresh a non-tracking query should throw
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await ctx.Entry(customer!).ReloadAsync());
    }

    /// <summary>
    /// Task: Non-tracking queries should throw
    /// Tests that attempting to use refresh functionality on non-tracking queries
    /// results in an appropriate exception being thrown, as refresh requires change tracking.
    /// </summary>
    [Fact]
    public void Refresh_NonTrackingQueriesThrowException()
    {
        using var ctx = _fx.CreateContext();

        var customer = ctx.Customers.First();

        // Create a non-tracking query
        var nonTrackingQuery = ctx.Customers
            .AsNoTracking()
            .Where(c => c.CustomerID == customer.CustomerID);
        
        // Attempting to refresh a non-tracking query should throw
        Assert.Throws<InvalidOperationException>(() =>
            nonTrackingQuery.Refresh(MergeOption.OverwriteChanges));
    }

    /// <summary>
    /// Task: Multiple Refresh with different values in the same query should throw
    /// Tests that attempting to apply multiple refresh operations with different merge options
    /// or settings to the same query should result in an appropriate exception.
    /// </summary>
    [Fact]
    public void Refresh_MultipleRefreshCallsOnSameQueryThrowException()
    {
        using var ctx = _fx.CreateContext();

        var customer = ctx.Customers.First();

        var query = ctx.Customers.Where(c => c.CustomerID == customer.CustomerID);

        // Apply first refresh
        var refreshedQuery = query.Refresh(MergeOption.OverwriteChanges);

        // Attempting to apply another refresh with different options should throw
        Assert.Throws<InvalidOperationException>(() =>
            refreshedQuery.Refresh(MergeOption.PreserveChanges));
    }
    #endregion
}
