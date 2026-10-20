using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query;

public class QueryCommentTest : QueryCommentTestBase
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TagWith_multiline(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders.TagWith("first\r\nsecond").OrderBy(order => order.Id).Select(order => order.Id);
        var result = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 1, 2 }, result);
        AssertSql("-- first\n-- second\n\nSELECT [o].[Id]\nFROM [Orders] AS [o]\nORDER BY [o].[Id]");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Comment_compact(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders.Comment("order lookup").OrderBy(order => order.Id).Select(order => order.Id);
        var result = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 1, 2 }, result);
        AssertSql("/* order lookup */\nSELECT [o].[Id]\nFROM [Orders] AS [o]\nORDER BY [o].[Id]");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Comment_delimited_text(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders.Comment("before */ after /* end").OrderBy(order => order.Id).Select(order => order.Id);
        var result = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 1, 2 }, result);
        AssertSql("/* before * / after / * end */\nSELECT [o].[Id]\nFROM [Orders] AS [o]\nORDER BY [o].[Id]");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Comment_display_text(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders.Comment("\uff4f\uff52\uff44\uff45\uff52 lookup").OrderBy(order => order.Id).Select(order => order.Id);
        var result = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 1, 2 }, result);
        AssertSql("/* order lookup */\nSELECT [o].[Id]\nFROM [Orders] AS [o]\nORDER BY [o].[Id]");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Comment_multiline(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders.Comment("first\r\nsecond").OrderBy(order => order.Id).Select(order => order.Id);
        var result = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { 1, 2 }, result);
        AssertSql("-- first\n-- second\nSELECT [o].[Id]\nFROM [Orders] AS [o]\nORDER BY [o].[Id]");
    }
}