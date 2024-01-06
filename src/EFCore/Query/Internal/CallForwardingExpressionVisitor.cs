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
            typeof(byte).GetRuntimeMethod(nameof(byte.Clamp), [typeof(byte), typeof(byte), typeof(byte)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(byte), typeof(byte), typeof(byte)])!
        },
        {
            typeof(byte).GetRuntimeMethod(nameof(byte.Max), [typeof(byte), typeof(byte)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(byte), typeof(byte)])!
        },
        {
            typeof(byte).GetRuntimeMethod(nameof(byte.Min), [typeof(byte), typeof(byte)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(byte), typeof(byte)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Abs), [typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(decimal)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Ceiling), [typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), [typeof(decimal)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Clamp), [typeof(decimal), typeof(decimal), typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(decimal), typeof(decimal), typeof(decimal)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Floor), [typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Floor), [typeof(decimal)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Max), [typeof(decimal), typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(decimal), typeof(decimal)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Min), [typeof(decimal), typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(decimal), typeof(decimal)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Round), [typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(decimal)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Round), [typeof(decimal), typeof(int)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(decimal), typeof(int)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Round), [typeof(decimal), typeof(int), typeof(MidpointRounding)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(decimal), typeof(int), typeof(MidpointRounding)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Round), [typeof(decimal), typeof(MidpointRounding)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(decimal), typeof(MidpointRounding)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Sign), [typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(decimal)])!
        },
        {
            typeof(decimal).GetRuntimeMethod(nameof(decimal.Truncate), [typeof(decimal)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), [typeof(decimal)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Abs), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Acos), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Acos), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Acosh), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Acosh), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Asin), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Asin), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Asinh), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Asinh), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Atan), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Atan), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Atan2), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Atanh), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Atanh), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.BitDecrement), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.BitDecrement), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.BitIncrement), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.BitIncrement), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Cbrt), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Cbrt), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Ceiling), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Clamp), [typeof(double), typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(double), typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.CopySign), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.CopySign), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Cos), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Cos), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Cosh), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Cosh), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Exp), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Exp), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Floor), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Floor), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.FusedMultiplyAdd), [typeof(double), typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.FusedMultiplyAdd), [typeof(double), typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Ieee754Remainder), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.IEEERemainder), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.ILogB), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.ILogB), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Log), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Log), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Log), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Log), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Log10), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Log10), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Log2), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Log2), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Max), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.MaxMagnitude), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.MaxMagnitude), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Min), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.MinMagnitude), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.MinMagnitude), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Pow), [typeof(double), typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Pow), [typeof(double), typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.ReciprocalEstimate), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.ReciprocalEstimate), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.ReciprocalSqrtEstimate), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.ReciprocalSqrtEstimate), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Round), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Round), [typeof(double), typeof(int)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(double), typeof(int)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Round), [typeof(double), typeof(int), typeof(MidpointRounding)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(double), typeof(int), typeof(MidpointRounding)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Round), [typeof(double), typeof(MidpointRounding)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(double), typeof(MidpointRounding)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.ScaleB), [typeof(double), typeof(int)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.ScaleB), [typeof(double), typeof(int)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Sign), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Sin), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sin), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Sinh), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sinh), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Sqrt), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Tan), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Tan), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Tanh), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Tanh), [typeof(double)])!
        },
        {
            typeof(double).GetRuntimeMethod(nameof(double.Truncate), [typeof(double)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), [typeof(double)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Abs), [typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Acos), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Acos), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Acosh), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Acosh), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Asin), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Asin), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Asinh), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Asinh), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Atan), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Atan2), [typeof(float), typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan2), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Atanh), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Atanh), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.BitDecrement), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.BitDecrement), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.BitIncrement), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.BitIncrement), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Cbrt), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Cbrt), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Ceiling), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Ceiling), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Clamp), [typeof(float), typeof(float), typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(float), typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.CopySign), [typeof(float), typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.CopySign), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Cos), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Cos), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Cosh), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Cosh), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Exp), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Exp), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Floor), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Floor), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.FusedMultiplyAdd), [typeof(float), typeof(float), typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.FusedMultiplyAdd), [typeof(float), typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Ieee754Remainder), [typeof(float), typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.IEEERemainder), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.ILogB), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.ILogB), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Log), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Log), [typeof(float), typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Log10), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Log10), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Log2), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Log2), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Max), [typeof(float), typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.MaxMagnitude), [typeof(float), typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.MaxMagnitude), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Min), [typeof(float), typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.MinMagnitude), [typeof(float), typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.MinMagnitude), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Pow), [typeof(float), typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Pow), [typeof(float), typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.ReciprocalEstimate), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.ReciprocalEstimate), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.ReciprocalSqrtEstimate), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.ReciprocalSqrtEstimate), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Round), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Round), [typeof(float), typeof(int)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), [typeof(float), typeof(int)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Round), [typeof(float), typeof(int), typeof(MidpointRounding)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), [typeof(float), typeof(int), typeof(MidpointRounding)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Round), [typeof(float), typeof(MidpointRounding)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), [typeof(float), typeof(MidpointRounding)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.ScaleB), [typeof(float), typeof(int)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.ScaleB), [typeof(float), typeof(int)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Sign), [typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Sin), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Sin), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Sinh), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Sinh), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Sqrt), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Sqrt), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Tan), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Tan), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Tanh), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Tanh), [typeof(float)])!
        },
        {
            typeof(float).GetRuntimeMethod(nameof(float.Truncate), [typeof(float)])!,
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Truncate), [typeof(float)])!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Abs), [typeof(int)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(int)])!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Clamp), [typeof(int), typeof(int), typeof(int)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(int), typeof(int), typeof(int)])!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Max), [typeof(int), typeof(int)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(int), typeof(int)])!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Min), [typeof(int), typeof(int)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(int), typeof(int)])!
        },
        {
            typeof(int).GetRuntimeMethod(nameof(int.Sign), [typeof(int)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(int)])!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Abs), [typeof(long)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(long)])!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Clamp), [typeof(long), typeof(long), typeof(long)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(long), typeof(long), typeof(long)])!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Max), [typeof(long), typeof(long)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(long), typeof(long)])!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Min), [typeof(long), typeof(long)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(long), typeof(long)])!
        },
        {
            typeof(long).GetRuntimeMethod(nameof(long.Sign), [typeof(long)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(long)])!
        },
        {
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Abs), [typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(float)])!
        },
        {
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Max), [typeof(float), typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(float), typeof(float)])!
        },
        {
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Min), [typeof(float), typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(float), typeof(float)])!
        },
        {
            typeof(MathF).GetRuntimeMethod(nameof(MathF.Sign), [typeof(float)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(float)])!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Abs), [typeof(sbyte)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(sbyte)])!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Clamp), [typeof(sbyte), typeof(sbyte), typeof(sbyte)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(sbyte), typeof(sbyte), typeof(sbyte)])!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Max), [typeof(sbyte), typeof(sbyte)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(sbyte), typeof(sbyte)])!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Min), [typeof(sbyte), typeof(sbyte)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(sbyte), typeof(sbyte)])!
        },
        {
            typeof(sbyte).GetRuntimeMethod(nameof(sbyte.Sign), [typeof(sbyte)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(sbyte)])!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Abs), [typeof(short)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(short)])!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Clamp), [typeof(short), typeof(short), typeof(short)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(short), typeof(short), typeof(short)])!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Max), [typeof(short), typeof(short)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(short), typeof(short)])!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Min), [typeof(short), typeof(short)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(short), typeof(short)])!
        },
        {
            typeof(short).GetRuntimeMethod(nameof(short.Sign), [typeof(short)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(short)])!
        },
        {
            typeof(uint).GetRuntimeMethod(nameof(uint.Clamp), [typeof(uint), typeof(uint), typeof(uint)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(uint), typeof(uint), typeof(uint)])!
        },
        {
            typeof(uint).GetRuntimeMethod(nameof(uint.Max), [typeof(uint), typeof(uint)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(uint), typeof(uint)])!
        },
        {
            typeof(uint).GetRuntimeMethod(nameof(uint.Min), [typeof(uint), typeof(uint)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(uint), typeof(uint)])!
        },
        {
            typeof(ulong).GetRuntimeMethod(nameof(ulong.Clamp), [typeof(ulong), typeof(ulong), typeof(ulong)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(ulong), typeof(ulong), typeof(ulong)])!
        },
        {
            typeof(ulong).GetRuntimeMethod(nameof(ulong.Max), [typeof(ulong), typeof(ulong)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(ulong), typeof(ulong)])!
        },
        {
            typeof(ulong).GetRuntimeMethod(nameof(ulong.Min), [typeof(ulong), typeof(ulong)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(ulong), typeof(ulong)])!
        },
        {
            typeof(ushort).GetRuntimeMethod(nameof(ushort.Clamp), [typeof(ushort), typeof(ushort), typeof(ushort)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Clamp), [typeof(ushort), typeof(ushort), typeof(ushort)])!
        },
        {
            typeof(ushort).GetRuntimeMethod(nameof(ushort.Max), [typeof(ushort), typeof(ushort)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Max), [typeof(ushort), typeof(ushort)])!
        },
        {
            typeof(ushort).GetRuntimeMethod(nameof(ushort.Min), [typeof(ushort), typeof(ushort)])!,
            typeof(Math).GetRuntimeMethod(nameof(Math.Min), [typeof(ushort), typeof(ushort)])!
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
