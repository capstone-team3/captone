using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using CapTone.Core.Models;
using Windows.Storage;
using System.IO;

namespace CapTone.Repository
{
    public class outputs : DbContext
    {
        public async static void InitializeDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("outputs.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputs.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS outputTable" +
                    " (name VARCHAR(20), " +
                     "phone_number VARCHAR(20)," +
                     "home_address VARCHAR(1000)," +
                     "items VARCHAR(500),"+ "primary key(phone_number, items, name, home_address))";


                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }
        public static void AddData(outputData inputData)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputs.db");
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT OR IGNORE INTO outputTable (name, phone_number, home_address, items) VALUES (@namePara,@phone_numberPara,@home_addressPara,@itemsPara)";

                insertCommand.Parameters.AddWithValue("@namePara", inputData.Name);
                insertCommand.Parameters.AddWithValue("@phone_numberPara", inputData.PhoneNumber);
                insertCommand.Parameters.AddWithValue("@home_addressPara", inputData.HomeAddress);
                insertCommand.Parameters.AddWithValue("@itemsPara", inputData.Items);
                
                insertCommand.ExecuteNonQuery();



                db.Close();
            }

        }
        public static List<outputData> GetAllData()
        {
            List<outputData> entries = new List<outputData>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputs.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * from outputTable", db);

                SqliteDataReader query = selectCommand.ExecuteReader();
                
                while (query.Read())
                {
                    entries.Add(new outputData() { Name = query.GetString(0), PhoneNumber = query.GetString(1), Items = query.GetString(3), HomeAddress = query.GetString(2) });
                }

                db.Close();
            }

            return entries;
        }
        public static void checkData() {

        }
        public static void DeleteAllData()
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputs.db");
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand deleteCommand = new SqliteCommand("DELETE from outputTable",db);

                deleteCommand.ExecuteNonQuery();

                db.Close();
            }
        }


        public static void DeleteOneItem(outputData inputData)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "outputs.db");
            using (SqliteConnection db =
              new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();


                SqliteCommand deleteCommand = new SqliteCommand();
                deleteCommand.Connection = db;

                deleteCommand.CommandText = string.Format("DELETE from outputTable WHERE phone_number='{0}' AND items='{1}'", inputData.PhoneNumber,inputData.Items);

                deleteCommand.ExecuteNonQuery();

                db.Close();
            }
        }

    }
}
