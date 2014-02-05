// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Resources
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Strings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.Data.Entity.Resources", typeof(Strings).GetTypeInfo().Assembly);

        /// <summary>
        ///     "The argument '{0}' cannot be null, empty or contain only white space."
        /// </summary>
        internal static string ArgumentIsNullOrWhitespace(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ArgumentIsNullOrWhitespace"), p0);
        }

        /// <summary>
        ///     "The properties expression '{0}' is not valid. The expression should represent a property: C#: 't => t.MyProperty'
        ///     VB.Net: 'Function(t) t.MyProperty'. When specifying multiple properties use an anonymous type: C#: 't => new {{
        ///     t.MyProperty1, t.MyProperty2 }}'  VB.Net: 'Function(t) New With {{ t.MyProperty1, t.MyProperty2 }}'."
        /// </summary>
        internal static string InvalidPropertiesExpression(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("InvalidPropertiesExpression"), p0);
        }

        /// <summary>
        ///     "The expression '{0}' is not a valid property expression. The expression should represent a property: C#: 't =>
        ///     t.MyProperty'  VB.Net: 'Function(t) t.MyProperty'."
        /// </summary>
        internal static string InvalidPropertyExpression(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("InvalidPropertyExpression"), p0);
        }

        /// <summary>
        ///     "The EntityConfiguration property '{0}' has not been set."
        /// </summary>
        internal static string MissingConfigurationItem(object p0)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MissingConfigurationItem"), p0);
        }

        private static string GetString(string name)
        {
            return _resourceManager.GetString(name)
                   ?? "ERROR: Resource '" + name + "' NOT FOUND!";
        }
    }
}
