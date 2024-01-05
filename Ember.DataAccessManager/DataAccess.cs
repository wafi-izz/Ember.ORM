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
    public SqlConnection Connection { get; set; }
    public DataConnection DataConnection { get; set; }
    public SqlCommand? Command { get; set; }
    public String? CommandString { get; set; }
    public SqlDataReader? ReaderData { get; set; }
    public DataTable DataTable { get; set; }
    //Transaction
    public DataAccessTransaction Transaction { get; set; }
    public DataAccess(ConnectionStringManager ConncetionString)
    {
        Connection = new SqlConnection(ConncetionString.CS);
        DataConnection = new DataConnection(ConncetionString);
        Connection.Open();
        DataConnection.Open();
        DataTable = new DataTable();
        Transaction = new DataAccessTransaction(DataConnection);
    }
    public DataAccess CreateCommand(String Query)
    {
        Command = new SqlCommand(Query, Connection);
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
        TransactionState = Transaction!.TransactionState;
    }
    public void Begin()
    {
        Transaction = Transaction.Begin();
        TransactionState = true;
    }
    public void Commit()
    {
        if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
        Transaction?.Commit();
        Connection.Close();
        TransactionState = Transaction!.TransactionState;
    }
    public void RollBack()
    {
        if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
        if (Transaction?.DataConnection != null)
        {
            Transaction.Rollback();
            TransactionState = Transaction!.TransactionState;
        }
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
    }
    public void Close()
    {
        if (Connection == typeof(SqlConnection))
        {
            MSSQLConnection!.Close();
        }
    }
    public dynamic BeginTransaction()
    {
        if (Connection == typeof(SqlConnection))
        {
            return MSSQLConnection!.BeginTransaction();
        }
        return null!;
    }
}
public class DataTransaction
{
    public Boolean TransactionState { get; set; }
    public DataConnection DataConnection { get; set; }
    public Type? Transaction { get; set; }
    public System.Data.SqlClient.SqlTransaction? MSSQLTransaction { get; set; }
    public NpgsqlTransaction? PostgreSQLTransaction { get; set; }
    public MySqlTransaction? MySQLTransaction { get; set; }
    public SQLiteTransaction? SQLiteTransaction { get; set; }
    public DataTransaction(DataConnection DataConnection)
    {
        this.DataConnection = DataConnection;

        //if (DataConnection.Connection == typeof(SqlConnection)) MSSQLTransaction = DataConnection.BeginTransaction();
        //if (DataConnection.Connection == typeof(NpgsqlConnection)) PostgreSQLTransaction = DataConnection.BeginTransaction();
        //if (DataConnection.Connection == typeof(MySqlConnection)) MySQLTransaction = DataConnection.BeginTransaction();
        //if (DataConnection.Connection == typeof(SQLiteConnection)) SQLiteTransaction = DataConnection.BeginTransaction();

        if (DataConnection.Connection == typeof(SqlConnection)) Transaction = typeof(System.Data.SqlClient.SqlTransaction);
        if (DataConnection.Connection == typeof(NpgsqlConnection)) Transaction = typeof(NpgsqlTransaction);
        if (DataConnection.Connection == typeof(MySqlConnection)) Transaction = typeof(MySqlTransaction);
        if (DataConnection.Connection == typeof(SQLiteConnection)) Transaction = typeof(SQLiteTransaction);
    }
    public void Begin()
    {
        if(Transaction == typeof(SqlConnection))
        {
            TransactionState = true;
            MSSQLTransaction = DataConnection.BeginTransaction();
        }
    }
    public void Commit()
    {
        if(Transaction == typeof(SqlConnection))
        {
            MSSQLTransaction!.Commit();
            TransactionState = false;
        }
    }
    public void Rollback()
    {
        if(Transaction == typeof(SqlConnection))
        {
            MSSQLTransaction!.Rollback();
            TransactionState = false;
        }
    }
}
#endregion
