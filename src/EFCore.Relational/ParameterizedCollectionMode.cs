// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates how parameterized collections are translated into SQL.
/// </summary>
public enum ParameterizedCollectionMode
{
    /// <summary>
    ///     Instructs EF to translate the collection to a set of constants:
    ///     <c>WHERE [x].[Id] IN (1, 2, 3)</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can produce better query plans for certain query types, but can also lead to query
    ///         plan bloat.
    ///     </para>
    ///     <para>
    ///         Note that it's possible to cause EF to translate a specific collection in a specific query to constants by wrapping the
    ///         parameterized collection in <see cref="EF.Constant{T}" />: <c>Where(x => EF.Constant(ids).Contains(x.Id)</c>. This overrides
    ///         the default.
    ///     </para>
    /// </remarks>
    Constants,

    /// <summary>
    ///     Instructs EF to translate the collection to a single array-like parameter:
    ///     <c>WHERE [x].[Id] IN (SELECT [i].[value] FROM OPENJSON(@ids) ...)</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This can produce suboptimal query plans for certain query types.
    ///     </para>
    ///     <para>
    ///         Note that it's possible to cause EF to translate a specific collection in a specific query to parameter by wrapping the
    ///         parameterized collection in <see cref="EF.Parameter{T}" />: <c>Where(x => EF.Parameter(ids).Contains(x.Id)</c>. This overrides
    ///         the default.
    ///     </para>
    /// </remarks>
    Parameter,

    /// <summary>
    ///     Instructs EF to translate the collection to a set of parameters:
    ///     <c>WHERE [x].[Id] IN (@ids1, @ids2, @ids3)</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that it's possible to cause EF to translate a specific collection in a specific query to parameter by wrapping the
    ///         parameterized collection in <see cref="EFExtensions.MultipleParameters{T}" />: <c>Where(x => EF.MultipleParameters(ids).Contains(x.Id)</c>. This overrides
    ///         the default.
    ///     </para>
    /// </remarks>
    MultipleParameters,
}
