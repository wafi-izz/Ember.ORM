﻿using Ember.DataSchemaManager.DataSchemas;
using Ember.DataSchemaManager.Dictionaries;
using Ember.DataSchemaManager.ObjectTypes;
using Ember.DataSchemaManager.BluePrints;
using Ember.DataStructure.Database;
using static Ember.DataSchemaManager.DataSchemas.TableSchema;

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
        GlobalDataSchema.MSSQLDB.Create(TableName, Table =>
        {
            Table.Integer("ObjectTypeID").PrimaryKey().Identity().Min(1).Max(100);
            Table.Integer("DD").ForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
            Table.String("ObjectTypeName", 500).Default("Some Name Default");
            Table.String("ObjectTypeName_AR", 500).Nullable();
            Table.Varchar("ShortName", "max");
            Table.String("ShortName_AR", 500);
            Table.Boolean("PermissionAble").Default(false).Nullable();
            Table.Boolean("Keyable").Default(false).Nullable();
            Table.Boolean("ObjectCustomPropertyable").Default(false).Nullable();
            Table.Date("ObjectDate").Default("10/10/1995").Nullable();
            Table.Time("ObjectTime").Default("10:00:00").Nullable();
            Table.DateTime("ObjectDateTime").Default("10/10/1995 10:00:00").Nullable();
            Table.Timestamp("ObjectDateTimestamp").Nullable();
            Table.Xml("ObjectDateXml").Nullable();
            Table.Binary("ObjectDateBinary").Nullable();
            Table.VarBinary("ObjectDateVarBinary").Nullable();
            Table.Geometry("ObjectDateGeometry").Nullable();
            Table.File("ObjectDateFile").Nullable();
            Table.Image("ObjectDateImage").Nullable();
        });
        GlobalDataSchema.PostgreDB.Create(TableName, Table =>
        {
            Table.Integer("ObjectTypeID").PrimaryKey().Identity().Min(1).Max(100);
            Table.Integer("ObjectTypeParentID").ForeignKey().References("ObjectTypeID").On("ObjectTypes").Constraint("some_custom_name");
            Table.String("ObjectTypeName", 500).Default("Some Name Default");
            Table.String("ObjectTypeName_AR", 500).Nullable();
            Table.Varchar("ShortName", "max");
            Table.String("ShortName_AR", 500);
            Table.Boolean("PermissionAble").Default(false).Nullable();
            Table.Boolean("Keyable").Default(false).Nullable();
            Table.Boolean("ObjectCustomPropertyable").Default(false).Nullable();
            Table.Date("ObjectDate").Default("10/10/1995").Nullable();
            Table.Time("ObjectTime").Default("10:00:00").Nullable();
            Table.DateTime("ObjectDateTime").Default("10/10/1995 10:00:00").Nullable();
            Table.Timestamp("ObjectDateTimestamp").Nullable();
            Table.Xml("ObjectDateXml").Nullable();
            Table.Binary("ObjectDateBinary").Nullable();
            Table.VarBinary("ObjectDateVarBinary").Nullable();
            //Table.Geometry("ObjectDateGeometry").Nullable();
            Table.File("ObjectDateFile").Nullable();
            Table.Image("ObjectDateImage").Nullable();
        });
        GlobalDataSchema.MSSQLDB.Alter(TableName, "NewObjectType", Table =>
        {
            Table.AlterColumn("ObjectTypeID").Rename("NameColumnName");
            Table.AlterColumn("ObjectTypeID").AddConstraint("1=1","ccc");
            Table.AlterColumn("ObjectTypeID").RemoveConstraint("ccc");
            Table.AlterColumn("ObjectTypeID").RemoveForeignKey();
            Table.CreateColumn("NewColumn").Integer().Default(11).Nullable();
            Table.AlterColumn("NewColumn").AddForeignKey().References("NameColumnName").On("ObjectTypes").Constraint("some_custom_name_2");
        });
        GlobalDataSchema.PostgreDB.Alter(TableName, "NewObjectType", Table =>
        {
            Table.AlterColumn("ObjectTypeID").Rename("NameColumnName");
            Table.AlterColumn("ObjectTypeID").String(500, StringType.NVARCHAR).Nullable();
            Table.AlterColumn("ObjectTypeID").AddConstraint("1=1","ttt");
            Table.AlterColumn("ObjectTypeID").RemoveConstraint("ttt");
            Table.AlterColumn("ObjectTypeID").RemoveForeignKey();
            Table.CreateColumn("NewColumn").Integer().Default(11).Nullable();
            Table.AlterColumn("NewColumn").AddForeignKey().References("NameColumnName").On("ObjectTypes").Constraint("some_custom_name_2");
        });
    }
    public void Down()
    {
        GlobalDataSchema.MSSQLDB.DropTable(TableName);
        GlobalDataSchema.PostgreDB.DropTable(TableName);
    }
}
