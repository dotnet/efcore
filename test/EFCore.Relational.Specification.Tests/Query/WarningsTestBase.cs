// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class WarningsTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
{
    protected WarningsTestBase(TFixture fixture)
    {
        Fixture = fixture;
        fixture.ListLoggerFactory.Clear();
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalFact]
    public virtual void Does_not_throw_for_top_level_single()
    {
        using var context = CreateContext();
        var query = context.Orders.Single(x => x.OrderID == 10248);

        Assert.NotNull(query);
    }

    [ConditionalFact]
    public virtual void Paging_operation_without_orderby_issues_warning()
    {
        using var context = CreateContext();
        var query = context.Customers.Skip(2).Take(3).ToList();
        Assert.Equal(3, query.Count);
    }

    [ConditionalFact]
    public virtual async Task Paging_operation_without_orderby_issues_warning_async()
    {
        using var context = CreateContext();
        var query = await context.Customers.Skip(2).Take(3).ToListAsync();
        Assert.Equal(3, query.Count);
    }

    [ConditionalFact]
    public virtual void FirstOrDefault_without_orderby_and_filter_issues_warning_subquery()
    {
        using var context = CreateContext();
        var query = context.Customers.Where(c => c.CustomerID == "ALFKI" && c.Orders.FirstOrDefault().OrderID > 1000).ToList();
        Assert.Single(query);
    }

    [ConditionalFact]
    public virtual void FirstOrDefault_without_orderby_but_with_filter_doesnt_issue_warning()
    {
        using var context = CreateContext();
        var query = context.Customers.FirstOrDefault(c => c.CustomerID == "ALFKI");
        Assert.NotNull(query);
    }

    [ConditionalFact]
    public virtual void Single_SingleOrDefault_without_orderby_doesnt_issue_warning()
    {
        using var context = CreateContext();
        var query1 = context.Customers.Single(c => c.CustomerID == "ALFKI");
        Assert.NotNull(query1);

        var query2 = context.Customers.SingleOrDefault(c => c.CustomerID == "AROUT");
        Assert.NotNull(query2);
    }

    [ConditionalFact]
    public virtual void LastOrDefault_with_order_by_does_not_issue_client_eval_warning()
    {
        using var context = CreateContext();
        var query1 = context.Customers
            .Where(c => c.CustomerID == "ALFKI" && c.Orders.OrderBy(o => o.OrderID).LastOrDefault().OrderID > 1000).ToList();
        Assert.NotNull(query1);

        var query2 = context.Customers.OrderBy(c => c.CustomerID).LastOrDefault();
        Assert.NotNull(query2);
    }

    [ConditionalFact]
    public virtual void Last_with_order_by_does_not_issue_client_eval_warning_if_at_top_level()
    {
        using var context = CreateContext();
        var query = context.Customers.OrderBy(c => c.CustomerID).Last();
        Assert.NotNull(query);
    }

    [ConditionalFact]
    public virtual void Max_does_not_issue_client_eval_warning_when_at_top_level()
    {
        using var context = CreateContext();
        var query = context.Orders.Select(o => o.OrderID).Max();
    }

    [ConditionalFact]
    public virtual void Comparing_collection_navigation_to_null_issues_possible_unintended_consequences_warning()
    {
        using var context = CreateContext();
        var query = context.Customers.Where(c => c.Orders != null).ToList();
        Assert.Equal(91, query.Count);
    }

    [ConditionalFact]
    public virtual void Comparing_two_collections_together_issues_possible_unintended_reference_comparison_warning()
    {
        using var context = CreateContext();
        var query = context.Customers.Where(c => c.Orders == c.Orders).ToList();
        Assert.Equal(91, query.Count);
    }

    protected TFixture Fixture { get; }
}
