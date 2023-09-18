// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CallForwardingExpressionVisitor : ExpressionVisitor
{
    private static readonly IReadOnlyDictionary<MethodInfo, MethodInfo> _forwardedMethods = new Dictionary<MethodInfo, MethodInfo>
    {
        {
            typeof(byte).GetRuntimeMethod(nameof(byte.Clamp), new[] { typeof(byte), typeof(byte), typeof(byte) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(byte), typeof(byte), typeof(byte) })!
        },
        {
            typeof(byte).GetRuntimeMethod(nameof(byte.Max), new[] { typeof(byte), typeof(byte) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(byte), typeof(byte) })!
        },
        {
            typeof(byte).GetRuntimeMethod(nameof(byte.Min), new[] { typeof(byte), typeof(byte) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(byte), typeof(byte) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Abs), new[] { typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Ceiling), new[] { typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Clamp), new[] { typeof(decimal), typeof(decimal), typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(decimal), typeof(decimal), typeof(decimal) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Floor), new[] { typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Max), new[] { typeof(decimal), typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(decimal), typeof(decimal) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Min), new[] { typeof(decimal), typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(decimal), typeof(decimal) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Round), new[] { typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Round), new[] { typeof(decimal), typeof(int) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Round), new[] { typeof(decimal), typeof(int), typeof(MidpointRounding) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int), typeof(MidpointRounding) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Round), new[] { typeof(decimal), typeof(MidpointRounding) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(MidpointRounding) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Sign), new[] { typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) })!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Truncate), new[] { typeof(decimal) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Abs), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Acos), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Acosh), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Acosh), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Asin), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Asinh), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Asinh), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Atan), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Atan2), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Atanh), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Atanh), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.BitDecrement), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.BitDecrement), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.BitIncrement), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.BitIncrement), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Cbrt), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Cbrt), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Ceiling), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Clamp), new[] { typeof(double), typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(double), typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.CopySign), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.CopySign), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Cos), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Cosh), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Cosh), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Exp), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Floor), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.FusedMultiplyAdd), new[] { typeof(double), typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.FusedMultiplyAdd), new[] { typeof(double), typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Ieee754Remainder), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.IEEERemainder), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.ILogB), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.ILogB), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Log), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Log), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Log10), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Log2), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Log2), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Max), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.MaxMagnitude), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.MaxMagnitude), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Min), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.MinMagnitude), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.MinMagnitude), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Pow), new[] { typeof(double), typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.ReciprocalEstimate), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.ReciprocalEstimate), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.ReciprocalSqrtEstimate), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.ReciprocalSqrtEstimate), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Round), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Round), new[] { typeof(double), typeof(int) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Round), new[] { typeof(double), typeof(int), typeof(MidpointRounding) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(int), typeof(MidpointRounding) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Round), new[] { typeof(double), typeof(MidpointRounding) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(MidpointRounding) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.ScaleB), new[] { typeof(double), typeof(int) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.ScaleB), new[] { typeof(double), typeof(int) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Sign), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Sin), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Sinh), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sinh), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Sqrt), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Tan), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Tanh), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Tanh), new[] { typeof(double) })!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Truncate), new[] { typeof(double) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Abs), new[] { typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Acos), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Acos), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Acosh), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Acosh), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Asin), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Asin), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Asinh), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Asinh), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Atan), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Atan2), new[] { typeof(float), typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan2), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Atanh), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Atanh), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.BitDecrement), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.BitDecrement), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.BitIncrement), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.BitIncrement), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Cbrt), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Cbrt), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Ceiling), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Ceiling), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Clamp), new[] { typeof(float), typeof(float), typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(float), typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.CopySign), new[] { typeof(float), typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.CopySign), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Cos), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Cos), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Cosh), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Cosh), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Exp), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Exp), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Floor), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Floor), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.FusedMultiplyAdd), new[] { typeof(float), typeof(float), typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.FusedMultiplyAdd), new[] { typeof(float), typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Ieee754Remainder), new[] { typeof(float), typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.IEEERemainder), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.ILogB), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.ILogB), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Log), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Log), new[] { typeof(float), typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Log10), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Log10), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Log2), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Log2), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Max), new[] { typeof(float), typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.MaxMagnitude), new[] { typeof(float), typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.MaxMagnitude), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Min), new[] { typeof(float), typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.MinMagnitude), new[] { typeof(float), typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.MinMagnitude), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Pow), new[] { typeof(float), typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Pow), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.ReciprocalEstimate), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.ReciprocalEstimate), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.ReciprocalSqrtEstimate), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.ReciprocalSqrtEstimate), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Round), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Round), new[] { typeof(float), typeof(int) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float), typeof(int) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Round), new[] { typeof(float), typeof(int), typeof(MidpointRounding) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float), typeof(int), typeof(MidpointRounding) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Round), new[] { typeof(float), typeof(MidpointRounding) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float), typeof(MidpointRounding) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.ScaleB), new[] { typeof(float), typeof(int) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.ScaleB), new[] { typeof(float), typeof(int) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Sign), new[] { typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Sin), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Sin), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Sinh), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Sinh), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Sqrt), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Sqrt), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Tan), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Tan), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Tanh), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Tanh), new[] { typeof(float) })!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Truncate), new[] { typeof(float) })!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Truncate), new[] { typeof(float) })!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Abs), new[] { typeof(int) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) })!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Clamp), new[] { typeof(int), typeof(int), typeof(int) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(int), typeof(int), typeof(int) })!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Max), new[] { typeof(int), typeof(int) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) })!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Min), new[] { typeof(int), typeof(int) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) })!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Sign), new[] { typeof(int) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) })!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Abs), new[] { typeof(long) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) })!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Clamp), new[] { typeof(long), typeof(long), typeof(long) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(long), typeof(long), typeof(long) })!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Max), new[] { typeof(long), typeof(long) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(long), typeof(long) })!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Min), new[] { typeof(long), typeof(long) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(long), typeof(long) })!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Sign), new[] { typeof(long) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) })!
        },
        {
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Abs), new[] { typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) })!
        },
        {
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Max), new[] { typeof(float), typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Min), new[] { typeof(float), typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) })!
        },
        {
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Sign), new[] { typeof(float) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) })!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Abs), new[] { typeof(sbyte) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(sbyte) })!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Clamp), new[] { typeof(sbyte), typeof(sbyte), typeof(sbyte) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(sbyte), typeof(sbyte), typeof(sbyte) })!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Max), new[] { typeof(sbyte), typeof(sbyte) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(sbyte), typeof(sbyte) })!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Min), new[] { typeof(sbyte), typeof(sbyte) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(sbyte), typeof(sbyte) })!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Sign), new[] { typeof(sbyte) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) })!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Abs), new[] { typeof(short) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) })!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Clamp), new[] { typeof(short), typeof(short), typeof(short) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(short), typeof(short), typeof(short) })!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Max), new[] { typeof(short), typeof(short) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(short), typeof(short) })!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Min), new[] { typeof(short), typeof(short) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(short), typeof(short) })!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Sign), new[] { typeof(short) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) })!
        },
        {
            typeof(uint).GetRuntimeMethod(nameof(uint.Clamp), new[] { typeof(uint), typeof(uint), typeof(uint) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(uint), typeof(uint), typeof(uint) })!
        },
        {
            typeof(uint).GetRuntimeMethod(nameof(uint.Max), new[] { typeof(uint), typeof(uint) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(uint), typeof(uint) })!
        },
        {
            typeof(uint).GetRuntimeMethod(nameof(uint.Min), new[] { typeof(uint), typeof(uint) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(uint), typeof(uint) })!
        },
        {
            typeof(ulong).GetRuntimeMethod(nameof(ulong.Clamp), new[] { typeof(ulong), typeof(ulong), typeof(ulong) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(ulong), typeof(ulong), typeof(ulong) })!
        },
        {
            typeof(ulong).GetRuntimeMethod(nameof(ulong.Max), new[] { typeof(ulong), typeof(ulong) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(ulong), typeof(ulong) })!
        },
        {
            typeof(ulong).GetRuntimeMethod(nameof(ulong.Min), new[] { typeof(ulong), typeof(ulong) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(ulong), typeof(ulong) })!
        },
        {
            typeof(ushort).GetRuntimeMethod(nameof(ushort.Clamp), new[] { typeof(ushort), typeof(ushort), typeof(ushort) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), new[] { typeof(ushort), typeof(ushort), typeof(ushort) })!
        },
        {
            typeof(ushort).GetRuntimeMethod(nameof(ushort.Max), new[] { typeof(ushort), typeof(ushort) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(ushort), typeof(ushort) })!
        },
        {
            typeof(ushort).GetRuntimeMethod(nameof(ushort.Min), new[] { typeof(ushort), typeof(ushort) })!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(ushort), typeof(ushort) })!
        }
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        => _forwardedMethods.TryGetValue(methodCallExpression.Method, out var destinationMethod)
            ? VisitMethodCall(Expression.Call(destinationMethod, methodCallExpression.Arguments))
            : base.VisitMethodCall(methodCallExpression);
}
