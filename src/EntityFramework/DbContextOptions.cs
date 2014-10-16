// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity
{
    public class DbContextOptions : IDbContextOptionsExtensions
    {
        private const string EntityFrameworkKey = "EntityFramework";
        private const string KeySuffix = "Key";

        private bool _locked;
        private IModel _model;
        private readonly List<DbContextOptionsExtension> _extensions = new List<DbContextOptionsExtension>();
        private readonly IDictionary<string, string> _rawOptions = new Dictionary<string, string>(); 

        public virtual DbContextOptions UseModel([NotNull] IModel model)
        {
            Check.NotNull(model, "model");
            CheckNotLocked();

            _model = model;

            return this;
        }

        [CanBeNull]
        public virtual IModel Model
        {
            get { return _model; }
        }

        public virtual void Lock()
        {
            _locked = true;
        }

        public virtual bool IsLocked
        {
            get { return _locked; }
        }

        protected internal IDictionary<string, string> RawOptions
        {
            get { return _rawOptions; }
        }

        protected virtual void ReadRawOptions(
            [NotNull] IConfiguration configuration, [NotNull] Type contextType)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(contextType, "contextType");

            ReadRawOptions(configuration, string.Concat(
                EntityFrameworkKey, Constants.KeyDelimiter, contextType.Name));

            ReadRawOptions(configuration, string.Concat(
                EntityFrameworkKey, Constants.KeyDelimiter, contextType.FullName));
        }

        protected virtual void ReadRawOptions(
            [NotNull] IConfiguration configuration, [NotNull] string contextKey)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotEmpty(contextKey, "contextKey");

            foreach (var pair in configuration.GetSubKeys(contextKey))
            {
                string value;
                if (!pair.Value.TryGet(null, out value))
                {
                    continue;
                }

                var key = pair.Key;
                if (key.EndsWith(KeySuffix, StringComparison.Ordinal)
                    && configuration.TryGet(value, out value))
                {
                    key = key.Substring(0, key.Length - KeySuffix.Length);
                }

                _rawOptions[key] = value;
            }
        }

        private void CheckNotLocked([CallerMemberName] string memberName = "")
        {
            if (_locked)
            {
                throw new InvalidOperationException(Strings.FormatEntityConfigurationLocked(memberName));
            }
        }

        void IDbContextOptionsExtensions.AddOrUpdateExtension<TExtension>(Action<TExtension> updater, string memberName)
        {
            Check.NotNull(updater, "updater");
            CheckNotLocked(memberName);

            var extension = _extensions.OfType<TExtension>().FirstOrDefault();

            if (extension == null)
            {
                extension = new TExtension();
                extension.Configure(_rawOptions);
                _extensions.Add(extension);
            }

            updater(extension);
        }

        void IDbContextOptionsExtensions.AddExtension(DbContextOptionsExtension extension, string memberName)
        {
            Check.NotNull(extension, "extension");
            CheckNotLocked(memberName);

            Contract.Assert(_extensions.All(e => e.GetType() != extension.GetType()));

            extension.Configure(_rawOptions);
            _extensions.Add(extension);
        }

        IReadOnlyList<DbContextOptionsExtension> IDbContextOptionsExtensions.Extensions
        {
            get { return _extensions; }
        }
    }
}
