// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata.Compiled;

namespace Microsoft.Data.Entity.Metadata
{
    public class KoolEntityBase
    {
        public int Id { get; set; }
        public string Foo { get; set; }
        public Guid Goo { get; set; }
    }

    public class KoolEntity1 : KoolEntityBase
    {
    }

    public class KoolEntity2 : KoolEntityBase
    {
    }

    public class KoolEntity3 : KoolEntityBase
    {
    }

    public class KoolEntity4 : KoolEntityBase
    {
    }

    public class KoolEntity5 : KoolEntityBase
    {
    }

    public class KoolEntity6 : KoolEntityBase
    {
    }

    public class KoolEntity7 : KoolEntityBase
    {
    }

    public class KoolEntity8 : KoolEntityBase
    {
    }

    public class KoolEntity9 : KoolEntityBase
    {
    }

    public class KoolEntity10 : KoolEntityBase
    {
    }

    public class KoolEntity11 : KoolEntityBase
    {
    }

    public class KoolEntity12 : KoolEntityBase
    {
    }

    public class KoolEntity13 : KoolEntityBase
    {
    }

    public class KoolEntity14 : KoolEntityBase
    {
    }

    public class KoolEntity15 : KoolEntityBase
    {
    }

    public class KoolEntity16 : KoolEntityBase
    {
    }

    public class KoolEntity17 : KoolEntityBase
    {
    }

    public class KoolEntity18 : KoolEntityBase
    {
    }

    public class KoolEntity19 : KoolEntityBase
    {
    }

    public class KoolEntity20 : KoolEntityBase
    {
    }

    public class KoolEntity21 : KoolEntityBase
    {
    }

    public class KoolEntity22 : KoolEntityBase
    {
    }

    public class KoolEntity23 : KoolEntityBase
    {
    }

    public class KoolEntity24 : KoolEntityBase
    {
    }

    public class KoolEntity25 : KoolEntityBase
    {
    }

    public class KoolEntity26 : KoolEntityBase
    {
    }

    public class KoolEntity27 : KoolEntityBase
    {
    }

    public class KoolEntity28 : KoolEntityBase
    {
    }

    public class KoolEntity29 : KoolEntityBase
    {
    }

    public class KoolEntity30 : KoolEntityBase
    {
    }

    public class KoolEntity31 : KoolEntityBase
    {
    }

    public class KoolEntity32 : KoolEntityBase
    {
    }

    public class KoolEntity33 : KoolEntityBase
    {
    }

    public class KoolEntity34 : KoolEntityBase
    {
    }

    public class KoolEntity35 : KoolEntityBase
    {
    }

    public class KoolEntity36 : KoolEntityBase
    {
    }

    public class KoolEntity37 : KoolEntityBase
    {
    }

    public class KoolEntity38 : KoolEntityBase
    {
    }

    public class KoolEntity39 : KoolEntityBase
    {
    }

    public class KoolEntity40 : KoolEntityBase
    {
    }

    public class KoolEntity41 : KoolEntityBase
    {
    }

    public class KoolEntity42 : KoolEntityBase
    {
    }

    public class KoolEntity43 : KoolEntityBase
    {
    }

    public class KoolEntity44 : KoolEntityBase
    {
    }

    public class KoolEntity45 : KoolEntityBase
    {
    }

    public class KoolEntity46 : KoolEntityBase
    {
    }

    public class KoolEntity47 : KoolEntityBase
    {
    }

    public class KoolEntity48 : KoolEntityBase
    {
    }

    public class KoolEntity49 : KoolEntityBase
    {
    }

    public class KoolEntity50 : KoolEntityBase
    {
    }

    // Proposed generated code below
// ReSharper disable InconsistentNaming
    public class _OneTwoThreeContextModel
        : CompiledModelBase<_OneTwoThreeContextEntities, _OneTwoThreeContextAnnotations>, IModel
    {
    }

    public class _OneTwoThreeContextEntities : CompiledEntitiesBase
    {
        public _OneTwoThreeContextEntities()
            : base(
                new _KoolEntity1EntityType(),
                new _KoolEntity10EntityType(),
                new _KoolEntity11EntityType(),
                new _KoolEntity12EntityType(),
                new _KoolEntity13EntityType(),
                new _KoolEntity14EntityType(),
                new _KoolEntity15EntityType(),
                new _KoolEntity16EntityType(),
                new _KoolEntity17EntityType(),
                new _KoolEntity18EntityType(),
                new _KoolEntity19EntityType(),
                new _KoolEntity2EntityType(),
                new _KoolEntity20EntityType(),
                new _KoolEntity21EntityType(),
                new _KoolEntity22EntityType(),
                new _KoolEntity23EntityType(),
                new _KoolEntity24EntityType(),
                new _KoolEntity25EntityType(),
                new _KoolEntity26EntityType(),
                new _KoolEntity27EntityType(),
                new _KoolEntity28EntityType(),
                new _KoolEntity29EntityType(),
                new _KoolEntity3EntityType(),
                new _KoolEntity30EntityType(),
                new _KoolEntity31EntityType(),
                new _KoolEntity32EntityType(),
                new _KoolEntity33EntityType(),
                new _KoolEntity34EntityType(),
                new _KoolEntity35EntityType(),
                new _KoolEntity36EntityType(),
                new _KoolEntity37EntityType(),
                new _KoolEntity38EntityType(),
                new _KoolEntity39EntityType(),
                new _KoolEntity4EntityType(),
                new _KoolEntity40EntityType(),
                new _KoolEntity41EntityType(),
                new _KoolEntity42EntityType(),
                new _KoolEntity43EntityType(),
                new _KoolEntity44EntityType(),
                new _KoolEntity45EntityType(),
                new _KoolEntity46EntityType(),
                new _KoolEntity47EntityType(),
                new _KoolEntity48EntityType(),
                new _KoolEntity49EntityType(),
                new _KoolEntity5EntityType(),
                new _KoolEntity50EntityType(),
                new _KoolEntity6EntityType(),
                new _KoolEntity7EntityType(),
                new _KoolEntity8EntityType(),
                new _KoolEntity9EntityType())
        {
        }
    }

    public class _OneTwoThreeContextAnnotations : CompiledAnnotationsBase
    {
        public _OneTwoThreeContextAnnotations()
            : base(
                new[] { "ModelAnnotation1", "ModelAnnotation2" },
                new[] { "ModelValue1", "ModelValue2" })
        {
        }
    }

    public class _KoolEntity1EntityType
        : CompiledEntityBase<KoolEntity1, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity1"; }
        }

        public string StorageName
        {
            get { return "KoolEntity1Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity1, int>(((KoolEntity1)entity).Id);
        }
    }

    public class _KoolEntity2EntityType
        : CompiledEntityBase<KoolEntity2, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity2"; }
        }

        public string StorageName
        {
            get { return "KoolEntity2Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity2, int>(((KoolEntity2)entity).Id);
        }
    }

    public class _KoolEntity3EntityType
        : CompiledEntityBase<KoolEntity3, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity3"; }
        }

        public string StorageName
        {
            get { return "KoolEntity3Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity3, int>(((KoolEntity3)entity).Id);
        }
    }

    public class _KoolEntity4EntityType
        : CompiledEntityBase<KoolEntity4, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity4"; }
        }

        public string StorageName
        {
            get { return "KoolEntity4Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity4, int>(((KoolEntity4)entity).Id);
        }
    }

    public class _KoolEntity5EntityType
        : CompiledEntityBase<KoolEntity5, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity5"; }
        }

        public string StorageName
        {
            get { return "KoolEntity5Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity5, int>(((KoolEntity5)entity).Id);
        }
    }

    public class _KoolEntity6EntityType
        : CompiledEntityBase<KoolEntity6, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity6"; }
        }

        public string StorageName
        {
            get { return "KoolEntity6Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity6, int>(((KoolEntity6)entity).Id);
        }
    }

    public class _KoolEntity7EntityType
        : CompiledEntityBase<KoolEntity7, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity7"; }
        }

        public string StorageName
        {
            get { return "KoolEntity7Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity7, int>(((KoolEntity7)entity).Id);
        }
    }

    public class _KoolEntity8EntityType
        : CompiledEntityBase<KoolEntity8, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity8"; }
        }

        public string StorageName
        {
            get { return "KoolEntity8Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity8, int>(((KoolEntity8)entity).Id);
        }
    }

    public class _KoolEntity9EntityType
        : CompiledEntityBase<KoolEntity9, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity9"; }
        }

        public string StorageName
        {
            get { return "KoolEntity9Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity9, int>(((KoolEntity9)entity).Id);
        }
    }

    public class _KoolEntity10EntityType
        : CompiledEntityBase<KoolEntity10, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity10"; }
        }

        public string StorageName
        {
            get { return "KoolEntity10Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity10, int>(((KoolEntity10)entity).Id);
        }
    }

    public class _KoolEntity11EntityType
        : CompiledEntityBase<KoolEntity11, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity11"; }
        }

        public string StorageName
        {
            get { return "KoolEntity11Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity11, int>(((KoolEntity11)entity).Id);
        }
    }

    public class _KoolEntity12EntityType
        : CompiledEntityBase<KoolEntity12, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity12"; }
        }

        public string StorageName
        {
            get { return "KoolEntity12Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity12, int>(((KoolEntity12)entity).Id);
        }
    }

    public class _KoolEntity13EntityType
        : CompiledEntityBase<KoolEntity13, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity13"; }
        }

        public string StorageName
        {
            get { return "KoolEntity13Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity13, int>(((KoolEntity13)entity).Id);
        }
    }

    public class _KoolEntity14EntityType
        : CompiledEntityBase<KoolEntity14, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity14"; }
        }

        public string StorageName
        {
            get { return "KoolEntity14Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity14, int>(((KoolEntity14)entity).Id);
        }
    }

    public class _KoolEntity15EntityType
        : CompiledEntityBase<KoolEntity15, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity15"; }
        }

        public string StorageName
        {
            get { return "KoolEntity15Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity15, int>(((KoolEntity15)entity).Id);
        }
    }

    public class _KoolEntity16EntityType
        : CompiledEntityBase<KoolEntity16, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity16"; }
        }

        public string StorageName
        {
            get { return "KoolEntity16Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity16, int>(((KoolEntity16)entity).Id);
        }
    }

    public class _KoolEntity17EntityType
        : CompiledEntityBase<KoolEntity17, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity17"; }
        }

        public string StorageName
        {
            get { return "KoolEntity17Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity17, int>(((KoolEntity17)entity).Id);
        }
    }

    public class _KoolEntity18EntityType
        : CompiledEntityBase<KoolEntity18, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity18"; }
        }

        public string StorageName
        {
            get { return "KoolEntity18Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity18, int>(((KoolEntity18)entity).Id);
        }
    }

    public class _KoolEntity19EntityType
        : CompiledEntityBase<KoolEntity19, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity19"; }
        }

        public string StorageName
        {
            get { return "KoolEntity19Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity19, int>(((KoolEntity19)entity).Id);
        }
    }

    public class _KoolEntity20EntityType
        : CompiledEntityBase<KoolEntity20, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity20"; }
        }

        public string StorageName
        {
            get { return "KoolEntity20Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity20, int>(((KoolEntity20)entity).Id);
        }
    }

    public class _KoolEntity21EntityType
        : CompiledEntityBase<KoolEntity21, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity21"; }
        }

        public string StorageName
        {
            get { return "KoolEntity21Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity21, int>(((KoolEntity21)entity).Id);
        }
    }

    public class _KoolEntity22EntityType
        : CompiledEntityBase<KoolEntity22, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity22"; }
        }

        public string StorageName
        {
            get { return "KoolEntity22Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity22, int>(((KoolEntity22)entity).Id);
        }
    }

    public class _KoolEntity23EntityType
        : CompiledEntityBase<KoolEntity23, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity23"; }
        }

        public string StorageName
        {
            get { return "KoolEntity23Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity23, int>(((KoolEntity23)entity).Id);
        }
    }

    public class _KoolEntity24EntityType
        : CompiledEntityBase<KoolEntity24, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity24"; }
        }

        public string StorageName
        {
            get { return "KoolEntity24Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity24, int>(((KoolEntity24)entity).Id);
        }
    }

    public class _KoolEntity25EntityType
        : CompiledEntityBase<KoolEntity25, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity25"; }
        }

        public string StorageName
        {
            get { return "KoolEntity25Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity25, int>(((KoolEntity25)entity).Id);
        }
    }

    public class _KoolEntity26EntityType
        : CompiledEntityBase<KoolEntity26, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity26"; }
        }

        public string StorageName
        {
            get { return "KoolEntity26Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity26, int>(((KoolEntity26)entity).Id);
        }
    }

    public class _KoolEntity27EntityType
        : CompiledEntityBase<KoolEntity27, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity27"; }
        }

        public string StorageName
        {
            get { return "KoolEntity27Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity27, int>(((KoolEntity27)entity).Id);
        }
    }

    public class _KoolEntity28EntityType
        : CompiledEntityBase<KoolEntity28, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity28"; }
        }

        public string StorageName
        {
            get { return "KoolEntity28Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity28, int>(((KoolEntity28)entity).Id);
        }
    }

    public class _KoolEntity29EntityType
        : CompiledEntityBase<KoolEntity29, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity29"; }
        }

        public string StorageName
        {
            get { return "KoolEntity29Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity29, int>(((KoolEntity29)entity).Id);
        }
    }

    public class _KoolEntity30EntityType
        : CompiledEntityBase<KoolEntity30, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity30"; }
        }

        public string StorageName
        {
            get { return "KoolEntity30Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity30, int>(((KoolEntity30)entity).Id);
        }
    }

    public class _KoolEntity31EntityType
        : CompiledEntityBase<KoolEntity31, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity31"; }
        }

        public string StorageName
        {
            get { return "KoolEntity31Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity31, int>(((KoolEntity31)entity).Id);
        }
    }

    public class _KoolEntity32EntityType
        : CompiledEntityBase<KoolEntity32, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity32"; }
        }

        public string StorageName
        {
            get { return "KoolEntity32Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity32, int>(((KoolEntity32)entity).Id);
        }
    }

    public class _KoolEntity33EntityType
        : CompiledEntityBase<KoolEntity33, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity33"; }
        }

        public string StorageName
        {
            get { return "KoolEntity33Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity33, int>(((KoolEntity33)entity).Id);
        }
    }

    public class _KoolEntity34EntityType
        : CompiledEntityBase<KoolEntity34, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity34"; }
        }

        public string StorageName
        {
            get { return "KoolEntity34Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity34, int>(((KoolEntity34)entity).Id);
        }
    }

    public class _KoolEntity35EntityType
        : CompiledEntityBase<KoolEntity35, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity35"; }
        }

        public string StorageName
        {
            get { return "KoolEntity35Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity35, int>(((KoolEntity35)entity).Id);
        }
    }

    public class _KoolEntity36EntityType
        : CompiledEntityBase<KoolEntity36, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity36"; }
        }

        public string StorageName
        {
            get { return "KoolEntity36Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity36, int>(((KoolEntity36)entity).Id);
        }
    }

    public class _KoolEntity37EntityType
        : CompiledEntityBase<KoolEntity37, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity37"; }
        }

        public string StorageName
        {
            get { return "KoolEntity37Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity37, int>(((KoolEntity37)entity).Id);
        }
    }

    public class _KoolEntity38EntityType
        : CompiledEntityBase<KoolEntity38, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity38"; }
        }

        public string StorageName
        {
            get { return "KoolEntity38Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity38, int>(((KoolEntity38)entity).Id);
        }
    }

    public class _KoolEntity39EntityType
        : CompiledEntityBase<KoolEntity39, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity39"; }
        }

        public string StorageName
        {
            get { return "KoolEntity39Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity39, int>(((KoolEntity39)entity).Id);
        }
    }

    public class _KoolEntity40EntityType
        : CompiledEntityBase<KoolEntity40, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity40"; }
        }

        public string StorageName
        {
            get { return "KoolEntity40Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity40, int>(((KoolEntity40)entity).Id);
        }
    }

    public class _KoolEntity41EntityType
        : CompiledEntityBase<KoolEntity41, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity41"; }
        }

        public string StorageName
        {
            get { return "KoolEntity41Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity41, int>(((KoolEntity41)entity).Id);
        }
    }

    public class _KoolEntity42EntityType
        : CompiledEntityBase<KoolEntity42, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity42"; }
        }

        public string StorageName
        {
            get { return "KoolEntity42Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity42, int>(((KoolEntity42)entity).Id);
        }
    }

    public class _KoolEntity43EntityType
        : CompiledEntityBase<KoolEntity43, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity43"; }
        }

        public string StorageName
        {
            get { return "KoolEntity43Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity43, int>(((KoolEntity43)entity).Id);
        }
    }

    public class _KoolEntity44EntityType
        : CompiledEntityBase<KoolEntity44, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity44"; }
        }

        public string StorageName
        {
            get { return "KoolEntity44Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity44, int>(((KoolEntity44)entity).Id);
        }
    }

    public class _KoolEntity45EntityType
        : CompiledEntityBase<KoolEntity45, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity45"; }
        }

        public string StorageName
        {
            get { return "KoolEntity45Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity45, int>(((KoolEntity45)entity).Id);
        }
    }

    public class _KoolEntity46EntityType
        : CompiledEntityBase<KoolEntity46, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity46"; }
        }

        public string StorageName
        {
            get { return "KoolEntity46Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity46, int>(((KoolEntity46)entity).Id);
        }
    }

    public class _KoolEntity47EntityType
        : CompiledEntityBase<KoolEntity47, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity47"; }
        }

        public string StorageName
        {
            get { return "KoolEntity47Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity47, int>(((KoolEntity47)entity).Id);
        }
    }

    public class _KoolEntity48EntityType
        : CompiledEntityBase<KoolEntity48, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity48"; }
        }

        public string StorageName
        {
            get { return "KoolEntity48Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity48, int>(((KoolEntity48)entity).Id);
        }
    }

    public class _KoolEntity49EntityType
        : CompiledEntityBase<KoolEntity49, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity49"; }
        }

        public string StorageName
        {
            get { return "KoolEntity49Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity49, int>(((KoolEntity49)entity).Id);
        }
    }

    public class _KoolEntity50EntityType
        : CompiledEntityBase<KoolEntity50, _KoolEntityProperties, _KoolEntityAnnotations>, IEntityType
    {
        public string Name
        {
            get { return "KoolEntity50"; }
        }

        public string StorageName
        {
            get { return "KoolEntity50Table"; }
        }

        public EntityKey CreateKey(object entity)
        {
            return new SimpleEntityKey<KoolEntity50, int>(((KoolEntity50)entity).Id);
        }
    }

    // The code below is shared by all entity types above to avoid having to have lots of hand-written
    // proposed code checked in. This sharing should not significantly impact functionality or managed
    // heap memory usage, except that there will be more than the usual amount of string interning
    // due to the similarity of all entity types

    public class _KoolEntityProperties : CompiledPropertiesBase
    {
        public _KoolEntityProperties()
        {
            Keys = new IProperty[] { new _KoolEntityIdProperty() };
            Properties = new[] { new _KoolEntityFooProperty(), new _KoolEntityGooProperty(), Keys[0] };
        }
    }

    public class _KoolEntityAnnotations : CompiledAnnotationsBase
    {
        public _KoolEntityAnnotations()
            : base(
                new[] { "Annotation1", "Annotation2" },
                new[] { "Value1", "Value2" })
        {
        }
    }

    public class _KoolEntityIdProperty : CompiledPropertyBase<KoolEntityBase, int, _KoolEntityIdAnnotations>, IProperty
    {
        public string Name
        {
            get { return "Id"; }
        }

        public string StorageName
        {
            get { return "MyKey"; }
        }

        public void SetValue(object instance, object value)
        {
            ((KoolEntityBase)instance).Id = (int)value;
        }

        public object GetValue(object instance)
        {
            return ((KoolEntityBase)instance).Id;
        }
    }

    public class _KoolEntityIdAnnotations : CompiledAnnotationsBase
    {
        public _KoolEntityIdAnnotations()
            : base(
                new[] { "IdAnnotation1", "IdAnnotation2" },
                new[] { "IdValue1", "IdValue2" })
        {
        }
    }

    public class _KoolEntityFooProperty : CompiledPropertyBase<KoolEntityBase, string, _KoolEntityFooAnnotations>, IProperty
    {
        public string Name
        {
            get { return "Foo"; }
        }

        public string StorageName
        {
            get { return "Foo"; }
        }

        public void SetValue(object instance, object value)
        {
            ((KoolEntityBase)instance).Foo = (string)value;
        }

        public object GetValue(object instance)
        {
            return ((KoolEntityBase)instance).Foo;
        }
    }

    public class _KoolEntityFooAnnotations : CompiledAnnotationsBase
    {
        public _KoolEntityFooAnnotations()
            : base(
                new[] { "FooAnnotation1", "FooAnnotation2" },
                new[] { "FooValue1", "FooValue2" })
        {
        }
    }

    public class _KoolEntityGooProperty : CompiledPropertyBase<KoolEntityBase, Guid>, IProperty
    {
        public string Name
        {
            get { return "Goo"; }
        }

        public string StorageName
        {
            get { return "Goo"; }
        }

        public void SetValue(object instance, object value)
        {
            ((KoolEntityBase)instance).Goo = (Guid)value;
        }

        public object GetValue(object instance)
        {
            return ((KoolEntityBase)instance).Goo;
        }
    }
// ReSharper restore InconsistentNaming
}
