using DataStructure.Base.DatabaseObjects;
using Ember.DataStructure.Base;
using Ember.DataSchemaManager.DataSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataStructure.Migrations.M_02_Tables;

public class T_01_ObjectTypes : Table, IMigratablesDictionary
{
    public String TableName { get; set; }
    public DataSchema Schema { get; set; }
    public T_01_ObjectTypes()
    {
        TableName = ExtractObjectName(this.GetType().Name);
    }
    public void Up()
    {
        Schema.Create(TableName, Table =>
        {
            Table.Integer("ObjectTypeID").PrimaryKey().Identity();
            Table.Integer("ObjectTypeParentID").ForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
            Table.String("ObjectTypeName", 500).Default("some name default");
            Table.String("ObjectTypeName_AR", 500).Nullable();
            Table.Varchar("ShortName", "max");
            Table.String("ShortName_AR", 500);
            Table.Boolean("PermissionAble").Default(false).Nullable();
            Table.Boolean("Keyable").Default(false).Nullable();
            Table.Boolean("ObjectCustomPropertyable").Default(false).Nullable();
        });
        Schema.Create(TableName + "2", Table =>
        {
            Table.Integer("ObjectTypeID").PrimaryKey().Identity();
            Table.Integer("ObjectTypeParentID").ForeignKey().References("ObjectTypeID").On("ObjectTypes").OnDelete("cascade").OnUpdate("cascade");
            Table.String("ObjectTypeName", 500).Default("some name default");
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
        Schema.DropTable(TableName + "2");
        Schema.DropTable(TableName);
    }
}