using AVILOT.AVQuestionsEngine.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AVILOT.QEngine2
{
    internal class Utils
    {
        private static SQLiteAsyncConnection _db;
        public static SQLiteAsyncConnection GetMainDatabase()
        {
            Debug.WriteLine("Getting main database started");
            if (_db != null) //check if the database has already been loaded
            {
                Debug.WriteLine("Database already exists");
                return _db;
            }
            // get path of possible databases
            var applicationFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var databasesesFolderPath = Path.Combine(applicationFolderPath, "database");
            Debug.WriteLine($"DB folder path:  {databasesesFolderPath}");
            //create databases folder if does not exist
            if (!Directory.Exists(databasesesFolderPath))
            {
                Debug.Print("creating databases folder because it does not exist");
                Directory.CreateDirectory(databasesesFolderPath);
            }

            var DbPath = Path.Combine(databasesesFolderPath, $"lusifly.db");
            if (File.Exists(DbPath))
            {
                Debug.Print("Database exists");
            }
            else
            {
                Debug.Print($"Creating new db");
            }

            var db = new SQLiteAsyncConnection(DbPath);
            if (db != null)
            {
                _db = db;
            }
            Debug.Print("returning database");
            return db;
        }


    }
}
