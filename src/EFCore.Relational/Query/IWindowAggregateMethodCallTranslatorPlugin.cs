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
public interface IWindowAggregateMethodCallTranslatorPlugin
{
    /// <summary>
    ///     Gets the method call translators.
    /// </summary>
    IEnumerable<IWindowAggregateMethodCallTranslator> Translators { get; }
}
