using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E2ETest.Namespace.SubDir
{
    public partial class AllDataTypes
    {
        [Column("AllDataTypesID")]
        public int AllDataTypesId { get; set; }
        [Column("bigintColumn")]
        public long BigintColumn { get; set; }
        [Column("bitColumn")]
        public bool BitColumn { get; set; }
        [Column("decimalColumn", TypeName = "decimal(18, 0)")]
        public decimal DecimalColumn { get; set; }
        [Column("decimal105Column", TypeName = "decimal(10, 5)")]
        public decimal Decimal105Column { get; set; }
        [Column("decimalDefaultColumn")]
        public decimal DecimalDefaultColumn { get; set; }
        [Column("intColumn")]
        public int IntColumn { get; set; }
        [Column("moneyColumn", TypeName = "money")]
        public decimal MoneyColumn { get; set; }
        [Column("numericColumn", TypeName = "numeric(18, 0)")]
        public decimal NumericColumn { get; set; }
        [Column("numeric152Column", TypeName = "numeric(15, 2)")]
        public decimal Numeric152Column { get; set; }
        [Column("numericDefaultColumn", TypeName = "numeric(18, 2)")]
        public decimal NumericDefaultColumn { get; set; }
        [Column("smallintColumn")]
        public short SmallintColumn { get; set; }
        [Column("smallmoneyColumn", TypeName = "smallmoney")]
        public decimal SmallmoneyColumn { get; set; }
        [Column("tinyintColumn")]
        public byte TinyintColumn { get; set; }
        [Column("floatColumn")]
        public double FloatColumn { get; set; }
        [Column("realColumn")]
        public float? RealColumn { get; set; }
        [Column("dateColumn", TypeName = "date")]
        public DateTime DateColumn { get; set; }
        [Column("datetimeColumn", TypeName = "datetime")]
        public DateTime? DatetimeColumn { get; set; }
        [Column("datetime2Column")]
        public DateTime? Datetime2Column { get; set; }
        [Column("datetime24Column", TypeName = "datetime2(4)")]
        public DateTime? Datetime24Column { get; set; }
        [Column("datetimeoffsetColumn")]
        public DateTimeOffset? DatetimeoffsetColumn { get; set; }
        [Column("datetimeoffset5Column", TypeName = "datetimeoffset(5)")]
        public DateTimeOffset? Datetimeoffset5Column { get; set; }
        [Column("smalldatetimeColumn", TypeName = "smalldatetime")]
        public DateTime? SmalldatetimeColumn { get; set; }
        [Column("timeColumn")]
        public TimeSpan? TimeColumn { get; set; }
        [Column("time4Column", TypeName = "time(4)")]
        public TimeSpan? Time4Column { get; set; }
        [Column("charColumn", TypeName = "char(1)")]
        public string CharColumn { get; set; }
        [Column("char10Column", TypeName = "char(10)")]
        public string Char10Column { get; set; }
        [Column("textColumn", TypeName = "text")]
        public string TextColumn { get; set; }
        [Column("varcharColumn")]
        [StringLength(1)]
        public string VarcharColumn { get; set; }
        [Column("varchar66Column")]
        [StringLength(66)]
        public string Varchar66Column { get; set; }
        [Column("varcharMaxColumn")]
        public string VarcharMaxColumn { get; set; }
        [Column("ncharColumn", TypeName = "nchar(1)")]
        public string NcharColumn { get; set; }
        [Column("nchar99Column", TypeName = "nchar(99)")]
        public string Nchar99Column { get; set; }
        [Column("ntextColumn", TypeName = "ntext")]
        public string NtextColumn { get; set; }
        [Column("nvarcharColumn")]
        [StringLength(1)]
        public string NvarcharColumn { get; set; }
        [Column("nvarchar100Column")]
        [StringLength(100)]
        public string Nvarchar100Column { get; set; }
        [Column("nvarcharMaxColumn")]
        public string NvarcharMaxColumn { get; set; }
        [Column("binaryColumn", TypeName = "binary(1)")]
        public byte[] BinaryColumn { get; set; }
        [Column("binary111Column", TypeName = "binary(111)")]
        public byte[] Binary111Column { get; set; }
        [Column("imageColumn", TypeName = "image")]
        public byte[] ImageColumn { get; set; }
        [Column("varbinaryColumn")]
        [MaxLength(1)]
        public byte[] VarbinaryColumn { get; set; }
        [Column("varbinary123Column")]
        [MaxLength(123)]
        public byte[] Varbinary123Column { get; set; }
        [Column("varbinaryMaxColumn")]
        public byte[] VarbinaryMaxColumn { get; set; }
        [Column("timestampColumn")]
        public byte[] TimestampColumn { get; set; }
        [Column("uniqueidentifierColumn")]
        public Guid? UniqueidentifierColumn { get; set; }
        [Column("xmlColumn", TypeName = "xml")]
        public string XmlColumn { get; set; }
        [Column("typeAliasColumn", TypeName = "TestTypeAlias")]
        public string TypeAliasColumn { get; set; }
        [Column("binaryVaryingColumn")]
        [MaxLength(1)]
        public byte[] BinaryVaryingColumn { get; set; }
        [Column("binaryVarying133Column")]
        [MaxLength(133)]
        public byte[] BinaryVarying133Column { get; set; }
        [Column("binaryVaryingMaxColumn")]
        public byte[] BinaryVaryingMaxColumn { get; set; }
        [Column("charVaryingColumn")]
        [StringLength(1)]
        public string CharVaryingColumn { get; set; }
        [Column("charVarying144Column")]
        [StringLength(144)]
        public string CharVarying144Column { get; set; }
        [Column("charVaryingMaxColumn")]
        public string CharVaryingMaxColumn { get; set; }
        [Column("characterColumn", TypeName = "char(1)")]
        public string CharacterColumn { get; set; }
        [Column("character155Column", TypeName = "char(155)")]
        public string Character155Column { get; set; }
        [Column("characterVaryingColumn")]
        [StringLength(1)]
        public string CharacterVaryingColumn { get; set; }
        [Column("characterVarying166Column")]
        [StringLength(166)]
        public string CharacterVarying166Column { get; set; }
        [Column("characterVaryingMaxColumn")]
        public string CharacterVaryingMaxColumn { get; set; }
        [Column("nationalCharacterColumn", TypeName = "nchar(1)")]
        public string NationalCharacterColumn { get; set; }
        [Column("nationalCharacter171Column", TypeName = "nchar(171)")]
        public string NationalCharacter171Column { get; set; }
        [Column("nationalCharVaryingColumn")]
        [StringLength(1)]
        public string NationalCharVaryingColumn { get; set; }
        [Column("nationalCharVarying177Column")]
        [StringLength(177)]
        public string NationalCharVarying177Column { get; set; }
        [Column("nationalCharVaryingMaxColumn")]
        public string NationalCharVaryingMaxColumn { get; set; }
        [Column("nationalCharacterVaryingColumn")]
        [StringLength(1)]
        public string NationalCharacterVaryingColumn { get; set; }
        [Column("nationalCharacterVarying188Column")]
        [StringLength(188)]
        public string NationalCharacterVarying188Column { get; set; }
        [Column("nationalCharacterVaryingMaxColumn")]
        public string NationalCharacterVaryingMaxColumn { get; set; }
    }
}
