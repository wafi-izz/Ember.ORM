using Ember.DataSchemaManager.BluePrints;
using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.TranscriptionInterfaces;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Ember.DataSchemaManager.SharedFuncctions.Shared;

namespace Ember.Transcription.RDBMS.PostgreSql;

/*TODO: 
 * column name must be enclosed in quotes
 * find a way to make column be as the user want to enforce postgre to use it
 */

internal class PostgreSqlTableTranscriber : TableTranscriber, ITableTranscriber
{
    public String Transcript { get; set; }
    public TableSchema TableSchema { get; set; }
    public PostgreSqlTableTranscriber(TableSchema TableSchema)
    {
        Transcript = "";
        this.TableSchema = TableSchema;
        Transcribe();
    }
    public void Transcribe()
    {
        foreach (TableBluePrint TableBluePrint in TableSchema.TableBluePrintList)
        {
            if (TableBluePrint.Action == BluePrintAction.Create) Create(TableBluePrint);
            if (TableBluePrint.Action == BluePrintAction.Alter) Alter(TableBluePrint);
            if (TableBluePrint.Action == BluePrintAction.Drop) Drop(TableBluePrint);
        }
    }
    #region Create
    public void Create(TableBluePrint TableBluePrint)
    {
        Transcript += $"CREATE TABLE \"{TableBluePrint.TableName}\"(\n";
        List<ColumnBluePrint> TriggerRequiredColumns = new List<ColumnBluePrint>();
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.ColumnList.Select((Column, Index) => (Column, Index + 1)))
        {
            Transcript += "\t";
            if (Column.Statement != null)
                Transcript += Column.Statement;
            else
            {
                Transcript += ColumnHead(Column);
                Transcript += PrimaryKey(Column);
                Transcript += IDENTITY(TableBluePrint.TableName, Column);
                Transcript += ForeignKeySection(TableBluePrint.TableName, Column);
                Transcript += DefaultValue(Column);
                Transcript += NullabilityState(Column);
            }
            Transcript += TableBluePrint.ColumnList.Count > Index ? $",\n" : "\n";
            if (Column.ColumnDataType["DataTypeName"]!.ToString() == ColumnTypeEnum.Timestamp.ToString())
                TriggerRequiredColumns.Add(Column);
        }
        Transcript += $");\n\n ";
        // loop again in search for timestamp columns
        // add trigger
        // live happily ever after
        foreach (ColumnBluePrint Column in TriggerRequiredColumns)
        {
            Transcript += $"\n" +
                $"Create OR Replace Function \"{TableBluePrint.TableName}_UpdateTimestampFunction\"() Returns Trigger As $$" + "\n" +
                $"Begin " + "\n" +
                $"New.{Column.ColumnName} = Now(); " + "\n" +
                $"Return New; " + "\n" +
                $"End " + "\n" +
                $"$$ Language plpgsql; " + "\n" +
                $"Create Trigger \"{TableBluePrint.TableName}_UpdateTimestampFunction\" " + "\n" +
                $"Before Insert Or Update On \"{TableBluePrint.TableName}\" " + "\n" +
                $"For Each Row " + "\n" +
                $"Execute Function \"{TableBluePrint.TableName}_UpdateTimestampFunction\"();" + "\n\n";
        }
    }
    public String ColumnHead(ColumnBluePrint Column)
    {
        String Length = Column.ColumnDataType["Length"] != null ? Column.ColumnDataType["Length"]!.ToString() : "";
        String DataTypeParameter = Length != "" && Length!.ToLower() != "max" ? $"({Length})" : "";
        return $"{Column.ColumnName} {TranscribeDataType(Column.ColumnDataType["DataTypeName"]!.ToString(), Column.ColumnDataType["DataTypeSQLName"]!.ToString(), DataTypeParameter)} ";
    }
    public String IDENTITY(String TableName, ColumnBluePrint Column)
    {
        return Column.IsIdentity ? $"GENERATED ALWAYS AS IDENTITY ( INCREMENT {Column.Identity["IncrementValue"]} START {Column.Identity["StartValue"]} MINVALUE {Column.MinValue} MAXVALUE {Column.MaxValue} CACHE 1 ) " : "";
    }
    public override String ForeignKeySection(String TableName, ColumnBluePrint Column, Boolean AddForeKeyString = false)
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
        String ForeKeyStringPerRequest = "";
        if (AddForeKeyString)
            ForeKeyStringPerRequest = $"Foreign Key ({Column.ColumnName})";
        return $"CONSTRAINT {ConstraintName} {ForeKeyStringPerRequest} REFERENCES \"{ForeignTable}\" ({ForeignTableColumnName}) {OnUpdate} {OnDelete} ";
    }
    #endregion
    #region Alter
    public void Alter(TableBluePrint TableBluePrint)
    {
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.ColumnList.Select((Column, Index) => (Column, Index + 1)))
        {
            if (Column.Action == TableBluePrintAlterationAction.CreateColumn) Transcript += CreateColumn(Column, TableBluePrint.TableName);
            if (Column.Action == TableBluePrintAlterationAction.AlterColumnName) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.AlterColumnType) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            if (Column.Action == TableBluePrintAlterationAction.AddForeignKey) Transcript += ForeignKey(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.RemoveForeignKey) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            if (Column.Action == TableBluePrintAlterationAction.AddConstraint) Transcript += AddConstraint(Column, TableBluePrint.TableName);
            if (Column.Action == TableBluePrintAlterationAction.RemoveConstraint) Transcript += RemoveConstraint(Column, TableBluePrint.TableName);
        }
    }
    public String CreateColumn(ColumnBluePrint Column, String TableName)
    {
        String Transcript = $"Alter Table \"{TableName}\" \n\tAdd ";
        Transcript += ColumnHead(Column);
        Transcript += PrimaryKey(Column);
        Transcript += IDENTITY(TableName, Column);
        Transcript += ForeignKeySection(TableName, Column);
        Transcript += DefaultValue(Column);
        Transcript += NullabilityState(Column);
        Transcript += ";\n\n";
        return Transcript;
    }
    public String RenameColumn(ColumnBluePrint Column, String TableName)
    {
        return $"Alter Table \"{TableName}\" \n\tRename Column {Column.ColumnName} to {Column.ColumnRename};\n\n";
    }
    public String AlterColumnType(ColumnBluePrint Column, String TableName)
    {
        return "";
    }
    public String ForeignKey(ColumnBluePrint Column, String TableName)
    {
        return $"Alter Table \"{TableName}\"" + "\n\t" +
            $"Add {ForeignKeySection(TableName, Column,true)};\n\n";
    }
    public String AddConstraint(ColumnBluePrint Column, String TableName)
    {
        Int64 CurrentTick = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        if (Column.ConstraintName == "")
            Column.ConstraintName = TableName + "_" + CurrentTick;
        return $"Alter Table \"{TableName}\"" + "\n\t" +
            $"Add Constraint {Column.ConstraintName} Check ({Column.ConstraintQuery});\n\n";
    }
    public String RemoveConstraint(ColumnBluePrint Column, String TableName)
    {
        return $"Alter Table \"{TableName}\"" + "\n\t" +
            $"Drop Constraint {Column.ConstraintName};\n\n";
    }
    #endregion
    #region Drop
    public void Drop(TableBluePrint TableBluePrint)
    {
        Transcript += DropTableQuery(TableBluePrint);
    }
    public override String DropTableQuery(TableBluePrint TableBluePrint)
    {
        //TODO: must specify witch schema to drop to or create in to.
        return $"DROP TABLE IF EXISTS \"{TableBluePrint.ObjectName}\";\n\n";
    }
    #endregion
}
