// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ConventionAnnotatable : Annotatable, IConventionAnnotatable
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual IEnumerable<ConventionAnnotation> GetAnnotations() => base.GetAnnotations().Cast<ConventionAnnotation>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionAnnotation AddAnnotation(
            [NotNull] string name, [CanBeNull] object value, ConfigurationSource configurationSource)
            => (ConventionAnnotation)base.AddAnnotation(name, CreateAnnotation(name, value, configurationSource));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionAnnotation SetAnnotation(
            [NotNull] string name, [CanBeNull] object value, ConfigurationSource configurationSource)
            => (ConventionAnnotation)base.SetAnnotation(name, CreateAnnotation(name, value, configurationSource));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ConventionAnnotation FindAnnotation(string name)
            => (ConventionAnnotation)base.FindAnnotation(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ConventionAnnotation RemoveAnnotation(string name)
            => (ConventionAnnotation)base.RemoveAnnotation(name);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Annotation CreateAnnotation(string name, object value)
            => CreateAnnotation(name, value, ConfigurationSource.Explicit);

        private static ConventionAnnotation CreateAnnotation(
            string name, object value, ConfigurationSource configurationSource)
            => new ConventionAnnotation(name, value, configurationSource);

        IEnumerable<IConventionAnnotation> IConventionAnnotatable.GetAnnotations() => GetAnnotations();

        void IConventionAnnotatable.SetAnnotation(string name, object value, bool fromDataAnnotation)
            => SetAnnotation(name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        IConventionAnnotation IConventionAnnotatable.AddAnnotation(string name, object value, bool fromDataAnnotation)
            => AddAnnotation(name, value, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        IConventionAnnotation IConventionAnnotatable.FindAnnotation(string name) => FindAnnotation(name);

        IConventionAnnotation IConventionAnnotatable.RemoveAnnotation(string name) => RemoveAnnotation(name);
    }
}
