// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Configures the precision of data that is allowed in this property.
///     For example, if the property is a <see cref="decimal" />
///     then this is the maximum number of digits.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PrecisionAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PrecisionAttribute" /> class.
    /// </summary>
    /// <param name="precision">The precision of the property.</param>
    /// <param name="scale">The scale of the property.</param>
    public PrecisionAttribute(int precision, int scale)
    {
        if (precision < 0)
        {
            throw new ArgumentException(AbstractionsStrings.ArgumentIsNegativeNumber(nameof(precision)));
        }

        if (scale < 0)
        {
            throw new ArgumentException(AbstractionsStrings.ArgumentIsNegativeNumber(nameof(scale)));
        }

        Precision = precision;
        Scale = scale;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PrecisionAttribute" /> class.
    /// </summary>
    /// <param name="precision">The precision of the property.</param>
    public PrecisionAttribute(int precision)
    {
        if (precision < 0)
        {
            throw new ArgumentException(AbstractionsStrings.ArgumentIsNegativeNumber(nameof(precision)));
        }

        Precision = precision;
    }

    /// <summary>
    ///     The precision of the property.
    /// </summary>
    public int Precision { get; }

    /// <summary>
    ///     The scale of the property.
    /// </summary>
    public int? Scale { get; }
}
