// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Sets the character set of a type (table), property or field (column) for MySQL.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class XGCharSetAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="XGCharSetAttribute" /> class.
        ///     Implicitly uses <see cref="Microsoft.EntityFrameworkCore.DelegationModes.ApplyToAll"/>.
        /// </summary>
        /// <param name="charSet"> The name of the character set to use. </param>
        public XGCharSetAttribute(string charSet)
            : this(charSet, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="XGCharSetAttribute" /> class.
        /// </summary>
        /// <param name="charSet"> The name of the character set to use. </param>
        /// <param name="delegationModes">
        /// Finely controls where to recursively apply the character set and where not.
        /// Ignored when <see cref="XGCharSetAttribute"/> is applied to properties/columns.
        /// </param>
        public XGCharSetAttribute(string charSet, DelegationModes delegationModes)
            : this(charSet, (DelegationModes?)delegationModes)
        {
        }

        protected XGCharSetAttribute(string charSet, DelegationModes? delegationModes)
        {
            CharSetName = charSet;
            DelegationModes = delegationModes;
        }

        /// <summary>
        ///     The name of the character set to use.
        /// </summary>
        public virtual string CharSetName { get; }

        /// <summary>
        /// Finely controls where to recursively apply the character set and where not.
        /// Implicitly uses <see cref="Microsoft.EntityFrameworkCore.DelegationModes.ApplyToAll"/> if set to <see langword="null"/>.
        /// Ignored when <see cref="XGCharSetAttribute"/> is applied to properties/columns.
        /// </summary>
        public virtual DelegationModes? DelegationModes { get; }
    }
}
