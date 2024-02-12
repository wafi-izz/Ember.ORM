using Ember.DataSchemaManager.BluePrints;
using Ember.Transcription.RDBMS.PostgreSql;
using Ember.Transcription.RDBMS.SqlServer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ember.Transcription;

public class TableTranscriber
{
    #region Create
    public virtual String PrimaryKey(ColumnBluePrint Column)
    {
        return Column.IsPrimaryKey ? "PRIMARY KEY " : "";
    }
    public virtual String ForeignKeySection(String TableName, ColumnBluePrint Column, Boolean AddForeKeyString = false)
    {
        if (!Column.IsForeignKey)
            return "";

        String ForeignTable = Column.ForeignKeyArguments["ForeignTable"]!.GetValue<String>();
        String ForeignTableColumnName = Column.ForeignKeyArguments["ForeignTableColumnName"]!.GetValue<String>();
        String ColumnName = Column.ColumnName;
        //on delete
        String OnDelete = "";
        if (Column.ForeignKeyArguments["OnDelete"] != null)
            OnDelete = $"ON DELETE {Column.ForeignKeyArguments["OnDelete"]}";

        //on update
        String OnUpdate = "";
        if (Column.ForeignKeyArguments["OnUpdate"] != null)
            OnUpdate = $"ON UPDATE {Column.ForeignKeyArguments["OnUpdate"]}";

        JsonNode? CustomConstraintName = Column.ForeignKeyArguments["CustomConstraintName"];
        String ConstraintName = CustomConstraintName != null ? CustomConstraintName.GetValue<String>() : $"FK_{TableName}_{ForeignTable}_{ColumnName}_{ForeignTableColumnName} ";
        return $"CONSTRAINT {ConstraintName} FOREIGN KEY ({ColumnName}) REFERENCES {ForeignTable} ({ForeignTableColumnName}) {OnUpdate} {OnDelete} ";
    }
    public virtual String DefaultValue(ColumnBluePrint Column)
    {
        return Column.Default != null ? $"DEFAULT('{Column.Default}') " : "";
    }
    public virtual String NullabilityState(ColumnBluePrint Column)
    {
        return Column.Nullable ? $"NULL " : "NOT NULL ";
    }
    #region Drop
    public virtual String DropTableQuery(TableBluePrint TableBluePrint)
    {
        return $"DROP TABLE IF EXISTS {TableBluePrint.ObjectName};\n\n"; // TODO : must specify witch schema to drop to or create in to.
    }
    #endregion

    public String? TranscribeDataType(String ColumnDataType, String SpecificColumnDataType, String Length)
    {
        String ClassName = this.GetType().Name;
        if (ClassName == nameof(SqlServerTableTranscriber))
        {
            if (ColumnDataType == ColumnTypeEnum.Boolean.ToString())
            {
                return $"BIT{Length}";
            }
            else if (new String[] { ColumnTypeEnum.File.ToString() }.Contains(ColumnDataType))
            {
                return "VarBinary";
            }
            else return $"{SpecificColumnDataType}{Length}";
        }
        if (ClassName == nameof(PostgreSqlTableTranscriber))
        {
            if (ColumnDataType == ColumnTypeEnum.String.ToString())
            {
                if (new String[] { StringType.VARCHAR.ToString(), StringType.CHAR.ToString(), StringType.TEXT.ToString(), StringType.NTEXT.ToString() }.Contains(SpecificColumnDataType.ToUpper())) return $"{SpecificColumnDataType}{Length}";
                //TODO: find a wat to collation Nvarchar for postgre
                if (new String[] { StringType.NVARCHAR.ToString(), StringType.NCHAR.ToString() }.Contains(SpecificColumnDataType.ToUpper())) return $"{SpecificColumnDataType.Substring(1)}{Length} /*A NATIONALIZED DATATYPE IS COMMING*/";
            }
            else if (ColumnDataType == ColumnTypeEnum.Boolean.ToString())
            {
                return "Boolean";
            }
            else if (ColumnDataType == ColumnTypeEnum.DateTime.ToString())
            {
                return "TimeStamp";
            }
            else if ( new String[] { ColumnTypeEnum.Binary.ToString() , ColumnTypeEnum.VarBinary.ToString() , ColumnTypeEnum.File.ToString() ,ColumnTypeEnum.Image.ToString() }.Contains(ColumnDataType))
            {
                return "ByteA";
            }
            else return $"{SpecificColumnDataType}{Length}";
        }
        return null;
    }
    #endregion
    #region Alter

    #endregion
}
