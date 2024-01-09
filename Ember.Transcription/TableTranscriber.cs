using Ember.DataSchemaManager.BluePrints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ember.Transcription
{
    public class TableTranscriber
    {
        public virtual String PrimaryKey(ColumnBluePrint Column)
        {
            return Column.IsPrimaryKey ? "PRIMARY KEY " : "";
        }
        public virtual String ForeignKeySection(String TableName, ColumnBluePrint Column)
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
        public virtual String ColumnHead(ColumnBluePrint Column)
        {
            var DataTypeParameter = Column.ColumnDataType["Length"] != null ? $"({Column.ColumnDataType["Length"]})" : "";
            return $"{Column.ColumnName} {Column.ColumnDataType["DataTypeSQLName"]}{DataTypeParameter} ";
        }
        public virtual String DefaultValue(ColumnBluePrint Column)
        {
            return Column.Default != null ? $"DEFAULT('{Column.Default}') " : "";
        }
        public virtual String NullabilityState(ColumnBluePrint Column)
        {
            return Column.Nullable ? $"NULL " : "NOT NULL ";
        }
    }
}
