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
    String TransScript { get; set; }
    TableSchema TableSchema { get; set; }
    void Transcribe();
    void Create(TableBluePrint TableBluePrint);
    String IDENTITY(ColumnBluePrint Column);
    void Alter(TableBluePrint TableBluePrint);
    void Drop(TableBluePrint TableBluePrint);
}
