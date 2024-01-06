// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot : ISnapshot
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public const int MaxGenericTypes = 30;

    private Snapshot()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static ISnapshot Empty = new Snapshot();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly FieldInfo EmptyField
        = typeof(Snapshot).GetField(nameof(Empty), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => throw new IndexOutOfRangeException();
        set => throw new IndexOutOfRangeException();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => throw new IndexOutOfRangeException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly MethodInfo GetValueMethod
        = typeof(ISnapshot).GetMethod(nameof(GetValue), 1, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
            null, CallingConventions.Any, [typeof(int)], null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    [RequiresDynamicCode("Creates types using MakeGenericType.")]
    public static Type CreateSnapshotType(Type[] types)
        => types.Length switch
        {
            1 => typeof(Snapshot<>).MakeGenericType(types),
            2 => typeof(Snapshot<,>).MakeGenericType(types),
            3 => typeof(Snapshot<,,>).MakeGenericType(types),
            4 => typeof(Snapshot<,,,>).MakeGenericType(types),
            5 => typeof(Snapshot<,,,,>).MakeGenericType(types),
            6 => typeof(Snapshot<,,,,,>).MakeGenericType(types),
            7 => typeof(Snapshot<,,,,,,>).MakeGenericType(types),
            8 => typeof(Snapshot<,,,,,,,>).MakeGenericType(types),
            9 => typeof(Snapshot<,,,,,,,,>).MakeGenericType(types),
            10 => typeof(Snapshot<,,,,,,,,,>).MakeGenericType(types),
            11 => typeof(Snapshot<,,,,,,,,,,>).MakeGenericType(types),
            12 => typeof(Snapshot<,,,,,,,,,,,>).MakeGenericType(types),
            13 => typeof(Snapshot<,,,,,,,,,,,,>).MakeGenericType(types),
            14 => typeof(Snapshot<,,,,,,,,,,,,,>).MakeGenericType(types),
            15 => typeof(Snapshot<,,,,,,,,,,,,,,>).MakeGenericType(types),
            16 => typeof(Snapshot<,,,,,,,,,,,,,,,>).MakeGenericType(types),
            17 => typeof(Snapshot<,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            18 => typeof(Snapshot<,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            19 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            20 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            21 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            22 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            23 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            24 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            25 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            26 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            27 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            28 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            29 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            30 => typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types),
            _ => throw new IndexOutOfRangeException()
        };

    /// <inheritdoc/>
    bool ISnapshot.IsEmpty
        => true;
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22,
        T23, T24, T25, T26, T27, T28, T29>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value0,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value1,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value2,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value3,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value4,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value5,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value6,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value7,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value8,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value9,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value10,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value11,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value12,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value13,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value14,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value15,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value16,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value17,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value18,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value19,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value20,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value21,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value22,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value23,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value24,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value25,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value26,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value27,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value28,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28, T29> e) => e._value29
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21,
        T22 value22,
        T23 value23,
        T24 value24,
        T25 value25,
        T26 value26,
        T27 value27,
        T28 value28,
        T29 value29)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
        _value22 = value22;
        _value23 = value23;
        _value24 = value24;
        _value25 = value25;
        _value26 = value26;
        _value27 = value27;
        _value28 = value28;
        _value29 = value29;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;
    private T22 _value22;
    private T23 _value23;
    private T24 _value24;
    private T25 _value25;
    private T26 _value26;
    private T27 _value27;
    private T28 _value28;
    private T29 _value29;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23,
            T24, T25, T26, T27, T28, T29>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            22 => _value22,
            23 => _value23,
            24 => _value24,
            25 => _value25,
            26 => _value26,
            27 => _value27,
            28 => _value28,
            29 => _value29,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                case 22:
                    _value22 = (T22)value!;
                    break;
                case 23:
                    _value23 = (T23)value!;
                    break;
                case 24:
                    _value24 = (T24)value!;
                    break;
                case 25:
                    _value25 = (T25)value!;
                    break;
                case 26:
                    _value26 = (T26)value!;
                    break;
                case 27:
                    _value27 = (T27)value!;
                    break;
                case 28:
                    _value28 = (T28)value!;
                    break;
                case 29:
                    _value29 = (T29)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22,
        T23, T24, T25, T26, T27, T28>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value0,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value1,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value2,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value3,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value4,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value5,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value6,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value7,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value8,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value9,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value10,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value11,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value12,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value13,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value14,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value15,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value16,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value17,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value18,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value19,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value20,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value21,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value22,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value23,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value24,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value25,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value26,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value27,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27, T28> e) => e._value28
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21,
        T22 value22,
        T23 value23,
        T24 value24,
        T25 value25,
        T26 value26,
        T27 value27,
        T28 value28)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
        _value22 = value22;
        _value23 = value23;
        _value24 = value24;
        _value25 = value25;
        _value26 = value26;
        _value27 = value27;
        _value28 = value28;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;
    private T22 _value22;
    private T23 _value23;
    private T24 _value24;
    private T25 _value25;
    private T26 _value26;
    private T27 _value27;
    private T28 _value28;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23,
            T24, T25, T26, T27, T28>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            22 => _value22,
            23 => _value23,
            24 => _value24,
            25 => _value25,
            26 => _value26,
            27 => _value27,
            28 => _value28,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                case 22:
                    _value22 = (T22)value!;
                    break;
                case 23:
                    _value23 = (T23)value!;
                    break;
                case 24:
                    _value24 = (T24)value!;
                    break;
                case 25:
                    _value25 = (T25)value!;
                    break;
                case 26:
                    _value26 = (T26)value!;
                    break;
                case 27:
                    _value27 = (T27)value!;
                    break;
                case 28:
                    _value28 = (T28)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22,
        T23, T24, T25, T26, T27>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value0,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value1,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value2,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value3,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value4,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value5,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value6,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value7,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value8,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value9,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value10,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value11,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value12,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value13,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value14,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value15,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value16,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value17,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value18,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value19,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value20,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value21,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value22,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value23,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value24,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value25,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value26,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26, T27> e) => e._value27
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21,
        T22 value22,
        T23 value23,
        T24 value24,
        T25 value25,
        T26 value26,
        T27 value27)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
        _value22 = value22;
        _value23 = value23;
        _value24 = value24;
        _value25 = value25;
        _value26 = value26;
        _value27 = value27;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;
    private T22 _value22;
    private T23 _value23;
    private T24 _value24;
    private T25 _value25;
    private T26 _value26;
    private T27 _value27;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23,
            T24, T25, T26, T27>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            22 => _value22,
            23 => _value23,
            24 => _value24,
            25 => _value25,
            26 => _value26,
            27 => _value27,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                case 22:
                    _value22 = (T22)value!;
                    break;
                case 23:
                    _value23 = (T23)value!;
                    break;
                case 24:
                    _value24 = (T24)value!;
                    break;
                case 25:
                    _value25 = (T25)value!;
                    break;
                case 26:
                    _value26 = (T26)value!;
                    break;
                case 27:
                    _value27 = (T27)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22,
        T23, T24, T25, T26>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value0,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value1,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value2,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value3,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value4,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value5,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value6,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value7,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value8,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value9,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value10,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value11,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value12,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value13,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value14,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value15,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value16,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value17,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value18,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value19,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value20,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value21,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value22,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value23,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value24,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value25,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25, T26> e) => e._value26
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21,
        T22 value22,
        T23 value23,
        T24 value24,
        T25 value25,
        T26 value26)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
        _value22 = value22;
        _value23 = value23;
        _value24 = value24;
        _value25 = value25;
        _value26 = value26;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;
    private T22 _value22;
    private T23 _value23;
    private T24 _value24;
    private T25 _value25;
    private T26 _value26;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23,
            T24, T25, T26>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            22 => _value22,
            23 => _value23,
            24 => _value24,
            25 => _value25,
            26 => _value26,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                case 22:
                    _value22 = (T22)value!;
                    break;
                case 23:
                    _value23 = (T23)value!;
                    break;
                case 24:
                    _value24 = (T24)value!;
                    break;
                case 25:
                    _value25 = (T25)value!;
                    break;
                case 26:
                    _value26 = (T26)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22,
        T23, T24, T25>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value0,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value1,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value2,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value3,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value4,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value5,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value6,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value7,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value8,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value9,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value10,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value11,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value12,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value13,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value14,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value15,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value16,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value17,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value18,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value19,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value20,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value21,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value22,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value23,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value24,
        (
            Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24,
                T25> e) => e._value25
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21,
        T22 value22,
        T23 value23,
        T24 value24,
        T25 value25)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
        _value22 = value22;
        _value23 = value23;
        _value24 = value24;
        _value25 = value25;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;
    private T22 _value22;
    private T23 _value23;
    private T24 _value24;
    private T25 _value25;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23,
            T24, T25>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            22 => _value22,
            23 => _value23,
            24 => _value24,
            25 => _value25,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                case 22:
                    _value22 = (T22)value!;
                    break;
                case 23:
                    _value23 = (T23)value!;
                    break;
                case 24:
                    _value24 = (T24)value!;
                    break;
                case 25:
                    _value25 = (T25)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22,
        T23, T24>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value16,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value17,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value18,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value19,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value20,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value21,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value22,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value23,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24> e)
            => e._value24
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21,
        T22 value22,
        T23 value23,
        T24 value24)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
        _value22 = value22;
        _value23 = value23;
        _value24 = value24;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;
    private T22 _value22;
    private T23 _value23;
    private T24 _value24;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23,
            T24>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            22 => _value22,
            23 => _value23,
            24 => _value24,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                case 22:
                    _value22 = (T22)value!;
                    break;
                case 23:
                    _value23 = (T23)value!;
                    break;
                case 24:
                    _value24 = (T24)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22,
        T23>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value16,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value17,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value18,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value19,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value20,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value21,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value22,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23> e)
            => e._value23
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21,
        T22 value22,
        T23 value23)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
        _value22 = value22;
        _value23 = value23;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;
    private T22 _value22;
    private T23 _value23;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>
            , T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            22 => _value22,
            23 => _value23,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                case 22:
                    _value22 = (T22)value!;
                    break;
                case 23:
                    _value23 = (T23)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value16,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value17,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value18,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value19,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value20,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value21,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22> e)
            => e._value22
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21,
        T22 value22)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
        _value22 = value22;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;
    private T22 _value22;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22>, T>)
            ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            22 => _value22,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                case 22:
                    _value22 = (T22)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value16,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value17,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value18,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value19,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value20,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21> e) => e._value21
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20,
        T21 value21)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
        _value21 = value21;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;
    private T21 _value21;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>, T>)
            ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            21 => _value21,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                case 21:
                    _value21 = (T21)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value16,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value17,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value18,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value19,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20> e) => e._value20
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19,
        T20 value20)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
        _value20 = value20;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;
    private T20 _value20;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>, T>)
            ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            20 => _value20,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                case 20:
                    _value20 = (T20)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value16,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value17,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value18,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19> e) => e._value19
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18,
        T19 value19)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
        _value19 = value19;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;
    private T19 _value19;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>, T>)ValueReaders
            [index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            19 => _value19,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                case 19:
                    _value19 = (T19)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value16,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value17,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18> e) => e._value18
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17,
        T18 value18)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
        _value18 = value18;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;
    private T18 _value18;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>, T>)ValueReaders[index]
            )(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            18 => _value18,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                case 18:
                    _value18 = (T18)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value16,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17> e) => e._value17
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16,
        T17 value17)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
        _value17 = value17;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;
    private T17 _value17;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>, T>)ValueReaders[index])(
            this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            17 => _value17,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                case 17:
                    _value17 = (T17)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value15,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> e) => e._value16
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15,
        T16 value16)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
        _value16 = value16;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;
    private T16 _value16;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            16 => _value16,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                case 16:
                    _value16 = (T16)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value14,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> e) => e._value15
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14,
        T15 value15)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
        _value15 = value15;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;
    private T15 _value15;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            15 => _value15,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                case 15:
                    _value15 = (T15)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value13,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> e) => e._value14
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13,
        T14 value14)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
        _value14 = value14;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;
    private T14 _value14;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            14 => _value14,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                case 14:
                    _value14 = (T14)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value12,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> e) => e._value13
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12,
        T13 value13)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
        _value13 = value13;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;
    private T13 _value13;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            13 => _value13,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                case 13:
                    _value13 = (T13)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value11,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> e) => e._value12
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11,
        T12 value12)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
        _value12 = value12;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;
    private T12 _value12;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            12 => _value12,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                case 12:
                    _value12 = (T12)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value10,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> e) => e._value11
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10,
        T11 value11)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
        _value11 = value11;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;
    private T11 _value11;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            11 => _value11,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                case 11:
                    _value11 = (T11)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value9,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> e) => e._value10
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9,
        T10 value10)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
        _value10 = value10;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;
    private T10 _value10;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            10 => _value10,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                case 10:
                    _value10 = (T10)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value8,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> e) => e._value9
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8,
        T9 value9)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
        _value9 = value9;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;
    private T9 _value9;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            9 => _value9,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                case 9:
                    _value9 = (T9)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value7,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8> e) => e._value8
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7,
        T8 value8)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
        _value8 = value8;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;
    private T8 _value8;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            8 => _value8,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                case 8:
                    _value8 = (T8)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7> e) => e._value6,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6, T7> e) => e._value7
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6,
        T7 value7)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
        _value7 = value7;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;
    private T7 _value7;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            7 => _value7,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                case 7:
                    _value7 = (T7)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5, T6> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6> e) => e._value5,
        (Snapshot<T0, T1, T2, T3, T4, T5, T6> e) => e._value6
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5,
        T6 value6)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
        _value6 = value6;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;
    private T6 _value6;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            6 => _value6,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                case 6:
                    _value6 = (T6)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4, T5>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4, T5> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4, T5> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4, T5> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4, T5> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4, T5> e) => e._value4,
        (Snapshot<T0, T1, T2, T3, T4, T5> e) => e._value5
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
        _value5 = value5;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;
    private T5 _value5;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4, T5>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            5 => _value5,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                case 5:
                    _value5 = (T5)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3, T4>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3, T4> e) => e._value0,
        (Snapshot<T0, T1, T2, T3, T4> e) => e._value1,
        (Snapshot<T0, T1, T2, T3, T4> e) => e._value2,
        (Snapshot<T0, T1, T2, T3, T4> e) => e._value3,
        (Snapshot<T0, T1, T2, T3, T4> e) => e._value4
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
        _value4 = value4;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;
    private T4 _value4;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3, T4>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            4 => _value4,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                case 4:
                    _value4 = (T4)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2, T3>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2, T3> e) => e._value0,
        (Snapshot<T0, T1, T2, T3> e) => e._value1,
        (Snapshot<T0, T1, T2, T3> e) => e._value2,
        (Snapshot<T0, T1, T2, T3> e) => e._value3
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2,
        T3 value3)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
        _value3 = value3;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;
    private T3 _value3;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2, T3>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            3 => _value3,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                case 3:
                    _value3 = (T3)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1, T2>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders =
    [
        (Snapshot<T0, T1, T2> e) => e._value0, (Snapshot<T0, T1, T2> e) => e._value1, (Snapshot<T0, T1, T2> e) => e._value2
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1,
        T2 value2)
    {
        _value0 = value0;
        _value1 = value1;
        _value2 = value2;
    }

    private T0 _value0;
    private T1 _value1;
    private T2 _value2;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1, T2>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            2 => _value2,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                case 2:
                    _value2 = (T2)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0, T1>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders = [(Snapshot<T0, T1> e) => e._value0, (Snapshot<T0, T1> e) => e._value1];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0,
        T1 value1)
    {
        _value0 = value0;
        _value1 = value1;
    }

    private T0 _value0;
    private T1 _value1;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0, T1>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index switch
        {
            0 => _value0,
            1 => _value1,
            _ => throw new IndexOutOfRangeException()
        };
        set
        {
            switch (index)
            {
                case 0:
                    _value0 = (T0)value!;
                    break;
                case 1:
                    _value1 = (T1)value!;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class Snapshot<T0>
    : ISnapshot
{
    private static readonly Delegate[] ValueReaders = [(Snapshot<T0> e) => e._value0];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Snapshot(
        T0 value0)
    {
        _value0 = value0;
    }

    private T0 _value0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public T GetValue<T>(int index)
        => ((Func<Snapshot<T0>, T>)ValueReaders[index])(this);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public object? this[int index]
    {
        get => index == 0 ? _value0 : throw new IndexOutOfRangeException();
        set => _value0 = index switch
        {
            0 => (T0)value!,
            _ => throw new IndexOutOfRangeException()
        };
    }
}
