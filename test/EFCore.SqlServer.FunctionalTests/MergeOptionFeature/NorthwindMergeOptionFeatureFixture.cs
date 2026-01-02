// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class NorthwindMergeOptionFeatureFixture : SharedStoreFixtureBase<NorthwindMergeOptionFeatureContext>
{
    protected override string StoreName
        => "NorthwindMergeOptionFeature";

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).EnableSensitiveDataLogging();

    protected override async Task SeedAsync(NorthwindMergeOptionFeatureContext context)
    {
        // Seed ComplexTypes data
        var complexProduct = new NorthwindMergeOptionFeatureContext.ComplexProductEntity
        {
            Details = new NorthwindMergeOptionFeatureContext.ComplexProductDetailsEntity { Name = "Test Product", Price = 99.99m },
            Reviews = []
        };

        var complexCustomer = new NorthwindMergeOptionFeatureContext.ComplexCustomerEntity
        {
            Name = "Test Customer",
            Contact = new NorthwindMergeOptionFeatureContext.ComplexContactInfoEntity { Email = "test@example.com", Phone = "555-0100" },
            Addresses =
            [
                new NorthwindMergeOptionFeatureContext.ComplexAddressEntity { Street = "123 Main St", City = "Anytown", PostalCode = "12345" }
            ]
        };

        context.ComplexProducts.Add(complexProduct);
        context.ComplexCustomers.Add(complexCustomer);

        // Seed ComputedColumns data
        var computedProduct1 = new NorthwindMergeOptionFeatureContext.ComputedProductEntity
        {
            Name = "Test Product",
            Price = 100.00m,
            Quantity = 10
        };

        var computedProduct2 = new NorthwindMergeOptionFeatureContext.ComputedProductEntity
        {
            Name = "Expensive Product",
            Price = 250.00m,
            Quantity = 3
        };

        var computedOrder1 = new NorthwindMergeOptionFeatureContext.ComputedOrderEntity
        {
            OrderDate = DateTime.Now.AddDays(-10),
            CustomerName = "Test Customer 1"
        };

        var computedOrder2 = new NorthwindMergeOptionFeatureContext.ComputedOrderEntity
        {
            OrderDate = DateTime.Now.AddDays(-5),
            CustomerName = "Test Customer 2"
        };

        context.ComputedProducts.AddRange(computedProduct1, computedProduct2);
        context.ComputedOrders.AddRange(computedOrder1, computedOrder2);

        // Seed GlobalFilters data
        var globalProduct1 = new NorthwindMergeOptionFeatureContext.GlobalProductEntity
        {
            Name = "Tenant 1 Product A",
            Price = 100.00m,
            TenantId = 1
        };

        var globalProduct2 = new NorthwindMergeOptionFeatureContext.GlobalProductEntity
        {
            Name = "Tenant 1 Product B",
            Price = 150.00m,
            TenantId = 1
        };

        var globalProduct3 = new NorthwindMergeOptionFeatureContext.GlobalProductEntity
        {
            Name = "Tenant 2 Product A",
            Price = 200.00m,
            TenantId = 2
        };

        var globalOrder1 = new NorthwindMergeOptionFeatureContext.GlobalOrderEntity
        {
            OrderDate = DateTime.Now.AddDays(-10),
            CustomerName = "Customer 1",
            TenantId = 1,
            IsDeleted = false
        };

        var globalOrder2 = new NorthwindMergeOptionFeatureContext.GlobalOrderEntity
        {
            OrderDate = DateTime.Now.AddDays(-5),
            CustomerName = "Customer 2",
            TenantId = 2,
            IsDeleted = false
        };

        context.GlobalProducts.AddRange(globalProduct1, globalProduct2, globalProduct3);
        context.GlobalOrders.AddRange(globalOrder1, globalOrder2);

        // Seed ManyToMany data
        var student1 = new NorthwindMergeOptionFeatureContext.StudentEntity { Name = "John Doe", Email = "john@example.com" };
        var student2 = new NorthwindMergeOptionFeatureContext.StudentEntity { Name = "Jane Smith", Email = "jane@example.com" };
        var student3 = new NorthwindMergeOptionFeatureContext.StudentEntity { Name = "Bob Johnson", Email = "bob@example.com" };

        var course1 = new NorthwindMergeOptionFeatureContext.CourseEntity { Title = "Mathematics", Credits = 3 };
        var course2 = new NorthwindMergeOptionFeatureContext.CourseEntity { Title = "Physics", Credits = 4 };
        var course3 = new NorthwindMergeOptionFeatureContext.CourseEntity { Title = "Chemistry", Credits = 3 };
        var course4 = new NorthwindMergeOptionFeatureContext.CourseEntity { Title = "Biology", Credits = 3 };

        var author1 = new NorthwindMergeOptionFeatureContext.AuthorEntity { Name = "Stephen King" };
        var author2 = new NorthwindMergeOptionFeatureContext.AuthorEntity { Name = "J.K. Rowling" };

        var book1 = new NorthwindMergeOptionFeatureContext.BookEntity { Title = "The Shining", Genre = "Horror" };
        var book2 = new NorthwindMergeOptionFeatureContext.BookEntity { Title = "IT", Genre = "Horror" };
        var book3 = new NorthwindMergeOptionFeatureContext.BookEntity { Title = "Harry Potter", Genre = "Fantasy" };

        context.Students.AddRange(student1, student2, student3);
        context.Courses.AddRange(course1, course2, course3, course4);
        context.Authors.AddRange(author1, author2);
        context.Books.AddRange(book1, book2, book3);

        await context.SaveChangesAsync();

        student1.Courses = [course1, course2];
        student2.Courses = [course2, course3];
        student3.Courses = [course1, course3, course4];

        author1.Books = [book1, book2];
        author2.Books = [book3];

        await context.SaveChangesAsync();

        // Seed PrimitiveCollections data
        var primitiveProduct1 = new NorthwindMergeOptionFeatureContext.PrimitiveProductEntity
        {
            Name = "Laptop",
            Tags = ["Electronics", "Computer", "Portable"]
        };

        var primitiveProduct2 = new NorthwindMergeOptionFeatureContext.PrimitiveProductEntity
        {
            Name = "Smartphone",
            Tags = ["Electronics", "Mobile", "Communication"]
        };

        var primitiveBlog1 = new NorthwindMergeOptionFeatureContext.PrimitiveBlogEntity
        {
            Title = "Tech Blog",
            Ratings = [5, 4, 5, 3, 4]
        };

        var primitiveBlog2 = new NorthwindMergeOptionFeatureContext.PrimitiveBlogEntity
        {
            Title = "Cooking Blog",
            Ratings = [4, 5, 4, 4, 5]
        };

        var primitiveUser1 = new NorthwindMergeOptionFeatureContext.PrimitiveUserEntity
        {
            Name = "John Doe",
            RelatedIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]
        };

        var primitiveUser2 = new NorthwindMergeOptionFeatureContext.PrimitiveUserEntity
        {
            Name = "Jane Smith",
            RelatedIds = [Guid.NewGuid(), Guid.NewGuid()]
        };

        context.PrimitiveProducts.AddRange(primitiveProduct1, primitiveProduct2);
        context.PrimitiveBlogs.AddRange(primitiveBlog1, primitiveBlog2);
        context.PrimitiveUsers.AddRange(primitiveUser1, primitiveUser2);

        // Seed ShadowProperties data
        var shadowCustomer1 = new NorthwindMergeOptionFeatureContext.ShadowCustomerEntity { Name = "Customer 1", Email = "customer1@example.com" };
        var shadowCustomer2 = new NorthwindMergeOptionFeatureContext.ShadowCustomerEntity { Name = "Customer 2", Email = "customer2@example.com" };

        context.ShadowCustomers.AddRange(shadowCustomer1, shadowCustomer2);
        await context.SaveChangesAsync();

        var shadowProduct1 = new NorthwindMergeOptionFeatureContext.ShadowProductEntity { Name = "Product 1", Price = 100.00m };
        var shadowProduct2 = new NorthwindMergeOptionFeatureContext.ShadowProductEntity { Name = "Product 2", Price = 200.00m };

        context.ShadowProducts.AddRange(shadowProduct1, shadowProduct2);

        context.Entry(shadowProduct1).Property("CreatedBy").CurrentValue = "System";
        context.Entry(shadowProduct1).Property("CreatedAt").CurrentValue = DateTime.Now.AddMonths(-2);
        context.Entry(shadowProduct1).Property("LastModified").CurrentValue = DateTime.Now.AddDays(-1);

        context.Entry(shadowProduct2).Property("CreatedBy").CurrentValue = "Admin";
        context.Entry(shadowProduct2).Property("CreatedAt").CurrentValue = DateTime.Now.AddDays(-15);
        context.Entry(shadowProduct2).Property("LastModified").CurrentValue = DateTime.Now.AddHours(-6);

        await context.SaveChangesAsync();

        var shadowOrder1 = new NorthwindMergeOptionFeatureContext.ShadowOrderEntity { OrderDate = DateTime.Now.AddDays(-10), TotalAmount = 150.00m };
        var shadowOrder2 = new NorthwindMergeOptionFeatureContext.ShadowOrderEntity { OrderDate = DateTime.Now.AddDays(-5), TotalAmount = 300.00m };

        context.ShadowOrders.AddRange(shadowOrder1, shadowOrder2);

        context.Entry(shadowOrder1).Property("CustomerId").CurrentValue = shadowCustomer1.Id;
        context.Entry(shadowOrder2).Property("CustomerId").CurrentValue = shadowCustomer2.Id;

        await context.SaveChangesAsync();

        // Seed TableSharing data
        var tablePerson = new NorthwindMergeOptionFeatureContext.TablePersonEntity
        {
            Name = "John Doe",
            DateOfBirth = new DateTime(1980, 1, 1)
        };

        context.TablePeople.Add(tablePerson);
        await context.SaveChangesAsync();

        var tableEmployee = new NorthwindMergeOptionFeatureContext.TableEmployeeEntity
        {
            Id = tablePerson.Id,
            Name = "John Doe",
            DateOfBirth = new DateTime(1980, 1, 1),
            Department = "Engineering",
            Salary = 75000
        };

        var tableBlog = new NorthwindMergeOptionFeatureContext.TableBlogEntity
        {
            Title = "Tech Blog",
            Content = "This is a technology blog."
        };

        context.TableBlogs.Add(tableBlog);
        await context.SaveChangesAsync();

        var tableBlogMetadata = new NorthwindMergeOptionFeatureContext.TableBlogMetadataEntity
        {
            BlogId = tableBlog.Id,
            MetaDescription = "A blog about technology",
            Keywords = "tech, programming, software"
        };

        var tableVehicle = new NorthwindMergeOptionFeatureContext.TableVehicleEntity
        {
            Make = "Generic",
            Model = "Vehicle"
        };

        var tableCar = new NorthwindMergeOptionFeatureContext.TableCarEntity
        {
            Make = "Toyota",
            Model = "Camry",
            NumberOfDoors = 4
        };

        context.TableEmployees.Add(tableEmployee);
        context.TableBlogMetadata.Add(tableBlogMetadata);
        context.TableVehicles.Add(tableVehicle);
        context.TableCars.Add(tableCar);

        // Seed ValueConverters data
        var converterProduct = new NorthwindMergeOptionFeatureContext.ConverterProductEntity
        {
            Name = "Laptop",
            Status = NorthwindMergeOptionFeatureContext.ConverterProductStatus.Active,
            Tags = ["electronics", "computer"]
        };

        var converterUser = new NorthwindMergeOptionFeatureContext.ConverterUserEntity
        {
            Name = "John Doe",
            BirthDate = new DateTime(1985, 3, 10),
            ExternalId = Guid.NewGuid()
        };

        var converterOrder = new NorthwindMergeOptionFeatureContext.ConverterOrderEntity
        {
            OrderNumber = "ORD001",
            Price = new NorthwindMergeOptionFeatureContext.ConverterMoney(199.99m)
        };

        context.ConverterProducts.Add(converterProduct);
        context.ConverterUsers.Add(converterUser);
        context.ConverterOrders.Add(converterOrder);

        await context.SaveChangesAsync();
    }
}
