// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.Data.Entity.ChangeTracking
{
    internal struct StateData
    {
        private const int BitsPerInt = 32;
        private const int BitsForEntityState = 3;
        private const int EntityStateMask = 0x07;

        private readonly int[] _bits;

        public StateData(int propertyCount)
        {
            _bits = new int[(propertyCount + BitsForEntityState - 1) / BitsPerInt + 1];
        }

        public void SetAllPropertiesModified(int propertyCount)
        {
            for (var i = 0; i < _bits.Length; i++)
            {
                _bits[i] |= CreateMaskForWrite(i, propertyCount);
            }
        }

        public EntityState EntityState
        {
            get { return (EntityState)(_bits[0] & EntityStateMask); }
            set { _bits[0] = (_bits[0] & ~EntityStateMask) | (int)value; }
        }

        public bool IsPropertyModified(int propertyIndex)
        {
            propertyIndex += BitsForEntityState;

            return (_bits[propertyIndex / BitsPerInt] & (1 << propertyIndex % BitsPerInt)) != 0;
        }

        public void SetPropertyModified(int propertyIndex, bool isModified)
        {
            propertyIndex += BitsForEntityState;

            if (isModified)
            {
                _bits[propertyIndex / BitsPerInt] |= 1 << propertyIndex % BitsPerInt;
            }
            else
            {
                _bits[propertyIndex / BitsPerInt] &= ~(1 << propertyIndex % BitsPerInt);
            }
        }

        public bool AnyPropertiesModified()
        {
            return _bits.Where((t, i) => (t & CreateMaskForRead(i)) != 0).Any();
        }

        private static int CreateMaskForRead(int i)
        {
            var mask = unchecked(((int)0xFFFFFFFF));
            if (i == 0)
            {
                mask &= ~EntityStateMask;
            }

            // TODO: Remove keys/readonly indexes from the mask to avoid setting them to modified
            return mask;
        }

        private int CreateMaskForWrite(int i, int propertyCount)
        {
            var mask = CreateMaskForRead(i);

            if (i == _bits.Length - 1)
            {
                var overlay = unchecked(((int)0xFFFFFFFF));
                var shift = (propertyCount + BitsForEntityState) % BitsPerInt;
                overlay = shift != 0 ? overlay << shift : 0;
                mask &= ~overlay;
            }

            // TODO: Remove keys/readonly indexes from the mask to avoid setting them to modified
            return mask;
        }
    }
}
