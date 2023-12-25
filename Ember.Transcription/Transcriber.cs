using Ember.Transcription.RDBMS.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ember.Schemas;

namespace Ember.Transcription;

public class Transcriber
{
    public Schema Schema { get; set; }
    public SqlTypeEnum ActiveSQL { get; set; }
    public SqlServerTranscriber SqlServer { get; set; }
    public Transcriber(Schema Schema, SqlTypeEnum ActiveSQL)
    {
        this.Schema = Schema;
        SqlServer = new SqlServerTranscriber();
        this.ActiveSQL = ActiveSQL;
    }
    public String Transcribe()
    {
        // TODO : when transcribing any SQL DB type make a rule function to mkae it more adhesive to that SQL Provider type. 
        if (ActiveSQL == SqlTypeEnum.SqlServer) return SqlServer.Transcribe(Schema);
        return "";
    }
}
public enum SqlTypeEnum
{
    SQL,
    SqlServer,
    PostgreSql,
    MySql,
    SqlLite,
}