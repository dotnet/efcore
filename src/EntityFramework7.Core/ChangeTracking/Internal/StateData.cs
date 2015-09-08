// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract partial class InternalEntityEntry
    {
        internal struct StateData
        {
            private const int BitsPerInt = 32;
            private const int BitsForEntityState = 3;
            private const int BitsForFlags = 1;
            private const int BitsForAdditionalState = BitsForEntityState + BitsForFlags;
            private const int EntityStateMask = 0x07;
            private const int TransparentSidecarMask = 0x08;
            private const int AdditionalStateMask = EntityStateMask | TransparentSidecarMask;

            private readonly int[] _bits;

            public StateData(int propertyCount)
            {
                _bits = new int[(propertyCount + BitsForAdditionalState - 1) / BitsPerInt + 1];
            }

            public void FlagAllProperties(int propertyCount, bool isFlagged)
            {
                for (var i = 0; i < _bits.Length; i++)
                {
                    if (isFlagged)
                    {
                        _bits[i] |= CreateMaskForWrite(i, propertyCount);
                    }
                    else
                    {
                        _bits[i] &= ~CreateMaskForWrite(i, propertyCount);
                    }
                }
            }

            public EntityState EntityState
            {
                get { return (EntityState)(_bits[0] & EntityStateMask); }
                set { _bits[0] = (_bits[0] & ~EntityStateMask) | (int)value; }
            }

            public bool TransparentSidecarInUse
            {
                get { return (_bits[0] & TransparentSidecarMask) != 0; }
                set { _bits[0] = (_bits[0] & ~TransparentSidecarMask) | (value ? TransparentSidecarMask : 0); }
            }

            public bool IsPropertyFlagged(int propertyIndex)
            {
                propertyIndex += BitsForAdditionalState;

                return (_bits[propertyIndex / BitsPerInt] & (1 << propertyIndex % BitsPerInt)) != 0;
            }

            public void FlagProperty(int propertyIndex, bool isFlagged)
            {
                propertyIndex += BitsForAdditionalState;

                if (isFlagged)
                {
                    _bits[propertyIndex / BitsPerInt] |= 1 << propertyIndex % BitsPerInt;
                }
                else
                {
                    _bits[propertyIndex / BitsPerInt] &= ~(1 << propertyIndex % BitsPerInt);
                }
            }

            public bool AnyPropertiesFlagged() => _bits.Where((t, i) => (t & CreateMaskForRead(i)) != 0).Any();

            private static int CreateMaskForRead(int i)
            {
                var mask = unchecked(((int)0xFFFFFFFF));
                if (i == 0)
                {
                    mask &= ~AdditionalStateMask;
                }

                return mask;
            }

            private int CreateMaskForWrite(int i, int propertyCount)
            {
                var mask = CreateMaskForRead(i);

                if (i == _bits.Length - 1)
                {
                    var overlay = unchecked(((int)0xFFFFFFFF));
                    var shift = (propertyCount + BitsForAdditionalState) % BitsPerInt;
                    overlay = shift != 0 ? overlay << shift : 0;
                    mask &= ~overlay;
                }

                return mask;
            }
        }
    }
}
