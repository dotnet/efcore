// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public sealed partial class InternalEntityEntry
{
    internal enum PropertyFlag
    {
        Modified = 0,
        Null = 1,
        Unknown = 2,
        IsLoaded = 3,
        IsTemporary = 4,
        IsStoreGenerated = 5
    }

    internal readonly struct StateData
    {
        private const int BitsPerInt = 32;
        private const int BitsForEntityState = 3;
        private const int BitsForEntityFlags = 5;
        private const int BitsForPropertyFlags = 8;
        private const int BitsForAdditionalState = BitsForEntityState + BitsForEntityFlags;
        private const int EntityStateMask = 0x07;
        private const int UnusedStateMask = 0xF8; // So entity state uses even number of bits
        private const int AdditionalStateMask = EntityStateMask | UnusedStateMask;
        private const int PropertyFlagMask = 0x01010101;

        private readonly int[] _bits;

        public StateData(int propertyCount, int navigationCount)
        {
            var bitsNumber = Math.Max(propertyCount, navigationCount) * BitsForPropertyFlags + BitsForAdditionalState - 1;
            _bits = new int[(bitsNumber / BitsPerInt) + 1];
        }

        public void FlagAllProperties(int propertyCount, PropertyFlag propertyFlag, bool flagged)
        {
            for (var i = 0; i < _bits.Length; i++)
            {
                if (flagged)
                {
                    _bits[i] |= CreateMaskForWrite(i, propertyCount, propertyFlag);
                }
                else
                {
                    _bits[i] &= ~CreateMaskForWrite(i, propertyCount, propertyFlag);
                }
            }
        }

        public EntityState EntityState
        {
            get => (EntityState)(_bits[0] & EntityStateMask);
            set => _bits[0] = (_bits[0] & ~EntityStateMask) | (int)value;
        }

        public bool IsPropertyFlagged(int propertyIndex, PropertyFlag propertyFlag)
        {
            propertyIndex = propertyIndex * BitsForPropertyFlags + (int)propertyFlag + BitsForAdditionalState;

            return (_bits[propertyIndex / BitsPerInt] & (1 << (propertyIndex % BitsPerInt))) != 0;
        }

        public void FlagProperty(int propertyIndex, PropertyFlag propertyFlag, bool isFlagged)
        {
            propertyIndex = propertyIndex * BitsForPropertyFlags + (int)propertyFlag + BitsForAdditionalState;

            if (isFlagged)
            {
                _bits[propertyIndex / BitsPerInt] |= 1 << (propertyIndex % BitsPerInt);
            }
            else
            {
                _bits[propertyIndex / BitsPerInt] &= ~(1 << (propertyIndex % BitsPerInt));
            }
        }

        public bool AnyPropertiesFlagged(PropertyFlag propertyFlag)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < _bits.Length; i++)
            {
                var bit = _bits[i];
                if ((bit & CreateMaskForRead(i, propertyFlag)) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static int CreateMaskForRead(int i, PropertyFlag propertyFlag)
        {
            var mask = PropertyFlagMask << (int)propertyFlag;
            if (i == 0)
            {
                mask &= ~AdditionalStateMask;
            }

            return mask;
        }

        private int CreateMaskForWrite(int i, int propertyCount, PropertyFlag propertyFlag)
        {
            var mask = CreateMaskForRead(i, propertyFlag);

            if (i == _bits.Length - 1)
            {
                var overlay = PropertyFlagMask << (int)propertyFlag;
                var shift = (propertyCount * BitsForPropertyFlags + BitsForAdditionalState) % BitsPerInt;
                overlay = shift != 0 ? overlay << shift : 0;
                mask &= ~overlay;
            }

            return mask;
        }
    }
}
