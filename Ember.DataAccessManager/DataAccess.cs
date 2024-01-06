using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Security.Principal;
using Npgsql;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;

namespace Ember.DataAccessManager;

public enum DatabaseProviderEnum
{
    SQL,
    SqlServer,
    PostgreSql,
    MySql,
    Sqlite,
}
public class DataAccess
{
    public DataConnection Connection { get; set; }
    public DataCommand Command { get; set; }
    public String? CommandString { get; set; }
    public dynamic? ReaderData { get; set; }
    public DataTable DataTable { get; set; }
    //Transaction
    public DataAccessTransaction Transaction { get; set; }
    public DataAccess(ConnectionStringManager ConncetionString)
    {
        Connection = new DataConnection(ConncetionString);
        Connection.Open();
        DataTable = new DataTable();
        Transaction = new DataAccessTransaction(Connection);
    }
    public DataAccess CreateCommand(String Query)
    {
        Command = new DataCommand(Query, Connection);
        if (Transaction.TransactionState) Command.Transaction = Transaction.Transaction;
        return this;
    }
    public DataTable ExecuteQuery()
    {
        try
        {
            ReaderData = Command.ExecuteReader();
            DataTable.Load(ReaderData);
        }
        catch (Exception)
        {
            if (Transaction.TransactionState) Transaction.RollBack();
            throw;
        }
        finally
        {
            if (Transaction == null)
            {
                Connection.Close();
            }
        }
        return DataTable;
    }
}


public class DataAccessTransaction
{
    public DataTransaction Transaction { get; set; }
    public DataConnection Connection { get; set; }
    public Boolean TransactionState { get; set; }
    public DataAccessTransaction(DataConnection Connection)
    {
        this.Connection = Connection;
        Transaction = new DataTransaction(Connection);
        TransactionState = false;
    }
    public void Begin()
    {
        Transaction.Begin();
        TransactionState = true;
    }
    public void Commit()
    {
        Transaction?.Commit();
        TransactionState = false;
    }
    public void RollBack()
    {
        Transaction.Rollback();
        TransactionState = false;
    }
}


public class ConnectionStringManager
{
    public String ConnectionName { get; set; }
    public String ConnectionString
    {
        get
        {
            return CS;
        }
    }
    public String CS
    {
        get
        {
            return ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;
        }
    }
    public String Provider
    {
        get
        {
            return ConfigurationManager.ConnectionStrings[ConnectionName].ProviderName;
        }
    }
    public ConnectionStringManager(String ConnectionName)
    {
        this.ConnectionName = ConnectionName;
    }
}







#region sub area for connection types
public class DataConnection
{
    public Type? Connection { get; set; }
    public SqlConnection? MSSQLConnection { get; set; }
    public NpgsqlConnection? PostgreSQLConnection { get; set; }
    public MySqlConnection? MySQLConnection { get; set; }
    public SQLiteConnection? SQLiteConnection { get; set; }
    public DataConnection(ConnectionStringManager DataConnections)
    {
        if (DataConnections.Provider == DatabaseProviderEnum.SqlServer.ToString()) MSSQLConnection = new SqlConnection(DataConnections.CS);
        if (DataConnections.Provider == DatabaseProviderEnum.PostgreSql.ToString()) PostgreSQLConnection = new NpgsqlConnection(DataConnections.CS);
        if (DataConnections.Provider == DatabaseProviderEnum.MySql.ToString()) MySQLConnection = new MySqlConnection(DataConnections.CS);
        if (DataConnections.Provider == DatabaseProviderEnum.Sqlite.ToString()) SQLiteConnection = new SQLiteConnection(DataConnections.CS);

        if (DataConnections.Provider == DatabaseProviderEnum.SqlServer.ToString()) Connection = typeof(SqlConnection);
        if (DataConnections.Provider == DatabaseProviderEnum.PostgreSql.ToString()) Connection = typeof(NpgsqlConnection);
        if (DataConnections.Provider == DatabaseProviderEnum.MySql.ToString()) Connection = typeof(MySqlConnection);
        if (DataConnections.Provider == DatabaseProviderEnum.Sqlite.ToString()) Connection = typeof(SQLiteConnection);

    }
    public void Open()
    {
        if (Connection == typeof(SqlConnection))
        {
            MSSQLConnection!.Open();
        }
        if (Connection == typeof(NpgsqlConnection))
        {
            PostgreSQLConnection!.Open();
        }
        if (Connection == typeof(MySqlConnection))
        {
            MySQLConnection!.Open();
        }
        if (Connection == typeof(SQLiteConnection))
        {
            SQLiteConnection!.Open();
        }
    }
    public void Close()
    {
        if (Connection == typeof(SqlConnection))
        {
            MSSQLConnection!.Close();
        }
        if (Connection == typeof(NpgsqlConnection))
        {
            PostgreSQLConnection!.Close();
        }
        if (Connection == typeof(MySqlConnection))
        {
            MySQLConnection!.Close();
        }
        if (Connection == typeof(SQLiteConnection))
        {
            SQLiteConnection!.Close();
        }
    }
    public dynamic BeginTransaction()
    {
        if (Connection == typeof(SqlConnection))
        {
            return MSSQLConnection!.BeginTransaction();
        }
        if (Connection == typeof(NpgsqlConnection))
        {
            return PostgreSQLConnection!.BeginTransaction();
        }
        if (Connection == typeof(MySqlConnection))
        {
            return MySQLConnection!.BeginTransaction();
        }
        if (Connection == typeof(SQLiteConnection))
        {
            return SQLiteConnection!.BeginTransaction();
        }
        return null!;
    }
}
public class DataTransaction
{
    public Boolean TransactionState { get; set; }
    public DataConnection DataConnection { get; set; }
    public Type? Transaction { get; set; }
    public SqlTransaction? MSSQLTransaction { get; set; }
    public NpgsqlTransaction? PostgreSQLTransaction { get; set; }
    public MySqlTransaction? MySQLTransaction { get; set; }
    public SQLiteTransaction? SQLiteTransaction { get; set; }
    public DataTransaction(DataConnection DataConnection)
    {
        this.DataConnection = DataConnection;
        if (DataConnection.Connection == typeof(SqlConnection)) Transaction = typeof(SqlTransaction);
        if (DataConnection.Connection == typeof(NpgsqlConnection)) Transaction = typeof(NpgsqlTransaction);
        if (DataConnection.Connection == typeof(MySqlConnection)) Transaction = typeof(MySqlTransaction);
        if (DataConnection.Connection == typeof(SQLiteConnection)) Transaction = typeof(SQLiteTransaction);
    }
    public void Begin()
    {
        if (Transaction == typeof(SqlTransaction))
        {
            MSSQLTransaction = DataConnection.BeginTransaction();
            TransactionState = true;
        }
        if (Transaction == typeof(NpgsqlTransaction))
        {
            PostgreSQLTransaction = DataConnection.BeginTransaction();
            TransactionState = true;
        }
        if (Transaction == typeof(MySqlTransaction))
        {
            MySQLTransaction = DataConnection.BeginTransaction();
            TransactionState = true;
        }
        if (Transaction == typeof(SQLiteTransaction))
        {
            SQLiteTransaction = DataConnection.BeginTransaction();
            TransactionState = true;
        }
    }
    public void Commit()
    {
        if (Transaction == typeof(SqlTransaction))
        {
            if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
            MSSQLTransaction!.Commit();
            DataConnection.Close();
            TransactionState = false;
        }
        if (Transaction == typeof(NpgsqlTransaction))
        {
            if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
            PostgreSQLTransaction!.Commit();
            DataConnection.Close();
            TransactionState = false;
        }
        if (Transaction == typeof(MySqlTransaction))
        {
            if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
            MySQLTransaction!.Commit();
            DataConnection.Close();
            TransactionState = false;
        }
        if (Transaction == typeof(SQLiteTransaction))
        {
            if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
            SQLiteTransaction!.Commit();
            DataConnection.Close();
            TransactionState = false;
        }
    }
    public void Rollback()
    {
        if (Transaction == typeof(SqlTransaction))
        {
            if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
            if (DataConnection != null)
            {
                MSSQLTransaction!.Rollback();
                TransactionState = false;
            }
        }
        if (Transaction == typeof(NpgsqlTransaction))
        {
            if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
            if (DataConnection != null)
            {
                PostgreSQLTransaction!.Rollback();
                TransactionState = false;
            }
        }
        if (Transaction == typeof(MySqlTransaction))
        {
            if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
            if (DataConnection != null)
            {
                MySQLTransaction!.Rollback();
                TransactionState = false;
            }
        }
        if (Transaction == typeof(SQLiteTransaction))
        {
            if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
            if (DataConnection != null)
            {
                SQLiteTransaction!.Rollback();
                TransactionState = false;
            }
        }
    }
}
public class DataCommand
{
    public DataConnection DataConnection { get; set; }
    public DataTransaction Transaction { get; set; }
    public SqlCommand? MSSQLCommand { get; set; }
    public NpgsqlCommand? PostgreSQLCommand { get; set; }
    public MySqlCommand? MySQLCommand { get; set; }
    public SQLiteCommand? SQLiteCommand { get; set; }
    public DataCommand(String Query,DataConnection DataConnection)
    {
        this.DataConnection = DataConnection;
        if (DataConnection.Connection == typeof(SqlConnection)) MSSQLCommand = new SqlCommand(Query,DataConnection.MSSQLConnection);
        if (DataConnection.Connection == typeof(NpgsqlConnection)) PostgreSQLCommand = new NpgsqlCommand(Query, DataConnection.PostgreSQLConnection);
        if (DataConnection.Connection == typeof(MySqlConnection)) MySQLCommand = new MySqlCommand(Query, DataConnection.MySQLConnection);
        if (DataConnection.Connection == typeof(SQLiteConnection)) SQLiteCommand = new SQLiteCommand(Query, DataConnection.SQLiteConnection);
    }
    public dynamic ExecuteReader()
    {
        if (DataConnection.Connection == typeof(SqlConnection))
        {
            MSSQLCommand!.Transaction = Transaction!.MSSQLTransaction;
            return MSSQLCommand!.ExecuteReader();
        }
        if (DataConnection.Connection == typeof(NpgsqlConnection))
        {
            PostgreSQLCommand!.Transaction = Transaction!.PostgreSQLTransaction;
            return PostgreSQLCommand!.ExecuteReader();
        }
        if (DataConnection.Connection == typeof(MySqlConnection))
        {
            MySQLCommand!.Transaction = Transaction!.MySQLTransaction;
            return MySQLCommand!.ExecuteReader();
        }
        if (DataConnection.Connection == typeof(SQLiteConnection))
        {
            SQLiteCommand!.Transaction = Transaction!.SQLiteTransaction;
            return SQLiteCommand!.ExecuteReader();
        }
        return null!;
    }
}
#endregion
