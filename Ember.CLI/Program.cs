using Ember.DataAccessManager;
using Ember.DataStructure;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Data.SqlClient;
using System.Data.SQLite;

try
{
    Console.WriteLine("Program Started.");

    String Main1 = EmberConnections.CS("Main1");
    String Helper2 = EmberConnections.CS("Helper2");
    String Side3 = EmberConnections.CS("Side3");
    String Backup4 = EmberConnections.CS("Backup4");

    SqlConnection Main1Conn = new SqlConnection(Main1);
    NpgsqlConnection Helper2Conn = new NpgsqlConnection(Helper2);
    MySqlConnection Side3Conn = new MySqlConnection(Side3);
    SQLiteConnection Backup4Conn = new SQLiteConnection(Backup4);

    Main1Conn.Open();
    SqlDataReader Main1Com = new SqlCommand("select 1 as fff",Main1Conn).ExecuteReader();
    Main1Conn.Close();


    Helper2Conn.Open();
    NpgsqlDataReader Helper2Com = new NpgsqlCommand("select 1 as fff", Helper2Conn).ExecuteReader();
    Helper2Conn.Close();


    Side3Conn.Open();
    MySqlDataReader Side3Com = new MySqlCommand("select 1 as fff", Side3Conn).ExecuteReader();
    Side3Conn.Close();


    Backup4Conn.Open();
    SQLiteDataReader Backup4Com = new SQLiteCommand("select 1 as fff", Backup4Conn).ExecuteReader();
    Backup4Conn.Close();

    var y = EmberConnections.CS("Main1");

    Int32 t = 1;

    List<String> GeneratedQuery = new Init().GeneratedSchema;

    Console.WriteLine("\n*************************************************************\n");
    Console.WriteLine(GeneratedQuery);
    Console.WriteLine("\n*************************************************************\n");

    string ConnectionString = EmberConnections.CS("Main1");
    DataAccess DA = new DataAccess(ConnectionString);
    DA.Transaction.Begin();
    DA.CreateCommand(GeneratedQuery[0]).ExecuteQuery();
    DA.Transaction.Commit();

    Console.WriteLine("Program Ended.");
}
catch (Exception e)
{
    Console.Error.WriteLine(e.Message);
}
