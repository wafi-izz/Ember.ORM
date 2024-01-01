using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ember.DataAccessManager;

public enum DataBaseType
{
    SqlServer,
}
public class DataAccess
{
    public SqlConnection Connection { get; set; }
    public SqlCommand? Command { get; set; }
    public String? CommandString { get; set; }
    public SqlDataReader? ReaderData { get; set; }
    public DataTable DataTable { get; set; }
    //Transaction
    public DataTransaction Transaction { get; set; }
    public DataAccess(String ConncetionString)
    {
        Connection = new SqlConnection(ConncetionString);
        Connection.Open();
        DataTable = new DataTable();
        Transaction = new DataTransaction(Connection);
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


public class DataTransaction
{
    public SqlTransaction? Transaction { get; set; }
    public SqlConnection Connection { get; set; }
    public Boolean TransactionState { get; set; }
    public DataTransaction(SqlConnection Connection)
    {
        this.Connection = Connection;
        TransactionState = false;
    }
    public void Begin()
    {
        Transaction = Connection.BeginTransaction();
        TransactionState = true;
    }
    public void Commit()
    {
        if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
        Transaction?.Commit();
        Connection.Close();
        TransactionState = false;
    }
    public void RollBack()
    {
        if (!TransactionState) { throw new Exception("No Transaction Begin Was Found."); }
        if (Transaction?.Connection != null)
        {
            Transaction.Rollback();
            TransactionState = false;
        }
    }
}
