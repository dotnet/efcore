// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Sqlite
{
    partial class SqliteConnection
    {
        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<TAccumulate>(string name, Func<TAccumulate, TAccumulate> func)
            => CreateAggregateCore(name, 0, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a)), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, TAccumulate>(string name, Func<TAccumulate, T1, TAccumulate> func)
            => CreateAggregateCore(name, 1, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, TAccumulate>(string name, Func<TAccumulate, T1, T2, TAccumulate> func)
            => CreateAggregateCore(name, 2, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, TAccumulate> func)
            => CreateAggregateCore(name, 3, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, TAccumulate> func)
            => CreateAggregateCore(name, 4, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, TAccumulate> func)
            => CreateAggregateCore(name, 5, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, TAccumulate> func)
            => CreateAggregateCore(name, 6, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, TAccumulate> func)
            => CreateAggregateCore(name, 7, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, TAccumulate> func)
            => CreateAggregateCore(name, 8, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, TAccumulate> func)
            => CreateAggregateCore(name, 9, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TAccumulate> func)
            => CreateAggregateCore(name, 10, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TAccumulate> func)
            => CreateAggregateCore(name, 11, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TAccumulate> func)
            => CreateAggregateCore(name, 12, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TAccumulate> func)
            => CreateAggregateCore(name, 13, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="T14">The type of the fourteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TAccumulate> func)
            => CreateAggregateCore(name, 14, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12), r.GetFieldValue<T14>(13))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="T14">The type of the fourteenth parameter of the function.</typeparam>
        /// <typeparam name="T15">The type of the fifteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TAccumulate>(string name, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TAccumulate> func)
            => CreateAggregateCore(name, 15, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12), r.GetFieldValue<T14>(13), r.GetFieldValue<T15>(14))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<TAccumulate>(string name, Func<TAccumulate, object[], TAccumulate> func)
            => CreateAggregateCore(name, -1, default(TAccumulate), IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, GetValues(r))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, TAccumulate> func)
            => CreateAggregateCore(name, 0, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a)), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, TAccumulate> func)
            => CreateAggregateCore(name, 1, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, TAccumulate> func)
            => CreateAggregateCore(name, 2, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, TAccumulate> func)
            => CreateAggregateCore(name, 3, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, TAccumulate> func)
            => CreateAggregateCore(name, 4, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, TAccumulate> func)
            => CreateAggregateCore(name, 5, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, TAccumulate> func)
            => CreateAggregateCore(name, 6, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, TAccumulate> func)
            => CreateAggregateCore(name, 7, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, TAccumulate> func)
            => CreateAggregateCore(name, 8, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, TAccumulate> func)
            => CreateAggregateCore(name, 9, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TAccumulate> func)
            => CreateAggregateCore(name, 10, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TAccumulate> func)
            => CreateAggregateCore(name, 11, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TAccumulate> func)
            => CreateAggregateCore(name, 12, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TAccumulate> func)
            => CreateAggregateCore(name, 13, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="T14">The type of the fourteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TAccumulate> func)
            => CreateAggregateCore(name, 14, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12), r.GetFieldValue<T14>(13))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="T14">The type of the fourteenth parameter of the function.</typeparam>
        /// <typeparam name="T15">The type of the fifteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TAccumulate> func)
            => CreateAggregateCore(name, 15, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12), r.GetFieldValue<T14>(13), r.GetFieldValue<T15>(14))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        public virtual void CreateAggregate<TAccumulate>(string name, TAccumulate seed, Func<TAccumulate, object[], TAccumulate> func)
            => CreateAggregateCore(name, -1, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, GetValues(r))), a => a);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 0, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a)), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 1, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 2, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 3, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 4, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 5, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 6, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 7, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 8, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 9, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 10, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 11, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 12, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 13, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="T14">The type of the fourteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 14, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12), r.GetFieldValue<T14>(13))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter of the function.</typeparam>
        /// <typeparam name="T2">The type of the second parameter of the function.</typeparam>
        /// <typeparam name="T3">The type of the third parameter of the function.</typeparam>
        /// <typeparam name="T4">The type of the fourth parameter of the function.</typeparam>
        /// <typeparam name="T5">The type of the fifth parameter of the function.</typeparam>
        /// <typeparam name="T6">The type of the sixth parameter of the function.</typeparam>
        /// <typeparam name="T7">The type of the seventh parameter of the function.</typeparam>
        /// <typeparam name="T8">The type of the eighth parameter of the function.</typeparam>
        /// <typeparam name="T9">The type of the ninth parameter of the function.</typeparam>
        /// <typeparam name="T10">The type of the tenth parameter of the function.</typeparam>
        /// <typeparam name="T11">The type of the eleventh parameter of the function.</typeparam>
        /// <typeparam name="T12">The type of the twelfth parameter of the function.</typeparam>
        /// <typeparam name="T13">The type of the thirteenth parameter of the function.</typeparam>
        /// <typeparam name="T14">The type of the fourteenth parameter of the function.</typeparam>
        /// <typeparam name="T15">The type of the fifteenth parameter of the function.</typeparam>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, 15, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, r.GetFieldValue<T1>(0), r.GetFieldValue<T2>(1), r.GetFieldValue<T3>(2), r.GetFieldValue<T4>(3), r.GetFieldValue<T5>(4), r.GetFieldValue<T6>(5), r.GetFieldValue<T7>(6), r.GetFieldValue<T8>(7), r.GetFieldValue<T9>(8), r.GetFieldValue<T10>(9), r.GetFieldValue<T11>(10), r.GetFieldValue<T12>(11), r.GetFieldValue<T13>(12), r.GetFieldValue<T14>(13), r.GetFieldValue<T15>(14))), resultSelector);

        /// <summary>
        ///     Creates or redefines an aggregate SQL function.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="name">The name of the SQL function.</param>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func">An accumulator function to be invoked on each element. Pass null to delete a function.</param>
        /// <param name="resultSelector">
        ///     A function to transform the final accumulator value into the result value. Pass null to
        ///     delete a function.
        /// </param>
        public virtual void CreateAggregate<TAccumulate, TResult>(string name, TAccumulate seed, Func<TAccumulate, object[], TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            => CreateAggregateCore(name, -1, seed, IfNotNull<TAccumulate, TAccumulate>(func, (a, r) => func(a, GetValues(r))), resultSelector);
    }
}
