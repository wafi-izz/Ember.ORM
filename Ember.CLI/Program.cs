using Ember.DataAccessManager;
using Ember.DataStructure;
using MySql.Data.MySqlClient;
using Npgsql;
using System.Data.SqlClient;
using System.Data.SQLite;

try
{
    Console.WriteLine("Program Started.");

    //ConnectionStringManager Main1 = new ConnectionStringManager("Main1");
    //ConnectionStringManager Helper2 = new ConnectionStringManager("Helper2");
    //ConnectionStringManager Side3 = new ConnectionStringManager("Side3");
    //ConnectionStringManager Backup4 = new ConnectionStringManager("Backup4");


    //SqlConnection Main1Conn = new SqlConnection(Main1.CS);
    //NpgsqlConnection Helper2Conn = new NpgsqlConnection(Helper2.CS);
    //MySqlConnection Side3Conn = new MySqlConnection(Side3.CS);
    //SQLiteConnection Backup4Conn = new SQLiteConnection(Backup4.CS);

    //Main1Conn.Open();
    //SqlDataReader Main1Com = new SqlCommand("select 1 as fff", Main1Conn).ExecuteReader();
    //Main1Conn.Close();


    //Helper2Conn.Open();
    //NpgsqlDataReader Helper2Com = new NpgsqlCommand("select 1 as fff", Helper2Conn).ExecuteReader();
    //Helper2Conn.Close();


    //Side3Conn.Open();
    //MySqlDataReader Side3Com = new MySqlCommand("select 1 as fff", Side3Conn).ExecuteReader();
    //Side3Conn.Close();


    //Backup4Conn.Open();
    //SQLiteDataReader Backup4Com = new SQLiteCommand("select 1 as fff", Backup4Conn).ExecuteReader();
    //Backup4Conn.Close();


    List<String> GeneratedQuery = new Init().GeneratedSchema;
    Console.WriteLine("\n*************************************************************\n");
    Console.WriteLine(GeneratedQuery);
    Console.WriteLine("\n*************************************************************\n");




    String GeneratedQuery2 =    "create table a(id int)" +
                                "select 1 as result";
    String GeneratedQuery3 = "create table a1(id int)" +
                                "select 1 as result";
    ConnectionStringManager ConnectionString = new ConnectionStringManager("Side3");
    DataAccess DA = new DataAccess(ConnectionString);
    DA.Transaction.Begin();
    var tb = DA.CreateCommand(GeneratedQuery3).ExecuteQuery();
    var ta = DA.CreateCommand(GeneratedQuery2).ExecuteQuery();
    DA.Transaction.Commit();
    

    GeneratedQuery2 =    "create table a(id int);" +
                                "select 1 as result;";
    GeneratedQuery3 = "create table a1(id int);" +
                                "select 1 as result;";
    ConnectionString = new ConnectionStringManager("Backup4");
    DA = new DataAccess(ConnectionString);
    DA.Transaction.Begin();
    tb = DA.CreateCommand(GeneratedQuery3).ExecuteQuery();
    ta = DA.CreateCommand(GeneratedQuery2).ExecuteQuery();
    DA.Transaction.Commit();

    Console.WriteLine("Program Ended.");
}
catch (Exception e)
{
    Console.Error.WriteLine(e.Message);
}
