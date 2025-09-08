// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Represents the most common JSON change tracking options. Set options either globally with the
    /// `UseJsonChangeTrackingOptions` extension method when calling `XGDbContextOptionsBuilder.UseMicrosoftJson()`
    /// or `XGDbContextOptionsBuilder.UseNewtonsoftJson()`, or for a specific model property when calling
    /// `PropertyBuilder.UseJsonChangeTrackingOptions()`.
    /// The default is `RootPropertyOnly`, resulting in the best performance but only limited change tracking support.
    /// Comparisons will use appropriate `Equals()` methods (including `IEquatablel&lt;T&gt;` implementations) or overloaded
    /// `==` operators.
    /// </summary>
    [Flags]
    public enum XGCommonJsonChangeTrackingOptions
    {
        /// <summary>
        /// Do not track changes inside of JSON column mapped properties but only for the root property itself.
        /// For example, if the JSON mapped property is a top level array of `int`, then changes to items of the
        /// array are not tracked, but changes to the array property itself (the reference) are.
        /// </summary>
        RootPropertyOnly,

        /// <summary>
        /// Track all changes in the JSON column mapped property (DOM, POCOs and `string`). This is the fasted option
        /// with full change tracking support and should be used in most cases where full change tracking support is
        /// required.
        /// If the JSON column mapped property implements a `Clone()` or `DeepClone()` method, it is called in snapshot
        /// generation and assumed to clone the full hierarchy.
        /// If the JSON column mapped property implements an `Equals(T, T)` method (e.g. for `IEquatable&lt;T&gt;`
        /// implementations) or overloads the `==` operator, it is being call for equivalence checks for the whole
        /// hierarchy.
        /// In case the JSON column mapped property is represented by a JSON DOM object, only its root reference will
        /// be checked. While this works well for `System.Text.Json`, you should consider the
        /// `FullHierarchyOptimizedSemantically` option for cases where you are using `Newtonsof.Json`.
        /// In rare cases, false positives can occur, that will lead to a modified state even if the property is
        /// semantically equivalent to its original value (e.g. when adding whitespace characters between inner
        /// properties).
        /// </summary>
        FullHierarchyOptimizedFast,

        /// <summary>
        /// Track all changes in the JSON column mapped property (DOM, POCOs and `string`). This is a medium fast option
        /// with full change tracking support and should be used only in cases, when the JSON column mapped property is
        /// represented by a JSON DOM object and change tracking should check for semantic equivalence. For other cases,
        /// consider the `FullHierarchyOptimizedFast` option instead.
        /// If the JSON column mapped property implements a `Clone()` or `DeepClone()` method, it is called in snapshot
        /// generation and assumed to clone the full hierarchy.
        /// If the JSON column mapped property implements an `Equals(T, T)` method (e.g. for `IEquatable&lt;T&gt;`
        /// implementations) or overloads the `==` operator, it is being call for equivalence checks for the whole
        /// hierarchy.
        /// This option is most usefull when using `Newtonsof.Json`. If instead `System.Text.Json` is being used,
        /// consider the `FullHierarchyOptimizedFast` option instead for common cases, unless you explicitly require
        /// semantic equivalence checks.
        /// </summary>
        FullHierarchyOptimizedSemantically,

        /// <summary>
        /// Track all changes in the JSON column mapped property (DOM, POCOs and `string`). This is the slowest option
        /// with full change tracking support and should be used only in cases when full semantic equivalence is
        /// required, because of its high performance impact. This option does not result in false positives (e.g. when
        /// adding whitespace characters between inner properties).
        /// Every change tracking comparison is being done by processing the JSON column mapped property by the JSON
        /// serializer and comparing the result with the original value, that already has been processed in the same
        /// fashion.
        /// </summary>
        FullHierarchySemantically,
    }
}
