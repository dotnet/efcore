// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class MiscellaneousTranslationsRelationalTestBase<TFixture>(TFixture fixture)
    : MiscellaneousTranslationsTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    public override Task Random_Shared_Next_with_no_args()
        => AssertTranslationFailed(() => base.Random_Shared_Next_with_no_args());

    public override Task Random_Shared_Next_with_one_arg()
        => AssertTranslationFailed(() => base.Random_Shared_Next_with_one_arg());

    public override Task Random_Shared_Next_with_two_args()
        => AssertTranslationFailed(() => base.Random_Shared_Next_with_two_args());

    public override Task Random_new_Next_with_no_args()
        => AssertTranslationFailed(() => base.Random_new_Next_with_no_args());

    public override Task Random_new_Next_with_one_arg()
        => AssertTranslationFailed(() => base.Random_new_Next_with_one_arg());

    public override Task Random_new_Next_with_two_args()
        => AssertTranslationFailed(() => base.Random_new_Next_with_two_args());
}
