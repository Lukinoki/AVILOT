using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AVILOT.Backend;
using AVILOT.Backend.Models;
using System.Linq;
using System.IO;
using System.Reflection;

namespace AVILOT
{
    public static class BackendService
    {
        public static Database db { get; private set; }
        public static Category selectedCategory { get; set; }

        public async static Task InitializeAsync(string dbPath)
        {
            if (!File.Exists(dbPath)) // check if this is the first run
            {
                AVILOT.App.Settings.DBInitialized = false;
            }

            Console.WriteLine("create db object");
            db = new Database(dbPath);
            Console.WriteLine("init db");
            await db.InitializeAsync();
            Console.WriteLine("db init");

            if (!AVILOT.App.Settings.DBInitialized)
            {
                try
                {
                    await loadDefaultDataset();
                    
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to load default dataset", e);
                }

                AVILOT.App.Settings.DBInitialized = true;
            }
            
        }
        private async static Task loadDefaultDataset()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "AVILOT.dataset_default.zip";
            var resource = assembly.GetManifestResourceStream(resourceName);
            Console.WriteLine("loading default dataset");
            await BackendService.db.updateDataset(resource, resourceName);
            Console.WriteLine("dataset updated");
        } 

        
    }
}
