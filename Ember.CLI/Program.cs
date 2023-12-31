using Ember.DataAccessManager;
using Ember.DataStructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

try
{
    Console.WriteLine("Program Started.");

    String GeneratedQuery = Init.Second();
    Console.WriteLine("\n*************************************************************\n");
    Console.WriteLine(GeneratedQuery);
    Console.WriteLine("\n*************************************************************\n");

    string ConnectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
    DataAccess DA = new DataAccess(ConnectionString);
    DA.Transaction.Begin();
    DA.CreateCommand(GeneratedQuery).ExecuteQuery();
    DA.Transaction.Commit();

    Console.WriteLine("Program Ended.");
    //new Main();
}
catch (Exception e)
{
    Console.Error.WriteLine(e.Message);
}
