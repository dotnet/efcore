// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CosmosEmulatorTestStore
    (string name, bool shared = true, Action<CosmosDbContextOptionsBuilder>? extensionConfiguration = null)
  : CosmosTestStore(name, shared, extensionConfiguration)
{
    /// <summary>
    /// The emulator becomes unstable with parallel CRUD container requests, use this lock to create containers.
    /// </summary>
    public static readonly SemaphoreSlim ContainerCrudSemaphore = new(1);

    private bool _databaseCreated;
    private bool _acquired;

    public override async Task<TestStore> InitializeAsync(
        IServiceProvider? serviceProvider,
        Func<DbContext>? createContext,
        Func<DbContext, Task>? seed = null,
        Func<DbContext, Task>? clean = null)
    {
        createContext ??= CreateDefaultContext;

        // Control concurrent amount of containers
        // Emulator could experience performance degradation with more than 10 concurrent containers
        // See: https://learn.microsoft.com/en-us/azure/cosmos-db/emulator#differences-between-the-emulator-and-cloud-service
        // Emulator also stops responding with more than ~25 containers concurrent containers.
        using (var context = createContext())
        {
            if (_acquired)
            {
                // Non shared tests will call initialize multiple times in the same fixture.
                // Each initialize could be with a different db context with a different number of containers, so we possibly have to wait for containers to become available.
                await WaitChangeContainerCountAsync(context);
            }
            else
            {
                await WaitAsync(context);
            }
        }
        _acquired = true;
        return await base.InitializeAsync(serviceProvider, createContext, seed, clean);
    }

    public override async Task<bool> CreatedContainersAsync(DbContext context, bool skipTokenCredential = false, CancellationToken cancellationToken = default)
    {
        await ContainerCrudSemaphore.WaitAsync(cancellationToken);
        try
        {
            return await base.CreatedContainersAsync(context, skipTokenCredential, cancellationToken);
        }
        finally
        {
            ContainerCrudSemaphore.Release();
        }
    }

    protected override async Task CreateDatabaseAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        await ContainerCrudSemaphore.WaitAsync(cancellationToken);
        try
        {
            await base.CreateDatabaseAsync(context, cancellationToken);
            _databaseCreated = true;
        }
        finally
        {
            ContainerCrudSemaphore.Release();
        }
    }

    protected override async Task DeleteContainersAsync(DbContext context)
    {
        await ContainerCrudSemaphore.WaitAsync();
        try
        {
            await base.DeleteContainersAsync(context);
        }
        finally
        {
            ContainerCrudSemaphore.Release();
        }
    }

    protected override async Task<bool> DeleteDatabaseAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        await ContainerCrudSemaphore.WaitAsync(cancellationToken);
        try
        {
            var r = await base.DeleteDatabaseAsync(context, cancellationToken);
            _databaseCreated = false;
            return r;
        }
        finally
        {
            ContainerCrudSemaphore.Release();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_acquired)
        {
            await using var deleteLock = await ReleaseAsync();
            _acquired = false;
            if (Shared)
            {
                // We only get a delete lock if this test store instance is the last one using the database, so we can safely delete the database if it's shared.
                if (deleteLock != null)
                {
                    // The emulator stops responding to create container requests at ~25 containers.
                    // So we delete containers if they are not being used anymore.
                    await DeleteDatabaseAsync(StoreContext);
                    GetTestStoreIndex(ServiceProvider).RemoveShared(GetType().Name + Name);
                }
            }
            else
            {
                // If the test store is not shared, the base dispose will delete the database.
                await base.DisposeAsync();
                return;
            }
        }

        await base.DisposeAsync();
    }

    #region Emulator parallelism control
    // Emulator could experience performance degradation with more than 10 concurrent containers,
    // See: https://learn.microsoft.com/en-us/azure/cosmos-db/emulator#differences-between-the-emulator-and-cloud-service
    private const int RecommendedContainerCount = 10;

    private static readonly SemaphoreSlim _concurrencyControlSemaphore = new(1);
    private static readonly Dictionary<string, TestStoreInfo> _testsMap = new();
    private static readonly List<TestStoreInfo> _waitingTests = new();
    private static int _currentContainerCount;

    private async Task WaitAsync(DbContext dbContext)
    {
        Task waitTask;
        await _concurrencyControlSemaphore.WaitAsync();
        try
        {
            waitTask = await GetWaitTask();
        }
        finally
        {
            _concurrencyControlSemaphore.Release();
        }

        await waitTask;

        async ValueTask<Task> GetWaitTask()
        {
            ref var infoRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_testsMap, Name, out var exists);
            if (!exists)
            {
                // Non-shared test stores get priority because their wait time affects their test run time.
                infoRef = new TestStoreInfo(GetContainerCount(dbContext), !Shared);
                if (_currentContainerCount == 0 || _currentContainerCount + infoRef.ContainerCount <= RecommendedContainerCount)
                {
                    infoRef.Start();
                    _currentContainerCount += infoRef.ContainerCount;
                    return Task.CompletedTask;
                }

                if (infoRef.Priority)
                {
                    InsertAt(_waitingTests, infoRef, x => x.Priority);
                }
                else
                {
                    _waitingTests.Add(infoRef);
                }
            }
            else
            {
                Debug.Assert(infoRef != null);

                if (infoRef.ReleaseCompletionSource != null)
                {
                    // The test store is still being deleted
                    await infoRef.ReleaseCompletionSource.Task;
                    return WaitAsync(dbContext);
                }

                if (infoRef.IsRunning)
                {
                    infoRef.RunningCounter++;
                    return Task.CompletedTask;
                }

                infoRef.WaitCounter++;
            }

            return infoRef.WaitCompletionSource.Task;
        }
    }

    private async Task WaitChangeContainerCountAsync(DbContext dbContext)
    {
        Task waitTask;
        var newContainerCount = GetContainerCount(dbContext);
        await _concurrencyControlSemaphore.WaitAsync();
        try
        {
            waitTask = await GetWaitTask();
        }
        finally
        {
            _concurrencyControlSemaphore.Release();
        }

        await waitTask;

        async ValueTask<Task> GetWaitTask()
        {
            var info = _testsMap[Name];
            Debug.Assert(info.IsRunning);

            var difference = newContainerCount - info.ContainerCount;
            if (difference > 0)
            {
                // @TODO: Can never be 0 right? Unless info.ContainerCount is 0, but then _containerCount == info.ContainerCount == true
                if (_currentContainerCount == 0 || _currentContainerCount == info.ContainerCount || _currentContainerCount + difference <= RecommendedContainerCount) // @TODO: We could allow some leeway here? RecommendedContainerCount -> MaxContainerCount?
                {
                    _currentContainerCount += difference;
                    return Task.CompletedTask;
                }

                // Re enter the queue.
                if (_databaseCreated)
                {
                    await DeleteContainersAsync(dbContext); // @TODO: Does this have to happen inside the lock?
                }
                _currentContainerCount -= info.ContainerCount;
                info.Reenter(newContainerCount);
                InsertAt(_waitingTests, info, x => x.ReEntered);

                return WaitAsync(dbContext);
            }
            else
            {
                if (_databaseCreated)
                {
                    await DeleteContainersAsync(dbContext); // @TODO: Does this have to happen inside the lock?
                }

                _currentContainerCount += difference;
                TryStartNextTest();

                return Task.CompletedTask;
            }
        }
    }

    private static void InsertAt<T>(List<T> values, T value, Func<T, bool> predicate)
    {
        var index = GetIndexPreceding(values, predicate);
        if (index == -1)
        {
            values.Add(value);
        }
        else
        {
            values.Insert(index, value);
        }
    }

    private static int GetIndexPreceding<T>(List<T> values, Func<T, bool> predicate)
    {
        for (var i = 0; i < values.Count; i++)
        {
            var v = values[i];
            if (!predicate(v))
            {
                return i;
            }
        }

        return -1;
    }

    private static int GetContainerCount(DbContext dbContext)
    {
        try
        {
            return dbContext.Model.GetEntityTypes().Select(x => x.GetContainer()).Where(x => x != null).Distinct().Count();
        }
        catch
        {
            return 0;
        }
    }

    private async Task<ReleaseLock?> ReleaseAsync()
    {
        await _concurrencyControlSemaphore.WaitAsync();
        try
        {
            var info = _testsMap[Name];
            if (--info.RunningCounter == 0)
            {
                info.Releasing();
                var releaseLock = new ReleaseLock(Name, info);
                return releaseLock;
            }
        }
        finally
        {
            _concurrencyControlSemaphore.Release();
        }

        return null;
    }

    private static void TryStartNextTest()
    {
        while (_waitingTests.Count > 0)
        {
            var nextInfo = _waitingTests[0];

            if (_currentContainerCount != 0 && _currentContainerCount + nextInfo.ContainerCount > RecommendedContainerCount)
            {
                break;
            }

            nextInfo.Start();
            _currentContainerCount += nextInfo.ContainerCount;
            _waitingTests.RemoveAt(0);
            nextInfo.WaitCompletionSource.SetResult();
        }
    }

    private class ReleaseLock(string name, TestStoreInfo info) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await _concurrencyControlSemaphore.WaitAsync();
            try
            {
                _testsMap.Remove(name);
                _currentContainerCount -= info.ContainerCount;

                TryStartNextTest();

                info.ReleaseCompletionSource!.SetResult();
            }
            finally
            {
                _concurrencyControlSemaphore.Release();
            }
        }
    }

    private class TestStoreInfo(int containerCount, bool priority)
    {
        public int ContainerCount { get; set; } = containerCount;

        public bool Priority { get; private set; } = priority;

        public TaskCompletionSource WaitCompletionSource { get; private set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int WaitCounter { get; set; } = 1;

        public bool IsRunning { get; private set; }
        public int RunningCounter { get; set; }

        public bool ReEntered { get; private set; }

        public TaskCompletionSource? ReleaseCompletionSource { get; private set; }

        public void Start()
        {
            IsRunning = true;
            RunningCounter = WaitCounter;
        }

        public void Releasing()
        {
            IsRunning = false;
            ReleaseCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void Reenter(int containerCount)
        {
            ReEntered = true;
            Priority = true;
            IsRunning = false;
            ContainerCount = containerCount;
            WaitCounter = RunningCounter;
            WaitCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    #endregion
}
