using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Ember.DataScheme.BluePrints;


public class TableBluePrint : BluePrint
{
    public String TableName
    {
        get
        {
            return ObjectName;
        }
    }
    private ColumnBluePrint Column { get; set; }
    public List<ColumnBluePrint> Columns { get; set; }
    public TableBluePrint()
    {
        Columns = new List<ColumnBluePrint>();
    }
    public void Compose()
    {
        Columns.Add(Column);
    }
    #region DataTypes
    public void ColumnInit()
    {
        if (Column != null)
        {
            Columns.Add(Column);
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
        Column.Statemant = Statement;
    }
    public TableBluePrint Integer(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Integer.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", "INT");
        return this;
    }
    public TableBluePrint String(String ColumnName, dynamic Length, StringType StringType = StringType.Varchar)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.String.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", StringType.ToString().ToLower());
        Column.ColumnDataType.Add("Length", Length);
        return this;
    }
    #region String Variations
    public TableBluePrint Varchar(String ColumnName, dynamic Length, String Nationalize = "")
    {
        String(ColumnName, Length, Nationalize == "" ? StringType.Varchar : StringType.NVarchar);
        return this;
    }
    public TableBluePrint Char(String ColumnName, dynamic Length, String Nationalize = "")
    {
        String(ColumnName, Length, Nationalize == "" ? StringType.Char : StringType.NChar);
        return this;
    }
    public TableBluePrint Text(String ColumnName, dynamic Length, String Nationalize = "")
    {
        String(ColumnName, Length, Nationalize == "" ? StringType.Text : StringType.NText);
        return this;
    }
    public TableBluePrint Binary(String ColumnName, dynamic Length)
    {
        String(ColumnName, Length, StringType.Binary);
        return this;
    }
    public TableBluePrint Varbinary(String ColumnName, dynamic Length)
    {
        String(ColumnName, Length, StringType.Varbinary);
        return this;
    }
    #endregion
    public TableBluePrint Boolean(String ColumnName)
    {
        ColumnInit();
        Column.ColumnName = ColumnName;
        Column.ColumnDataType.Add("DataTypeName", ColumnTypeEnum.Boolean.ToString());
        Column.ColumnDataType.Add("DataTypeSQLName", "BIT");
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
        if (Columns.Count >= 1 && Columns.FirstOrDefault(x => x.IsPrimaryKey && x.ColumnName != Column.ColumnName) != null)
            throw new Exception($"the table '{TableName}' already a Primary Key assigned other than the Column '{Column.ColumnName}'");
        if (!Column.IsForeignKey)
            Column.IsPrimaryKey = true;
        else
            throw new Exception($"the column '{Column.ColumnName}' is already assigned as a Foreign Key");
        return this;
    }
    public TableBluePrint Identity(Int32 IncrementValue = 1, Int32 StartValue = 1)
    {
        if (Columns.Count >= 1 && Columns.FirstOrDefault(x => x.IsIdentity && x.ColumnName != Column.ColumnName) != null)
        {
            List<String> IdentitiedColumns = Columns.Where(x => x.IsIdentity).Select(x => x.ColumnName).ToList<String>();
            String IdentitiedColumnsSentance = "";
            foreach (var item in IdentitiedColumns)
            {
                IdentitiedColumnsSentance += item + " - ";
            }
            IdentitiedColumnsSentance += $"{Column.ColumnName}";
            throw new Exception($"Multiple identity columns specified for table '{TableName}' columns '{IdentitiedColumnsSentance}'. Only one identity column per table is allowed");
        }
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
    #endregion
}

public class ColumnBluePrint
{
    public String ColumnName { get; set; }
    public JsonObject ColumnDataType { get; set; }
    public Boolean IsPrimaryKey { get; set; }
    public Boolean IsIdentity { get; set; }
    public Boolean IsForeignKey { get; set; }
    public JsonObject ForeignKeyArguments { get; set; }
    public JsonObject Identity { get; set; }
    public dynamic Default { get; set; }
    public Boolean Nullable { get; set; }
    public String Statemant { get; set; }
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
    String,
    Boolean,
}
public enum StringType
{
    Text,
    NText,
    Binary,
    Varbinary,
    Char,
    NChar,
    Varchar,
    NVarchar,
}