using Ember.DataSchemaManager.BluePrints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.Transcription.TranscriptionInterfaces;

public interface ITableTranscriber
{
    void Transcribe();
    void Create(TableBluePrint TableBluePrint);
    string ColumnHead(ColumnBluePrint Column);
    string ForeignKeySection(string TableName, ColumnBluePrint Column);
    void Alter(TableBluePrint TableBluePrint);
    void Drop(TableBluePrint TableBluePrint);
}
