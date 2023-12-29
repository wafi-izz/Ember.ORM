using Ember.DataSchemaManager.DataSchemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.Transcription.RDBMS.SqlServer;

public class SqlServerTranscriber
{
    public SqlServerTranscriber() { }
    public String Transcribe(DataSchema Schema)
    {
        SqlServerTableTranscriber TableTranscription = new SqlServerTableTranscriber(Schema.TableSchema);
        return TableTranscription.TransScript;
    }
}