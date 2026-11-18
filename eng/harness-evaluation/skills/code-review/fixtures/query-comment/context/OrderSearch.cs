using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application;

public sealed class OrderSearch(OrdersContext context)
{
    public Task<List<int>> SearchAsync(
        string account,
        string requestDescription,
        CancellationToken cancellationToken)
        => context.Orders
            .Where(order => order.Account == account)
            .Comment(requestDescription)
            .OrderBy(order => order.Id)
            .Select(order => order.Id)
            .ToListAsync(cancellationToken);
}