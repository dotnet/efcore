// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class MiscellaneousTranslationsRelationalTestBase<TFixture>(TFixture fixture) : MiscellaneousTranslationsTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    public override Task Random_Shared_Next_with_no_args(bool async)
        => AssertTranslationFailed(() => base.Random_Shared_Next_with_no_args(async));

    public override Task Random_Shared_Next_with_one_arg(bool async)
        => AssertTranslationFailed(() => base.Random_Shared_Next_with_one_arg(async));

    public override Task Random_Shared_Next_with_two_args(bool async)
        => AssertTranslationFailed(() => base.Random_Shared_Next_with_two_args(async));

    public override Task Random_new_Next_with_no_args(bool async)
        => AssertTranslationFailed(() => base.Random_new_Next_with_no_args(async));

    public override Task Random_new_Next_with_one_arg(bool async)
        => AssertTranslationFailed(() => base.Random_new_Next_with_one_arg(async));

    public override Task Random_new_Next_with_two_args(bool async)
        => AssertTranslationFailed(() => base.Random_new_Next_with_two_args(async));
}
