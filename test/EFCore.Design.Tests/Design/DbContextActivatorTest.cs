// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Design;

public class DbContextActivatorTest
{
    [ConditionalFact]
    public void CreateInstance_works()
        => Assert.IsType<TestContext>(DbContextActivator.CreateInstance(typeof(TestContext)));

    [ConditionalFact]
    public void CreateInstance_with_arguments_works()
        => Assert.IsType<TestContext>(
            DbContextActivator.CreateInstance(
                typeof(TestContext),
                null,
                null,
                ["A", "B"]));

    private class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(nameof(DbContextActivatorTest));
    }

    [ConditionalFact]
    public void CreateInstance_throws_if_constructor_throws()
        => Assert.Equal(
            DesignStrings.CannotCreateContextInstance(typeof(ThrowingTestContext).FullName, "Bang!"),
            Assert.Throws<OperationException>(() => DbContextActivator.CreateInstance(typeof(ThrowingTestContext))).Message);

    private class ThrowingTestContext : DbContext
    {
        public ThrowingTestContext()
        {
            throw new Exception("Bang!");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(nameof(DbContextActivatorTest));
    }

    [ConditionalFact]
    public void CreateInstance_throws_if_constructor_not_parameterless()
    {
        var message = Assert.Throws<OperationException>(
            () => DbContextActivator.CreateInstance(typeof(ParameterTestContext))).Message;

        Assert.StartsWith(DesignStrings.CannotCreateContextInstance(nameof(ParameterTestContext), "").Substring(0, 10), message);
        Assert.Contains("Microsoft.EntityFrameworkCore.Design.DbContextActivatorTest+ParameterTestContext", message);
    }

#pragma warning disable CS9113 // Parameter 'foo' is unread
    private class ParameterTestContext(string foo) : DbContext
#pragma warning restore CS9113
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(nameof(DbContextActivatorTest));
    }
}
