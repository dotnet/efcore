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
public static class SqlServerWindowAggregateFunctionExtensions
{
   

    /// <summary>
    /// todo
    /// </summary>
    /// <param name="final">todo</param>
    /// <returns>todo</returns>
    /// <exception cref="Exception">todo</exception>
    public static long CountBig(this IWindowFinal final)
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
    public static long? CountBig(this IWindowFinal final, Func<bool> filter)
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
    public static long CountBig<TSource>(this IWindowFinal final, TSource source)
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
    public static long? CountBig<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
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
    public static double? Stdev<TSource>(this IWindowFinal final, TSource source)
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
    public static double? Stdev<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
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
    public static double? StdevP<TSource>(this IWindowFinal final, TSource source)
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
    public static double? StdevP<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
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
    public static double? Var<TSource>(this IWindowFinal final, TSource source)
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
    public static double? Var<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
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
    public static double? VarP<TSource>(this IWindowFinal final, TSource source)
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
    public static double? VarP<TSource>(this IWindowFinal final, TSource source, Func<bool> filter)
    {
        throw new Exception();
    }
}
