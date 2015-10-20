// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable 0169 

using Microsoft.Data.Entity.Storage;
using System;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Utilities
{
    // This code exists only to trick the ILC compliation to include metadata 
    // about combinations of entity types and our internal types.
    // This is the jumping off point for a reasoning about what generic types
    // may exist at runtime.
    // https://github.com/aspnet/EntityFramework/issues/3477
    internal class ImpliedEntityType<TEntity>
        where TEntity : class
    {
        InternalImplyValuesAndTypes<TEntity> EntityValueProp;

        InternalImpliesTypes<TEntity> EntityProp;
        InternalImpliesTypes<char> CharProp;
        InternalImpliesTypes<short> Int16Prop;
        InternalImpliesTypes<ushort> UInt16Prop;
        InternalImpliesTypes<int> Int32Prop;
        InternalImpliesTypes<uint> UInt32Prop;
        InternalImpliesTypes<long> Int64Prop;
        InternalImpliesTypes<long> UInt64Prop;
        InternalImpliesTypes<double> DoubleProp;
        InternalImpliesTypes<decimal> DecimalProp;
        InternalImpliesTypes<float> FloatProp;
        InternalImpliesTypes<string> StringProp;
        InternalImpliesTypes<bool> BoolProp;
        InternalImpliesTypes<byte> ByteProp;
        InternalImpliesTypes<Guid> GuidProp;
        InternalImpliesTypes<TimeSpan> TimeSpanProp;
        InternalImpliesTypes<DateTime> DateTimeProp;
        InternalImpliesTypes<DateTimeOffset> DateTimeOffsetProp;
    }

    internal class InternalImpliesTypes<T>
    {
        void CompileQuery()
         => ((IDatabase)new object()).CompileQuery<T>(null);

        SimpleEntityKeyFactory<T> KeyFactoryType;
    }

    internal class InternalImplyTypes<TSystemType, TUserType>
        where TUserType : class
    {
        ClrPropertyGetter<TUserType, TSystemType> ClrPropertyGetter;
        ClrPropertySetter<TUserType, TSystemType> ClrPropertySetter;
    }

    internal class InternalImplyValuesAndTypes<TEntity>
        where TEntity : class
    {
        InternalImplyTypes<char, TEntity> CharProp;
        InternalImplyTypes<short, TEntity> Int16Prop;
        InternalImplyTypes<ushort, TEntity> UInt16Prop;
        InternalImplyTypes<int, TEntity> Int32Prop;
        InternalImplyTypes<uint, TEntity> UInt32Prop;
        InternalImplyTypes<long, TEntity> Int64Prop;
        InternalImplyTypes<long, TEntity> UInt64Prop;
        InternalImplyTypes<double, TEntity> DoubleProp;
        InternalImplyTypes<decimal, TEntity> DecimalProp;
        InternalImplyTypes<float, TEntity> FloatProp;
        InternalImplyTypes<string, TEntity> StringProp;
        InternalImplyTypes<bool, TEntity> BoolProp;
        InternalImplyTypes<byte, TEntity> ByteProp;
        InternalImplyTypes<Guid, TEntity> GuidProp;
        InternalImplyTypes<TimeSpan, TEntity> TimeSpanProp;
        InternalImplyTypes<DateTime, TEntity> DateTimeProp;
        InternalImplyTypes<DateTimeOffset, TEntity> DateTimeOffsetProp;
    }
}
