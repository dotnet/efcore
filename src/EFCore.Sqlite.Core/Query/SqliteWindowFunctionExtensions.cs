// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query;

/// <summary>
/// todo
/// </summary>
public static class SqliteWindowFunctionExtensions
{
    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Range(this IFrame frame, int preceding, int following)
        => throw new NotImplementedException();

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Range(this IFrame frame, RowsPreceding preceding, int following)
        => throw new NotImplementedException();

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Range(this IFrame frame, int preceding, RowsFollowing following)
        => throw new NotImplementedException();



    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Groups(this IFrame frame, int preceding)
        => throw new NotImplementedException();

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Groups(this IFrame frame, RowsPreceding preceding)
        => throw new NotImplementedException();

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Groups(this IFrame frame, int preceding, int following)
        => throw new NotImplementedException();


    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Groups(this IFrame frame, RowsPreceding preceding, int following)
        => throw new NotImplementedException();

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Groups(this IFrame frame, int preceding, RowsFollowing following)
        => throw new NotImplementedException();

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="preceding">todo</param>
    /// <param name="following">todo</param>
    /// <returns>todo</returns>
    public static IFrameResults Groups(this IFrame frame, RowsPreceding preceding, RowsFollowing following)
        => throw new NotImplementedException();

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="frame">todo</param>
    /// <param name="frameExclude">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="NotImplementedException">todo</exception>
    public static IWindowFinal Exclude(this IFrameResults frame, FrameExclude frameExclude)
        => throw new NotImplementedException();
}
