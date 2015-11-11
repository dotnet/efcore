// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#pragma warning disable 0169 

using System;

namespace Microsoft.Data.Entity.Utilities
{
    internal partial class RelationalImplyTypes
    {
        // value types
        private ImplyGeneric<char> Prop1;
        private ImplyGeneric<char?> Prop2;
        private ImplyGeneric<short> Prop3;
        private ImplyGeneric<short?> Prop4;
        private ImplyGeneric<ushort> Prop5;
        private ImplyGeneric<ushort?> Prop6;
        private ImplyGeneric<int> Prop7;
        private ImplyGeneric<int?> Prop8;
        private ImplyGeneric<uint> Prop9;
        private ImplyGeneric<uint?> Prop10;
        private ImplyGeneric<long> Prop11;
        private ImplyGeneric<long?> Prop12;
        private ImplyGeneric<ulong> Prop13;
        private ImplyGeneric<ulong?> Prop14;
        private ImplyGeneric<double> Prop15;
        private ImplyGeneric<double?> Prop16;
        private ImplyGeneric<decimal> Prop17;
        private ImplyGeneric<decimal?> Prop18;
        private ImplyGeneric<float> Prop19;
        private ImplyGeneric<float?> Prop20;
        private ImplyGeneric<bool> Prop21;
        private ImplyGeneric<bool?> Prop22;
        private ImplyGeneric<byte> Prop23;
        private ImplyGeneric<byte?> Prop24;
        private ImplyGeneric<Guid> Prop25;
        private ImplyGeneric<Guid?> Prop26;
        private ImplyGeneric<TimeSpan> Prop27;
        private ImplyGeneric<TimeSpan?> Prop28;
        private ImplyGeneric<DateTime> Prop29;
        private ImplyGeneric<DateTime?> Prop30;
        private ImplyGeneric<DateTimeOffset> Prop31;
        private ImplyGeneric<DateTimeOffset?> Prop32;
    }

    internal partial class ImplyGeneric<T>
    {
        // system objects
        private ImplyGeneric<object, T> Prop33;
        private ImplyGeneric<T, object> Prop34;
        private ImplyGeneric<string, T> Prop35;
        private ImplyGeneric<T, string> Prop36;

        private ImplyGeneric<char, T> Prop37;
        private ImplyGeneric<char?, T> Prop38;
        private ImplyGeneric<T, char> Prop39;
        private ImplyGeneric<T, char?> Prop40;
        private ImplyGeneric<short, T> Prop41;
        private ImplyGeneric<short?, T> Prop42;
        private ImplyGeneric<T, short> Prop43;
        private ImplyGeneric<T, short?> Prop44;
        private ImplyGeneric<ushort, T> Prop45;
        private ImplyGeneric<ushort?, T> Prop46;
        private ImplyGeneric<T, ushort> Prop47;
        private ImplyGeneric<T, ushort?> Prop48;
        private ImplyGeneric<int, T> Prop49;
        private ImplyGeneric<int?, T> Prop50;
        private ImplyGeneric<T, int> Prop51;
        private ImplyGeneric<T, int?> Prop52;
        private ImplyGeneric<uint, T> Prop53;
        private ImplyGeneric<uint?, T> Prop54;
        private ImplyGeneric<T, uint> Prop55;
        private ImplyGeneric<T, uint?> Prop56;
        private ImplyGeneric<long, T> Prop57;
        private ImplyGeneric<long?, T> Prop58;
        private ImplyGeneric<T, long> Prop59;
        private ImplyGeneric<T, long?> Prop60;
        private ImplyGeneric<ulong, T> Prop61;
        private ImplyGeneric<ulong?, T> Prop62;
        private ImplyGeneric<T, ulong> Prop63;
        private ImplyGeneric<T, ulong?> Prop64;
        private ImplyGeneric<double, T> Prop65;
        private ImplyGeneric<double?, T> Prop66;
        private ImplyGeneric<T, double> Prop67;
        private ImplyGeneric<T, double?> Prop68;
        private ImplyGeneric<decimal, T> Prop69;
        private ImplyGeneric<decimal?, T> Prop70;
        private ImplyGeneric<T, decimal> Prop71;
        private ImplyGeneric<T, decimal?> Prop72;
        private ImplyGeneric<float, T> Prop73;
        private ImplyGeneric<float?, T> Prop74;
        private ImplyGeneric<T, float> Prop75;
        private ImplyGeneric<T, float?> Prop76;
        private ImplyGeneric<bool, T> Prop77;
        private ImplyGeneric<bool?, T> Prop78;
        private ImplyGeneric<T, bool> Prop79;
        private ImplyGeneric<T, bool?> Prop80;
        private ImplyGeneric<byte, T> Prop81;
        private ImplyGeneric<byte?, T> Prop82;
        private ImplyGeneric<T, byte> Prop83;
        private ImplyGeneric<T, byte?> Prop84;
        private ImplyGeneric<Guid, T> Prop85;
        private ImplyGeneric<Guid?, T> Prop86;
        private ImplyGeneric<T, Guid> Prop87;
        private ImplyGeneric<T, Guid?> Prop88;
        private ImplyGeneric<TimeSpan, T> Prop89;
        private ImplyGeneric<TimeSpan?, T> Prop90;
        private ImplyGeneric<T, TimeSpan> Prop91;
        private ImplyGeneric<T, TimeSpan?> Prop92;
        private ImplyGeneric<DateTime, T> Prop93;
        private ImplyGeneric<DateTime?, T> Prop94;
        private ImplyGeneric<T, DateTime> Prop95;
        private ImplyGeneric<T, DateTime?> Prop96;
        private ImplyGeneric<DateTimeOffset, T> Prop97;
        private ImplyGeneric<DateTimeOffset?, T> Prop98;
        private ImplyGeneric<T, DateTimeOffset> Prop99;
        private ImplyGeneric<T, DateTimeOffset?> Prop100;
    }
}
