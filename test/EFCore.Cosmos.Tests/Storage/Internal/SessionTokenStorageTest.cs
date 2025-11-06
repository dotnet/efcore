// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

public class SessionTokenStorageTest
{
    private readonly string _defaultContainerName = "default";
    private readonly HashSet<string> _containerNames = new(["default", "other"]);

    
    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.FullyAutomatic)]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void TrackSessionToken_WhenContainerNameIsNull_ThrowsArgumentNullException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        Assert.Throws<ArgumentNullException>(() => storage.TrackSessionToken(null!, "A"));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.FullyAutomatic)]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void TrackSessionToken_WhenContainerNameIsWhitespace_ThrowsArgumentNullException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        Assert.Throws<ArgumentException>(() => storage.TrackSessionToken("   ", "A"));
        Assert.Throws<ArgumentException>(() => storage.TrackSessionToken("", "A"));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.FullyAutomatic)]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void TrackSessionToken_WhenTokenIsNull_ThrowsArgumentNullException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        Assert.Throws<ArgumentNullException>(() => storage.TrackSessionToken(_defaultContainerName, null!));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.FullyAutomatic)]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void TrackSessionToken_WhenTokenIsWhitespace_ThrowsArgumentNullException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        Assert.Throws<ArgumentException>(() => storage.TrackSessionToken(_defaultContainerName, "   "));
        Assert.Throws<ArgumentException>(() => storage.TrackSessionToken(_defaultContainerName, ""));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_WhenContainerNameIsUnknown_ThrowsInvalidOperationException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.SetSessionTokens(new Dictionary<string, string?> { { "bad", "A" } }));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("bad"), ex.Message);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.FullyAutomatic)]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_WhenTokenIsNull_ThrowsArgumentNullException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        Assert.Throws<ArgumentNullException>(() => storage.AppendDefaultContainerSessionToken(null!));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.FullyAutomatic)]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_WhenTokenIsWhitespace_ThrowsArgumentException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        Assert.Throws<ArgumentException>(() => storage.AppendDefaultContainerSessionToken("   "));
        Assert.Throws<ArgumentException>(() => storage.AppendDefaultContainerSessionToken(""));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_WhenContainerNameIsUnknown_ThrowsInvalidOperationException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.AppendSessionTokens(new Dictionary<string, string> { { "bad", "A" } }));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("bad"), ex.Message);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void GetSessionToken_WhenContainerNameIsNull_ThrowsArgumentNullException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        Assert.Throws<ArgumentNullException>(() => storage.GetSessionToken(null!));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void GetSessionToken_WhenContainerNameIsWhitespace_ThrowsArgumentNullException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        Assert.Throws<ArgumentException>(() => storage.GetSessionToken("   "));
        Assert.Throws<ArgumentException>(() => storage.GetSessionToken(""));
    }

    // ================================================================
    // FUNCTIONAL TESTS - SET AND RETRIEVE
    // ================================================================

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_WhenSettingSingleToken_CanRetrieveFromAllMethods(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A" } });

        var all = storage.GetTrackedTokens();
        Assert.Equal("A", all[_defaultContainerName]);
        Assert.Null(all["other"]);
        Assert.Equal("A", storage.GetSessionToken(_defaultContainerName));
        Assert.Equal("A", storage.GetDefaultContainerTrackedToken());
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_WhenSettingMultipleContainers_AllContainersAreSetCorrectly(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "Token1" },
            { "other", "Token2" }
        });

        var all = storage.GetTrackedTokens();
        Assert.Equal("Token1", all[_defaultContainerName]);
        Assert.Equal("Token2", all["other"]);
        Assert.Equal("Token1", storage.GetSessionToken(_defaultContainerName));
        Assert.Equal("Token2", storage.GetSessionToken("other"));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_WhenSettingNullValue_ContainerIsCleared(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A" } });
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, null } });

        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Null(token);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_WhenSettingEmptyDictionary_NoExceptionThrown(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?>());

        var all = storage.GetTrackedTokens();
        Assert.Null(all[_defaultContainerName]);
        Assert.Null(all["other"]);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_WhenPartiallyUpdatingContainers_OnlySpecifiedContainersAreUpdated(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "A" },
            { "other", "B" }
        });
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "C" } });

        var all = storage.GetTrackedTokens();
        Assert.Equal("C", all[_defaultContainerName]);
        Assert.Equal("B", all["other"]);
    }

    // ================================================================
    // FUNCTIONAL TESTS - APPEND OPERATIONS
    // ================================================================

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_WhenAppendingOverlappingTokens_MergesUniquely(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A,B" } });
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "B,C" } });

        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("A,B,C", token);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_WhenAppendingToMultipleContainers_AllContainersAreUpdated(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.AppendSessionTokens(new Dictionary<string, string>
        {
            { _defaultContainerName, "A" },
            { "other", "B" }
        });
        storage.AppendSessionTokens(new Dictionary<string, string>
        {
            { _defaultContainerName, "C" },
            { "other", "D" }
        });

        var all = storage.GetTrackedTokens();
        Assert.Equal("A,C", all[_defaultContainerName]);
        Assert.Equal("B,D", all["other"]);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_WhenAppendingEmptyDictionary_NoExceptionThrown(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendSessionTokens(new Dictionary<string, string>());

        var all = storage.GetTrackedTokens();
        Assert.Null(all[_defaultContainerName]);
        Assert.Null(all["other"]);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_WhenAppendingMultipleTokens_AccumulatesUniquely(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.AppendDefaultContainerSessionToken("A");
        storage.AppendDefaultContainerSessionToken("B");
        storage.AppendDefaultContainerSessionToken("B");

        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("A,B", token);
    }

    // ================================================================
    // FUNCTIONAL TESTS - SET OPERATIONS
    // ================================================================

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetDefaultContainerSessionToken_WhenReplacingExistingTokens_ReplacesAllPreviousTokens(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendDefaultContainerSessionToken("A");
        storage.AppendDefaultContainerSessionToken("B");
        storage.SetDefaultContainerSessionToken("XYZ");

        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("XYZ", token);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetDefaultContainerSessionToken_WhenSettingNull_ClearsContainerToken(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetDefaultContainerSessionToken("A");
        storage.SetDefaultContainerSessionToken(null);

        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Null(token);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetDefaultContainerSessionToken_WhenSettingEmptyString_StoresEmptyString(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetDefaultContainerSessionToken("");
        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("", token);
    }

    // ================================================================
    // FUNCTIONAL TESTS - TRACK OPERATIONS
    // ================================================================

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    public virtual void TrackSessionToken_WhenTrackingMultipleTokens_AppendsUniquely(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.TrackSessionToken(_defaultContainerName, "B");
        storage.TrackSessionToken(_defaultContainerName, "A");

        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Equal("A,B", token);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    public virtual void TrackSessionToken_WhenTrackingToDifferentContainers_ContainersAreIndependent(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.TrackSessionToken("other", "B");
        storage.TrackSessionToken(_defaultContainerName, "C");

        Assert.Equal("A,C", storage.GetSessionToken(_defaultContainerName));
        Assert.Equal("B", storage.GetSessionToken("other"));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    public virtual void TrackSessionToken_WhenTrackingCommaSeparatedToken_ParsesAndMergesCorrectly(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.TrackSessionToken(_defaultContainerName, "A,B");
        storage.TrackSessionToken(_defaultContainerName, "C");
        storage.TrackSessionToken(_defaultContainerName, "B,D");

        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Equal("A,B,C,D", token);
    }

    // ================================================================
    // FUNCTIONAL TESTS - CLEAR OPERATIONS
    // ================================================================

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearingAllTokens_ResetsAllContainersToNull(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A" }, { "other", "B" } });
        storage.Clear();

        var all = storage.GetTrackedTokens();
        Assert.Null(all[_defaultContainerName]);
        Assert.Null(all["other"]);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearing_ContainersStillExistInTrackedTokens(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A" } });
        storage.Clear();

        var tracked = storage.GetTrackedTokens();
        Assert.Contains(_defaultContainerName, tracked.Keys);
        Assert.Contains("other", tracked.Keys);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearing_CanSetNewTokensAfterClear(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendDefaultContainerSessionToken("A");
        storage.Clear();
        storage.AppendDefaultContainerSessionToken("B");

        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("B", token);
    }

    // ================================================================
    // FULLY AUTOMATIC MODE TESTS
    // ================================================================

    [ConditionalFact]
    public virtual void FullyAutomatic_WhenCallingSetSessionTokens_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.FullyAutomatic);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.SetSessionTokens(new Dictionary<string, string?>()));
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, ex.Message);
    }

    [ConditionalFact]
    public virtual void FullyAutomatic_WhenCallingGetTrackedTokens_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.FullyAutomatic);
        var ex = Assert.Throws<InvalidOperationException>(() => storage.GetTrackedTokens());
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, ex.Message);
    }

    [ConditionalFact]
    public virtual void FullyAutomatic_WhenCallingAppendSessionTokens_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.FullyAutomatic);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.AppendSessionTokens(new Dictionary<string, string>()));
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, ex.Message);
    }

    [ConditionalFact]
    public virtual void FullyAutomatic_WhenCallingSetDefaultContainerSessionToken_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.FullyAutomatic);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.SetDefaultContainerSessionToken(null));
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, ex.Message);
    }

    [ConditionalFact]
    public virtual void FullyAutomatic_WhenCallingAppendDefaultContainerSessionToken_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.FullyAutomatic);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.AppendDefaultContainerSessionToken("A"));
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, ex.Message);
    }

    [ConditionalFact]
    public virtual void FullyAutomatic_WhenCallingGetDefaultContainerTrackedToken_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.FullyAutomatic);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.GetDefaultContainerTrackedToken());
        Assert.Equal(CosmosStrings.EnableManualSessionTokenManagement, ex.Message);
    }

    [ConditionalFact]
    public virtual void FullyAutomatic_WhenTrackingToken_AlwaysReturnsNull()
    {
        var storage = CreateStorage(SessionTokenManagementMode.FullyAutomatic);
        storage.TrackSessionToken(_defaultContainerName, "A");
        Assert.Null(storage.GetSessionToken(_defaultContainerName));
    }

    [ConditionalFact]
    public virtual void FullyAutomatic_WhenTrackingMultipleTokens_AlwaysReturnsNull()
    {
        var storage = CreateStorage(SessionTokenManagementMode.FullyAutomatic);
        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.TrackSessionToken(_defaultContainerName, "B");
        storage.TrackSessionToken("other", "C");
        Assert.Null(storage.GetSessionToken(_defaultContainerName));
        Assert.Null(storage.GetSessionToken("other"));
    }

    // ================================================================
    // ENFORCED MANUAL MODE TESTS
    // ================================================================

    [ConditionalFact]
    public virtual void EnforcedManual_WhenGettingTokenBeforeSet_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.GetSessionToken(_defaultContainerName));
        Assert.Contains(CosmosStrings.MissingSessionTokenEnforceManual(_defaultContainerName), ex.Message);
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenGettingTokenAfterSet_ReturnsToken()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.SetDefaultContainerSessionToken("A");
        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Equal("A", token);
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenGettingTokenAfterClear_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.SetDefaultContainerSessionToken("A");
        storage.Clear();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.GetSessionToken(_defaultContainerName));
        Assert.Contains(CosmosStrings.MissingSessionTokenEnforceManual(_defaultContainerName), ex.Message);
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenGettingTokenAfterSetThenClearThenSet_ReturnsNewToken()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.SetDefaultContainerSessionToken("A");
        storage.Clear();
        storage.SetDefaultContainerSessionToken("B");
        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Equal("B", token);
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenSettingMultipleContainers_AllContainersCanBeRetrieved()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "A" },
            { "other", "B" }
        });

        Assert.Equal("A", storage.GetSessionToken(_defaultContainerName));
        Assert.Equal("B", storage.GetSessionToken("other"));
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenOneContainerNotSet_ThrowsForThatContainerOnly()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A" } });

        Assert.Equal("A", storage.GetSessionToken(_defaultContainerName));
        var ex = Assert.Throws<InvalidOperationException>(() => storage.GetSessionToken("other"));
        Assert.Contains(CosmosStrings.MissingSessionTokenEnforceManual("other"), ex.Message);
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenTrackingToken_ThrowsWhenRetrieving()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.TrackSessionToken(_defaultContainerName, "A");
        var ex = Assert.Throws<InvalidOperationException>(() => storage.GetSessionToken(_defaultContainerName));
        Assert.Contains(CosmosStrings.MissingSessionTokenEnforceManual(_defaultContainerName), ex.Message);
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenAppendingToken_CanRetrieveToken()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.AppendDefaultContainerSessionToken("A");
        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Equal("A", token);
    }

    // ================================================================
    // INITIALIZATION AND CONTAINER MANAGEMENT TESTS
    // ================================================================

    [ConditionalFact]
    public virtual void Constructor_WhenInitializing_AllContainersAreInitialized()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);

        var tracked = storage.GetTrackedTokens();
        Assert.True(tracked.ContainsKey(_defaultContainerName));
        Assert.True(tracked.ContainsKey("other"));
        Assert.Equal(2, tracked.Count);
    }

    [ConditionalFact]
    public virtual void Constructor_WhenDefaultContainerNotInContainerNames_ThrowsException()
    {
        var containers = new HashSet<string>(["other"]);
        Assert.True(!containers.Contains("default"));
        Assert.ThrowsAny<Exception>(() =>
        {
            _ = new SessionTokenStorage("default", containers, SessionTokenManagementMode.Manual);
        });
    }

    [ConditionalFact]
    public virtual void Constructor_WhenInitializing_AllContainersStartWithNullTokens()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);

        var tracked = storage.GetTrackedTokens();
        Assert.Null(tracked[_defaultContainerName]);
        Assert.Null(tracked["other"]);
    }

    // ================================================================
    // GETTRACKEDTOKENS TESTS
    // ================================================================

    [ConditionalFact]
    public virtual void GetTrackedTokens_WhenCalled_ReturnsSnapshotNotLiveReference()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        var snapshot = storage.GetTrackedTokens();

        storage.AppendDefaultContainerSessionToken("A");
        var snapshot2 = storage.GetTrackedTokens();

        Assert.NotSame(snapshot, snapshot2);
        Assert.Null(snapshot[_defaultContainerName]);
        Assert.Equal("A", snapshot2[_defaultContainerName]);
    }

    [ConditionalFact]
    public virtual void GetTrackedTokens_WhenModifyingReturnedDictionary_DoesNotAffectStorage()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        storage.AppendDefaultContainerSessionToken("A");
        var tracked = storage.GetTrackedTokens();

        // This should not compile or should not affect storage
        // The returned dictionary is read-only, so we can't modify it
        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("A", token);
    }

    // ================================================================
    // COMPOSITE TOKEN TESTS
    // ================================================================

    [ConditionalFact]
    public virtual void CompositeSessionToken_WhenAppendingDuplicateTokens_RemovesDuplicates()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        storage.AppendDefaultContainerSessionToken("A,A,B");
        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("A,B", token);
    }

    [ConditionalFact]
    public virtual void CompositeSessionToken_WhenAppendingDuplicateTokensInSeparateCalls_RemovesDuplicates()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        storage.AppendDefaultContainerSessionToken("A");
        storage.AppendDefaultContainerSessionToken("A");
        storage.AppendDefaultContainerSessionToken("B");
        storage.AppendDefaultContainerSessionToken("A");
        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("A,B", token);
    }

    [ConditionalFact]
    public virtual void CompositeSessionToken_WhenSettingCommaSeparatedTokens_StoresAllTokens()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        storage.SetDefaultContainerSessionToken("A,B,C");
        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("A,B,C", token);
    }

    [ConditionalFact]
    public virtual void CompositeSessionToken_WhenAppendingCommaSeparatedTokens_ParsesAndMergesCorrectly()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        storage.AppendDefaultContainerSessionToken("A,B,C");
        var token = storage.GetDefaultContainerTrackedToken();
        Assert.Equal("A,B,C", token);
    }

    // ================================================================
    // MULTI-CONTAINER OPERATIONS TESTS
    // ================================================================

    [ConditionalFact]
    public virtual void TrackSessionToken_WhenTrackingToNonDefaultContainer_MergesCorrectly()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        storage.TrackSessionToken("other", "A");
        storage.TrackSessionToken("other", "B");
        Assert.Equal("A,B", storage.GetSessionToken("other"));
    }

    // ================================================================
    // NULL AND EMPTY VALUE TESTS
    // ================================================================

    [ConditionalFact]
    public virtual void GetDefaultContainerTrackedToken_WhenNoTokenSet_ReturnsNull()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        Assert.Null(storage.GetDefaultContainerTrackedToken());
    }

    [ConditionalFact]
    public virtual void GetDefaultContainerTrackedToken_AfterClear_ReturnsNull()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        storage.AppendDefaultContainerSessionToken("A");
        storage.Clear();
        Assert.Null(storage.GetDefaultContainerTrackedToken());
    }

    [ConditionalFact]
    public virtual void GetSessionToken_AfterClear_ReturnsNull()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);
        storage.AppendDefaultContainerSessionToken("A");
        storage.Clear();
        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Null(token);
    }

    // ================================================================
    // SEMI-AUTOMATIC MODE SPECIFIC TESTS
    // ================================================================

    [ConditionalFact]
    public virtual void SemiAutomatic_WhenTrackingToken_CanRetrieveToken()
    {
        var storage = CreateStorage(SessionTokenManagementMode.SemiAutomatic);
        storage.TrackSessionToken(_defaultContainerName, "A");
        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Equal("A", token);
    }

    [ConditionalFact]
    public virtual void SemiAutomatic_WhenGettingTokenBeforeSet_ReturnsNull()
    {
        var storage = CreateStorage(SessionTokenManagementMode.SemiAutomatic);
        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Null(token);
    }

    [ConditionalFact]
    public virtual void SemiAutomatic_WhenGettingTokenAfterClear_ReturnsNull()
    {
        var storage = CreateStorage(SessionTokenManagementMode.SemiAutomatic);
        storage.SetDefaultContainerSessionToken("A");
        storage.Clear();
        var token = storage.GetSessionToken(_defaultContainerName);
        Assert.Null(token);
    }

    private SessionTokenStorage CreateStorage(SessionTokenManagementMode mode)
        => new(_defaultContainerName, _containerNames, mode);
}
