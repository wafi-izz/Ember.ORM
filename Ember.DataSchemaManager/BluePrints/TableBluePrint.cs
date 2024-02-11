using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static Ember.DataSchemaManager.SharedFuncctions.Shared;

namespace Ember.DataSchemaManager.BluePrints;

/* TODO:
 * add indexes
 * **** alter
    * make the add remove constraints
    * make the add remove foreign key
    * ... and what else
 * **** create
    * last of all - add the reset of variations
 */
/* TODO:
 * setting file for each database for conventions and such
 * global variables for static values line integer size and it siblings
 */


/*Thought Bubble
 * common constraints
    * less than number
    * more than number
    * less than other field
    * more than other field
    * less than sql value
    * more than sql value
    * is within a list (value in (1, 2, 3))
    * had boolean (less than number or more than sql value)
 */

public class TableBluePrint : BluePrint
{
    public String TableName
    {
        get
        {
            return ObjectName;
        }
    }
    public String TableRename
    {
        get
        {
            return ObjectRename;
        }
    }
    private ColumnBluePrint Column { get; set; }
    private Boolean ColumnInitRequired { get; set; }
    public List<ColumnBluePrint> ColumnList { get; set; }
    public TableBluePrint()
    {
        ColumnInitRequired = true;
        ColumnList = new List<ColumnBluePrint>();
    }
    public void Compose()
    {
        ColumnList.Add(Column);
    }
    public void ColumnInit()
    {
        if (!ColumnInitRequired)
        {
            ColumnInitRequired = true;
            return;
        }

        if (Column != null)
        {
            ColumnList.Add(Column);
            Column = new ColumnBluePrint();
        }
        else
        {
            Column = new ColumnBluePrint();
        }
    }
    public void RowStatement(String Statement)
    {
        ColumnInit();
        Column.Statement = Statement;
    }
    #region Create
    #region First Phase
    public TableBluePrint Integer(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Integer.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", "INT");
        return this;
    }
    public TableBluePrint String(String ColumnName, dynamic Length, StringType StringType = StringType.VARCHAR)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.String.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", StringType.ToString());
        Column.ColumnDataType.Add("Length", Length);
        return this;
    }
    #region String Variations
    public TableBluePrint Varchar(String ColumnName, dynamic Length, Boolean Nationalize = false)
    {
        String(ColumnName, Length, Nationalize ? StringType.VARCHAR : StringType.NVARCHAR);
        return this;
    }
    public TableBluePrint Char(String ColumnName, dynamic Length, Boolean Nationalize = false)
    {
        String(ColumnName, Length, Nationalize ? StringType.CHAR : StringType.NCHAR);
        return this;
    }
    public TableBluePrint Text(String ColumnName, dynamic Length, Boolean Nationalize = false)
    {
        String(ColumnName, Length, Nationalize ? StringType.TEXT : StringType.NTEXT);
        return this;
    }
    #endregion
    public TableBluePrint Boolean(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Boolean.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.Boolean.ToString()); // this is an specific sql provider datatype shouldn't be defined here.
        return this;
    }
    public TableBluePrint Date(String ColumnName) //TODO: see if SQL providers specify a format.
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Date.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.Date.ToString());
        return this;
    }
    public TableBluePrint Time(String ColumnName) //TODO: see if SQL providers specify a format.
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Time.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.Time.ToString());
        return this;
    }
    public TableBluePrint DateTime(String ColumnName) //TODO: see if SQL providers specify a format.
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.DateTime.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.DateTime.ToString());
        return this;
    }
    public TableBluePrint Timestamp(String ColumnName) //TODO: see if SQL providers specify a format.
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Timestamp.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.Timestamp.ToString());
        return this;
    }
    public TableBluePrint Xml(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Xml.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.Xml.ToString());
        return this;
    }
    public TableBluePrint Binary(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Binary.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.Binary.ToString());
        return this;
    }
    public TableBluePrint VarBinary(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.VarBinary.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.VarBinary.ToString());
        return this;
    }
    public TableBluePrint Geometry(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Geometry.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.Geometry.ToString());
        return this;
    }
    public TableBluePrint File(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.File.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.File.ToString());
        return this;
    }
    public TableBluePrint Image(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Image.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", ColumnTypeEnum.Image.ToString());
        return this;
    }
    #endregion
    #region Foreigns
    public TableBluePrint ForeignKey()
    {
        if (!Column.IsPrimaryKey)
            Column.IsForeignKey = true;
        else
            throw new Exception($"the column '{Column.ColumnName}' is already assigned as a Primary Key");
        return this;
    }
    public TableBluePrint References(String ForeignTableColumnName)
    {
        Column.ForeignKeyArguments.Add(nameof(ForeignTableColumnName), ForeignTableColumnName);
        return this;
    }
    public TableBluePrint On(String ForeignTable)
    {
        Column.ForeignKeyArguments.Add(nameof(ForeignTable), ForeignTable);
        return this;
    }
    public TableBluePrint OnDelete(String OnDelete)
    {
        Column.ForeignKeyArguments.Add(nameof(OnDelete), OnDelete);
        return this;
    }
    public TableBluePrint OnUpdate(String OnUpdate)
    {
        Column.ForeignKeyArguments.Add(nameof(OnUpdate), OnUpdate);
        return this;
    }
    public TableBluePrint Constraint(String CustomConstraintName)
    {
        Column.ForeignKeyArguments.Add(nameof(CustomConstraintName), CustomConstraintName);
        return this;
    }
    #endregion
    #region Second Phase
    public TableBluePrint PrimaryKey()
    {
        if (ColumnList.Count >= 1 && ColumnList.FirstOrDefault(x => x.IsPrimaryKey && x.ColumnName != Column.ColumnName) != null)
            throw new ArgumentException($"the table '{TableName}' already a Primary Key assigned other than the Column '{Column.ColumnName}'");
        if (!Column.IsForeignKey)
            Column.IsPrimaryKey = true;
        else
            throw new ArgumentException($"the column '{Column.ColumnName}' is already assigned as a Foreign Key");
        return this;
    }
    public TableBluePrint Identity(Int32 IncrementValue = 1, Int32 StartValue = 1)
    {
        /* this might not be needed */
        if (ColumnList.Count >= 1 && ColumnList.FirstOrDefault(x => x.IsIdentity && x.ColumnName != Column.ColumnName) != null)
        {
            List<String> IdentitiedColumns = ColumnList.Where(x => x.IsIdentity).Select(x => x.ColumnName).ToList<String>();
            String IdentitiedColumnsSentance = "";
            foreach (var item in IdentitiedColumns)
            {
                IdentitiedColumnsSentance += item + " - ";
            }
            IdentitiedColumnsSentance += $"{Column.ColumnName}";
            throw new ArgumentException($"Multiple identity columns specified for table '{TableName}' columns '{IdentitiedColumnsSentance}'. Only one identity column per table is allowed");
        }
        /* --- */
        Column.IsIdentity = true;
        Column.Identity.Add(nameof(IncrementValue), IncrementValue);
        Column.Identity.Add(nameof(StartValue), StartValue);
        return this;
    }
    public TableBluePrint Default(dynamic DefaultValue)
    {
        Column.Default = DefaultValue;
        return this;
    }
    public TableBluePrint Nullable()
    {
        Column.Nullable = true;
        return this;
    }
    public TableBluePrint Min(dynamic Value)
    {
        if (!IsNumeric(Value.ToString()))
            throw new ArgumentException("Min Value Must Be a Number!");
        Column.MinValue = Value;
        return this;
    }
    public TableBluePrint Max(dynamic Value)
    {
        if (!IsNumeric(Value.ToString()))
            throw new ArgumentException("Max Value Must Be a Number!");
        Column.MaxValue = Value;
        return this;
    }
    #endregion
    #endregion
    #region Alter
    #region First Phase
    public TableBluePrint AlterColumn(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        return this;
    }
    public TableBluePrint CreateColumn(String ColumnName)
    {
        ColumnInit();
        Column.Action = TableBluePrintAlterationAction.CreateColumn;
        Column.ColumnName = ColumnName;
        return this;
    }
    #endregion
    #region Second Phase
    public void Rename(String NewName)
    {
        Column.Action = TableBluePrintAlterationAction.AlterColumnName;
        Column.ColumnRename = NewName;
    }
    public void InitilizeAlteration()
    {
        if (Column.Action == null) Column.Action = TableBluePrintAlterationAction.AlterColumnType;
        ColumnInitRequired = false;
    }
    public TableBluePrint Integer()
    {
        InitilizeAlteration();
        Integer(Column.ColumnName);
        return this;
    }
    public TableBluePrint String(dynamic Length, StringType StringType = StringType.VARCHAR)
    {
        InitilizeAlteration();
        String(Column.ColumnName, Length, StringType);
        return this;
    }
    public TableBluePrint Boolean()
    {
        InitilizeAlteration();
        Boolean(Column.ColumnName);
        return this;
    }
    public TableBluePrint AddConstraint(String ConstrainQuery)
    {
        Column.Action = TableBluePrintAlterationAction.AddConstraint;
        Column.ConstrainQuery = ConstrainQuery;
        return this;
    }
    public TableBluePrint RemoveConstraint(String ConstrainName)
    {
        Column.Action = TableBluePrintAlterationAction.RemoveConstraint;
        Column.ConstrainName = ConstrainName;
        return this;
    }
    public TableBluePrint AddForeignKey()
    {
        Column.Action = TableBluePrintAlterationAction.AddForeignKey;
        ForeignKey();
        return this;
    }
    public void RemoveForeignKey()
    {
        Column.Action = TableBluePrintAlterationAction.RemoveForeignKey;
        Column.RemovedForeignKey = true;
    }
    public TableBluePrint LessThan(dynamic Value)
    {
        if (!IsNumeric(Value.ToString()))
            throw new ArgumentException("Min Value Must Be a Number!");
        Column.MinValue = Value;
        return this;
    }
    public TableBluePrint MoreThan(dynamic Value)
    {
        if (!IsNumeric(Value.ToString()))
            throw new ArgumentException("Max Value Must Be a Number!");
        Column.MaxValue = Value;
        return this;
    }
    #endregion
    #endregion
}

public class ColumnBluePrint
{
    public String Statement { get; set; }
    public String ColumnName { get; set; }
    public JsonObject ColumnDataType { get; } //NOTE: there an element name "DataTypeSQLName" it is used to distinguish between the different naming across the providers.
    public Boolean IsPrimaryKey { get; set; }
    public Boolean IsIdentity { get; set; }
    public Boolean IsForeignKey { get; set; }
    public JsonObject ForeignKeyArguments { get; }
    public JsonObject Identity { get; }
    public dynamic Default { get; set; }
    public Boolean Nullable { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public String ColumnRename { get; set; }
    public String ConstrainQuery { get; set; }
    public String ConstrainName { get; set; }
    public Boolean RemovedForeignKey { get; set; }
    public TableBluePrintAlterationAction? Action { get; set; }
    public ColumnBluePrint()
    {
        ColumnDataType = new JsonObject();
        ForeignKeyArguments = new JsonObject();
        Identity = new JsonObject();
    }
}
public enum ColumnTypeEnum
{
    Integer,
    Numeric,
    String,
    Boolean,
    Date,
    Time,
    DateTime,
    Timestamp,
    Xml,
    Binary,
    VarBinary,
    Geometry,
    File,
    Image,
    //i think these are what I'll support for now
}
// aaaah ... should all the supported types be here or over at the transcriber class
public enum StringType
{
    TEXT,
    NTEXT,
    CHAR,
    NCHAR,
    VARCHAR,
    NVARCHAR,
}
//TODO: integer and its types enum
public enum IntegerType { }
//TODO: Numeric and its types enum
public enum NumericType { }