// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        internal enum PropertyFlag
        {
            TemporaryOrModified = 0,
            Null = 1
        }

        internal struct StateData
        {
            private const int BitsPerInt = 32;
            private const int BitsForEntityState = 3;
            private const int BitsForEntityFlags = 1;
            private const int BitsForPropertyFlags = 2;
            private const int BitsForAdditionalState = BitsForEntityState + BitsForEntityFlags;
            private const int EntityStateMask = 0x07;
            private const int UnusedStateMask = 0x08; // So entity state uses even number of bits
            private const int AdditionalStateMask = EntityStateMask | UnusedStateMask;

            private readonly int[] _bits;

            public StateData(int propertyCount)
            {
                _bits = new int[(propertyCount * BitsForPropertyFlags + BitsForAdditionalState - 1) / BitsPerInt + 1];
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
                get { return (EntityState)(_bits[0] & EntityStateMask); }
                set { _bits[0] = (_bits[0] & ~EntityStateMask) | (int)value; }
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

            public bool AnyPropertiesFlagged(PropertyFlag propertyFlag) => _bits.Where((t, i) => (t & CreateMaskForRead(i, propertyFlag)) != 0).Any();

            private static int CreateMaskForRead(int i, PropertyFlag propertyFlag)
            {
                var mask = 0x55555555 << (int)propertyFlag;
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
                    var overlay = 0x55555555 << (int)propertyFlag;
                    var shift = (propertyCount * BitsForPropertyFlags + BitsForAdditionalState) % BitsPerInt;
                    overlay = shift != 0 ? overlay << shift : 0;
                    mask &= ~overlay;
                }

                return mask;
            }
        }
    }
}
