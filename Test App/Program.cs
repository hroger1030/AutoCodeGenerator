using System;
using System.Configuration;

namespace TestApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            string conn = ConfigurationManager.AppSettings["SQLConnection"];

            //SqlDatabase test_db = new SqlDatabase("Orion", conn);
            //SqlTable test_table = new SqlTable(test_db, "Drive");

            //StringBuilder sb = new StringBuilder();

            //try
            //{
            //    sb.AppendLine(CodeGenerator.GenerateInlineCountAllProc(test_table));
            //    sb.AppendLine(CodeGenerator.GenerateInlineCountSearchProc(test_table, sort_list));
            //    sb.AppendLine(CodeGenerator.GenerateInlineDeleteAllProc(test_table));
            //    sb.AppendLine(CodeGenerator.GenerateInlineDeleteManyProc(test_table));
            //    sb.AppendLine(CodeGenerator.GenerateInlineSelectAllPaginatedProc(test_table, sort_list, sort_list));
            //    sb.AppendLine(CodeGenerator.GenerateInlineSelectAllProc(test_table, sort_list));
            //    sb.AppendLine(CodeGenerator.GenerateInlineSelectManyByXProc(test_table, sort_list, sort_list));
            //    sb.AppendLine(CodeGenerator.GenerateInlineSelectManyProc(test_table, sort_list));
            //    sb.AppendLine(CodeGenerator.GenerateInlineSelectSingleProc(test_table));
            //    sb.AppendLine(CodeGenerator.GenerateInlineSetProc(test_table));
            //}
            //catch (Exception ex)
            //{
            //    sb.AppendLine(ex.ToString());
            //}

            //FileIo.WriteToFile("c:\\temp\\inline_test.sql", sb.ToString());

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            return;
        }
    }
}
