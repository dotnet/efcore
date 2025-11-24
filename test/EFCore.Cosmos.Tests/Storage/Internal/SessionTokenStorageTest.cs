// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

public class SessionTokenStorageTest
{
    private readonly string _defaultContainerName = "default";
    private readonly string _otherContainerName = "other";
    private readonly HashSet<string> _containerNames = new(["default", "other"]);

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_SetSingle_Default(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A" } });

        AssertDefault(storage, "A");
        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_SetSingle_Other(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.SetSessionTokens(new Dictionary<string, string?> { { _otherContainerName, "A" } });

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertDefault(storage, "");
            }
            else
            {
                AssertDefault(storage, null);
            }
        }
        AssertOther(storage, "A");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_Multiple(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "Token1" },
            { _otherContainerName, "Token2" }
        });

        AssertDefault(storage, "Token1");
        AssertOther(storage, "Token2");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_OverwritesSet(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "A" },
            { _otherContainerName, "B" }
        });
        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "" },
            { _otherContainerName, "" }
        });

        AssertDefault(storage, "");
        AssertOther(storage, "");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_OverwritesTracked(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.TrackSessionToken(_defaultContainerName, "Token1");
        storage.TrackSessionToken(_otherContainerName, "Token2");
        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "A" },
            { _otherContainerName, "B" }
        });

        AssertDefault(storage, "A");
        AssertOther(storage, "B");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_SingleContainer_OverwritesOnlySingleContainer(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "A" },
            { _otherContainerName, "B" }
        });
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "C" } });

        AssertDefault(storage, "C");
        AssertOther(storage, "B");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_Null_SetsNull(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A" }, { _otherContainerName, "B" } });
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, null } });

        AssertDefault(storage, null);
        AssertOther(storage, "B");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetDefaultContainerSessionToken_SetsToken(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetDefaultContainerSessionToken("A");

        AssertDefault(storage, "A");

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetDefaultContainerSessionToken_OverwritesSet(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetDefaultContainerSessionToken("A");
        storage.SetDefaultContainerSessionToken("B");

        AssertDefault(storage, "B");

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetDefaultContainerSessionToken_OverwritesTracked(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.SetDefaultContainerSessionToken("B");

        AssertDefault(storage, "B");
        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_NoPreviousToken_SetsToken(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendDefaultContainerSessionToken("A");

        AssertDefault(storage, "A");

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_PreviousSetToken_AppendsToken(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetDefaultContainerSessionToken("A");
        storage.AppendDefaultContainerSessionToken("B");

        AssertDefault(storage, "A,B");

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_PreviousSetToken_Duplicate_DoesNotAppendToken(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetDefaultContainerSessionToken("A");
        storage.AppendDefaultContainerSessionToken("A");

        AssertDefault(storage, "A");
        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_PreviousTrackedToken_AppendsToken(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.AppendDefaultContainerSessionToken("B");

        AssertDefault(storage, "A,B");
        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_PreviousTrackedToken_Duplicate_DoesNotAppendToken(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.AppendDefaultContainerSessionToken("A");

        AssertDefault(storage, "A");
        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertOther(storage, "");
            }
            else
            {
                AssertOther(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_MultipleContainers_NoPreviousTokens_SetsTokens(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.AppendSessionTokens(new Dictionary<string, string>
        {
            { _defaultContainerName, "A" },
            { _otherContainerName, "B" }
        });

        AssertDefault(storage, "A");
        AssertOther(storage, "B");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_SingleContainer_NoPreviousTokens_SetsTokens(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.AppendSessionTokens(new Dictionary<string, string>
        {
            { _otherContainerName, "B" }
        });

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertDefault(storage, "");
            }
            else
            {
                AssertDefault(storage, null);
            }
        }
        AssertOther(storage, "B");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_PreviousSetTokens_AppendsTokens(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "A" },
            { _otherContainerName, "B" }
        });
        storage.AppendSessionTokens(new Dictionary<string, string>
        {
            { _defaultContainerName, "C" },
            { _otherContainerName, "D" }
        });

        AssertDefault(storage, "A,C");
        AssertOther(storage, "B,D");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_PreviousSetToken_AppendsAndSetsTokens(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _otherContainerName, "B" }
        });
        storage.AppendSessionTokens(new Dictionary<string, string>
        {
            { _defaultContainerName, "C" },
            { _otherContainerName, "D" }
        });

        AssertDefault(storage, "C");
        AssertOther(storage, "B,D");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_PreviousTrackedToken_AppendsAndSetsTokens(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.TrackSessionToken(_otherContainerName, "B");
        storage.AppendSessionTokens(new Dictionary<string, string>
        {
            { _defaultContainerName, "C" },
            { _otherContainerName, "D" }
        });

        AssertDefault(storage, "C");
        AssertOther(storage, "B,D");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetDefaultContainerSessionToken_RemovesDuplicates(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetDefaultContainerSessionToken("A,A,B");
        AssertDefault(storage, "A,B");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_RemovesDuplicates(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendDefaultContainerSessionToken("A,A,B");
        AssertDefault(storage, "A,B");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetSessionTokens_RemovesDuplicates(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A,B,A" }, { _otherContainerName, "B,C,B" } });
        AssertDefault(storage, "A,B");
        AssertOther(storage, "B,C");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_RemovesDuplicates(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A,B,A" }, { _otherContainerName, "B,C,B" } });
        AssertDefault(storage, "A,B");
        AssertOther(storage, "B,C");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_PreviouslySetTokens_RemovesDuplicates(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A,C,E" }, { _otherContainerName, "J,K,L" } });
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A,B,B" }, { _otherContainerName, "K,A,A" } });
        AssertDefault(storage, "A,C,E,B");
        AssertOther(storage, "J,K,L,A");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_EmptyStrings_DoesNotAppend(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A,C" }, { _otherContainerName, "J,K" } });
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "" }, { _otherContainerName, "" } });
        AssertDefault(storage, "A,C");
        AssertOther(storage, "J,K");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendSessionTokens_NoPreviousTokens_EmptyStrings_Sets(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "" }, { _otherContainerName, "" } });
        AssertDefault(storage, "");
        AssertOther(storage, "");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void TrackSessionToken_SetsToken(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.TrackSessionToken(_otherContainerName, "A");

        AssertDefaultTracked(storage, "A");
        AssertOtherTracked(storage, "A");

        if (mode == SessionTokenManagementMode.Manual)
        {
            AssertDefaultUsed(storage, "A");
            AssertOtherUsed(storage, "A");
        }
        else if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertDefaultUsed(storage, "");
                AssertOtherUsed(storage, "");
            }
            else
            {
                AssertDefaultUsed(storage, null);
                AssertOtherUsed(storage, null);
            }
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void TrackSessionToken_Appends(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.TrackSessionToken(_defaultContainerName, "B");
        storage.TrackSessionToken(_defaultContainerName, "A");

        storage.TrackSessionToken(_otherContainerName, "A");
        storage.TrackSessionToken(_otherContainerName, "C");
        storage.TrackSessionToken(_otherContainerName, "A");

        AssertDefaultTracked(storage, "A,B");
        AssertOtherTracked(storage, "A,C");

        if (mode == SessionTokenManagementMode.Manual)
        {
            AssertDefaultUsed(storage, "A,B");
            AssertOtherUsed(storage, "A,C");
        }
        else if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            if (mode == SessionTokenManagementMode.Manual)
            {
                AssertDefaultUsed(storage, "");
                AssertOtherUsed(storage, "");
            }
            else
            {
                AssertDefaultUsed(storage, null);
                AssertOtherUsed(storage, null);
            }
        }
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenGettingTokenBeforeSet_ThrowsInvalidOperationException()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.GetSessionToken(_defaultContainerName));
        Assert.Contains(CosmosStrings.MissingSessionTokenEnforceManual(_defaultContainerName), ex.Message);
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
    public virtual void EnforcedManual_SetDefaultContainerSessionToken_SetsAndUses()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.SetDefaultContainerSessionToken("A");
        AssertDefault(storage, "A");
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.GetSessionToken(_otherContainerName));
        Assert.Contains(CosmosStrings.MissingSessionTokenEnforceManual(_otherContainerName), ex.Message);
    }

    [ConditionalFact]
    public virtual void EnforcedManual_SetSessionTokens_SetsAndUses()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.SetSessionTokens(new Dictionary<string, string?>
        {
            { _defaultContainerName, "A" },
            { _otherContainerName, "B" }
        });

        AssertDefault(storage, "A");
        AssertOther(storage, "B");
    }

    [ConditionalFact]
    public virtual void EnforcedManual_WhenOneContainerNotSet_ThrowsForThatContainerOnly()
    {
        var storage = CreateStorage(SessionTokenManagementMode.EnforcedManual);
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A" } });

        Assert.Equal("A", storage.GetSessionToken(_defaultContainerName));
        var ex = Assert.Throws<InvalidOperationException>(() => storage.GetSessionToken(_otherContainerName));
        Assert.Contains(CosmosStrings.MissingSessionTokenEnforceManual(_otherContainerName), ex.Message);
    }

    [ConditionalFact]
    public virtual void SemiAutomatic_WhenTrackingToken_SetsButDoesnotUseToken()
    {
        var storage = CreateStorage(SessionTokenManagementMode.SemiAutomatic);
        storage.TrackSessionToken(_defaultContainerName, "A");
        AssertDefaultTracked(storage, "A");
        AssertDefaultUsed(storage, null);
    }

    [ConditionalFact]
    public virtual void SemiAutomatic_WhenSetToken_SetsAndUses()
    {
        var storage = CreateStorage(SessionTokenManagementMode.SemiAutomatic);
        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.AppendDefaultContainerSessionToken("A");
        AssertDefault(storage, "A");
    }

    [ConditionalFact]
    public virtual void Manual_TrackedToken_UsesToken()
    {
        var storage = CreateStorage(SessionTokenManagementMode.Manual);

        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.TrackSessionToken(_otherContainerName, "B");

        Assert.True(storage.GetSessionToken(_defaultContainerName) == "A");
        Assert.True(storage.GetSessionToken(_otherContainerName) == "B");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Manual_Constructor_AllContainersHaveEmptyString(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        var tokens = storage.GetTrackedTokens();
        Assert.True(tokens[_defaultContainerName] == "");
        Assert.True(tokens[_otherContainerName] == "");
        Assert.True(storage.GetDefaultContainerTrackedToken() == "");

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            Assert.True(storage.GetSessionToken(_defaultContainerName) == "");
            Assert.True(storage.GetSessionToken(_otherContainerName) == "");
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Manual_Clear_ResetsAllContainersToEmptyString(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A" }, { _otherContainerName, "B" } });
        storage.Clear();

        var tokens = storage.GetTrackedTokens();
        Assert.True(tokens[_defaultContainerName] == "");
        Assert.True(tokens[_otherContainerName] == "");
        Assert.True(storage.GetDefaultContainerTrackedToken() == "");
        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            Assert.True(storage.GetSessionToken(_defaultContainerName) == "");
            Assert.True(storage.GetSessionToken(_otherContainerName) == "");
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearing_ContainersStillExistInTrackedTokens(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.Clear();

        var tokens = storage.GetTrackedTokens();
        Assert.Contains(_defaultContainerName, tokens.Keys);
        Assert.Contains(_otherContainerName, tokens.Keys);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearingSetTokens_ResetsAllContainers(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A" }, { _otherContainerName, "B" } });
        storage.Clear();
        
        var tokens = storage.GetTrackedTokens();

        if (mode == SessionTokenManagementMode.Manual || mode == SessionTokenManagementMode.EnforcedManual)
        {
            Assert.True(tokens[_defaultContainerName] == "");
            Assert.True(tokens[_otherContainerName] == "");
            Assert.True(storage.GetDefaultContainerTrackedToken() == "");

            if (mode != SessionTokenManagementMode.EnforcedManual)
            {
                Assert.True(storage.GetSessionToken(_defaultContainerName) == "");
                Assert.True(storage.GetSessionToken(_otherContainerName) == "");
            }
        }
        else
        {
            Assert.Null(tokens[_defaultContainerName]);
            Assert.Null(tokens[_otherContainerName]);
            Assert.Null(storage.GetDefaultContainerTrackedToken());

            Assert.Null(storage.GetSessionToken(_defaultContainerName));
            Assert.Null(storage.GetSessionToken(_otherContainerName));
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearingTrackedTokens_ResetsAllContainers(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        storage.TrackSessionToken(_defaultContainerName, "A");
        storage.TrackSessionToken(_otherContainerName, "B");
        storage.Clear();

        var tokens = storage.GetTrackedTokens();

        if (mode == SessionTokenManagementMode.Manual || mode == SessionTokenManagementMode.EnforcedManual)
        {
            Assert.True(tokens[_defaultContainerName] == "");
            Assert.True(tokens[_otherContainerName] == "");
            Assert.True(storage.GetDefaultContainerTrackedToken() == "");

            if (mode != SessionTokenManagementMode.EnforcedManual)
            {
                Assert.True(storage.GetSessionToken(_defaultContainerName) == "");
                Assert.True(storage.GetSessionToken(_otherContainerName) == "");
            }
        }
        else
        {
            Assert.Null(tokens[_defaultContainerName]);
            Assert.Null(tokens[_otherContainerName]);
            Assert.Null(storage.GetDefaultContainerTrackedToken());

            Assert.Null(storage.GetSessionToken(_defaultContainerName));
            Assert.Null(storage.GetSessionToken(_otherContainerName));
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearing_CanSetNewTokensAfterClear(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "A" }, { _otherContainerName, "B" } });

        storage.Clear();
        storage.SetSessionTokens(new Dictionary<string, string?> { { _defaultContainerName, "C" }, { _otherContainerName, "D" } });

        var expectedDefault = "C";
        var expectedOther = "D";
        var defaultContainerTrackedToken = storage.GetDefaultContainerTrackedToken();
        var tokens = storage.GetTrackedTokens();

        Assert.Equal(expectedDefault, defaultContainerTrackedToken);

        Assert.Equal(expectedDefault, tokens[_defaultContainerName]);
        Assert.Equal(expectedOther, tokens[_otherContainerName]);

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            Assert.Equal(expectedDefault, storage.GetSessionToken(_defaultContainerName));
            Assert.Equal(expectedOther, storage.GetSessionToken(_otherContainerName));
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearing_CanAppendNewTokensAfterClear(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A" }, { _otherContainerName, "B" } });

        storage.Clear();
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "C" }, { _otherContainerName, "D" } });

        var expectedDefault = "C";
        var expectedOther = "D";
        var defaultContainerTrackedToken = storage.GetDefaultContainerTrackedToken();
        var tokens = storage.GetTrackedTokens();

        Assert.Equal(expectedDefault, defaultContainerTrackedToken);

        Assert.Equal(expectedDefault, tokens[_defaultContainerName]);
        Assert.Equal(expectedOther, tokens[_otherContainerName]);

        if (mode != SessionTokenManagementMode.EnforcedManual)
        {
            Assert.Equal(expectedDefault, storage.GetSessionToken(_defaultContainerName));
            Assert.Equal(expectedOther, storage.GetSessionToken(_otherContainerName));
        }
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Clear_WhenClearing_CanTrackNewTokensAfterClear(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        storage.AppendSessionTokens(new Dictionary<string, string> { { _defaultContainerName, "A" }, { _otherContainerName, "B" } });

        storage.Clear();

        storage.TrackSessionToken(_defaultContainerName, "C");
        storage.TrackSessionToken(_otherContainerName, "D");

        AssertDefaultTracked(storage, "C");
        AssertOtherTracked(storage, "D");
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void Constructor_AllContainersAreInitialized(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);

        var tokens = storage.GetTrackedTokens();
        Assert.Equal(2, tokens.Count);
        Assert.True(tokens.ContainsKey(_defaultContainerName));
        Assert.True(tokens.ContainsKey(_otherContainerName));
    }

    [ConditionalFact]
    public virtual void Constructor_WhenInitializing_AllContainersStartWithNullTokens()
    {
        var storage = CreateStorage(SessionTokenManagementMode.SemiAutomatic);

        var tokens = storage.GetTrackedTokens();
        Assert.Null(tokens[_defaultContainerName]);
        Assert.Null(tokens[_otherContainerName]);
        Assert.Null(storage.GetDefaultContainerTrackedToken());
        Assert.Null(storage.GetSessionToken(_defaultContainerName));
        Assert.Null(storage.GetSessionToken(_otherContainerName));
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void GetTrackedTokens_WhenCalled_ReturnsSnapshotNotLiveReference(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        var snapshot = storage.GetTrackedTokens();

        storage.AppendDefaultContainerSessionToken("A");
        var snapshot2 = storage.GetTrackedTokens();

        Assert.NotSame(snapshot, snapshot2);
        if (mode == SessionTokenManagementMode.Manual || mode == SessionTokenManagementMode.EnforcedManual)
        {
            Assert.True(snapshot[_defaultContainerName] == "");
        }
        else
        {
            Assert.Null(snapshot[_defaultContainerName]);
        }
        Assert.Equal("A", snapshot2[_defaultContainerName]);
    }

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
        storage.TrackSessionToken(_otherContainerName, "C");
        Assert.Null(storage.GetSessionToken(_defaultContainerName));
        Assert.Null(storage.GetSessionToken(_otherContainerName));
    }

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
    public virtual void SetSessionTokens_WhenContainerNameIsUnknown_ThrowsInvalidOperationException(SessionTokenManagementMode mode)
    {
        var storage = CreateStorage(mode);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.SetSessionTokens(new Dictionary<string, string?> { { "bad", "A" } }));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("bad"), ex.Message);
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

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void SetDefaultContainerSessionToken_NotInUse_ThrowsInvalidOperationException(SessionTokenManagementMode mode)
    {
        var storage = new SessionTokenStorage("bad", _containerNames, mode);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.SetDefaultContainerSessionToken("A"));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("bad"), ex.Message);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void AppendDefaultContainerSessionToken_NotInUse_ThrowsInvalidOperationException(SessionTokenManagementMode mode)
    {
        var storage = new SessionTokenStorage("bad", _containerNames, mode);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.AppendDefaultContainerSessionToken("A"));
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("bad"), ex.Message);
    }

    [ConditionalTheory]
    [InlineData(SessionTokenManagementMode.SemiAutomatic)]
    [InlineData(SessionTokenManagementMode.Manual)]
    [InlineData(SessionTokenManagementMode.EnforcedManual)]
    public virtual void GetDefaultContainerTrackedToken_NotInUse_ThrowsInvalidOperationException(SessionTokenManagementMode mode)
    {
        var storage = new SessionTokenStorage("bad", _containerNames, mode);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            storage.GetDefaultContainerTrackedToken());
        Assert.Equal(CosmosStrings.ContainerNameDoesNotExist("bad"), ex.Message);
    }


    private SessionTokenStorage CreateStorage(SessionTokenManagementMode mode)
        => new(_defaultContainerName, _containerNames, mode);

    private void AssertDefault(SessionTokenStorage storage, string? value)
    {
        AssertDefaultTracked(storage, value);
        AssertDefaultUsed(storage, value);
    }

    private void AssertDefaultUsed(SessionTokenStorage storage, string? value)
    {
        if (value == null)
        {
            Assert.Null(storage.GetSessionToken(_defaultContainerName));
        }
        else
        {
            Assert.Equal(value, storage.GetSessionToken(_defaultContainerName));
        }
    }

    private void AssertDefaultTracked(SessionTokenStorage storage, string? value)
    {
        if (value == null)
        {
            Assert.Null(storage.GetDefaultContainerTrackedToken());
            Assert.Null(storage.GetTrackedTokens()[_defaultContainerName]);
        }
        else
        {
            Assert.Equal(value, storage.GetDefaultContainerTrackedToken());
            Assert.Equal(value, storage.GetTrackedTokens()[_defaultContainerName]);
        }
    }

    private void AssertOther(SessionTokenStorage storage, string? value)
    {
        AssertOtherTracked(storage, value);
        AssertOtherUsed(storage, value);
    }

    private void AssertOtherUsed(SessionTokenStorage storage, string? value)
    {
        if (value == null)
        {
            Assert.Null(storage.GetSessionToken(_otherContainerName));
        }
        else
        {
            Assert.Equal(value, storage.GetSessionToken(_otherContainerName));
        }
    }

    private void AssertOtherTracked(SessionTokenStorage storage, string? value)
    {
        if (value == null)
        {
            Assert.Null(storage.GetTrackedTokens()[_otherContainerName]);
        }
        else
        {
            Assert.Equal(value, storage.GetTrackedTokens()[_otherContainerName]);
        }
    }

}
