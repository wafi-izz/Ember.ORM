using Ember.DataSchemaManager.DataSchemas;
using Ember.DataSchemaManager.Dictionaries;
using Ember.DataSchemaManager.ObjectTypes;
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
        GlobalDataSchema.MyPostgreDB.Create(TableName, Table =>
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
        GlobalDataSchema.MyMSSQLDB.Create(TableName, Table =>
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
        GlobalDataSchema.MyMSSQLDB.Create(TableName + "2", Table =>
        {
            Table.Integer("ObjectTypeID").PrimaryKey().Identity();
            Table.Integer("ObjectTypeParentID").ForeignKey().References("ObjectTypeID").On("ObjectTypes").OnDelete("cascade").OnUpdate("cascade");
            Table.String("ObjectTypeName", 500).Default("Some Name Default");
            Table.String("ObjectTypeName_AR", 500).Nullable();
            Table.Varchar("ShortName", "max", "N");
            Table.String("ShortName_AR", 500);
            Table.Boolean("PermissionAble").Default(false).Nullable();
            Table.Boolean("Keyable").Default(false).Nullable();
            Table.Boolean("ObjectCustomPropertyable").Default(false).Nullable();
            Table.RowStatement("some_name int default(11) null");
        });

    }
    public void Down()
    {
        GlobalDataSchema.MyMSSQLDB.DropTable(TableName + "2");
        GlobalDataSchema.MyMSSQLDB.DropTable(TableName);
        GlobalDataSchema.MyPostgreDB.DropTable(TableName);
    }
}
