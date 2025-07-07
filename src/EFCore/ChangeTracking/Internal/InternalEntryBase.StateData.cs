// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public partial class InternalEntryBase
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected internal enum PropertyFlag
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Modified = 0,

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Null = 1,

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Unknown = 2,

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IsLoaded = 3,
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IsTemporary = 4,
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IsStoreGenerated = 5
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected internal readonly struct StateData
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public StateData(int propertyCount, int navigationCount)
        {
            // Properties and navigations use different flags
            var bitsNumber = Math.Max(propertyCount, navigationCount) * BitsForPropertyFlags + BitsForAdditionalState - 1;
            _bits = new int[(bitsNumber / BitsPerInt) + 1];
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityState EntityState
        {
            get => (EntityState)(_bits[0] & EntityStateMask);
            set => _bits[0] = (_bits[0] & ~EntityStateMask) | (int)value;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public bool IsPropertyFlagged(int propertyIndex, PropertyFlag propertyFlag)
        {
            propertyIndex = propertyIndex * BitsForPropertyFlags + (int)propertyFlag + BitsForAdditionalState;

            return (_bits[propertyIndex / BitsPerInt] & (1 << (propertyIndex % BitsPerInt))) != 0;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
