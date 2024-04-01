// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class AdHocNavigationsQueryTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "AdHocNavigationsQueryTests";

    #region 3409

    [ConditionalFact]
    public virtual async Task ThenInclude_with_interface_navigations()
    {
        var contextFactory = await InitializeAsync<Context3409>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var results = context.Parents
                .Include(p => p.ChildCollection)
                .ThenInclude(c => c.SelfReferenceCollection)
                .ToList();

            Assert.Single(results);
            Assert.Equal(1, results[0].ChildCollection.Count);
            Assert.Equal(2, results[0].ChildCollection.Single().SelfReferenceCollection.Count);
        }

        using (var context = contextFactory.CreateContext())
        {
            var results = context.Children
                .Select(
                    c => new { c.SelfReferenceBackNavigation, c.SelfReferenceBackNavigation.ParentBackNavigation })
                .ToList();

            Assert.Equal(3, results.Count);
            Assert.Equal(2, results.Count(c => c.SelfReferenceBackNavigation != null));
            Assert.Equal(2, results.Count(c => c.ParentBackNavigation != null));
        }

        using (var context = contextFactory.CreateContext())
        {
            var results = context.Children
                .Select(
                    c => new
                    {
                        SelfReferenceBackNavigation
                            = EF.Property<Context3409.IChild>(c, "SelfReferenceBackNavigation"),
                        ParentBackNavigationB
                            = EF.Property<Context3409.IParent>(
                                EF.Property<Context3409.IChild>(c, "SelfReferenceBackNavigation"),
                                "ParentBackNavigation")
                    })
                .ToList();

            Assert.Equal(3, results.Count);
            Assert.Equal(2, results.Count(c => c.SelfReferenceBackNavigation != null));
            Assert.Equal(2, results.Count(c => c.ParentBackNavigationB != null));
        }

        using (var context = contextFactory.CreateContext())
        {
            var results = context.Children
                .Include(c => c.SelfReferenceBackNavigation)
                .ThenInclude(c => c.ParentBackNavigation)
                .ToList();

            Assert.Equal(3, results.Count);
            Assert.Equal(2, results.Count(c => c.SelfReferenceBackNavigation != null));
            Assert.Equal(1, results.Count(c => c.ParentBackNavigation != null));
        }
    }

    private class Context3409(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Child> Children { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>()
                .HasMany(p => (ICollection<Child>)p.ChildCollection)
                .WithOne(c => (Parent)c.ParentBackNavigation);

            modelBuilder.Entity<Child>()
                .HasMany(c => (ICollection<Child>)c.SelfReferenceCollection)
                .WithOne(c => (Child)c.SelfReferenceBackNavigation);
        }

        public Task SeedAsync()
        {
            var parent1 = new Parent();

            var child1 = new Child();
            var child2 = new Child();
            var child3 = new Child();

            parent1.ChildCollection = new List<IChild> { child1 };
            child1.SelfReferenceCollection = new List<IChild> { child2, child3 };

            Parents.AddRange(parent1);
            Children.AddRange(child1, child2, child3);

            return SaveChangesAsync();
        }

        public interface IParent
        {
            int Id { get; set; }

            ICollection<IChild> ChildCollection { get; set; }
        }

        public interface IChild
        {
            int Id { get; set; }

            int? ParentBackNavigationId { get; set; }
            IParent ParentBackNavigation { get; set; }

            ICollection<IChild> SelfReferenceCollection { get; set; }
            int? SelfReferenceBackNavigationId { get; set; }
            IChild SelfReferenceBackNavigation { get; set; }
        }

        public class Parent : IParent
        {
            public int Id { get; set; }

            public ICollection<IChild> ChildCollection { get; set; }
        }

        public class Child : IChild
        {
            public int Id { get; set; }

            public int? ParentBackNavigationId { get; set; }
            public IParent ParentBackNavigation { get; set; }

            public ICollection<IChild> SelfReferenceCollection { get; set; }
            public int? SelfReferenceBackNavigationId { get; set; }
            public IChild SelfReferenceBackNavigation { get; set; }
        }
    }

    #endregion

    #region 3758

    [ConditionalFact]
    public virtual async Task Customer_collections_materialize_properly()
    {
        var contextFactory = await InitializeAsync<Context3758>(seed: c => c.SeedAsync());

        using var ctx = contextFactory.CreateContext();

        var query1 = ctx.Customers.Select(c => c.Orders1);
        var result1 = query1.ToList();

        Assert.Equal(2, result1.Count);
        Assert.IsType<HashSet<Context3758.Order>>(result1[0]);
        Assert.Equal(2, result1[0].Count);
        Assert.Equal(2, result1[1].Count);

        var query2 = ctx.Customers.Select(c => c.Orders2);
        var result2 = query2.ToList();

        Assert.Equal(2, result2.Count);
        Assert.IsType<Context3758.MyGenericCollection<Context3758.Order>>(result2[0]);
        Assert.Equal(2, result2[0].Count);
        Assert.Equal(2, result2[1].Count);

        var query3 = ctx.Customers.Select(c => c.Orders3);
        var result3 = query3.ToList();

        Assert.Equal(2, result3.Count);
        Assert.IsType<Context3758.MyNonGenericCollection>(result3[0]);
        Assert.Equal(2, result3[0].Count);
        Assert.Equal(2, result3[1].Count);

        var query4 = ctx.Customers.Select(c => c.Orders4);

        Assert.Equal(
            CoreStrings.NavigationCannotCreateType(
                "Orders4", typeof(Context3758.Customer).Name,
                typeof(Context3758.MyInvalidCollection<Context3758.Order>).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => query4.ToList()).Message);
    }

    protected class Context3758(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(
                b =>
                {
                    b.HasMany(e => e.Orders1).WithOne().HasForeignKey("CustomerId1");
                    b.HasMany(e => e.Orders2).WithOne().HasForeignKey("CustomerId2");
                    b.HasMany(e => e.Orders3).WithOne().HasForeignKey("CustomerId3");
                    b.HasMany(e => e.Orders4).WithOne().HasForeignKey("CustomerId4");
                });
        }

        public Task SeedAsync()
        {
            var o111 = new Order { Name = "O111" };
            var o112 = new Order { Name = "O112" };
            var o121 = new Order { Name = "O121" };
            var o122 = new Order { Name = "O122" };
            var o131 = new Order { Name = "O131" };
            var o132 = new Order { Name = "O132" };
            var o141 = new Order { Name = "O141" };

            var o211 = new Order { Name = "O211" };
            var o212 = new Order { Name = "O212" };
            var o221 = new Order { Name = "O221" };
            var o222 = new Order { Name = "O222" };
            var o231 = new Order { Name = "O231" };
            var o232 = new Order { Name = "O232" };
            var o241 = new Order { Name = "O241" };

            var c1 = new Customer
            {
                Name = "C1",
                Orders1 = new List<Order> { o111, o112 },
                Orders2 = [],
                Orders3 = [],
                Orders4 = new MyInvalidCollection<Order>(42)
            };

            c1.Orders2.AddRange(new[] { o121, o122 });
            c1.Orders3.AddRange(new[] { o131, o132 });
            c1.Orders4.Add(o141);

            var c2 = new Customer
            {
                Name = "C2",
                Orders1 = new List<Order> { o211, o212 },
                Orders2 = [],
                Orders3 = [],
                Orders4 = new MyInvalidCollection<Order>(42)
            };

            c2.Orders2.AddRange(new[] { o221, o222 });
            c2.Orders3.AddRange(new[] { o231, o232 });
            c2.Orders4.Add(o241);

            Customers.AddRange(c1, c2);
            Orders.AddRange(
                o111, o112, o121, o122,
                o131, o132, o141, o211,
                o212, o221, o222, o231,
                o232, o241);

            return SaveChangesAsync();
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ICollection<Order> Orders1 { get; set; }
            public MyGenericCollection<Order> Orders2 { get; set; }
            public MyNonGenericCollection Orders3 { get; set; }
            public MyInvalidCollection<Order> Orders4 { get; set; }
        }

        public class Order
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class MyGenericCollection<TElement> : List<TElement>;

        public class MyNonGenericCollection : List<Order>;

        public class MyInvalidCollection<TElement> : List<TElement>
        {
            public MyInvalidCollection(int argument)
            {
                var _ = argument;
            }
        }
    }

    #endregion

    #region 7312

    [ConditionalFact]
    public virtual async Task Reference_include_on_derived_type_with_sibling_works()
    {
        var contextFactory = await InitializeAsync<Context7312>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Proposals.OfType<Context7312.ProposalLeave>().Include(l => l.LeaveType).ToList();

            Assert.Single(query);
        }
    }

    private class Context7312(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<ProposalCustom> ProposalCustoms { get; set; }
        public DbSet<ProposalLeave> ProposalLeaves { get; set; }

        public Task SeedAsync()
        {
            AddRange(
                new Proposal(),
                new ProposalCustom { Name = "CustomProposal" },
                new ProposalLeave { LeaveStart = DateTime.Now, LeaveType = new ProposalLeaveType() }
            );
            return SaveChangesAsync();
        }

        public class Proposal
        {
            public int Id { get; set; }
        }

        public class ProposalCustom : Proposal
        {
            public string Name { get; set; }
        }

        public class ProposalLeave : Proposal
        {
            public DateTime LeaveStart { get; set; }
            public virtual ProposalLeaveType LeaveType { get; set; }
        }

        public class ProposalLeaveType
        {
            public int Id { get; set; }
            public ICollection<ProposalLeave> ProposalLeaves { get; set; }
        }
    }

    #endregion

    #region 9038

    [ConditionalFact]
    public virtual async Task Include_collection_optional_reference_collection()
    {
        var contextFactory = await InitializeAsync<Context9038>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var result = await context.People.OfType<Context9038.PersonTeacher9038>()
                .Include(m => m.Students)
                .ThenInclude(m => m.Family)
                .ThenInclude(m => m.Members)
                .ToListAsync();

            Assert.Equal(2, result.Count);
            Assert.True(result.All(r => r.Students.Count > 0));
        }

        using (var context = contextFactory.CreateContext())
        {
            var result = await context.Set<Context9038.PersonTeacher9038>()
                .Include(m => m.Family.Members)
                .Include(m => m.Students)
                .ToListAsync();

            Assert.Equal(2, result.Count);
            Assert.True(result.All(r => r.Students.Count > 0));
            Assert.Null(result.Single(t => t.Name == "Ms. Frizzle").Family);
            Assert.NotNull(result.Single(t => t.Name == "Mr. Garrison").Family);
        }
    }

    private class Context9038(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Person9038> People { get; set; }

        public DbSet<PersonFamily9038> Families { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonTeacher9038>().HasBaseType<Person9038>();
            modelBuilder.Entity<PersonKid9038>().HasBaseType<Person9038>();
            modelBuilder.Entity<PersonFamily9038>();

            modelBuilder.Entity<PersonKid9038>(
                entity =>
                {
                    entity.Property("Discriminator").HasMaxLength(63);
                    entity.HasIndex("Discriminator");
                    entity.HasOne(m => m.Teacher)
                        .WithMany(m => m.Students)
                        .HasForeignKey(m => m.TeacherId)
                        .HasPrincipalKey(m => m.Id)
                        .OnDelete(DeleteBehavior.Restrict);
                });
        }

        public Task SeedAsync()
        {
            var famalies = new List<PersonFamily9038> { new() { LastName = "Garrison" }, new() { LastName = "Cartman" } };
            var teachers = new List<PersonTeacher9038>
            {
                new() { Name = "Ms. Frizzle" }, new() { Name = "Mr. Garrison", Family = famalies[0] }
            };
            var students = new List<PersonKid9038>
            {
                new()
                {
                    Name = "Arnold",
                    Grade = 2,
                    Teacher = teachers[0]
                },
                new()
                {
                    Name = "Eric",
                    Grade = 4,
                    Teacher = teachers[1],
                    Family = famalies[1]
                }
            };

            People.AddRange(teachers);
            People.AddRange(students);
            return SaveChangesAsync();
        }

        public abstract class Person9038
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public int? TeacherId { get; set; }

            public PersonFamily9038 Family { get; set; }
        }

        public class PersonKid9038 : Person9038
        {
            public int Grade { get; set; }

            public PersonTeacher9038 Teacher { get; set; }
        }

        public class PersonTeacher9038 : Person9038
        {
            public ICollection<PersonKid9038> Students { get; set; }
        }

        public class PersonFamily9038
        {
            public int Id { get; set; }

            public string LastName { get; set; }

            public ICollection<Person9038> Members { get; set; }
        }
    }

    #endregion

    #region 10635

    [ConditionalFact]
    public virtual async Task Include_with_order_by_on_interface_key()
    {
        var contextFactory = await InitializeAsync<Context10635>(seed: c => c.SeedAsync());
        using (var context = contextFactory.CreateContext())
        {
            var query = context.Parents.Include(p => p.Children).OrderBy(p => p.Id).ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Parents.OrderBy(p => p.Id).Select(p => p.Children.ToList()).ToList();
        }
    }

    private class Context10635(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Parent10635> Parents { get; set; }
        public DbSet<Child10635> Children { get; set; }

        public Task SeedAsync()
        {
            var c11 = new Child10635 { Name = "Child111" };
            var c12 = new Child10635 { Name = "Child112" };
            var c13 = new Child10635 { Name = "Child113" };
            var c21 = new Child10635 { Name = "Child121" };

            var p1 = new Parent10635 { Name = "Parent1", Children = new[] { c11, c12, c13 } };
            var p2 = new Parent10635 { Name = "Parent2", Children = new[] { c21 } };
            Parents.AddRange(p1, p2);
            Children.AddRange(c11, c12, c13, c21);
            return SaveChangesAsync();
        }

        public interface IEntity10635
        {
            int Id { get; set; }
        }

        public class Parent10635 : IEntity10635
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public virtual ICollection<Child10635> Children { get; set; }
        }

        public class Child10635 : IEntity10635
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParentId { get; set; }
        }
    }

    #endregion

    #region 11923

    [ConditionalFact]
    public virtual async Task Collection_without_setter_materialized_correctly()
    {
        var contextFactory = await InitializeAsync<Context11923>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query1 = context.Blogs
            .Select(
                b => new
                {
                    Collection1 = b.Posts1,
                    Collection2 = b.Posts2,
                    Collection3 = b.Posts3
                }).ToList();

        var query2 = context.Blogs
            .Select(
                b => new
                {
                    Collection1 = b.Posts1.OrderBy(p => p.Id).First().Comments.Count,
                    Collection2 = b.Posts2.OrderBy(p => p.Id).First().Comments.Count,
                    Collection3 = b.Posts3.OrderBy(p => p.Id).First().Comments.Count
                }).ToList();

        Assert.Throws<InvalidOperationException>(
            () => context.Blogs
                .Select(
                    b => new
                    {
                        Collection1 = b.Posts1.OrderBy(p => p.Id),
                        Collection2 = b.Posts2.OrderBy(p => p.Id),
                        Collection3 = b.Posts3.OrderBy(p => p.Id)
                    }).ToList());
    }

    private class Context11923(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(
                b =>
                {
                    b.HasMany(e => e.Posts1).WithOne().HasForeignKey("BlogId1");
                    b.HasMany(e => e.Posts2).WithOne().HasForeignKey("BlogId2");
                    b.HasMany(e => e.Posts3).WithOne().HasForeignKey("BlogId3");
                });

            modelBuilder.Entity<Post>();
        }

        public Task SeedAsync()
        {
            var p111 = new Post { Name = "P111" };
            var p112 = new Post { Name = "P112" };
            var p121 = new Post { Name = "P121" };
            var p122 = new Post { Name = "P122" };
            var p123 = new Post { Name = "P123" };
            var p131 = new Post { Name = "P131" };

            var p211 = new Post { Name = "P211" };
            var p212 = new Post { Name = "P212" };
            var p221 = new Post { Name = "P221" };
            var p222 = new Post { Name = "P222" };
            var p223 = new Post { Name = "P223" };
            var p231 = new Post { Name = "P231" };

            var b1 = new Blog { Name = "B1" };
            var b2 = new Blog { Name = "B2" };

            b1.Posts1.AddRange(new[] { p111, p112 });
            b1.Posts2.AddRange(new[] { p121, p122, p123 });
            b1.Posts3.Add(p131);

            b2.Posts1.AddRange(new[] { p211, p212 });
            b2.Posts2.AddRange(new[] { p221, p222, p223 });
            b2.Posts3.Add(p231);

            Blogs.AddRange(b1, b2);
            Posts.AddRange(p111, p112, p121, p122, p123, p131, p211, p212, p221, p222, p223, p231);
            return SaveChangesAsync();
        }

        public class Blog
        {
            public Blog()
            {
                Posts1 = [];
                Posts2 = [];
                Posts3 = [];
            }

            public Blog(List<Post> posts1, CustomCollection posts2, HashSet<Post> posts3)
            {
                Posts1 = posts1;
                Posts2 = posts2;
                Posts3 = posts3;
            }

            public int Id { get; set; }
            public string Name { get; set; }

            public List<Post> Posts1 { get; }
            public CustomCollection Posts2 { get; }
            public HashSet<Post> Posts3 { get; }
        }

        public class Post
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Comment> Comments { get; set; }
        }

        public class Comment
        {
            public int Id { get; set; }
        }

        public class CustomCollection : List<Post>;
    }

    #endregion

    #region 11944

    [ConditionalFact]
    public virtual async Task Include_collection_works_when_defined_on_intermediate_type()
    {
        var contextFactory = await InitializeAsync<Context11944>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Schools.Include(s => ((Context11944.ElementarySchool)s).Students);
            var result = query.ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result.OfType<Context11944.ElementarySchool>().Single().Students.Count);
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Schools.Select(s => ((Context11944.ElementarySchool)s).Students.Where(ss => true).ToList());
            var result = query.ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Count() == 2);
        }
    }

    protected class Context11944(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<ElementarySchool> ElementarySchools { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<ElementarySchool>().HasMany(s => s.Students).WithOne(s => s.School);

        public Task SeedAsync()
        {
            var student1 = new Student();
            var student2 = new Student();
            var school = new School();
            var elementarySchool = new ElementarySchool { Students = [student1, student2] };

            Students.AddRange(student1, student2);
            Schools.AddRange(school);
            ElementarySchools.Add(elementarySchool);

            return SaveChangesAsync();
        }

        public class Student
        {
            public int Id { get; set; }
            public ElementarySchool School { get; set; }
        }

        public class School
        {
            public int Id { get; set; }
        }

        public abstract class PrimarySchool : School
        {
            public List<Student> Students { get; set; }
        }

        public class ElementarySchool : PrimarySchool;
    }

    #endregion

    #region 12456

    [ConditionalFact]
    public virtual async Task Let_multiple_references_with_reference_to_outer()
    {
        var contextFactory = await InitializeAsync<Context12456>();
        using (var context = contextFactory.CreateContext())
        {
            var users = (from a in context.Activities
                         let cs = context.CompetitionSeasons.First(s => s.StartDate <= a.DateTime && a.DateTime < s.EndDate)
                         select new { cs.Id, Points = a.ActivityType.Points.Where(p => p.CompetitionSeason == cs) }).ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            var users = context.Activities
                .Select(
                    a => new
                    {
                        Activity = a,
                        CompetitionSeason = context.CompetitionSeasons
                            .First(s => s.StartDate <= a.DateTime && a.DateTime < s.EndDate)
                    })
                .Select(
                    a => new
                    {
                        a.Activity,
                        CompetitionSeasonId = a.CompetitionSeason.Id,
                        Points = a.Activity.Points
                            ?? a.Activity.ActivityType.Points
                                .Where(p => p.CompetitionSeason == a.CompetitionSeason)
                                .Select(p => p.Points).SingleOrDefault()
                    }).ToList();
        }
    }

    private class Context12456(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Activity> Activities { get; set; }
        public DbSet<CompetitionSeason> CompetitionSeasons { get; set; }

        public class CompetitionSeason
        {
            public int Id { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<ActivityTypePoints> ActivityTypePoints { get; set; }
        }

        public class Point
        {
            public int Id { get; set; }
            public CompetitionSeason CompetitionSeason { get; set; }
            public int? Points { get; set; }
        }

        public class ActivityType
        {
            public int Id { get; set; }
            public List<ActivityTypePoints> Points { get; set; }
        }

        public class ActivityTypePoints
        {
            public int Id { get; set; }
            public int ActivityTypeId { get; set; }
            public int CompetitionSeasonId { get; set; }
            public int Points { get; set; }

            public ActivityType ActivityType { get; set; }
            public CompetitionSeason CompetitionSeason { get; set; }
        }

        public class Activity
        {
            public int Id { get; set; }
            public int ActivityTypeId { get; set; }
            public DateTime DateTime { get; set; }
            public int? Points { get; set; }
            public ActivityType ActivityType { get; set; }
        }
    }

    #endregion

    #region 12582

    [ConditionalFact]
    public virtual async Task Include_collection_with_OfType_base()
    {
        var contextFactory = await InitializeAsync<Context12582>(seed: c => c.SeedAsync());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Employees
                .Include(i => i.Devices)
                .OfType<Context12582.IEmployee>()
                .ToList();

            Assert.Single(query);

            var employee = (Context12582.Employee)query[0];
            Assert.Equal(2, employee.Devices.Count);
        }

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Employees
                .Select(e => e.Devices.Where(d => d.Device != "foo").Cast<Context12582.IEmployeeDevice>())
                .ToList();

            Assert.Single(query);
            var result = query[0];
            Assert.Equal(2, result.Count());
        }
    }

    private class Context12582(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeDevice> Devices { get; set; }

        public Task SeedAsync()
        {
            var d1 = new EmployeeDevice { Device = "d1" };
            var d2 = new EmployeeDevice { Device = "d2" };
            var e = new Employee { Devices = new List<EmployeeDevice> { d1, d2 }, Name = "e" };

            Devices.AddRange(d1, d2);
            Employees.Add(e);
            return SaveChangesAsync();
        }

        public interface IEmployee
        {
            string Name { get; set; }
        }

        public class Employee : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<EmployeeDevice> Devices { get; set; }
        }

        public interface IEmployeeDevice
        {
            string Device { get; set; }
        }

        public class EmployeeDevice : IEmployeeDevice
        {
            public int Id { get; set; }
            public int EmployeeId { get; set; }
            public string Device { get; set; }
            public Employee Employee { get; set; }
        }
    }

    #endregion

    #region 12748

    [ConditionalFact]
    public virtual async Task Correlated_collection_correctly_associates_entities_with_byte_array_keys()
    {
        var contextFactory = await InitializeAsync<Context12748>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();
        var query = from blog in context.Blogs
                    select new
                    {
                        blog.Name,
                        Comments = blog.Comments.Select(
                            u => new { u.Id }).ToArray()
                    };
        var result = query.ToList();
        Assert.Single(result[0].Comments);
    }

    private class Context12748(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public Task SeedAsync()
        {
            Blogs.Add(new Blog { Name = Encoding.UTF8.GetBytes("Awesome Blog") });
            Comments.Add(new Comment { BlogName = Encoding.UTF8.GetBytes("Awesome Blog") });
            return SaveChangesAsync();
        }

        public class Blog
        {
            [Key]
            public byte[] Name { get; set; }

            public List<Comment> Comments { get; set; }
        }

        public class Comment
        {
            public int Id { get; set; }
            public byte[] BlogName { get; set; }
            public Blog Blog { get; set; }
        }
    }

    #endregion

    #region 20609

    [ConditionalFact]
    public virtual async Task Can_ignore_invalid_include_path_error()
    {
        var contextFactory = await InitializeAsync<Context20609>(
            onConfiguring: o => o.ConfigureWarnings(x => x.Ignore(CoreEventId.InvalidIncludePathError)));

        using var context = contextFactory.CreateContext();
        var result = context.Set<Context20609.ClassA>().Include("SubB").ToList();
    }

    protected class Context20609(DbContextOptions options) : DbContext(options)
    {
        public DbSet<BaseClass> BaseClasses { get; set; }
        public DbSet<SubA> SubAs { get; set; }
        public DbSet<SubB> SubBs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClassA>().HasBaseType<BaseClass>().HasOne(x => x.SubA).WithMany();
            modelBuilder.Entity<ClassB>().HasBaseType<BaseClass>().HasOne(x => x.SubB).WithMany();
        }

        public class BaseClass
        {
            public string Id { get; set; }
        }

        public class ClassA : BaseClass
        {
            public SubA SubA { get; set; }
        }

        public class ClassB : BaseClass
        {
            public SubB SubB { get; set; }
        }

        public class SubA
        {
            public int Id { get; set; }
        }

        public class SubB
        {
            public int Id { get; set; }
        }
    }

    #endregion

    #region 20813

    [ConditionalFact]
    public virtual async Task SelectMany_and_collection_in_projection_in_FirstOrDefault()
    {
        var contextFactory = await InitializeAsync<Context20813>();

        using var context = contextFactory.CreateContext();
        var referenceId = "a";
        var customerId = new Guid("1115c816-6c4c-4016-94df-d8b60a22ffa1");
        var query = context.Orders
            .Where(o => o.ExternalReferenceId == referenceId && o.CustomerId == customerId)
            .Select(
                o => new
                {
                    IdentityDocuments = o.IdentityDocuments.Select(
                        id => new
                        {
                            Images = o.IdentityDocuments
                                .SelectMany(id => id.Images)
                                .Select(i => new { i.Image }),
                        })
                }).SingleOrDefault();
    }

    private class Context20813(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Order> Orders { get; set; }

        public class Order
        {
            private ICollection<IdentityDocument> _identityDocuments;

            public Guid Id { get; set; }

            public Guid CustomerId { get; set; }

            public string ExternalReferenceId { get; set; }

            public ICollection<IdentityDocument> IdentityDocuments
            {
                get => _identityDocuments = _identityDocuments ?? new Collection<IdentityDocument>();
                set => _identityDocuments = value;
            }
        }

        public class IdentityDocument
        {
            private ICollection<IdentityDocumentImage> _images;

            public Guid Id { get; set; }

            [ForeignKey(nameof(Order))]
            public Guid OrderId { get; set; }

            public Order Order { get; set; }

            public ICollection<IdentityDocumentImage> Images
            {
                get => _images = _images ?? new Collection<IdentityDocumentImage>();
                set => _images = value;
            }
        }

        public class IdentityDocumentImage
        {
            public Guid Id { get; set; }

            [ForeignKey(nameof(IdentityDocument))]
            public Guid IdentityDocumentId { get; set; }

            public byte[] Image { get; set; }

            public IdentityDocument IdentityDocument { get; set; }
        }
    }

    #endregion

    #region 21768

    [ConditionalFact]
    public virtual async Task Using_explicit_interface_implementation_as_navigation_works()
    {
        var contextFactory = await InitializeAsync<Context21768>();
        using var context = contextFactory.CreateContext();
        Expression<Func<Context21768.IBook, Context21768.BookViewModel>> projection =
            b => new Context21768.BookViewModel
            {
                FirstPage = b.FrontCover.Illustrations.FirstOrDefault(
                        i => i.State >= Context21768.IllustrationState.Approved)
                    != null
                        ? new Context21768.PageViewModel
                        {
                            Uri = b.FrontCover.Illustrations
                                .FirstOrDefault(i => i.State >= Context21768.IllustrationState.Approved).Uri
                        }
                        : null,
            };

        var result = context.Books.Where(b => b.Id == 1).Select(projection).SingleOrDefault();
    }

    private class Context21768(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCover> BookCovers { get; set; }
        public DbSet<CoverIllustration> CoverIllustrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }

        public class BookViewModel
        {
            public PageViewModel FirstPage { get; set; }
        }

        public class PageViewModel
        {
            public string Uri { get; set; }
        }

        public interface IBook
        {
            public int Id { get; set; }

            public IBookCover FrontCover { get; }
            public int FrontCoverId { get; set; }

            public IBookCover BackCover { get; }
            public int BackCoverId { get; set; }
        }

        public interface IBookCover
        {
            public int Id { get; set; }
            public IEnumerable<ICoverIllustration> Illustrations { get; }
        }

        public interface ICoverIllustration
        {
            public int Id { get; set; }
            public IBookCover Cover { get; }
            public int CoverId { get; set; }
            public string Uri { get; set; }
            public IllustrationState State { get; set; }
        }

        public class Book : IBook
        {
            public int Id { get; set; }

            public BookCover FrontCover { get; set; }
            public int FrontCoverId { get; set; }

            public BookCover BackCover { get; set; }
            public int BackCoverId { get; set; }

            IBookCover IBook.FrontCover
                => FrontCover;

            IBookCover IBook.BackCover
                => BackCover;
        }

        public class BookCover : IBookCover
        {
            public int Id { get; set; }
            public ICollection<CoverIllustration> Illustrations { get; set; }

            IEnumerable<ICoverIllustration> IBookCover.Illustrations
                => Illustrations;
        }

        public class CoverIllustration : ICoverIllustration
        {
            public int Id { get; set; }
            public BookCover Cover { get; set; }
            public int CoverId { get; set; }
            public string Uri { get; set; }
            public IllustrationState State { get; set; }

            IBookCover ICoverIllustration.Cover
                => Cover;
        }

        public enum IllustrationState
        {
            New,
            PendingApproval,
            Approved,
            Printed
        }
    }

    #endregion

    #region 22568

    [ConditionalFact]
    public virtual async Task Cycles_in_auto_include()
    {
        var contextFactory = await InitializeAsync<Context22568>(seed: c => c.SeedAsync());
        using (var context = contextFactory.CreateContext())
        {
            var principals = context.Set<Context22568.PrincipalOneToOne>().ToList();
            Assert.Single(principals);
            Assert.NotNull(principals[0].Dependent);
            Assert.NotNull(principals[0].Dependent.Principal);

            var dependents = context.Set<Context22568.DependentOneToOne>().ToList();
            Assert.Single(dependents);
            Assert.NotNull(dependents[0].Principal);
            Assert.NotNull(dependents[0].Principal.Dependent);
        }

        using (var context = contextFactory.CreateContext())
        {
            var principals = context.Set<Context22568.PrincipalOneToMany>().ToList();
            Assert.Single(principals);
            Assert.NotNull(principals[0].Dependents);
            Assert.True(principals[0].Dependents.All(e => e.Principal != null));

            var dependents = context.Set<Context22568.DependentOneToMany>().ToList();
            Assert.Equal(2, dependents.Count);
            Assert.True(dependents.All(e => e.Principal != null));
            Assert.True(dependents.All(e => e.Principal.Dependents != null));
            Assert.True(dependents.All(e => e.Principal.Dependents.All(i => i.Principal != null)));
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Equal(
                CoreStrings.AutoIncludeNavigationCycle("'PrincipalManyToMany.Dependents', 'DependentManyToMany.Principals'"),
                Assert.Throws<InvalidOperationException>(() => context.Set<Context22568.PrincipalManyToMany>().ToList()).Message);

            Assert.Equal(
                CoreStrings.AutoIncludeNavigationCycle("'DependentManyToMany.Principals', 'PrincipalManyToMany.Dependents'"),
                Assert.Throws<InvalidOperationException>(() => context.Set<Context22568.DependentManyToMany>().ToList()).Message);

            context.Set<Context22568.PrincipalManyToMany>().IgnoreAutoIncludes().ToList();
            context.Set<Context22568.DependentManyToMany>().IgnoreAutoIncludes().ToList();
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Equal(
                CoreStrings.AutoIncludeNavigationCycle("'CycleA.Bs', 'CycleB.C', 'CycleC.As'"),
                Assert.Throws<InvalidOperationException>(() => context.Set<Context22568.CycleA>().ToList()).Message);

            Assert.Equal(
                CoreStrings.AutoIncludeNavigationCycle("'CycleB.C', 'CycleC.As', 'CycleA.Bs'"),
                Assert.Throws<InvalidOperationException>(() => context.Set<Context22568.CycleB>().ToList()).Message);

            Assert.Equal(
                CoreStrings.AutoIncludeNavigationCycle("'CycleC.As', 'CycleA.Bs', 'CycleB.C'"),
                Assert.Throws<InvalidOperationException>(() => context.Set<Context22568.CycleC>().ToList()).Message);

            context.Set<Context22568.CycleA>().IgnoreAutoIncludes().ToList();
            context.Set<Context22568.CycleB>().IgnoreAutoIncludes().ToList();
            context.Set<Context22568.CycleC>().IgnoreAutoIncludes().ToList();
        }
    }

    protected class Context22568(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PrincipalOneToOne>().Navigation(e => e.Dependent).AutoInclude();
            modelBuilder.Entity<DependentOneToOne>().Navigation(e => e.Principal).AutoInclude();
            modelBuilder.Entity<PrincipalOneToMany>().Navigation(e => e.Dependents).AutoInclude();
            modelBuilder.Entity<DependentOneToMany>().Navigation(e => e.Principal).AutoInclude();
            modelBuilder.Entity<PrincipalManyToMany>().Navigation(e => e.Dependents).AutoInclude();
            modelBuilder.Entity<DependentManyToMany>().Navigation(e => e.Principals).AutoInclude();

            modelBuilder.Entity<CycleA>().Navigation(e => e.Bs).AutoInclude();
            modelBuilder.Entity<CycleB>().Navigation(e => e.C).AutoInclude();
            modelBuilder.Entity<CycleC>().Navigation(e => e.As).AutoInclude();
        }

        public Task SeedAsync()
        {
            Add(new PrincipalOneToOne { Dependent = new DependentOneToOne() });
            Add(
                new PrincipalOneToMany
                {
                    Dependents = [new(), new()]
                });

            return SaveChangesAsync();
        }

        public class PrincipalOneToOne
        {
            public int Id { get; set; }
            public DependentOneToOne Dependent { get; set; }
        }

        public class DependentOneToOne
        {
            public int Id { get; set; }

            [ForeignKey("Principal")]
            public int PrincipalId { get; set; }

            public PrincipalOneToOne Principal { get; set; }
        }

        public class PrincipalOneToMany
        {
            public int Id { get; set; }
            public List<DependentOneToMany> Dependents { get; set; }
        }

        public class DependentOneToMany
        {
            public int Id { get; set; }

            [ForeignKey("Principal")]
            public int PrincipalId { get; set; }

            public PrincipalOneToMany Principal { get; set; }
        }

        public class PrincipalManyToMany
        {
            public int Id { get; set; }
            public List<DependentManyToMany> Dependents { get; set; }
        }

        public class DependentManyToMany
        {
            public int Id { get; set; }
            public List<PrincipalManyToMany> Principals { get; set; }
        }

        public class CycleA
        {
            public int Id { get; set; }
            public List<CycleB> Bs { get; set; }
        }

        public class CycleB
        {
            public int Id { get; set; }
            public CycleC C { get; set; }
        }

        public class CycleC
        {
            public int Id { get; set; }

            [ForeignKey("B")]
            public int BId { get; set; }

            private CycleB B { get; set; }
            public List<CycleA> As { get; set; }
        }
    }

    #endregion

    #region 23674

    [ConditionalFact]
    public virtual async Task Walking_back_include_tree_is_not_allowed_1()
    {
        var contextFactory = await InitializeAsync<Context23674>();

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Set<Context23674.Principal>()
                .Include(p => p.ManyDependents)
                .ThenInclude(m => m.Principal.SingleDependent);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.NavigationBaseIncludeIgnored.ToString(),
                    CoreResources.LogNavigationBaseIncludeIgnored(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage("ManyDependent.Principal"),
                    "CoreEventId.NavigationBaseIncludeIgnored"),
                Assert.Throws<InvalidOperationException>(
                    () => query.ToList()).Message);
        }
    }

    [ConditionalFact]
    public virtual async Task Walking_back_include_tree_is_not_allowed_2()
    {
        var contextFactory = await InitializeAsync<Context23674>();

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Set<Context23674.Principal>().Include(p => p.SingleDependent.Principal.ManyDependents);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.NavigationBaseIncludeIgnored.ToString(),
                    CoreResources.LogNavigationBaseIncludeIgnored(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage("SingleDependent.Principal"),
                    "CoreEventId.NavigationBaseIncludeIgnored"),
                Assert.Throws<InvalidOperationException>(
                    () => query.ToList()).Message);
        }
    }

    [ConditionalFact]
    public virtual async Task Walking_back_include_tree_is_not_allowed_3()
    {
        var contextFactory = await InitializeAsync<Context23674>();

        using (var context = contextFactory.CreateContext())
        {
            // This does not warn because after round-tripping from one-to-many from dependent side, the number of dependents could be larger.
            var query = context.Set<Context23674.ManyDependent>()
                .Include(p => p.Principal.ManyDependents)
                .ThenInclude(m => m.SingleDependent)
                .ToList();
        }
    }

    [ConditionalFact]
    public virtual async Task Walking_back_include_tree_is_not_allowed_4()
    {
        var contextFactory = await InitializeAsync<Context23674>();

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Set<Context23674.SingleDependent>().Include(p => p.ManyDependent.SingleDependent.Principal);

            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    CoreEventId.NavigationBaseIncludeIgnored.ToString(),
                    CoreResources.LogNavigationBaseIncludeIgnored(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage("ManyDependent.SingleDependent"),
                    "CoreEventId.NavigationBaseIncludeIgnored"),
                Assert.Throws<InvalidOperationException>(
                    () => query.ToList()).Message);
        }
    }

    private class Context23674(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Principal>();


        public class Principal
        {
            public int Id { get; set; }
            public List<ManyDependent> ManyDependents { get; set; }
            public SingleDependent SingleDependent { get; set; }
        }

        public class ManyDependent
        {
            public int Id { get; set; }
            public Principal Principal { get; set; }
            public SingleDependent SingleDependent { get; set; }
        }

        public class SingleDependent
        {
            public int Id { get; set; }
            public Principal Principal { get; set; }
            public int PrincipalId { get; set; }
            public int ManyDependentId { get; set; }
            public ManyDependent ManyDependent { get; set; }
        }
    }

    #endregion

    #region 23676

    [ConditionalFact]
    public virtual async Task Projection_with_multiple_includes_and_subquery_with_set_operation()
    {
        var contextFactory = await InitializeAsync<Context23676>();

        using var context = contextFactory.CreateContext();
        var id = 1;
        var person = await context.Persons
            .Include(p => p.Images)
            .Include(p => p.Actor)
            .ThenInclude(a => a.Movies)
            .ThenInclude(p => p.Movie)
            .Include(p => p.Director)
            .ThenInclude(a => a.Movies)
            .ThenInclude(p => p.Movie)
            .Select(
                x => new
                {
                    x.Id,
                    x.Name,
                    x.Surname,
                    x.Birthday,
                    x.Hometown,
                    x.Bio,
                    x.AvatarUrl,
                    Images = x.Images
                        .Select(
                            i => new
                            {
                                i.Id,
                                i.ImageUrl,
                                i.Height,
                                i.Width
                            }).ToList(),
                    KnownByFilms = x.Actor.Movies
                        .Select(m => m.Movie)
                        .Union(
                            x.Director.Movies
                                .Select(m => m.Movie))
                        .Select(
                            m => new
                            {
                                m.Id,
                                m.Name,
                                m.PosterUrl,
                                m.Rating
                            }).ToList()
                })
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    private class Context23676(DbContextOptions options) : DbContext(options)
    {
        public DbSet<PersonEntity> Persons { get; set; }

        public class PersonEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public DateTime Birthday { get; set; }
            public string Hometown { get; set; }
            public string Bio { get; set; }
            public string AvatarUrl { get; set; }

            public ActorEntity Actor { get; set; }
            public DirectorEntity Director { get; set; }
            public IList<PersonImageEntity> Images { get; } = new List<PersonImageEntity>();
        }

        public class PersonImageEntity
        {
            public int Id { get; set; }
            public string ImageUrl { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
            public PersonEntity Person { get; set; }
        }

        public class ActorEntity
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public PersonEntity Person { get; set; }

            public IList<MovieActorEntity> Movies { get; } = new List<MovieActorEntity>();
        }

        public class MovieActorEntity
        {
            public int Id { get; set; }
            public int ActorId { get; set; }
            public ActorEntity Actor { get; set; }

            public int MovieId { get; set; }
            public MovieEntity Movie { get; set; }

            public string RoleInFilm { get; set; }

            public int Order { get; set; }
        }

        public class DirectorEntity
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public PersonEntity Person { get; set; }

            public IList<MovieDirectorEntity> Movies { get; } = new List<MovieDirectorEntity>();
        }

        public class MovieDirectorEntity
        {
            public int Id { get; set; }
            public int DirectorId { get; set; }
            public DirectorEntity Director { get; set; }

            public int MovieId { get; set; }
            public MovieEntity Movie { get; set; }
        }

        public class MovieEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Rating { get; set; }
            public string Description { get; set; }
            public DateTime ReleaseDate { get; set; }
            public int DurationInMins { get; set; }
            public int Budget { get; set; }
            public int Revenue { get; set; }
            public string PosterUrl { get; set; }

            public IList<MovieDirectorEntity> Directors { get; set; } = new List<MovieDirectorEntity>();
            public IList<MovieActorEntity> Actors { get; set; } = new List<MovieActorEntity>();
        }
    }

    #endregion

    #region 26433

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Count_member_over_IReadOnlyCollection_works(bool async)
    {
        var contextFactory = await InitializeAsync<Context26433>(seed: c => c.SeedAsync());
        using var context = contextFactory.CreateContext();

        var query = context.Authors
            .Select(a => new { BooksCount = a.Books.Count });

        var authors = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(3, Assert.Single(authors).BooksCount);
    }

    protected class Context26433(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }

        public Task SeedAsync()
        {
            base.Add(
                new Author
                {
                    FirstName = "William",
                    LastName = "Shakespeare",
                    Books = new List<Book>
                    {
                        new() { Title = "Hamlet" },
                        new() { Title = "Othello" },
                        new() { Title = "MacBeth" }
                    }
                });

            return SaveChangesAsync();
        }

        public class Author
        {
            [Key]
            public int AuthorId { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public IReadOnlyCollection<Book> Books { get; set; }
        }

        public class Book
        {
            [Key]
            public int BookId { get; set; }

            public string Title { get; set; }
            public int AuthorId { get; set; }
            public Author Author { get; set; }
        }
    }

    #endregion
}
