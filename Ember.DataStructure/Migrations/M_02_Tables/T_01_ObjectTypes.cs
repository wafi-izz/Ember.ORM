using Ember.DataSchemaManager.DataSchemas;
using Ember.DataSchemaManager.Dictionaries;
using Ember.DataSchemaManager.ObjectTypes;
using Ember.DataSchemaManager.BluePrints;
using Ember.DataStructure.Database;

namespace Ember.DataStructure.Migrations.M_02_Tables;

public class T_01_ObjectTypes : Table, IMigratablesDictionary
{
    public String TableName { get; set; }
    public T_01_ObjectTypes()
    {
        TableName = ExtractObjectName(this.GetType().Name);
    }
    public void Up()
    {
        GlobalDataSchema.PostgreDB.Create(TableName, Table =>
        {
            Table.Integer("ObjectTypeID").PrimaryKey().Identity().Min(1).Max(100);
            Table.String("ObjectTypeID").PrimaryKey().Identity().Min(1).Max(100);
            Table.Integer("ObjectTypeParentID").ForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
            Table.Integer("ObjectTypeParentID").References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
            Table.Integer("ObjectTypeParentID").ForeignKey().References("ObjectTypeID").Constraint("some_custom_name");
            Table.Integer("ObjectTypeParentID").ForeignKey().On("ObjectTypes").Constraint("some_custom_name");
            Table.String("ObjectTypeName", 500).Default("Some Name Default");
            Table.String("ObjectTypeName_AR", 500).Nullable();
            Table.Varchar("ShortName", "max");
            Table.String("ShortName_AR", 500);
            Table.Boolean("PermissionAble").Default(false).Nullable();
            Table.Boolean("Keyable").Default(false).Nullable();
            Table.Boolean("ObjectCustomPropertyable").Default(false).Nullable();
        });
        GlobalDataSchema.MSSQLDB.Create(TableName, Table =>
        {
            Table.Integer("ObjectTypeID").PrimaryKey().Identity();
            Table.Integer("ObjectTypeParentID").ForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
            Table.String("ObjectTypeName", 500).Default("Some Name Default");
            Table.String("ObjectTypeName_AR", 500).Nullable();
            Table.Varchar("ShortName", "max");
            Table.String("ShortName_AR", 500);
            Table.Boolean("PermissionAble").Default(false).Nullable();
            Table.Boolean("Keyable").Default(false).Nullable();
            Table.Boolean("ObjectCustomPropertyable").Default(false).Nullable();
        });
        GlobalDataSchema.MSSQLDB.Alter(TableName,"NewObjectType", Table =>
        {
            Table.AlterColumn("ObjectTypeID").Rename("NameColumnName");
            Table.AlterColumn("ObjectTypeID").String(500,StringType.NVARCHAR).Nullable();
            Table.AlterColumn("ObjectTypeID").AddConstraint("constrain stuff (full sentance)");
            Table.AlterColumn("ObjectTypeID").RemoveConstraint("constrain name to remove");
            Table.AlterColumn("ObjectTypeID").AddForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
            Table.AlterColumn("ObjectTypeID").RemoveForeignKey();
            Table.CreateColumn("NewColumn").Integer().Default(11).Nullable();
        });
        GlobalDataSchema.PostgreDB.Alter(TableName,"NewObjectType", Table =>
        {
            Table.AlterColumn("ObjectTypeID").Rename("NameColumnName");
            Table.AlterColumn("ObjectTypeID").String(500,StringType.NVARCHAR).Nullable();
            Table.AlterColumn("ObjectTypeID").AddConstraint("constrain stuff (full sentance)");
            Table.AlterColumn("ObjectTypeID").RemoveConstraint("constrain name to remove");
            Table.AlterColumn("ObjectTypeID").AddForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
            Table.AlterColumn("ObjectTypeID").RemoveForeignKey();
            Table.CreateColumn("NewColumn").Integer().Default(11).Nullable();
        });
        var tt = new TableBluePrint();
        Console.ReadLine();
    }
    public void Down()
    {
        GlobalDataSchema.MSSQLDB.DropTable(TableName);
        GlobalDataSchema.PostgreDB.DropTable(TableName);
    }
}
