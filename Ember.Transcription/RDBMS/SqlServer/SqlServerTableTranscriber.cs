using Ember.DataSchemaManager.BluePrints;
using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.TranscriptionInterfaces;
using System.Data.Common;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Principal;

namespace Ember.Transcription.RDBMS.SqlServer;


internal class SqlServerTableTranscriber : TableTranscriber, ITableTranscriber
{
    public String Transcript { get; set; }
    public TableSchema TableSchema { get; set; }
    public SqlServerTableTranscriber(TableSchema TableSchema)
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
        Transcript += $"CREATE TABLE {TableBluePrint.TableName}(\n";
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.ColumnList.Select((Column, Index) => (Column, Index + 1)))
        {
            Transcript += "\t";
            if (Column.Statemant != null)
                Transcript += Column.Statemant;
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
        Console.WriteLine("sda");
        }
        Transcript += $");\n\n ";
    }
    public String ColumnHead(ColumnBluePrint Column)
    {
        String Length = Column.ColumnDataType["Length"] != null ? Column.ColumnDataType["Length"]!.ToString() : "";
        String DataTypeParameter = Length != "" ? $"({Length})" : "";
        return $"{Column.ColumnName} {TranscribeDataType(Column.ColumnDataType["DataTypeName"]!.ToString(), Column.ColumnDataType["DataTypeSQLName"]!.ToString(), DataTypeParameter)} ";
    }
    public String IDENTITY(String TableName, ColumnBluePrint Column)
    {
        return Column.IsIdentity ? $"IDENTITY({Column.Identity["IncrementValue"]},{Column.Identity["StartValue"]}) CONSTRAINT CHC_{TableName}_IDENTITY_RANGE CHECK ({Column.ColumnName} >= {Column.MaxValue} AND {Column.ColumnName} <= {Column.MaxValue})" : "";
    }
    #endregion
    #region Alter
    public void Alter(TableBluePrint TableBluePrint)
    {
        List<ColumnBluePrint> OverlappedRenameAction = TableBluePrint.ColumnList.GroupBy(x => new { x.ColumnName, x.Action }).Where(x => x.Count() > 1).SelectMany(x => x).ToList();
        if (OverlappedRenameAction.Count() > 1)
        {
            throw new Exception($"Table {TableBluePrint.TableName}, Column {OverlappedRenameAction.First().ColumnName}\nthere are two rename actions on this column, one is action is sufficient");
        }
        foreach ((ColumnBluePrint Column, Int32 Index) in TableBluePrint.ColumnList.Select((Column, Index) => (Column, Index + 1)))
        {
            if (Column.Action == TableBluePrintAlterationAction.CreateColumn) Transcript += CreateColumn(Column, TableBluePrint.TableName);
            if (Column.Action == TableBluePrintAlterationAction.AlterColumnName) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if (Column.Action == TableBluePrintAlterationAction.AlterColumnType) Transcript += AlterColumnType(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.AddForeignKey) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.RemoveForeignKey) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.AddConstraint) Transcript += RenameColumn(Column, TableBluePrint.TableName);
            //if(Column.Action == TableBluePrintAlterationAction.RemoveConstraint) Transcript += RenameColumn(Column, TableBluePrint.TableName);
        }
    }
    public String CreateColumn(ColumnBluePrint Column, String TableName)
    {
        String Transcript = $"Alter Table {TableName}\n\tAdd ";
        Transcript += ColumnHead(Column);
        Transcript += PrimaryKey(Column);
        Transcript += IDENTITY(TableName, Column);
        Transcript += ForeignKeySection(TableName, Column);
        Transcript += DefaultValue(Column);
        Transcript += NullabilityState(Column);
        Transcript += "\n\n";
        return Transcript;
    }
    public String RenameColumn(ColumnBluePrint Column, String TableName)
    {
        // TODO : Remove check constraints (for now only check constraints) first and then re add then
        // "SELECT OBJECT_DEFINITION(object_id) AS ConstraintText FROM sys.check_constraints WHERE name = 'CHC_ObjectTypes_IDENTITYrANGE' AND parent_object_id = OBJECT_ID('ObjectTypes');"
        Int64 CurrentTick = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        String ConstraintName = $"{Column.ColumnName}_{CurrentTick}_ConstraintName";
        String CheckString = $"{Column.ColumnName}_{CurrentTick}_CheckString";
        String AlterScript = "\n" + 
            $"DECLARE @{ConstraintName} nvarchar(1000) = (SELECT cc.name AS CheckConstraintName FROM sys.check_constraints cc JOIN sys.columns c ON cc.parent_object_id = c.object_id AND cc.parent_column_id = c.column_id WHERE c.object_id = OBJECT_ID('{TableName}') AND c.name = '{Column.ColumnName}')\n" +
            $"DECLARE @{CheckString} nvarchar(1000) = REPLACE((SELECT OBJECT_DEFINITION(object_id) AS ConstraintText FROM sys.check_constraints WHERE name = @{ConstraintName} AND parent_object_id = OBJECT_ID('{TableName}')),'{Column.ColumnName}','{Column.ColumnRename}')\n" +
            $"if not (@{ConstraintName} = '')\n" +
                $"\tBEGIN\n" +
                    $"\t\tEXEC('ALTER TABLE {TableName} DROP CONSTRAINT ' + @{ConstraintName})\n" +
                    $"\t\tEXEC sp_rename '{TableName}.{Column.ColumnName}', '{Column.ColumnRename}', 'COLUMN'\n" +
                    $"\t\tEXEC('ALTER TABLE {TableName} ADD CONSTRAINT ' + @{ConstraintName} + ' CHECK ' + @{CheckString})\n" +
                $"\tEND\n" +
            $"ELSE\n" +
                    $"\tBEGIN\n" +
                        $"\t\tEXEC sp_rename '{TableName}.ObjectTypeID', 'NameColumnName', 'COLUMN'\n" +
                    $"\tEND\n";
        return $"{AlterScript}\n\n";
    }
    public String AlterColumnType(ColumnBluePrint Column, String TableName)
    {
        return "";
    }
    #endregion
    #region Drop
    public void Drop(TableBluePrint TableBluePrint)
    {
        Transcript += DropTableQuery(TableBluePrint);
    }
    #endregion
}
