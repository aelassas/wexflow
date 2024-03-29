using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SQLite;

namespace Wexflow.NetCore.Tests
{
    [TestClass]
    public class Sql
    {
        private static readonly string SqliteConnectionString = @"Data Source=C:\WexflowTesting\sqlite\HelloWorld.db;Version=3";

        [TestInitialize]
        public void TestInitialize()
        {
            InitDataTable();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            InitDataTable();
        }

        [TestMethod]
        public void SqlTest()
        {
            _ = Helper.StartWorkflow(158);

            // sqlite
            const string sql = "select Id, Description from Data;";
            using SQLiteConnection conn = new(SqliteConnectionString);
            SQLiteCommand comm = new(sql, conn);
            conn.Open();
            using var reader = comm.ExecuteReader();
            while (reader.Read())
            {
                var id = int.Parse(reader["Id"].ToString() ?? throw new InvalidOperationException());
                var desc = (string)reader["Description"];

                if (id == 1)
                {
                    Assert.AreEqual("Hello World Description 1! updated", desc);
                }
                else if (id == 2)
                {
                    Assert.AreEqual("Hello World Description 2! updated", desc);
                }
                else if (id == 3)
                {
                    Assert.AreEqual("Hello World Description 3! updated", desc);
                }
                else if (id == 4)
                {
                    Assert.AreEqual("Hello World Description 4! updated", desc);
                }
                else if (id == 5)
                {
                    Assert.AreEqual("Hello World Description 5!", desc);
                }
            }
        }

        private static void InitDataTable()
        {
            const string sql =
                  "UPDATE Data SET Description = 'Hello World Description 1!' WHERE Id = 1;"
                + "UPDATE Data SET Description = 'Hello World Description 2!' WHERE Id = 2;"
                + "UPDATE Data SET Description = 'Hello World Description 3!' WHERE Id = 3;"
                + "UPDATE Data SET Description = 'Hello World Description 4!' WHERE Id = 4;"
                + "UPDATE Data SET Description = 'Hello World Description 5!' WHERE Id = 5;";

            using SQLiteConnection conn = new(SqliteConnectionString);
            SQLiteCommand comm = new(sql, conn);
            conn.Open();
            _ = comm.ExecuteNonQuery();
        }
    }
}
