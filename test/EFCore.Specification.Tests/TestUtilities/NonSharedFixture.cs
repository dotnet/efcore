// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class NonSharedFixture : IAsyncLifetime
{
    private TestStore? _testStore;

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public virtual TestStore GetOrCreateTestStore(Func<TestStore> createTestStore)
        => _testStore ??= createTestStore();

    public virtual async ValueTask DisposeAsync()
    {
        if (_testStore != null)
        {
            await _testStore.DisposeAsync();
            _testStore = null;
        }
    }
}
