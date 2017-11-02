using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Samples
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class BloggingContext : DbContext
    {
        public static long InstanceCount;

        public BloggingContext(DbContextOptions options)
            : base(options) 
            => Interlocked.Increment(ref InstanceCount);

        public DbSet<Blog> Blogs { get; set; }
    }

    public class BlogController
    {
        private readonly BloggingContext _context;

        public BlogController(BloggingContext context) => _context = context;

        public async Task ActionAsync() => await _context.Blogs.FirstAsync();
    }

    public class Startup
    {
        private const string ConnectionString
            = @"Server=(localdb)\mssqllocaldb;Database=Demo.ContextPooling;Integrated Security=True;ConnectRetryCount=0";

        public void ConfigureServices(IServiceCollection services)
        {
            // Switch the lines below to compare pooling with the traditional instance-per-request approach.
            //services.AddDbContext<BloggingContext>(c => c.UseSqlServer(ConnectionString));
            services.AddDbContextPool<BloggingContext>(c => c.UseSqlServer(ConnectionString));
        }
    }

    public class Program
    {
        private const int Threads = 32;
        private const int Seconds = 10;

        private static long _requestsProcessed;

        private static async Task Main()
        {
            var serviceCollection = new ServiceCollection();
            new Startup().ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            SetupDatabase(serviceProvider);

            var stopwatch = new Stopwatch();

            MonitorResults(TimeSpan.FromSeconds(Seconds), stopwatch);

            await Task.WhenAll(
                Enumerable
                    .Range(0, Threads)
                    .Select(_ => SimulateRequestsAsync(serviceProvider, stopwatch)));
        }

        private static void SetupDatabase(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<BloggingContext>();

                if (context.Database.EnsureCreated())
                {
                    context.Blogs.Add(new Blog { Name = "The Dog Blog", Url = "http://sample.com/dogs" });
                    context.Blogs.Add(new Blog { Name = "The Cat Blog", Url = "http://sample.com/cats" });
                    context.SaveChanges();
                }
            }
        }

        private static async Task SimulateRequestsAsync(IServiceProvider serviceProvider, Stopwatch stopwatch)
        {
            while (stopwatch.IsRunning)
            {
                using (var serviceScope = serviceProvider.CreateScope())
                {
                    await new BlogController(serviceScope.ServiceProvider.GetService<BloggingContext>()).ActionAsync();
                }

                Interlocked.Increment(ref _requestsProcessed);
            }
        }

        private static async void MonitorResults(TimeSpan duration, Stopwatch stopwatch)
        {
            var lastInstanceCount = 0L;
            var lastRequestCount = 0L;
            var lastElapsed = TimeSpan.Zero;

            stopwatch.Start();

            while (stopwatch.Elapsed < duration)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                var instanceCount = BloggingContext.InstanceCount;
                var requestCount = _requestsProcessed;
                var elapsed = stopwatch.Elapsed;
                var currentElapsed = elapsed - lastElapsed;
                var currentRequests = requestCount - lastRequestCount;

                Console.WriteLine(
                    $"[{DateTime.Now:HH:mm:ss.fff}] "
                    + $"Context creations/second: {instanceCount - lastInstanceCount} | "
                    + $"Requests/second: {Math.Round(currentRequests / currentElapsed.TotalSeconds)}");

                lastInstanceCount = instanceCount;
                lastRequestCount = requestCount;
                lastElapsed = elapsed;
            }

            Console.WriteLine();
            Console.WriteLine($"Total context creations: {BloggingContext.InstanceCount}");
            Console.WriteLine(
                $"Requests per second:     {Math.Round(_requestsProcessed / stopwatch.Elapsed.TotalSeconds)}");

            stopwatch.Stop();
        }
    }
}
