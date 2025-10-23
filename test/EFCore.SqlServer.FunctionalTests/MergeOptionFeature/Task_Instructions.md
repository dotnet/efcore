## Task list

### Test for made:
- Existing entries in all states -ok 
- Unchanged entries with original value set to something that doesn't match the database state - ok
- Modified entries with properties marked as modified, but with the original value set to something that matches the database state - ok
- Owned entity that was replaced by a different instance, so it's tracked as both Added and Deleted - OK
- A derived entity that was replaced by a base entity with same key value - OK
- Different terminating operators: ToList, FirstOrDefault, etc... - OK
- Streaming (non-buffering) query that's consumed one-by-one - OK
- Queries with Include, Include with filter and ThenInclude - OK
- Projecting a related entity in Select without Include - OK
- Creating a new instance of the target entity in Select with calculated values that are going to be client-evaluated - OK
- Projecting an entity multiple times in Select with same key, but different property values - OK
- Lazy-loading proxies with navigations in loaded and unloaded states - OK
- Non-tracking queries should throw - OK
- Multiple Refresh with different values in the same query should throw

### The test model should incorporate the following features:
- Collection and non-collection owned types
- Collection and non-collection complex properties of value and reference types
- Many-to-many relationships without an explicit join type
- Global query filters
- Shadow and non-shadow properties
- Properties mapped to computed columns
- Properties with value converters
- Primitive collections
- Table-sharing with shared non-key columns


https://github.com/dotnet/efcore/pull/36556