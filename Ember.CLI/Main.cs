using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ember.DataAccessManager;

namespace Ember.CLI;

public class Main
{
    public Main()
    {
        string ConnectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
        DataAccess DA = new DataAccess(ConnectionString);
        try
        {
            DA.Transaction.Begin();

            //var Result1 = DA.CreateCommand(CreateDB.CommandString).ExecuteQuery();
            var Result1 = DA.CreateCommand("create table a1(id int)").ExecuteQuery();
            var Result2 = DA.CreateCommand("create table a2(id int)").ExecuteQuery();
            var Result3 = DA.CreateCommand("create table a3(id int)").ExecuteQuery();
            var Result33 = DA.CreateCommand("create table a3(id int)").ExecuteQuery();

            DA.Transaction.Commit();
            var t = 1;
        }
        catch (Exception e)
        {
            DA.Transaction.RollBack();
            throw;
        }
    }
}
