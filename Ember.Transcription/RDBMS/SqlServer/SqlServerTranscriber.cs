using Ember.DataSchemaManager.DataSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.Transcription.RDBMS.SqlServer;

internal class SqlServerTranscriber
{
    internal SqlServerTranscriber() { }
    internal String Transcribe(DataSchema Schema)
    {
        return new SqlServerTableTranscriber(Schema.TableSchema).TransScript;
    }
}