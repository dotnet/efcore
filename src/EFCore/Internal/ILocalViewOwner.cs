// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     Implemented by sets that may hold a <see cref="ChangeTracking.LocalView{TEntity}" />, so that the state manager can notify the view
///     when it stops tracking all entities without resetting the context.
/// </summary>
internal interface ILocalViewOwner
{
    void OnStateManagerCleared();
}
