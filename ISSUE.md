[DateTime converted to Nullable<DateTime> inside optional navigation property, throws exception on AddDays #6261](https://github.com/aspnet/EntityFramework/issues/6261)
================================

I believe this is a regression from RC1.

Trying to query all the orders completed within the last day:

(Note: Placement is the optional navigational property and, in the context of my solution, if it exists it indicates than the order has been placed)

```
var ordersCompleted = await dbContext.Orders
    .Where(o => o.IsCompleted)
    .Where(o => o.Error == OrderErrorType.None)
    .Where(o => o.Placement.Placed.AddDays(1) > now)
    .ToListAsync();
```

Exception:

```
Message: Method 'System.DateTime AddDays(Double)' declared on type 'System.DateTime' cannot be called with instance of type 'System.Nullable\`1[System.DateTime]'
StackTrace: 
    at System.Linq.Expressions.Expression.ValidateCallInstanceType(Type instanceType, MethodInfo method)
    at System.Linq.Expressions.Expression.ValidateStaticOrInstanceMethod(Expression instance, MethodInfo method)
    at System.Linq.Expressions.Expression.Call(Expression instance, MethodInfo method, IEnumerable1 arguments) at Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal.NavigationRewritingExpressionVisitor.VisitMethodCall(MethodCallExpression node) at Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal.NavigationRewritingExpressionVisitor.VisitBinary(BinaryExpression node) at Remotion.Linq.Clauses.WhereClause.TransformExpressions(Func2 transformation)
    at Remotion.Linq.QueryModelVisitorBase.VisitBodyClauses(ObservableCollection1 bodyClauses, QueryModel queryModel) at Remotion.Linq.QueryModelVisitorBase.VisitQueryModel(QueryModel queryModel) at Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal.NavigationRewritingExpressionVisitor.Rewrite(QueryModel queryModel) at Microsoft.EntityFrameworkCore.Query.EntityQueryModelVisitor.OptimizeQueryModel(QueryModel queryModel) at Microsoft.EntityFrameworkCore.Query.EntityQueryModelVisitor.CreateAsyncQueryExecutor[TResult](QueryModel queryModel) at Microsoft.EntityFrameworkCore.Query.Internal.CompiledQueryCache.GetOrAddAsyncQuery[TResult](Object cacheKey, Func1 compiler)
    at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.ExecuteAsyncTResult
    at Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable1.System.Collections.Generic.IAsyncEnumerable<TResult>.GetEnumerator() at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.<ToListAsync>d__1291.MoveNext()
    --- End of stack trace from previous location where exception was thrown ---
    at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
    at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
    at VegeRun.Services.Orders.Queries.OrderQueries.d__3.MoveNext() in C:\Users\Michael\Code\Personal\20160429\VegeRun\src\VegeRun.Services\Orders\Queries\OrderQueries.cs:line 155
```

Relevant classes (minimalized):

``` csharp
public class Order
{
    public Guid Id { get; set; }
    public bool IsCompleted { get; set; }
    public OrderErrorType Error { get; set; }
    public Guid? PlacementId { get; set; }
    public virtual OrderPlacement Placement { get; set; }
}

public class OrderPlacement
{
    public Guid Id { get; set; }
    public DateTime Placed { get; set; }
}
```

Workaround:

``` csharp
var ordersCompleted2 = (await dbContext.Orders
    .Where(o => o.IsCompleted)
    .Where(o => o.Error == OrderErrorType.None)
    .Join(dbContext.OrderPlacements, o => o.PlacementId, p => p.Id,
            (o, p) => new { Order = o, Placement = p })
    .Where(op => op.Placement.Placed.AddDays(1) > now)
    .ToListAsync())
    .Select(op => op.Order)
```