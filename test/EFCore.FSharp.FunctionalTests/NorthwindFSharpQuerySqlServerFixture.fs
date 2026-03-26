// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.FSharp.FunctionalTests

open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Query

type NorthwindFSharpQuerySqlServerFixture<'TModelCustomizer
    when 'TModelCustomizer : (new: unit -> 'TModelCustomizer)
    and 'TModelCustomizer :> ITestModelCustomizer>() =
    inherit NorthwindQuerySqlServerFixture<'TModelCustomizer>()

    override self.StoreName = "NorthwindFSharp"
