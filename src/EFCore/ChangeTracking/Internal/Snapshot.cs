// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
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
        public object this[int index]
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
        {
            throw new IndexOutOfRangeException();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Delegate[] CreateReaders<TSnapshot>()
        {
            var genericArguments = typeof(TSnapshot).GetTypeInfo().GenericTypeArguments;
            var delegates = new Delegate[genericArguments.Length];

            for (var i = 0; i < genericArguments.Length; ++i)
            {
                var snapshotParameter = Expression.Parameter(typeof(TSnapshot), "snapshot");

                delegates[i] = Expression.Lambda(
                        typeof(Func<,>).MakeGenericType(typeof(TSnapshot), genericArguments[i]),
                        Expression.Field(snapshotParameter, "_value" + i), snapshotParameter)
                    .Compile();
            }

            return delegates;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static Type CreateSnapshotType([NotNull] Type[] types)
        {
            switch (types.Length)
            {
                case 1:
                    return typeof(Snapshot<>).MakeGenericType(types);
                case 2:
                    return typeof(Snapshot<,>).MakeGenericType(types);
                case 3:
                    return typeof(Snapshot<,,>).MakeGenericType(types);
                case 4:
                    return typeof(Snapshot<,,,>).MakeGenericType(types);
                case 5:
                    return typeof(Snapshot<,,,,>).MakeGenericType(types);
                case 6:
                    return typeof(Snapshot<,,,,,>).MakeGenericType(types);
                case 7:
                    return typeof(Snapshot<,,,,,,>).MakeGenericType(types);
                case 8:
                    return typeof(Snapshot<,,,,,,,>).MakeGenericType(types);
                case 9:
                    return typeof(Snapshot<,,,,,,,,>).MakeGenericType(types);
                case 10:
                    return typeof(Snapshot<,,,,,,,,,>).MakeGenericType(types);
                case 11:
                    return typeof(Snapshot<,,,,,,,,,,>).MakeGenericType(types);
                case 12:
                    return typeof(Snapshot<,,,,,,,,,,,>).MakeGenericType(types);
                case 13:
                    return typeof(Snapshot<,,,,,,,,,,,,>).MakeGenericType(types);
                case 14:
                    return typeof(Snapshot<,,,,,,,,,,,,,>).MakeGenericType(types);
                case 15:
                    return typeof(Snapshot<,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 16:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 17:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 18:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 19:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 20:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 21:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 22:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 23:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 24:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 25:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 26:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 27:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 28:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 29:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case 30:
                    return typeof(Snapshot<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
            }

            throw new IndexOutOfRangeException();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29>
        : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21,
            [CanBeNull] T22 value22,
            [CanBeNull] T23 value23,
            [CanBeNull] T24 value24,
            [CanBeNull] T25 value25,
            [CanBeNull] T26 value26,
            [CanBeNull] T27 value27,
            [CanBeNull] T28 value28,
            [CanBeNull] T29 value29)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    case 22:
                        return _value22;
                    case 23:
                        return _value23;
                    case 24:
                        return _value24;
                    case 25:
                        return _value25;
                    case 26:
                        return _value26;
                    case 27:
                        return _value27;
                    case 28:
                        return _value28;
                    case 29:
                        return _value29;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
                        break;
                    case 22:
                        _value22 = (T22)value;
                        break;
                    case 23:
                        _value23 = (T23)value;
                        break;
                    case 24:
                        _value24 = (T24)value;
                        break;
                    case 25:
                        _value25 = (T25)value;
                        break;
                    case 26:
                        _value26 = (T26)value;
                        break;
                    case 27:
                        _value27 = (T27)value;
                        break;
                    case 28:
                        _value28 = (T28)value;
                        break;
                    case 29:
                        _value29 = (T29)value;
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
    public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28>
        : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21,
            [CanBeNull] T22 value22,
            [CanBeNull] T23 value23,
            [CanBeNull] T24 value24,
            [CanBeNull] T25 value25,
            [CanBeNull] T26 value26,
            [CanBeNull] T27 value27,
            [CanBeNull] T28 value28)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    case 22:
                        return _value22;
                    case 23:
                        return _value23;
                    case 24:
                        return _value24;
                    case 25:
                        return _value25;
                    case 26:
                        return _value26;
                    case 27:
                        return _value27;
                    case 28:
                        return _value28;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
                        break;
                    case 22:
                        _value22 = (T22)value;
                        break;
                    case 23:
                        _value23 = (T23)value;
                        break;
                    case 24:
                        _value24 = (T24)value;
                        break;
                    case 25:
                        _value25 = (T25)value;
                        break;
                    case 26:
                        _value26 = (T26)value;
                        break;
                    case 27:
                        _value27 = (T27)value;
                        break;
                    case 28:
                        _value28 = (T28)value;
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
    public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27>
        : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21,
            [CanBeNull] T22 value22,
            [CanBeNull] T23 value23,
            [CanBeNull] T24 value24,
            [CanBeNull] T25 value25,
            [CanBeNull] T26 value26,
            [CanBeNull] T27 value27)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    case 22:
                        return _value22;
                    case 23:
                        return _value23;
                    case 24:
                        return _value24;
                    case 25:
                        return _value25;
                    case 26:
                        return _value26;
                    case 27:
                        return _value27;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
                        break;
                    case 22:
                        _value22 = (T22)value;
                        break;
                    case 23:
                        _value23 = (T23)value;
                        break;
                    case 24:
                        _value24 = (T24)value;
                        break;
                    case 25:
                        _value25 = (T25)value;
                        break;
                    case 26:
                        _value26 = (T26)value;
                        break;
                    case 27:
                        _value27 = (T27)value;
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
    public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26>
        : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21,
            [CanBeNull] T22 value22,
            [CanBeNull] T23 value23,
            [CanBeNull] T24 value24,
            [CanBeNull] T25 value25,
            [CanBeNull] T26 value26)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    case 22:
                        return _value22;
                    case 23:
                        return _value23;
                    case 24:
                        return _value24;
                    case 25:
                        return _value25;
                    case 26:
                        return _value26;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
                        break;
                    case 22:
                        _value22 = (T22)value;
                        break;
                    case 23:
                        _value23 = (T23)value;
                        break;
                    case 24:
                        _value24 = (T24)value;
                        break;
                    case 25:
                        _value25 = (T25)value;
                        break;
                    case 26:
                        _value26 = (T26)value;
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
    public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25>
        : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21,
            [CanBeNull] T22 value22,
            [CanBeNull] T23 value23,
            [CanBeNull] T24 value24,
            [CanBeNull] T25 value25)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    case 22:
                        return _value22;
                    case 23:
                        return _value23;
                    case 24:
                        return _value24;
                    case 25:
                        return _value25;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
                        break;
                    case 22:
                        _value22 = (T22)value;
                        break;
                    case 23:
                        _value23 = (T23)value;
                        break;
                    case 24:
                        _value24 = (T24)value;
                        break;
                    case 25:
                        _value25 = (T25)value;
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
    public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24>
        : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21,
            [CanBeNull] T22 value22,
            [CanBeNull] T23 value23,
            [CanBeNull] T24 value24)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    case 22:
                        return _value22;
                    case 23:
                        return _value23;
                    case 24:
                        return _value24;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
                        break;
                    case 22:
                        _value22 = (T22)value;
                        break;
                    case 23:
                        _value23 = (T23)value;
                        break;
                    case 24:
                        _value24 = (T24)value;
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
    public sealed class Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>
        : ISnapshot
    {
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21,
            [CanBeNull] T22 value22,
            [CanBeNull] T23 value23)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    case 22:
                        return _value22;
                    case 23:
                        return _value23;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
                        break;
                    case 22:
                        _value22 = (T22)value;
                        break;
                    case 23:
                        _value23 = (T23)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21,
            [CanBeNull] T22 value22)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    case 22:
                        return _value22;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
                        break;
                    case 22:
                        _value22 = (T22)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20,
            [CanBeNull] T21 value21)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    case 21:
                        return _value21;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
                        break;
                    case 21:
                        _value21 = (T21)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19,
            [CanBeNull] T20 value20)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    case 20:
                        return _value20;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
                        break;
                    case 20:
                        _value20 = (T20)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18,
            [CanBeNull] T19 value19)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    case 19:
                        return _value19;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
                        break;
                    case 19:
                        _value19 = (T19)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17,
            [CanBeNull] T18 value18)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    case 18:
                        return _value18;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
                        break;
                    case 18:
                        _value18 = (T18)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16,
            [CanBeNull] T17 value17)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    case 17:
                        return _value17;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
                        break;
                    case 17:
                        _value17 = (T17)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15,
            [CanBeNull] T16 value16)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    case 16:
                        return _value16;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
                        break;
                    case 16:
                        _value16 = (T16)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14,
            [CanBeNull] T15 value15)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    case 15:
                        return _value15;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
                        break;
                    case 15:
                        _value15 = (T15)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13,
            [CanBeNull] T14 value14)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    case 14:
                        return _value14;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
                        break;
                    case 14:
                        _value14 = (T14)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12,
            [CanBeNull] T13 value13)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    case 13:
                        return _value13;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
                        break;
                    case 13:
                        _value13 = (T13)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11,
            [CanBeNull] T12 value12)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    case 12:
                        return _value12;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
                        break;
                    case 12:
                        _value12 = (T12)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10,
            [CanBeNull] T11 value11)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    case 11:
                        return _value11;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
                        break;
                    case 11:
                        _value11 = (T11)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9,
            [CanBeNull] T10 value10)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    case 10:
                        return _value10;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
                        break;
                    case 10:
                        _value10 = (T10)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8,
            [CanBeNull] T9 value9)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    case 9:
                        return _value9;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
                        break;
                    case 9:
                        _value9 = (T9)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7,
            [CanBeNull] T8 value8)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7, T8>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    case 8:
                        return _value8;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
                        break;
                    case 8:
                        _value8 = (T8)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6,
            [CanBeNull] T7 value7)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6, T7>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    case 7:
                        return _value7;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
                        break;
                    case 7:
                        _value7 = (T7)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5, T6>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5,
            [CanBeNull] T6 value6)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5, T6>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    case 6:
                        return _value6;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
                        break;
                    case 6:
                        _value6 = (T6)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4, T5>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4,
            [CanBeNull] T5 value5)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4, T5>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    case 5:
                        return _value5;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
                        break;
                    case 5:
                        _value5 = (T5)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3, T4>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3,
            [CanBeNull] T4 value4)
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
            => ((Func<Snapshot<T0, T1, T2, T3, T4>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    case 4:
                        return _value4;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
                        break;
                    case 4:
                        _value4 = (T4)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2, T3>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2,
            [CanBeNull] T3 value3)
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
            => ((Func<Snapshot<T0, T1, T2, T3>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    case 3:
                        return _value3;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
                        break;
                    case 3:
                        _value3 = (T3)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1, T2>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1,
            [CanBeNull] T2 value2)
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
            => ((Func<Snapshot<T0, T1, T2>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    case 2:
                        return _value2;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
                        break;
                    case 2:
                        _value2 = (T2)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0, T1>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0,
            [CanBeNull] T1 value1)
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
            => ((Func<Snapshot<T0, T1>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    case 1:
                        return _value1;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    case 1:
                        _value1 = (T1)value;
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
        private static readonly Delegate[] _valueReaders
            = Snapshot.CreateReaders<Snapshot<T0>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Snapshot(
            [CanBeNull] T0 value0)
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
            => ((Func<Snapshot<T0>, T>)_valueReaders[index])(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _value0;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _value0 = (T0)value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}
