// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

internal class SqlServerWindowAggregateMethods
{
    static SqlServerWindowAggregateMethods()
    {
        var aggMethods = typeof(SqlServerWindowAggregateFunctionExtensions).GetMethods().Where(mi => typeof(IWindowFinal).IsAssignableFrom(mi.GetParameters().FirstOrDefault()?.ParameterType)).ToList();

        CountBigAll = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.CountBig) && m.GetParameters().Length == 1);
        CountBigAllFilter = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.CountBig) && m.GetParameters().Length == 2 && typeof(Func<bool>).IsAssignableFrom(m.GetParameters()[1].ParameterType));

        CountBigCol = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.CountBig) && m.GetParameters().Length == 2 && !typeof(Func<bool>).IsAssignableFrom(m.GetParameters()[1].ParameterType));
        CountBigColFilter = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.CountBig) && m.GetParameters().Length == 3);

        Stdev = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.Stdev) && m.GetParameters().Length == 2);
        StdevFilter = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.Stdev) && m.GetParameters().Length == 3);

        StdevP = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.StdevP) && m.GetParameters().Length == 2);
        StdevPFilter = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.StdevP) && m.GetParameters().Length == 3);

        Var = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.Var) && m.GetParameters().Length == 2);
        VarFilter = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.Var) && m.GetParameters().Length == 3);

        VarP = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.VarP) && m.GetParameters().Length == 2);
        VarPFilter = aggMethods.Single(m => m.Name == nameof(SqlServerWindowAggregateFunctionExtensions.VarP) && m.GetParameters().Length == 3);
    }

    public static MethodInfo CountBigAll { get; }
    public static MethodInfo CountBigAllFilter { get; }

    public static MethodInfo CountBigCol { get; }
    public static MethodInfo CountBigColFilter { get; }

    public static MethodInfo Stdev { get; }
    public static MethodInfo StdevFilter { get; }

    public static MethodInfo StdevP { get; }
    public static MethodInfo StdevPFilter { get; }

    public static MethodInfo Var { get; }
    public static MethodInfo VarFilter { get; }

    public static MethodInfo VarP { get; }
    public static MethodInfo VarPFilter { get; }
}
