using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Windows.Storage;
using System.IO;

namespace CapTone.Repository
{
    class userlist : DbContext
    {
        public async static void InitializeDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("userlist.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "userlist.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS userlistTable" +
                    " (username VARCHAR(20) PRIMARY KEY) ";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }
        public static void AddData(string inputData)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "userlist.db");
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT OR IGNORE INTO userlistTable (username) VALUES (@usernamePara)";

                insertCommand.Parameters.AddWithValue("@usernamePara", inputData);

                insertCommand.ExecuteNonQuery();

                db.Close();
            }

        }
        public static List<string> GetAllData()
        {
            List<string> entries = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "userlist.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * from userlistTable", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    entries.Add(query.GetString(0));

                }

                db.Close();
            }

            return entries;
        }
        public static void DeleteAllData()
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "userlist.db");
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand("DELETE from userlistTable", db);

                deleteCommand.ExecuteNonQuery();

                db.Close();
            }
        }




        public static void DeleteOneItem(string delItem)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "userlist.db");
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();


                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = db;

                deleteCommand.CommandText = string.Format("DELETE from userlistTable WHERE username='{0}'", delItem);

                deleteCommand.ExecuteNonQuery();

                db.Close();
            }
        }



    }
}
