// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
/// todo
/// </summary>
public enum RowsPreceding
{
    /// <summary>
    /// todo
    /// </summary>
    CurrentRow,

    /// <summary>
    /// todo
    /// </summary>
    UnboundedPreceding
}

/// <summary>
/// todo
/// </summary>
public enum RowsFollowing
{
    /// <summary>
    /// todo
    /// </summary>
    CurrentRow,

    /// <summary>
    /// todo
    /// </summary>
    UnboundedFollowing
}

/// <summary>
/// todo
/// </summary>
public enum FrameExclude
{
    /// <summary>
    /// todo
    /// </summary>
    NoOthers,

    /// <summary>
    /// todo
    /// </summary>
    CurrentRow,

    /// <summary>
    /// todo
    /// </summary>
    Group,

    /// <summary>
    /// todo
    /// </summary>
    Ties
}

/// <summary>
/// todo
/// </summary>
public interface IOver : IOrderRoot, IWindowFinal
{
    /// <summary>
    /// todo
    /// </summary>
    /// <returns>todo</returns>
    IPartition PartitionBy(params object[] partitions);
}

/// <summary>
/// todo
/// </summary>
public interface IPartition : IOrderRoot, IWindowFinal
{
}

/// <summary>
/// todo
/// </summary>
public interface IOrderRoot
{
    /// <summary>
    /// todo
    /// </summary>
    /// <returns>todo</returns>
    IOrderThen OrderBy(object orderBy);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="orderBy">todo</param>
    /// <returns>todo</returns>
    IOrderThen OrderByDescending(object orderBy);
}

/// <summary>
/// todo
/// </summary>
public interface IOrderThen : IFrame, IWindowFinal
{
    /// <summary>
    /// todo
    /// </summary>
    /// <param name="orderBy">todo</param>
    /// <returns>todo</returns>
    IOrderThen ThenBy(object orderBy);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="orderBy">todo</param>
    /// <returns>todo</returns>
    IOrderThen ThenByDescending(object orderBy);
}

/// <summary>
/// todo
/// </summary>
public interface IFrame
{
    /// <summary>
    /// todo
    /// </summary>
    /// <param name="preceding">todo</param>
    IFrameResults Rows(int preceding);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="preceding">todo</param>
    IFrameResults Rows(RowsPreceding preceding);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    IFrameResults Rows(int preceding, int following);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    IFrameResults Rows(RowsPreceding preceding, int following);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    IFrameResults Rows(int preceding, RowsFollowing following);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    IFrameResults Rows(RowsPreceding preceding, RowsFollowing following);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="preceding">todo</param>
    IFrameResults Range(RowsPreceding preceding);

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    IFrameResults Range(RowsPreceding preceding, RowsFollowing following);
}

/// <summary>
/// todo
/// </summary>
public interface IFrameResults : IWindowFinal
{ }

/// <summary>
/// todo
/// </summary>
public interface IWindowFinal
{
}
