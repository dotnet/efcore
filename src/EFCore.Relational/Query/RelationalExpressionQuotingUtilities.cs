// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Utilities used for implementing <see cref="IRelationalQuotableExpression" />.
/// </summary>
[Experimental("EF1003")]
public static class RelationalExpressionQuotingUtilities
{
    private static readonly ParameterExpression RelationalModelParameter
        = Parameter(typeof(RelationalModel), "relationalModel");
    private static readonly ParameterExpression RelationalTypeMappingSourceParameter
        = Parameter(typeof(RelationalTypeMappingSource), "relationalTypeMappingSource");

    private static readonly MethodInfo RelationalModelFindTableMethod
        = typeof(RelationalModel).GetMethod(nameof(RelationalModel.FindTable), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo RelationalModelFindDefaultTableMethod
        = typeof(RelationalModel).GetMethod(nameof(RelationalModel.FindDefaultTable), [typeof(string)])!;

    private static readonly MethodInfo RelationalModelFindViewMethod
        = typeof(RelationalModel).GetMethod(nameof(RelationalModel.FindView), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo RelationalModelFindQueryMethod
        = typeof(RelationalModel).GetMethod(nameof(RelationalModel.FindQuery), [typeof(string)])!;

    private static readonly MethodInfo RelationalModelFindFunctionMethod
        = typeof(RelationalModel).GetMethod(
            nameof(RelationalModel.FindFunction), [typeof(string), typeof(string), typeof(IReadOnlyList<string>)])!;

    private static ConstructorInfo? _annotationConstructor;
    private static ConstructorInfo? _dictionaryConstructor;
    private static MethodInfo? _dictionaryAddMethod;
    private static MethodInfo? _hashSetAddMethod;

    private static readonly MethodInfo RelationalTypeMappingSourceFindMappingMethod
        = typeof(RelationalTypeMappingSource)
            .GetMethod(
                nameof(RelationalTypeMappingSource.FindMapping),
                [
                    typeof(Type), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(bool), typeof(bool), typeof(int),
                    typeof(int)
                ])!;

    /// <summary>
    ///     If <paramref name="expression" /> is <see langword="null" />, returns a <see cref="ConstantExpression" /> with a
    ///     <see langword="null" /> value. Otherwise, calls <see cref="IRelationalQuotableExpression.Quote" /> and returns the result.
    /// </summary>
    public static Expression VisitOrNull<T>(T? expression) where T : IRelationalQuotableExpression
        => expression is null ? Constant(null, typeof(T)) : expression.Quote();

    /// <summary>
    ///     Quotes a relational type mapping.
    /// </summary>
    public static Expression QuoteTypeMapping(RelationalTypeMapping? typeMapping)
        => typeMapping is null
            ? Constant(null, typeof(RelationalTypeMapping))
            : Call(
                RelationalTypeMappingSourceParameter,
                RelationalTypeMappingSourceFindMappingMethod,
                Constant(typeMapping.ClrType, typeof(Type)),
                Constant(typeMapping.StoreType, typeof(string)),
                Constant(false), // TODO: keyOrIndex not accessible
                Constant(typeMapping.IsUnicode, typeof(bool?)),
                Constant(typeMapping.Size, typeof(int?)),
                Constant(false, typeof(bool?)), // TODO: rowversion not accessible
                Constant(typeMapping.IsFixedLength, typeof(bool?)),
                Constant(typeMapping.Precision, typeof(int?)),
                Constant(typeMapping.Scale, typeof(int?)));

    /// <summary>
    ///     Quotes an <see cref="ITableBase" />.
    /// </summary>
    public static Expression QuoteTableBase(ITableBase tableBase)
        => tableBase switch
        {
            ITable table
                => Call(
                    RelationalModelParameter,
                    RelationalModelFindTableMethod,
                    Constant(table.Name, typeof(string)),
                    Constant(table.Schema, typeof(string))),

            TableBase table
                => Call(
                    RelationalModelParameter,
                    RelationalModelFindDefaultTableMethod,
                    Constant(table.Name, typeof(string))),

            IView view
                => Call(
                    RelationalModelParameter,
                    RelationalModelFindViewMethod,
                    Constant(view.Name, typeof(string)),
                    Constant(view.Schema, typeof(string))),

            ISqlQuery query
                => Call(
                    RelationalModelParameter,
                    RelationalModelFindQueryMethod,
                    Constant(query.Name, typeof(string))),

            IStoreFunction function
                => Call(
                    RelationalModelParameter,
                    RelationalModelFindFunctionMethod,
                    Constant(function.Name, typeof(string)),
                    Constant(function.Schema, typeof(string)),
                    NewArrayInit(typeof(string), function.Parameters.Select(p => Constant(p.StoreType)))),

            IStoreStoredProcedure => throw new UnreachableException(),

            _ => throw new UnreachableException()
        };

    /// <summary>
    ///     Quotes a set of string tags.
    /// </summary>
    public static Expression QuoteTags(ISet<string> tags)
        => ListInit(
            New(typeof(HashSet<string>)),
            tags.Select(
                t => ElementInit(
                    _hashSetAddMethod ??= typeof(HashSet<string>).GetMethod(nameof(HashSet<string>.Add))!,
                    Constant(t))));

    /// <summary>
    ///     Quotes the annotations on a <see cref="TableExpressionBase" />.
    /// </summary>
    public static Expression QuoteAnnotations(IReadOnlyDictionary<string, IAnnotation>? annotations)
        => annotations is null or { Count: 0 }
            ? Constant(null, typeof(IReadOnlyDictionary<string, IAnnotation>))
            : ListInit(
                New(_dictionaryConstructor ??= typeof(IDictionary<string, IAnnotation>).GetConstructor([])!),
                annotations.Select(
                    a => ElementInit(
                        _dictionaryAddMethod ??= typeof(Dictionary<string, IAnnotation>).GetMethod("Add")!,
                        Constant(a.Key),
                        New(
                            _annotationConstructor ??= typeof(Annotation).GetConstructor([typeof(string), typeof(object)])!,
                            Constant(a.Key),
                            Constant(a.Value)))));
}
