using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.RDBMS.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.Transcription.RDBMS.PostgreSql;

internal class PostgreSqlTranscriber : ITranscriber
{
    internal PostgreSqlTranscriber() { }
    public String Transcribe(DataSchema Schema)
    {
        return new PostgreSqlTableTranscriber(Schema.TableSchema).Transcript;
    }
}
