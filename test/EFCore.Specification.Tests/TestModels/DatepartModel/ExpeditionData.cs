// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.DatepartModel;

public class ExpeditionData : ISetSource
{
    public static readonly ExpeditionData Instance = new();

    private ExpeditionData()
    {
        Expeditions = CreateExpeditions();
    }

    public IReadOnlyList<Expedition> Expeditions { get; } = null!;

    public IQueryable<TEntity> Set<TEntity>() where TEntity : class
        => typeof(TEntity) == typeof(Expedition)
            ? (IQueryable<TEntity>)Expeditions.AsQueryable()
            : throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));

    public static IReadOnlyList<Expedition> CreateExpeditions()
        => [
            new Expedition()
            {
                Id = 1,
                Destination = "The Amazon",
                StartDate = new DateTime(638713350005555555), // "1/1/2025 1:30:00.5555555 PM"
                StartTime = new TimeOnly(486005555555), // "1:30.5555555 PM"
                EndDate = new DateTimeOffset(638739396005555555, new()), // "1/31/2025 5:00:00.5555555 PM +00:00"
                Duration = new TimeSpan(863995555555) // "23:59:59.5555555"
            },
            new Expedition()
            {
                Id = 2,
                Destination = "Antarctica",
                StartDate = new DateTime(638713350004444444), // "1/1/2025 1:30:00.4444444 PM"
                StartTime = new TimeOnly(486004444444), // "1:30.4444444 PM"
                EndDate = new DateTimeOffset(638739396004444444, new()), // "1/31/2025 5:00:00.4444444 PM +00:00"
                Duration = new TimeSpan(863994444444) // "23:59:59.4444444"
            },
            new Expedition()
            {
                Id = 3,
                Destination = "Côn Đảo, Vietnam",
                StartDate = new DateTime(638713350003333333), // "1/1/2025 1:30:00.3333333 PM"
                StartTime = new TimeOnly(486003333333), // "1:30.3333333 PM"
                EndDate = new DateTimeOffset(638739396003333333, new()), // "1/31/2025 5:00:00.3333333 PM +00:00"
                Duration = new TimeSpan(863993333333) // "23:59:59.3333333"
            },
            new Expedition()
            {
                Id = 4,
                Destination = "Australia & New Zealand",
                StartDate = new DateTime(638713350002222222), // "1/1/2025 1:30:00.2222222 PM"
                StartTime = new TimeOnly(486002222222), // "1:30.2222222 PM"
                EndDate = new DateTimeOffset(638739396002222222, new()), // "1/31/2025 5:00:00.2222222 PM +00:00"
                Duration = new TimeSpan(863992222222) // "23:59:59.2222222"
            },
            new Expedition()
            {
                Id = 5,
                Destination = "Galapagos Islands",
                StartDate = new DateTime(638713350001111111), // "1/1/2025 1:30:00.1111111 PM"
                StartTime = new TimeOnly(486001111111), // "1:30.1111111 PM"
                EndDate = new DateTimeOffset(638739396001111111, new()), // "1/31/2025 5:00:00.1111111 PM +00:00"
                Duration = new TimeSpan(863991111111) // "23:59:59.1111111"
            },
        ];
}
