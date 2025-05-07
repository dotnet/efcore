// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class LoggingTestBase
{
    [ConditionalFact]
    public void Logs_context_initialization_default_options()
        => Assert.Equal(ExpectedMessage(DefaultOptions), ActualMessage(CreateOptionsBuilder));

    [ConditionalFact]
    public void Logs_context_initialization_no_tracking()
        => Assert.Equal(
            ExpectedMessage("NoTracking " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)));

    [ConditionalFact]
    public void Logs_context_initialization_sensitive_data_logging()
        => Assert.Equal(
            ExpectedMessage("SensitiveDataLoggingEnabled " + DefaultOptions),
            ActualMessage(s => CreateOptionsBuilder(s).EnableSensitiveDataLogging()));

    protected virtual string ExpectedMessage(string optionsFragment)
        => CoreResources.LogContextInitialized(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
            ProductInfo.GetVersion(),
            nameof(LoggingContext),
            ProviderName,
            ProviderVersion,
            optionsFragment ?? "None").Trim();

    [ConditionalFact]
    public virtual void InvalidIncludePathError_throws_by_default()
    {
        using var context = new InvalidIncludePathErrorContext(CreateOptionsBuilder(new ServiceCollection()));

        Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                CoreEventId.InvalidIncludePathError.ToString(),
                CoreResources.LogInvalidIncludePath(CreateTestLogger())
                    .GenerateMessage("Wheels", "Wheels"),
                "CoreEventId.InvalidIncludePathError"),
            Assert.Throws<InvalidOperationException>(
                () => context.Set<Animal>().Include("Wheels").Load()).Message);
    }

    protected class InvalidIncludePathErrorContext(DbContextOptionsBuilder optionsBuilder) : DbContext(optionsBuilder.Options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Animal>();
    }

    protected class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Person FavoritePerson { get; set; }
    }

    protected class Cat : Animal
    {
        public string Breed { get; set; }
        public string Type { get; set; }
        public int Identity { get; set; }
    }

    protected class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FavoriteBreed { get; set; }
    }

    protected abstract TestLogger CreateTestLogger();

    protected abstract DbContextOptionsBuilder CreateOptionsBuilder(IServiceCollection services);

    protected abstract string ProviderName { get; }

    protected abstract string ProviderVersion { get; }

    protected virtual string DefaultOptions
        => null;

    protected virtual string ActualMessage(Func<IServiceCollection, DbContextOptionsBuilder> optionsActions)
    {
        var loggerFactory = new ListLoggerFactory();
        var optionsBuilder = optionsActions(new ServiceCollection().AddSingleton<ILoggerFactory>(loggerFactory));

        using (var context = new LoggingContext(optionsBuilder))
        {
            var _ = context.Model;
        }

        return loggerFactory.Log.Single(t => t.Id.Id == CoreEventId.ContextInitialized.Id).Message;
    }

    protected class LoggingContext(DbContextOptionsBuilder optionsBuilder) : DbContext(optionsBuilder.Options);
}
