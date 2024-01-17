using Ember.DataSchemaManager.BluePrints;
using Ember.DataSchemaManager.DataSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.Transcription.TranscriptionInterfaces;

public interface ITableTranscriber
{
    String Transcript { get; set; }
    TableSchema TableSchema { get; set; }
    void Transcribe();
    void Create(TableBluePrint TableBluePrint);
    String ColumnHead(ColumnBluePrint Column);
    String IDENTITY(String TableName,ColumnBluePrint Column);
    void Alter(TableBluePrint TableBluePrint);
    String CreateColumn(ColumnBluePrint Column, String TableName);
    String RenameColumn(ColumnBluePrint Column, String TableName);
    String AlterColumnType(ColumnBluePrint Column, String TableName);
    void Drop(TableBluePrint TableBluePrint);
}
