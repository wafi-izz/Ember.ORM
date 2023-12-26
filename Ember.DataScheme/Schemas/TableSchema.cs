﻿using Ember.DataScheme.BluePrints;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataScheme.Schemas;

public class TableSchema
{
    public ObservableCollection<TableBluePrint> TableBluePrintList { get; set; }
    public ObservableCollection<String> TableList { get; }
    public TableSchema(ObservableCollection<String> TableList)
    {
        TableBluePrintList = new ObservableCollection<TableBluePrint>();
        this.TableList = TableList;
    }
    public Boolean HasTable(string TableName)
    {
        return (TableList.Contains(TableName));
    }
    public delegate void TableBluePrintCallBack(TableBluePrint TableBluePrint);
    public void Create(String TableName, TableBluePrintCallBack TableBluePrintCallBack)
    {
        TableBluePrint TableBluePrint = new TableBluePrint();
        TableBluePrint.Action = BluePrintAction.Create;
        TableBluePrint.ObjectName = TableName;
        TableBluePrintCallBack.Invoke(TableBluePrint);
        TableBluePrint.Compose();
        TableBluePrintList.Add(TableBluePrint);
    }
    public void Drop(String TableName)
    {
        TableBluePrint TableBluePrint = new TableBluePrint();
        TableBluePrint.Action = BluePrintAction.Drop;
        TableBluePrint.ObjectName = TableName;
        TableBluePrintList.Add(TableBluePrint);
    }
}