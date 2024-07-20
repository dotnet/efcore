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
public static class RelationalWindowAggregateFunctionExtensions
{
    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Average<TSource>(this IWindowFinal final, TSource source)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <param name="filter">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Average<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static int? Count(this IWindowFinal final)
    {
        throw new Exception();
    }


    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <param name="filter">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static int? Count(this IWindowFinal final, Func<bool> filter)
    {
        throw new Exception();
    }


    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static int? Count<TSource>(this IWindowFinal final, TSource source)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <param name="filter">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static int? Count<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
    {
        throw new Exception();
    }


    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static double CumeDist(this IWindowFinal final)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static long DenseRank(this IOrderThen final)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource FirstValue<TSource>(this IOrderThen final, TSource source)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource FirstValue<TSource>(this IFrameResults final, TSource source)
    {
        //todo - how do we force this to include an order by?  
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <param name="offset">todo</param>
    /// <param name="defaultValue">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Lag<TSource>(this IOrderThen final, TSource source, int offset, TSource defaultValue)
    {
        //todo - how do we force this to include an order by?  
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource LastValue<TSource>(this IOrderThen final, TSource source)
    {
        //todo - how do we force this to include an order by?  
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource LastValue<TSource>(this IFrameResults final, TSource source)
    {
        //todo - how do we force this to include an order by?  
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <param name="offset">todo</param>
    /// <param name="defaultValue">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Lead<TSource>(this IOrderThen final, TSource source, int offset, TSource defaultValue)
    {
        //todo - how do we force this to include an order by?  
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Max<TSource>(this IWindowFinal final, TSource source)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <param name="filter">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Max<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Min<TSource>(this IWindowFinal final, TSource source)
    {
        throw new Exception();
    }


    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <param name="filter">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Min<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <param name="numberOfGroups">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static long NTile(this IOrderThen final, int numberOfGroups)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static double PercentRank(this IOrderThen final)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static long Rank(this IOrderThen final)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static long RowNumber(this IOrderThen final)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Sum<TSource>(this IWindowFinal final, TSource source)
    {
        throw new Exception();
    }

    /// <summary>
    /// todo
    /// </summary>
    /// <typeparam name="TSource">todo</typeparam>
    /// <param name="final">todo</param>
    /// <param name="source">todo</param>
    /// <param name="filter">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static TSource? Sum<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
    {
        throw new Exception();
    }
}
