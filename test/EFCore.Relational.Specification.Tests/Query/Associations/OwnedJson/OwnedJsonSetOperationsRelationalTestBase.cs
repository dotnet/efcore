// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// All set operation tests fail over owned JSON collections:
// System.Collections.Generic.KeyNotFoundException : The given key 'Property: RootEntity.RelatedCollection#RelatedType.__synthesizedOrdinal (no field, int) Shadow Required PK AfterSave:Throw ValueGenerated.OnAdd' was not present in the dictionary.
//   at System.Collections.Generic.Dictionary`2.get_Item(TKey key)
//   at Microsoft.EntityFrameworkCore.Query.StructuralTypeProjectionExpression.BindProperty(IProperty property) in /Users/roji/projects/efcore/src/EFCore.Relational/Query/StructuralTypeProjectionExpression.cs:line 365


