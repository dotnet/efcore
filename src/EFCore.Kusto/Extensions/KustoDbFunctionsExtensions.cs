using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Kusto.Extensions;

public static class KustoDbFunctionsExtensions
{
    public static bool IsDefined(this DbFunctions _, object? expression)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsDefined)));

    public static T CoalesceUndefined<T>(
        this DbFunctions _,
        T expression1,
        T expression2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CoalesceUndefined)));

    public static double VectorDistance(this DbFunctions _, ReadOnlyMemory<byte> vector1, ReadOnlyMemory<byte> vector2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));

    public static double VectorDistance(
        this DbFunctions _,
        ReadOnlyMemory<byte> vector1,
        ReadOnlyMemory<byte> vector2,
        [NotParameterized] bool useBruteForce)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));

    public static double VectorDistance(
        this DbFunctions _,
        ReadOnlyMemory<byte> vector1,
        ReadOnlyMemory<byte> vector2,
        [NotParameterized] bool useBruteForce,
        [NotParameterized] DistanceFunction distanceFunction)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));

    public static double VectorDistance(this DbFunctions _, ReadOnlyMemory<sbyte> vector1, ReadOnlyMemory<sbyte> vector2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));

    public static double VectorDistance(
        this DbFunctions _,
        ReadOnlyMemory<sbyte> vector1,
        ReadOnlyMemory<sbyte> vector2,
        [NotParameterized] bool useBruteForce)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));

    public static double VectorDistance(
        this DbFunctions _,
        ReadOnlyMemory<sbyte> vector1,
        ReadOnlyMemory<sbyte> vector2,
        [NotParameterized] bool useBruteForce,
        [NotParameterized] DistanceFunction distanceFunction)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));

    public static double VectorDistance(this DbFunctions _, ReadOnlyMemory<float> vector1, ReadOnlyMemory<float> vector2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));

    public static double VectorDistance(
        this DbFunctions _,
        ReadOnlyMemory<float> vector1,
        ReadOnlyMemory<float> vector2,
        [NotParameterized] bool useBruteForce)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));

    public static double VectorDistance(
        this DbFunctions _,
        ReadOnlyMemory<float> vector1,
        ReadOnlyMemory<float> vector2,
        [NotParameterized] bool useBruteForce,
        [NotParameterized] DistanceFunction distanceFunction)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(VectorDistance)));
}
