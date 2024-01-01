using Ember.DataStructure;
using System.Configuration;

try
{
    Console.WriteLine("Program Started.");

    List<String> GeneratedQuery = new Init().GeneratedSchema;

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
