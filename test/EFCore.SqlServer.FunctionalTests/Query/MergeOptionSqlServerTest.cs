// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

public class MergeOptionSqlServerTest(MergeOptionSqlServerTest.MergeOptionSqlServerFixture fixture)
    : MergeOptionTestBase<MergeOptionSqlServerTest.MergeOptionSqlServerFixture>(fixture)
{
    protected override void UseTransaction(DbContext context, Action<DbContext> testAction)
    {
        using var transaction = context.Database.BeginTransaction();
        testAction(context);
        transaction.Rollback();
    }

    protected override async Task UseTransactionAsync(DbContext context, Func<DbContext, Task> testAction)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        await testAction(context);
        await transaction.RollbackAsync();
    }

    protected override void UpdateProductNameInDatabase(DbContext context, int id, string newName)
        => context.Database.ExecuteSql($"UPDATE [Product] SET Name = {newName} WHERE Id = {id}");

    protected override Task UpdateProductNameInDatabaseAsync(DbContext context, int id, string newName)
        => context.Database.ExecuteSqlAsync($"UPDATE [Product] SET Name = {newName} WHERE Id = {id}");

    protected override void UpdateProductPriceInDatabase(DbContext context, int id, decimal newPrice)
        => context.Database.ExecuteSql($"UPDATE [Product] SET Price = {newPrice} WHERE Id = {id}");

    protected override void UpdateOrderCustomerNameInDatabase(DbContext context, int id, string newName)
        => context.Database.ExecuteSql($"UPDATE [Order] SET CustomerName = {newName} WHERE Id = {id}");

    protected override void UpdateProductInDatabase(DbContext context, int id, decimal newPrice, int newQuantity)
        => context.Database.ExecuteSql($"UPDATE [Product] SET Price = {newPrice}, Quantity = {newQuantity} WHERE Id = {id}");

    protected override void AddStudentCourseInDatabase(DbContext context, int studentId, int courseId)
        => context.Database.ExecuteSql($"INSERT INTO [CourseStudent] (CoursesId, StudentsId) VALUES ({courseId}, {studentId})");

    protected override Task AddStudentCourseInDatabaseAsync(DbContext context, int studentId, int courseId)
        => context.Database.ExecuteSqlAsync($"INSERT INTO [CourseStudent] (CoursesId, StudentsId) VALUES ({courseId}, {studentId})");

    protected override void UpdateBookPublisherInDatabase(DbContext context, int bookId, string newPublisher)
        => context.Database.ExecuteSql($"UPDATE [Book] SET Publisher = {newPublisher} WHERE Id = {bookId}");

    protected override void UpdateProductTagsInDatabase(DbContext context, int productId, List<string> newTags)
    {
        var tagsJson = System.Text.Json.JsonSerializer.Serialize(newTags);
        context.Database.ExecuteSql($"UPDATE [Product] SET Tags = {tagsJson} WHERE Id = {productId}");
    }

    protected override void UpdateProductStatusInDatabase(DbContext context, int productId, ProductStatus newStatus)
        => context.Database.ExecuteSql($"UPDATE [Product] SET Status = {(int)newStatus} WHERE Id = {productId}");

    protected override void UpdateStudentNameInDatabase(DbContext context, int studentId, string newName)
        => context.Database.ExecuteSql($"UPDATE [Student] SET Name = {newName} WHERE Id = {studentId}");

    protected override void UpdateOrderShippingCityInDatabase(DbContext context, int orderId, string newCity)
        => context.Database.ExecuteSql($"UPDATE [Order] SET ShippingAddress_City = {newCity} WHERE Id = {orderId}");

    protected override void UpdatePremiumProductRewardPointsInDatabase(DbContext context, int productId, int newRewardPoints)
        => context.Database.ExecuteSql($"UPDATE [Product] SET RewardPoints = {newRewardPoints} WHERE Id = {productId}");

    public class MergeOptionSqlServerFixture : MergeOptionFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .UseSqlServer(b => b.ExecutionStrategy(c => new SqlServerExecutionStrategy(c)))
                .ConfigureWarnings(w =>
                {
                    w.Ignore(CoreEventId.FirstWithoutOrderByAndFilterWarning);
                    w.Ignore(SqlServerEventId.DecimalTypeDefaultWarning);
                });

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<PremiumProduct>();
        }
    }
}
