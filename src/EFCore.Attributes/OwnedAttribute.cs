using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Marks a type as owned. All references to this type will be configured as owned entity types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OwnedAttribute : Attribute
    {
    }
}
