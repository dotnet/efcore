// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGu.Infrastructure
{
    public class CharSet
    {
        public virtual string Name { get; }
        public virtual int MaxBytesPerChar { get; }

        public CharSet(string name, int maxBytesPerChar)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name.ToLowerInvariant();
            MaxBytesPerChar = maxBytesPerChar > 0 ? maxBytesPerChar : throw new ArgumentOutOfRangeException(nameof(maxBytesPerChar));
        }

        public static implicit operator string(CharSet charSet) => charSet?.ToString();

        public virtual bool IsUnicode => MaxBytesPerChar >= 2;

        public override string ToString() => Name;

        protected virtual bool Equals(CharSet other)
        {
            return Name == other.Name &&
                   MaxBytesPerChar == other.MaxBytesPerChar;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((CharSet) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ MaxBytesPerChar;
            }
        }

        #region Predefined charsets

        public static CharSet ArmScii8 = new CharSet(nameof(ArmScii8), 1);
        public static CharSet Ascii = new CharSet(nameof(Ascii), 1);
        public static CharSet Big5 = new CharSet(nameof(Big5), 2);
        public static CharSet Binary = new CharSet(nameof(Binary), 1);
        public static CharSet Cp1250 = new CharSet(nameof(Cp1250), 1);
        public static CharSet Cp1251 = new CharSet(nameof(Cp1251), 1);
        public static CharSet Cp1256 = new CharSet(nameof(Cp1256), 1);
        public static CharSet Cp1257 = new CharSet(nameof(Cp1257), 1);
        public static CharSet Cp850 = new CharSet(nameof(Cp850), 1);
        public static CharSet Cp852 = new CharSet(nameof(Cp852), 1);
        public static CharSet Cp866 = new CharSet(nameof(Cp866), 1);
        public static CharSet Cp932 = new CharSet(nameof(Cp932), 2);
        public static CharSet Dec8 = new CharSet(nameof(Dec8), 1);
        public static CharSet EucJpMs = new CharSet(nameof(EucJpMs), 3);
        public static CharSet EucKr = new CharSet(nameof(EucKr), 2);
        public static CharSet Gb18030 = new CharSet(nameof(Gb18030), 4);
        public static CharSet Gb2312 = new CharSet(nameof(Gb2312), 2);
        public static CharSet Gbk = new CharSet(nameof(Gbk), 2);
        public static CharSet GeoStd8 = new CharSet(nameof(GeoStd8), 1);
        public static CharSet Greek = new CharSet(nameof(Greek), 1);
        public static CharSet Hebrew = new CharSet(nameof(Hebrew), 1);
        public static CharSet Hp8 = new CharSet(nameof(Hp8), 1);
        public static CharSet KeyBcs2 = new CharSet(nameof(KeyBcs2), 1);
        public static CharSet Koi8R = new CharSet(nameof(Koi8R), 1);
        public static CharSet Koi8U = new CharSet(nameof(Koi8U), 1);
        public static CharSet Latin1 = new CharSet(nameof(Latin1), 1);
        public static CharSet Latin2 = new CharSet(nameof(Latin2), 1);
        public static CharSet Latin5 = new CharSet(nameof(Latin5), 1);
        public static CharSet Latin7 = new CharSet(nameof(Latin7), 1);
        public static CharSet MacCe = new CharSet(nameof(MacCe), 1);
        public static CharSet MacRoman = new CharSet(nameof(MacRoman), 1);
        public static CharSet SJis = new CharSet(nameof(SJis), 2);
        public static CharSet Swe7 = new CharSet(nameof(Swe7), 1);
        public static CharSet Tis620 = new CharSet(nameof(Tis620), 1);
        public static CharSet Ucs2 = new CharSet(nameof(Ucs2), 2);
        public static CharSet UJis = new CharSet(nameof(UJis), 3);
        public static CharSet Utf16 = new CharSet(nameof(Utf16), 4);
        public static CharSet Utf16Le = new CharSet(nameof(Utf16Le), 4);
        public static CharSet Utf32 = new CharSet(nameof(Utf32), 4);
        [Obsolete("Use 'Utf8Mb4' instead.")] public static CharSet Utf8 = new CharSet(nameof(Utf8), 3);
        public static CharSet Utf8Mb3 = new CharSet(nameof(Utf8Mb3), 3); // Alias for "utf8"
        public static CharSet Utf8Mb4 = new CharSet(nameof(Utf8Mb4), 4);

        #endregion

        public static CharSet GetCharSetFromName(string name)
            => (CharSet)GetFieldInfoFromName(name)?.GetValue(null);

        internal static FieldInfo GetFieldInfoFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return typeof(CharSet)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .SingleOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
