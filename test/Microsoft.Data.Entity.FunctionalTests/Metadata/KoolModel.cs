// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Compiled;

namespace Microsoft.Data.Entity.FunctionalTests.Metadata
{
    public class KoolEntity1 // 0
    {
        public KoolEntity1()
        {
            NavTo2s = new List<KoolEntity2>();
        }

        public int Id1 { get; set; } // 2 ****
        public Guid Id2 { get; set; } // 3 ****

        public string Foo1 { get; set; } // 0 ****
        public Guid Goo1 { get; set; } // 1 ****

        public int KoolEntity2Id { get; set; } // 4 **** FK1 D:0.4 P:11.2
        public KoolEntity2 NavTo2 { get; set; } // 0 **** Nav1 E:0 F:0.0

        public ICollection<KoolEntity2> NavTo2s { get; set; } // 1 **** Nav2 E:0 F:11.0
    }

    public class KoolEntity2 // 11
    {
        public KoolEntity2()
        {
            NavTo1s = new List<KoolEntity1>();
        }

        public int Id { get; set; } // 2 ****
        public string Foo2 { get; set; } // 0 ****
        public Guid Goo2 { get; set; } // 1 ****

        public int KoolEntity1Id1 { get; set; } // 3 **** FK1 D:11.3/4 P:0.2/3
        public Guid KoolEntity1Id2 { get; set; } // 4 **** FK1
        public KoolEntity1 NavTo1 { get; set; } // 0 **** Nav1 E:11 F:11.0

        public int KoolEntity3Id { get; set; } // 5 **** FK2 D:11.5 P:13.2
        public KoolEntity3 NavTo3 { get; set; } // 1 **** Nav3 E:11 F:11.1

        public ICollection<KoolEntity1> NavTo1s { get; set; } // 2 **** Nav2 E:11 F:0.0
    }

    public class KoolEntity3 // 13
    {
        public KoolEntity3()
        {
            NavTo2s = new List<KoolEntity2>();
        }

        public int Id { get; set; } // 2 ****
        public string Foo3 { get; set; } // 0 ****
        public Guid Goo3 { get; set; } // 1 ****

        public int KoolEntity4Id { get; set; } // 3 **** FK1 D:13.3 P14.2
        public KoolEntity4 NavTo4 { get; set; } // 1 **** Nav2 E:13 F:13.0

        public ICollection<KoolEntity2> NavTo2s { get; set; } // 0 **** Nav1 E:13 F:11.1
    }

    public class KoolEntity4 // 14
    {
        public KoolEntity4()
        {
            NavTo3s = new List<KoolEntity3>();
        }

        public int Id { get; set; } // 2 ****
        public string Foo4 { get; set; } // 0 ****
        public Guid Goo4 { get; set; } // 1 ****

        public ICollection<KoolEntity3> NavTo3s { get; set; } // 0 **** Nav1 E:14 F:13.0
    }

    public class KoolEntity5 // 15
    {
        private readonly ISet<KoolEntity6> _kool6s = new HashSet<KoolEntity6>();

        public int Id { get; set; } // 2 ****
        public string Foo5 { get; set; } // 0 ****
        public Guid Goo5 { get; set; } // 1 ****

        public void AddKool6(KoolEntity6 kool6)
        {
            _kool6s.Add(kool6);
        }

        public void RemoveKool6(KoolEntity6 kool6)
        {
            _kool6s.Remove(kool6);
        }

        public IEnumerable<KoolEntity6> Kool6s // 0 **** Nav1 E:15 F16.0
        {
            get { return _kool6s; }
        }
    }

    public class KoolEntity6 // 16
    {
        public int Id { get; set; } // 2 ****
        public string Foo6 { get; set; } // 0 ****
        public Guid Goo6 { get; set; } // 1 ****

        public int Kool5Id { get; set; } // 3 **** FK1 D:16.3 P15.2
        public KoolEntity5 Kool5 { get; set; } // 0 **** Nav1 E:16 F16.0
    }

    public class KoolEntity7
    {
        public int Id { get; set; }
        public string Foo7 { get; set; }
        public Guid Goo7 { get; set; }
    }

    public class KoolEntity8
    {
        public int Id { get; set; }
        public string Foo8 { get; set; }
        public Guid Goo8 { get; set; }
    }

    public class KoolEntity9
    {
        public int Id { get; set; }
        public string Foo9 { get; set; }
        public Guid Goo9 { get; set; }
    }

    public class KoolEntity10
    {
        public int Id { get; set; }
        public string Foo10 { get; set; }
        public Guid Goo10 { get; set; }
    }

    public class KoolEntity11
    {
        public int Id { get; set; }
        public string Foo11 { get; set; }
        public Guid Goo11 { get; set; }
    }

    public class KoolEntity12
    {
        public int Id { get; set; }
        public string Foo12 { get; set; }
        public Guid Goo12 { get; set; }
    }

    public class KoolEntity13
    {
        public int Id { get; set; }
        public string Foo13 { get; set; }
        public Guid Goo13 { get; set; }
    }

    public class KoolEntity14
    {
        public int Id { get; set; }
        public string Foo14 { get; set; }
        public Guid Goo14 { get; set; }
    }

    public class KoolEntity15
    {
        private int _id;
        private string _foo15;
        private Guid _goo15;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Foo15
        {
            get { return _foo15; }
            set { _foo15 = value; }
        }

        public Guid Goo15
        {
            get { return _goo15; }
            set { _goo15 = value; }
        }

        // ReSharper disable once InconsistentNaming
        public static KoolEntity15 _EntityFramework_Create(object[] valueBuffer)
        {
            return new KoolEntity15
                {
                    _id = (int)valueBuffer[2],
                    _foo15 = (string)valueBuffer[0],
                    _goo15 = (Guid)valueBuffer[1]
                };
        }
    }

    public class KoolEntity16
    {
        public int Id { get; set; }
        public string Foo16 { get; set; }
        public Guid Goo16 { get; set; }
    }

    public class KoolEntity17
    {
        public int Id { get; set; }
        public string Foo17 { get; set; }
        public Guid Goo17 { get; set; }
    }

    public class KoolEntity18
    {
        public int Id { get; set; }
        public string Foo18 { get; set; }
        public Guid Goo18 { get; set; }
    }

    public class KoolEntity19
    {
        public int Id { get; set; }
        public string Foo19 { get; set; }
        public Guid Goo19 { get; set; }
    }

    public class KoolEntity20
    {
        public int Id { get; set; }
        public string Foo20 { get; set; }
        public Guid Goo20 { get; set; }
    }

    // Proposed generated code below
    // ReSharper disable InconsistentNaming

    public class _OneTwoThreeContextModel : CompiledModel, IModel
    {
        protected override IEntityType[] LoadEntityTypes()
        {
            return new IEntityType[]
                {
                    new _KoolEntity1EntityType(this), // 0
                    new _KoolEntity10EntityType(this), // 1
                    new _KoolEntity11EntityType(this), // 2
                    new _KoolEntity12EntityType(this), // 3
                    new _KoolEntity13EntityType(this), // 4
                    new _KoolEntity14EntityType(this), // 5
                    new _KoolEntity15EntityType(this), // 6
                    new _KoolEntity16EntityType(this), // 7 
                    new _KoolEntity17EntityType(this), // 8
                    new _KoolEntity18EntityType(this), // 9
                    new _KoolEntity19EntityType(this), // 10
                    new _KoolEntity2EntityType(this), // 11
                    new _KoolEntity20EntityType(this), // 12
                    new _KoolEntity3EntityType(this), // 13
                    new _KoolEntity4EntityType(this), // 14
                    new _KoolEntity5EntityType(this), // 15
                    new _KoolEntity6EntityType(this), // 16
                    new _KoolEntity7EntityType(this), // 17
                    new _KoolEntity8EntityType(this), // 18
                    new _KoolEntity9EntityType(this) // 19
                };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "ModelAnnotation1", "ModelAnnotation2" },
                new[] { "ModelValue1", "ModelValue2" }).ToArray();
        }
    }

    public class _KoolEntity1EntityType : CompiledEntityType<KoolEntity1>, IEntityType
    {
        public _KoolEntity1EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity1"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity1Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity1Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[]
                {
                    new _KoolEntity1Foo1Property(this),
                    new _KoolEntity1Goo1Property(this),
                    new _KoolEntity1Id1Property(this),
                    new _KoolEntity1Id2Property(this),
                    new _KoolEntity1KoolEntity2IdProperty(this)
                };
        }

        protected override IForeignKey[] LoadForeignKeys()
        {
            return new IForeignKey[] { new _KoolEntity1Fk1(Model) };
        }

        protected override INavigation[] LoadNavigations()
        {
            return new INavigation[] { new _KoolEntity1NavTo2(Model), new _KoolEntity1NavTo2s(Model) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity1Key : CompiledKey, IKey
    {
        public _KoolEntity1Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(0, new short[] { 2 }); }
        }
    }

    public class _KoolEntity2EntityType : CompiledEntityType<KoolEntity2>, IEntityType
    {
        public _KoolEntity2EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity2"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity2Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity2Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[]
                {
                    new _KoolEntity2Foo2Property(this),
                    new _KoolEntity2Goo2Property(this),
                    new _KoolEntity2IdProperty(this),
                    new _KoolEntity2KoolEntity1Id1Property(this),
                    new _KoolEntity2KoolEntity1Id2operty(this),
                    new _KoolEntity2KoolEntity3IdProperty(this)
                };
        }

        protected override IForeignKey[] LoadForeignKeys()
        {
            return new IForeignKey[] { new _KoolEntity2Fk1(Model), new _KoolEntity2Fk2(Model) };
        }

        protected override INavigation[] LoadNavigations()
        {
            return new INavigation[] { new _KoolEntity2NavTo1(Model), new _KoolEntity2NavTo1s(Model), new _KoolEntity2NavTo3(Model) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity2Key : CompiledKey, IKey
    {
        public _KoolEntity2Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(11, new short[] { 2 }); }
        }
    }

    public class _KoolEntity3EntityType : CompiledEntityType<KoolEntity3>, IEntityType
    {
        public _KoolEntity3EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity3"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity3Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity3Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[]
                {
                    new _KoolEntity3Foo3Property(this),
                    new _KoolEntity3Goo3Property(this),
                    new _KoolEntity3IdProperty(this),
                    new _KoolEntity3KoolEntity4IdProperty(this)
                };
        }

        protected override IForeignKey[] LoadForeignKeys()
        {
            return new IForeignKey[] { new _KoolEntity3Fk1(Model) };
        }

        protected override INavigation[] LoadNavigations()
        {
            return new INavigation[] { new _KoolEntity3NavTo2s(Model), new _KoolEntity3NavTo4(Model) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity3Key : CompiledKey, IKey
    {
        public _KoolEntity3Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(13, new short[] { 2 }); }
        }
    }

    public class _KoolEntity4EntityType : CompiledEntityType<KoolEntity4>, IEntityType
    {
        public _KoolEntity4EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity4"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity4Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity4Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity4Foo4Property(this), new _KoolEntity4Goo4Property(this), new _KoolEntity4IdProperty(this) };
        }

        protected override INavigation[] LoadNavigations()
        {
            return new INavigation[] { new _KoolEntity4NavTo3s(Model) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity4Key : CompiledKey, IKey
    {
        public _KoolEntity4Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(14, new short[] { 2 }); }
        }
    }

    public class _KoolEntity5EntityType : CompiledEntityType<KoolEntity5>, IEntityType
    {
        public _KoolEntity5EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity5"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity5Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity5Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity5Foo5Property(this), new _KoolEntity5Goo5Property(this), new _KoolEntity5IdProperty(this) };
        }

        protected override INavigation[] LoadNavigations()
        {
            return new INavigation[] { new _KoolEntity5NavTo6s(Model) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity5Key : CompiledKey, IKey
    {
        public _KoolEntity5Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(15, new short[] { 2 }); }
        }
    }

    public class _KoolEntity6EntityType : CompiledEntityType<KoolEntity6>, IEntityType
    {
        public _KoolEntity6EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity6"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity6Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity6Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[]
                {
                    new _KoolEntity6Foo6Property(this), 
                    new _KoolEntity6Goo6Property(this), 
                    new _KoolEntity6IdProperty(this), 
                    new _KoolEntity6Kool5IdProperty(this)
                };
        }

        protected override IForeignKey[] LoadForeignKeys()
        {
            return new IForeignKey[] { new _KoolEntity6Fk1(Model) };
        }

        protected override INavigation[] LoadNavigations()
        {
            return new INavigation[] { new _KoolEntity6NavTo5(Model) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity6Key : CompiledKey, IKey
    {
        public _KoolEntity6Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(16, new short[] { 2 }); }
        }
    }

    public class _KoolEntity7EntityType : CompiledEntityType<KoolEntity7>, IEntityType
    {
        public _KoolEntity7EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity7"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity7Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity7Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity7Foo7Property(this), new _KoolEntity7Goo7Property(this), new _KoolEntity7IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity7Key : CompiledKey, IKey
    {
        public _KoolEntity7Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(17, new short[] { 2 }); }
        }
    }

    public class _KoolEntity8EntityType : CompiledEntityType<KoolEntity8>, IEntityType
    {
        public _KoolEntity8EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity8"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity8Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity8Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity8Foo8Property(this), new _KoolEntity8Goo8Property(this), new _KoolEntity8IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity8Key : CompiledKey, IKey
    {
        public _KoolEntity8Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(18, new short[] { 2 }); }
        }
    }

    public class _KoolEntity9EntityType : CompiledEntityType<KoolEntity9>, IEntityType
    {
        public _KoolEntity9EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity9"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity9Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity9Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity9Foo9Property(this), new _KoolEntity9Goo9Property(this), new _KoolEntity9IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity9Key : CompiledKey, IKey
    {
        public _KoolEntity9Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(19, new short[] { 2 }); }
        }
    }

    public class _KoolEntity10EntityType : CompiledEntityType<KoolEntity10>, IEntityType
    {
        public _KoolEntity10EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity10"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity10Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity10Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity10Foo10Property(this), new _KoolEntity10Goo10Property(this), new _KoolEntity10IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity10Key : CompiledKey, IKey
    {
        public _KoolEntity10Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(1, new short[] { 2 }); }
        }
    }

    public class _KoolEntity11EntityType : CompiledEntityType<KoolEntity11>, IEntityType
    {
        public _KoolEntity11EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity11"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity11Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity11Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity11Foo11Property(this), new _KoolEntity11Goo11Property(this), new _KoolEntity11IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity11Key : CompiledKey, IKey
    {
        public _KoolEntity11Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(2, new short[] { 2 }); }
        }
    }

    public class _KoolEntity12EntityType : CompiledEntityType<KoolEntity12>, IEntityType
    {
        public _KoolEntity12EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity12"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity12Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity12Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity12Foo12Property(this), new _KoolEntity12Goo12Property(this), new _KoolEntity12IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity12Key : CompiledKey, IKey
    {
        public _KoolEntity12Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(3, new short[] { 2 }); }
        }
    }

    public class _KoolEntity13EntityType : CompiledEntityType<KoolEntity13>, IEntityType
    {
        public _KoolEntity13EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity13"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity13Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity13Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity13Foo13Property(this), new _KoolEntity13Goo13Property(this), new _KoolEntity13IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity13Key : CompiledKey, IKey
    {
        public _KoolEntity13Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(4, new short[] { 2 }); }
        }
    }

    public class _KoolEntity14EntityType : CompiledEntityType<KoolEntity14>, IEntityType
    {
        public _KoolEntity14EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity14"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity14Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity14Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity14Foo14Property(this), new _KoolEntity14Goo14Property(this), new _KoolEntity14IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity14Key : CompiledKey, IKey
    {
        public _KoolEntity14Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(5, new short[] { 2 }); }
        }
    }

    public class _KoolEntity15EntityType : CompiledEntityType<KoolEntity15>, IEntityType, IEntityMaterializer
    {
        public _KoolEntity15EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity15"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity15Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity15Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity15Foo15Property(this), new _KoolEntity15Goo15Property(this), new _KoolEntity15IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }

        public bool CreateEntityWasUsed { get; set; }

        public object CreatEntity(object[] valueBuffer)
        {
            CreateEntityWasUsed = true;
            return KoolEntity15._EntityFramework_Create(valueBuffer);
        }
    }

    public class _KoolEntity15Key : CompiledKey, IKey
    {
        public _KoolEntity15Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(6, new short[] { 2 }); }
        }
    }

    public class _KoolEntity16EntityType : CompiledEntityType<KoolEntity16>, IEntityType
    {
        public _KoolEntity16EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity16"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity16Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity16Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity16Foo16Property(this), new _KoolEntity16Goo16Property(this), new _KoolEntity16IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity16Key : CompiledKey, IKey
    {
        public _KoolEntity16Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(7, new short[] { 2 }); }
        }
    }

    public class _KoolEntity17EntityType : CompiledEntityType<KoolEntity17>, IEntityType
    {
        public _KoolEntity17EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity17"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity17Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity17Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity17Foo17Property(this), new _KoolEntity17Goo17Property(this), new _KoolEntity17IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity17Key : CompiledKey, IKey
    {
        public _KoolEntity17Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(8, new short[] { 2 }); }
        }
    }

    public class _KoolEntity18EntityType : CompiledEntityType<KoolEntity18>, IEntityType
    {
        public _KoolEntity18EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity18"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity18Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity18Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity18Foo18Property(this), new _KoolEntity18Goo18Property(this), new _KoolEntity18IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity18Key : CompiledKey, IKey
    {
        public _KoolEntity18Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(9, new short[] { 2 }); }
        }
    }

    public class _KoolEntity19EntityType : CompiledEntityType<KoolEntity19>, IEntityType
    {
        public _KoolEntity19EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity19"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity19Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity19Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity19Foo19Property(this), new _KoolEntity19Goo19Property(this), new _KoolEntity19IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity19Key : CompiledKey, IKey
    {
        public _KoolEntity19Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(10, new short[] { 2 }); }
        }
    }

    public class _KoolEntity20EntityType : CompiledEntityType<KoolEntity20>, IEntityType
    {
        public _KoolEntity20EntityType(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "KoolEntity20"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity20Table"; }
        }

        protected override IKey LoadKey()
        {
            return new _KoolEntity20Key(Model);
        }

        protected override IProperty[] LoadProperties()
        {
            return new IProperty[] { new _KoolEntity20Foo20Property(this), new _KoolEntity20Goo20Property(this), new _KoolEntity20IdProperty(this) };
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" }).ToArray();
        }
    }

    public class _KoolEntity20Key : CompiledKey, IKey
    {
        public _KoolEntity20Key(IModel model)
            : base(model)
        {
        }

        protected override KeyDefinition Definition
        {
            get { return new KeyDefinition(12, new short[] { 2 }); }
        }
    }

    public class _KoolEntity1Id1Property : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity1Id1Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id1"; }
        }

        public override string StorageName
        {
            get { return "MyKey1"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity1Id2Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity1Id2Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id2"; }
        }

        public override string StorageName
        {
            get { return "MyKey2"; }
        }

        public int Index
        {
            get { return 3; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity1Foo1Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity1Foo1Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo1"; }
        }

        public override string StorageName
        {
            get { return "Foo1"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo1Annotation1", "Foo1Annotation2" },
                new[] { "Foo1Value1", "Foo1Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity1Goo1Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity1Goo1Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo1"; }
        }

        public override string StorageName
        {
            get { return "Goo1"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity1KoolEntity2IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity1KoolEntity2IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "KoolEntity2Id"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity2Id"; }
        }

        public int Index
        {
            get { return 4; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity2IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity2IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity2Foo2Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity2Foo2Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo2"; }
        }

        public override string StorageName
        {
            get { return "Foo2"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo2Annotation1", "Foo2Annotation2" },
                new[] { "Foo2Value1", "Foo2Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity2Goo2Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity2Goo2Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo2"; }
        }

        public override string StorageName
        {
            get { return "Goo2"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity2KoolEntity1Id1Property : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity2KoolEntity1Id1Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "KoolEntity1Id1"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity1Id1"; }
        }

        public int Index
        {
            get { return 3; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity2KoolEntity1Id2operty : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity2KoolEntity1Id2operty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "KoolEntity1Id2"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity1Id2"; }
        }

        public int Index
        {
            get { return 4; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity2KoolEntity3IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity2KoolEntity3IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "KoolEntity3Id"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity3Id"; }
        }

        public int Index
        {
            get { return 5; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity3IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity3IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity3Foo3Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity3Foo3Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo3"; }
        }

        public override string StorageName
        {
            get { return "Foo3"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo3Annotation1", "Foo3Annotation2" },
                new[] { "Foo3Value1", "Foo3Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity3Goo3Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity3Goo3Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo3"; }
        }

        public override string StorageName
        {
            get { return "Goo3"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity3KoolEntity4IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity3KoolEntity4IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "KoolEntity4Id"; }
        }

        public override string StorageName
        {
            get { return "KoolEntity4Id"; }
        }

        public int Index
        {
            get { return 3; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity4IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity4IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity4Foo4Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity4Foo4Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo4"; }
        }

        public override string StorageName
        {
            get { return "Foo4"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo4Annotation1", "Foo4Annotation2" },
                new[] { "Foo4Value1", "Foo4Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity4Goo4Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity4Goo4Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo4"; }
        }

        public override string StorageName
        {
            get { return "Goo4"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity5IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity5IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity5Foo5Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity5Foo5Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo5"; }
        }

        public override string StorageName
        {
            get { return "Foo5"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo5Annotation1", "Foo5Annotation2" },
                new[] { "Foo5Value1", "Foo5Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity5Goo5Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity5Goo5Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo5"; }
        }

        public override string StorageName
        {
            get { return "Goo5"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity6IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity6IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity6Foo6Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity6Foo6Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo6"; }
        }

        public override string StorageName
        {
            get { return "Foo6"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo6Annotation1", "Foo6Annotation2" },
                new[] { "Foo6Value1", "Foo6Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity6Goo6Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity6Goo6Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo6"; }
        }

        public override string StorageName
        {
            get { return "Goo6"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity6Kool5IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity6Kool5IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Kool5Id"; }
        }

        public override string StorageName
        {
            get { return "Kool5Id"; }
        }

        public int Index
        {
            get { return 3; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity7IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity7IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity7Foo7Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity7Foo7Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo7"; }
        }

        public override string StorageName
        {
            get { return "Foo7"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo7Annotation1", "Foo7Annotation2" },
                new[] { "Foo7Value1", "Foo7Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity7Goo7Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity7Goo7Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo7"; }
        }

        public override string StorageName
        {
            get { return "Goo7"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity8IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity8IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity8Foo8Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity8Foo8Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo8"; }
        }

        public override string StorageName
        {
            get { return "Foo8"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo8Annotation1", "Foo8Annotation2" },
                new[] { "Foo8Value1", "Foo8Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity8Goo8Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity8Goo8Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo8"; }
        }

        public override string StorageName
        {
            get { return "Goo8"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity9IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity9IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity9Foo9Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity9Foo9Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo9"; }
        }

        public override string StorageName
        {
            get { return "Foo9"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo9Annotation1", "Foo9Annotation2" },
                new[] { "Foo9Value1", "Foo9Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity9Goo9Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity9Goo9Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo9"; }
        }

        public override string StorageName
        {
            get { return "Goo9"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity10IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity10IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity10Foo10Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity10Foo10Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo10"; }
        }

        public override string StorageName
        {
            get { return "Foo10"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo10Annotation1", "Foo10Annotation2" },
                new[] { "Foo10Value1", "Foo10Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity10Goo10Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity10Goo10Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo10"; }
        }

        public override string StorageName
        {
            get { return "Goo10"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity11IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity11IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity11Foo11Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity11Foo11Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo11"; }
        }

        public override string StorageName
        {
            get { return "Foo11"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo11Annotation1", "Foo11Annotation2" },
                new[] { "Foo11Value1", "Foo11Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity11Goo11Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity11Goo11Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo11"; }
        }

        public override string StorageName
        {
            get { return "Goo11"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity12IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity12IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity12Foo12Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity12Foo12Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo12"; }
        }

        public override string StorageName
        {
            get { return "Foo12"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo12Annotation1", "Foo12Annotation2" },
                new[] { "Foo12Value1", "Foo12Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity12Goo12Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity12Goo12Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo12"; }
        }

        public override string StorageName
        {
            get { return "Goo12"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity13IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity13IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity13Foo13Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity13Foo13Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo13"; }
        }

        public override string StorageName
        {
            get { return "Foo13"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo13Annotation1", "Foo13Annotation2" },
                new[] { "Foo13Value1", "Foo13Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity13Goo13Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity13Goo13Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo13"; }
        }

        public override string StorageName
        {
            get { return "Goo13"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity14IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity14IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity14Foo14Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity14Foo14Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo14"; }
        }

        public override string StorageName
        {
            get { return "Foo14"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo14Annotation1", "Foo14Annotation2" },
                new[] { "Foo14Value1", "Foo14Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity14Goo14Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity14Goo14Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo14"; }
        }

        public override string StorageName
        {
            get { return "Goo14"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity15IdProperty : CompiledPropertyNoAnnotations<int>, IProperty, IClrPropertyGetter, IClrPropertySetter
    {
        public _KoolEntity15IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }

        public bool GetterCalled { get; set; }
        public bool SetterCalled { get; set; }

        public object GetClrValue(object instance)
        {
            GetterCalled = true;
            return ((KoolEntity15)instance).Id;
        }

        public void SetClrValue(object instance, object value)
        {
            SetterCalled = true;
            ((KoolEntity15)instance).Id = (int)value;
        }
    }

    public class _KoolEntity15Foo15Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity15Foo15Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo15"; }
        }

        public override string StorageName
        {
            get { return "Foo15"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo15Annotation1", "Foo15Annotation2" },
                new[] { "Foo15Value1", "Foo15Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity15Goo15Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity15Goo15Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo15"; }
        }

        public override string StorageName
        {
            get { return "Goo15"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity16IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity16IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity16Foo16Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity16Foo16Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo16"; }
        }

        public override string StorageName
        {
            get { return "Foo16"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo16Annotation1", "Foo16Annotation2" },
                new[] { "Foo16Value1", "Foo16Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity16Goo16Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity16Goo16Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo16"; }
        }

        public override string StorageName
        {
            get { return "Goo16"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity17IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity17IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity17Foo17Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity17Foo17Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo17"; }
        }

        public override string StorageName
        {
            get { return "Foo17"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo17Annotation1", "Foo17Annotation2" },
                new[] { "Foo17Value1", "Foo17Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity17Goo17Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity17Goo17Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo17"; }
        }

        public override string StorageName
        {
            get { return "Goo17"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity18IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity18IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity18Foo18Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity18Foo18Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo18"; }
        }

        public override string StorageName
        {
            get { return "Foo18"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo18Annotation1", "Foo18Annotation2" },
                new[] { "Foo18Value1", "Foo18Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity18Goo18Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity18Goo18Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo18"; }
        }

        public override string StorageName
        {
            get { return "Goo18"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity19IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity19IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity19Foo19Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity19Foo19Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo19"; }
        }

        public override string StorageName
        {
            get { return "Foo19"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo19Annotation1", "Foo19Annotation2" },
                new[] { "Foo19Value1", "Foo19Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity19Goo19Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity19Goo19Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo19"; }
        }

        public override string StorageName
        {
            get { return "Goo19"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity20IdProperty : CompiledPropertyNoAnnotations<int>, IProperty
    {
        public _KoolEntity20IdProperty(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Id"; }
        }

        public override string StorageName
        {
            get { return "MyKey"; }
        }

        public int Index
        {
            get { return 2; }
        }

        public int ShadowIndex
        {
            get { return -2; }
        }
    }

    public class _KoolEntity20Foo20Property : CompiledProperty<string>, IProperty
    {
        public _KoolEntity20Foo20Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Foo20"; }
        }

        public override string StorageName
        {
            get { return "Foo20"; }
        }

        protected override IAnnotation[] LoadAnnotations()
        {
            return ZipAnnotations(
                new[] { "Foo20Annotation1", "Foo20Annotation2" },
                new[] { "Foo20Value1", "Foo20Value2" }).ToArray();
        }

        public int Index
        {
            get { return 0; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity20Goo20Property : CompiledPropertyNoAnnotations<Guid>, IProperty
    {
        public _KoolEntity20Goo20Property(IEntityType entityType)
            : base(entityType)
        {
        }

        public string Name
        {
            get { return "Goo20"; }
        }

        public override string StorageName
        {
            get { return "Goo20"; }
        }

        public int Index
        {
            get { return 1; }
        }

        public int ShadowIndex
        {
            get { return -1; }
        }
    }

    public class _KoolEntity1Fk1 : CompiledSimpleForeignKey, IForeignKey
    {
        public _KoolEntity1Fk1(IModel model)
            : base(model)
        {
        }

        protected override ForeignKeyDefinition Definition
        {
            get { return new ForeignKeyDefinition(0, 4, 11); }
        }
    }

    public class _KoolEntity2Fk1 : CompiledForeignKey, IForeignKey
    {
        public _KoolEntity2Fk1(IModel model)
            : base(model)
        {
        }

        protected override ForeignKeyDefinition Definition
        {
            get { return new ForeignKeyDefinition(11, new short[] { 3, 4 }, 0, new short[] { 2, 3 }); }
        }
    }

    public class _KoolEntity2Fk2 : CompiledSimpleForeignKey, IForeignKey
    {
        public _KoolEntity2Fk2(IModel model)
            : base(model)
        {
        }

        protected override ForeignKeyDefinition Definition
        {
            get { return new ForeignKeyDefinition(11, 2, 13); }
        }
    }

    public class _KoolEntity3Fk1 : CompiledSimpleForeignKey, IForeignKey
    {
        public _KoolEntity3Fk1(IModel model)
            : base(model)
        {
        }

        protected override ForeignKeyDefinition Definition
        {
            get { return new ForeignKeyDefinition(13, 3, 14); }
        }
    }

    public class _KoolEntity6Fk1 : CompiledSimpleForeignKey, IForeignKey
    {
        public _KoolEntity6Fk1(IModel model)
            : base(model)
        {
        }

        protected override ForeignKeyDefinition Definition
        {
            get { return new ForeignKeyDefinition(16, 3, 15); }
        }
    }

    public class _KoolEntity1NavTo2 : CompiledNavigation, INavigation, IClrPropertyGetter, IClrPropertySetter
    {
        public _KoolEntity1NavTo2(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "NavTo2"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(0, 0, 0); }
        }

        public object GetClrValue(object instance)
        {
            return ((KoolEntity1)instance).NavTo2;
        }

        public void SetClrValue(object instance, object value)
        {
            ((KoolEntity1)instance).NavTo2 = (KoolEntity2)value;
        }
    }

    public class _KoolEntity1NavTo2s : CompiledNavigation, INavigation, IClrCollectionAccessor
    {
        public _KoolEntity1NavTo2s(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "NavTo2s"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(0, 11, 0); }
        }

        public void Add(object instance, object value)
        {
            ((KoolEntity1)instance).NavTo2s.Add((KoolEntity2)value);
        }

        public bool Contains(object instance, object value)
        {
            return ((KoolEntity1)instance).NavTo2s.Contains((KoolEntity2)value);
        }

        public void Remove(object instance, object value)
        {
            ((KoolEntity1)instance).NavTo2s.Remove((KoolEntity2)value);
        }
    }

    public class _KoolEntity2NavTo1 : CompiledNavigation, INavigation
    {
        public _KoolEntity2NavTo1(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "NavTo1"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(11, 11, 0); }
        }
    }

    public class _KoolEntity2NavTo1s : CompiledNavigation, INavigation
    {
        public _KoolEntity2NavTo1s(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "NavTo1s"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(11, 0, 0); }
        }
    }

    public class _KoolEntity2NavTo3 : CompiledNavigation, INavigation
    {
        public _KoolEntity2NavTo3(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "NavTo3"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(11, 11, 1); }
        }
    }

    public class _KoolEntity3NavTo2s : CompiledNavigation, INavigation
    {
        public _KoolEntity3NavTo2s(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "NavTo2s"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(13, 11, 1); }
        }
    }

    public class _KoolEntity3NavTo4 : CompiledNavigation, INavigation
    {
        public _KoolEntity3NavTo4(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "NavTo4"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(13, 13, 0); }
        }
    }

    public class _KoolEntity4NavTo3s : CompiledNavigation, INavigation
    {
        public _KoolEntity4NavTo3s(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "NavTo3s"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(14, 13, 0); }
        }
    }

    public class _KoolEntity5NavTo6s : CompiledNavigation, INavigation, IClrCollectionAccessor
    {
        public _KoolEntity5NavTo6s(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "Kool6s"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(15, 16, 0); }
        }

        public void Add(object instance, object value)
        {
            ((KoolEntity5)instance).AddKool6((KoolEntity6)value);
        }

        public bool Contains(object instance, object value)
        {
            return ((KoolEntity5)instance).Kool6s.Contains((KoolEntity6)value);
        }

        public void Remove(object instance, object value)
        {
            ((KoolEntity5)instance).RemoveKool6((KoolEntity6)value);
        }
    }

    public class _KoolEntity6NavTo5 : CompiledNavigation, INavigation, IClrPropertyGetter, IClrPropertySetter
    {
        public _KoolEntity6NavTo5(IModel model)
            : base(model)
        {
        }

        public string Name
        {
            get { return "Kool5"; }
        }

        protected override NavigationDefinition Definition
        {
            get { return new NavigationDefinition(16, 16, 0); }
        }

        public object GetClrValue(object instance)
        {
            return ((KoolEntity6)instance).Kool5;
        }

        public void SetClrValue(object instance, object value)
        {
            ((KoolEntity6)instance).Kool5 = (KoolEntity5)value;
        }
    }

    // ReSharper restore InconsistentNaming
}
