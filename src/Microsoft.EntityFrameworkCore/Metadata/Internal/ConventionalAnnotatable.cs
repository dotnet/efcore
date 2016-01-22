// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class ConventionalAnnotatable : Annotatable
    {
        public new virtual IEnumerable<ConventionalAnnotation> GetAnnotations() => base.GetAnnotations().Cast<ConventionalAnnotation>();

        public virtual ConventionalAnnotation AddAnnotation(
            [NotNull] string name, [NotNull] object value, ConfigurationSource configurationSource)
            => (ConventionalAnnotation)base.AddAnnotation(name, CreateAnnotation(name, value, configurationSource));

        public new virtual ConventionalAnnotation AddAnnotation([NotNull] string name, [NotNull] object value)
            => (ConventionalAnnotation)base.AddAnnotation(name, value);

        public virtual ConventionalAnnotation SetAnnotation(
            [NotNull] string name, [NotNull] object value, ConfigurationSource configurationSource)
            => (ConventionalAnnotation)base.SetAnnotation(name, CreateAnnotation(name, value, configurationSource));

        public new virtual ConventionalAnnotation GetOrAddAnnotation([NotNull] string name, [NotNull] object value)
            => (ConventionalAnnotation)base.GetOrAddAnnotation(name, value);

        public new virtual ConventionalAnnotation FindAnnotation([NotNull] string name)
            => (ConventionalAnnotation)base.FindAnnotation(name);

        public new virtual ConventionalAnnotation RemoveAnnotation([NotNull] string name)
            => (ConventionalAnnotation)base.RemoveAnnotation(name);

        private static ConventionalAnnotation CreateAnnotation(
            string name, object value, ConfigurationSource configurationSource)
            => new ConventionalAnnotation(name, value, configurationSource);

        protected override Annotation CreateAnnotation(string name, object value)
            => CreateAnnotation(name, value, ConfigurationSource.Explicit);
    }
}
