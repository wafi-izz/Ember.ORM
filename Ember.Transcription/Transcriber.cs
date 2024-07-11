using Ember.Transcription.RDBMS.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ember.DataSchemaManager.DataSchemas;
using Ember.Transcription.RDBMS.PostgreSql;

namespace Ember.Transcription;

public class Transcriber
{
    public DataSchema Schema { get; set; }
    private ITranscriber? RDBMSTranscriber { get; set; }
    public Transcriber(DataSchema Schema)
    {
        this.Schema = Schema;
    }
    public String? Transcribe()
    {
        if (Schema.DatabaseProvider == DatabaseProviderType.SqlServer) RDBMSTranscriber = new SqlServerTranscriber();
        else if (Schema.DatabaseProvider == DatabaseProviderType.PostgreSql) RDBMSTranscriber = new PostgreSqlTranscriber();
        else return null;

        return RDBMSTranscriber.Transcribe(Schema);
    }
}
