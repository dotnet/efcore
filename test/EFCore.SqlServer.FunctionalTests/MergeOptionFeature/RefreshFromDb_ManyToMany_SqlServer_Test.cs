// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.MergeOptionFeature;

public class RefreshFromDb_ManyToMany_SqlServer_Test : IClassFixture<RefreshFromDb_ManyToMany_SqlServer_Test.ManyToManyFixture>
{
    private readonly ManyToManyFixture _fixture;

    public RefreshFromDb_ManyToMany_SqlServer_Test(ManyToManyFixture fixture)
        => _fixture = fixture;

    [Fact]
    public async Task Test_ManyToManyRelationships()
    {
        using var ctx = _fixture.CreateContext();

        // Get a student with their courses loaded
        var student = await ctx.Students.Include(s => s.Courses).OrderBy(c => c.Id).FirstAsync();
        var originalCourseCount = student.Courses.Count;

        try
        {
            // Get a course that the student is not enrolled in
            var courseToAdd = await ctx.Courses
                .Where(c => !student.Courses.Contains(c))
                .OrderBy(c => c.Id)
                .FirstAsync();

            // Simulate external change to many-to-many relationship by adding a join table record
            await ctx.Database.ExecuteSqlRawAsync(
                "INSERT INTO [StudentCourse] ([StudentsId], [CoursesId]) VALUES ({0}, {1})",
                student.Id, courseToAdd.Id);

            // student je već attachan u kontekstu
            var coll = ctx.Entry(student).Collection(s => s.Courses);

            // Ako je već bila učitana, spusti flag pa ponovno učitaj
            coll.IsLoaded = false;
            await coll.LoadAsync();   // ili coll.Load();

            // Assert that the new course is now included
            Assert.Equal(originalCourseCount + 1, student.Courses.Count);
            Assert.Contains(student.Courses, c => c.Id == courseToAdd.Id);
        }
        finally
        {
            // Cleanup - remove the added relationship
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM [StudentCourse] WHERE [StudentsId] = {0} AND [CoursesId] IN (SELECT [Id] FROM [Courses] WHERE [Id] NOT IN (SELECT [CoursesId] FROM [StudentCourse] WHERE [StudentsId] != {0}))",
                student.Id);
        }
    }

    [Fact]
    public async Task Test_ManyToManyRelationships_RemoveRelation()
    {
        using var ctx = _fixture.CreateContext();

        // Get a student with courses
        var student = await ctx.Students.Include(s => s.Courses).OrderBy(c => c.Id).FirstAsync(s => s.Courses.Any());
        var originalCourseCount = student.Courses.Count;
        var courseToRemove = student.Courses.First();

        try
        {
            // Simulate external removal of many-to-many relationship
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM [StudentCourse] WHERE [StudentsId] = {0} AND [CoursesId] = {1}",
                student.Id, courseToRemove.Id);

            ctx.Entry(student).State = EntityState.Detached;
            student = await ctx.Students
                .Include(s => s.Courses)
                .FirstAsync(s => s.Id == student.Id);

            // Assert that the course is no longer included
            Assert.Equal(originalCourseCount - 1, student.Courses.Count);
            Assert.DoesNotContain(student.Courses, c => c.Id == courseToRemove.Id);
        }
        finally
        {
            // Cleanup - restore the removed relationship
            await ctx.Database.ExecuteSqlRawAsync(
                "INSERT INTO [StudentCourse] ([StudentsId], [CoursesId]) VALUES ({0}, {1})",
                student.Id, courseToRemove.Id);
        }
    }

    [Fact]
    public async Task Test_ManyToManyRelationships_BothSides()
    {
        using var ctx = _fixture.CreateContext();

        // Get both sides of the many-to-many relationship
        var student = await ctx.Students.Include(s => s.Courses).OrderBy(c => c.Id).FirstAsync();
        var course = await ctx.Courses.Include(c => c.Students).OrderBy(c => c.Id).FirstAsync(c => !student.Courses.Contains(c));

        var originalStudentCourseCount = student.Courses.Count;
        var originalCourseStudentCount = course.Students.Count;

        try
        {
            // Add relationship externally
            await ctx.Database.ExecuteSqlRawAsync(
                "INSERT INTO [StudentCourse] ([StudentsId], [CoursesId]) VALUES ({0}, {1})",
                student.Id, course.Id);

            // Refresh both sides
            var courses = ctx.Entry(student).Collection(s => s.Courses);
            var students = ctx.Entry(course).Collection(c => c.Students);

            courses.IsLoaded = false;
            students.IsLoaded = false;

            await courses.LoadAsync();
            await students.LoadAsync();

            // Assert both sides are updated
            Assert.Equal(originalStudentCourseCount + 1, student.Courses.Count);
            Assert.Equal(originalCourseStudentCount + 1, course.Students.Count);
            Assert.Contains(student.Courses, c => c.Id == course.Id);
            Assert.Contains(course.Students, s => s.Id == student.Id);
        }
        finally
        {
            // Cleanup
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM [StudentCourse] WHERE [StudentsId] = {0} AND [CoursesId] = {1}",
                student.Id, course.Id);
        }
    }

    [Fact]
    public async Task Test_ManyToManyRelationships_MultipleRelations()
    {
        using var ctx = _fixture.CreateContext();

        var author = await ctx.Authors.Include(a => a.Books).OrderBy(c => c.Id).FirstAsync();
        var originalBookCount = author.Books.Count;

        try
        {
            // Get books not authored by this author
            var booksToAdd = await
                ctx
                .Books
                .Where(b => !author.Books.Contains(b))
                .OrderBy(c => c.Id)
                .Take(2)
                .ToListAsync();

            // Add multiple relationships externally
            foreach (var book in booksToAdd)
            {
                await ctx.Database.ExecuteSqlRawAsync(
                    "INSERT INTO [AuthorBook] ([AuthorsId], [BooksId]) VALUES ({0}, {1})",
                    author.Id, book.Id);
            }

            // Refresh the author's books collection
            var books = ctx.Entry(author).Collection(a => a.Books);
            books.IsLoaded = false;
            await books.LoadAsync();

            // Assert multiple books were added
            Assert.Equal(originalBookCount + booksToAdd.Count, author.Books.Count);
            foreach (var book in booksToAdd)
            {
                Assert.Contains(author.Books, b => b.Id == book.Id);
            }
        }
        finally
        {
            // Cleanup - remove all added relationships for this author
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM [AuthorBook] WHERE [AuthorsId] = {0} AND [BooksId] NOT IN (SELECT [BooksId] FROM [AuthorBook] ab2 WHERE ab2.[AuthorsId] = {0} GROUP BY [BooksId] HAVING COUNT(*) > 1)",
                author.Id);
        }
    }

    public class ManyToManyFixture : SharedStoreFixtureBase<ManyToManyContext>
    {
        protected override string StoreName
            => "ManyToManyRefreshFromDb";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).EnableSensitiveDataLogging();

        protected override async Task SeedAsync(ManyToManyContext context)
        {
            // Seed students
            var student1 = new Student { Name = "John Doe", Email = "john@example.com" };
            var student2 = new Student { Name = "Jane Smith", Email = "jane@example.com" };
            var student3 = new Student { Name = "Bob Johnson", Email = "bob@example.com" };

            // Seed courses
            var course1 = new Course { Title = "Mathematics", Credits = 3 };
            var course2 = new Course { Title = "Physics", Credits = 4 };
            var course3 = new Course { Title = "Chemistry", Credits = 3 };
            var course4 = new Course { Title = "Biology", Credits = 3 };

            // Seed authors
            var author1 = new Author { Name = "Stephen King" };
            var author2 = new Author { Name = "J.K. Rowling" };

            // Seed books
            var book1 = new Book { Title = "The Shining", Genre = "Horror" };
            var book2 = new Book { Title = "IT", Genre = "Horror" };
            var book3 = new Book { Title = "Harry Potter", Genre = "Fantasy" };

            context.Students.AddRange(student1, student2, student3);
            context.Courses.AddRange(course1, course2, course3, course4);
            context.Authors.AddRange(author1, author2);
            context.Books.AddRange(book1, book2, book3);

            await context.SaveChangesAsync();

            // Set up initial many-to-many relationships
            student1.Courses = [course1, course2];
            student2.Courses = [course2, course3];
            student3.Courses = [course1, course3, course4];

            author1.Books = [book1, book2];
            author2.Books = [book3];

            await context.SaveChangesAsync();
        }
    }

    public class ManyToManyContext : DbContext
    {
        public ManyToManyContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Author> Authors { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Student-Course many-to-many relationship
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(s => s.Email)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasMany(s => s.Courses)
                    .WithMany(c => c.Students)
                    .UsingEntity(j => j.ToTable("StudentCourse"));
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(c => c.Credits)
                    .IsRequired();
            });

            // Configure Author-Book many-to-many relationship
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasMany(a => a.Books)
                    .WithMany(b => b.Authors)
                    .UsingEntity(j => j.ToTable("AuthorBook"));
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(b => b.Id);

                entity.Property(b => b.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(b => b.Genre)
                    .HasMaxLength(50);
            });
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public List<Course> Courses { get; set; } = [];
    }

    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public int Credits { get; set; }
        public List<Student> Students { get; set; } = [];
    }

    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<Book> Books { get; set; } = [];
    }

    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Genre { get; set; }
        public List<Author> Authors { get; set; } = [];
    }
}
