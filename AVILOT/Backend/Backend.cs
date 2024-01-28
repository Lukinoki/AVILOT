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

        private static Category _selectedCategory { get; set;}
        public static Category selectedCategory
        {
            get {
                return _selectedCategory; 
            }
            set
            {
                _selectedCategory = value;
                AVILOT.App.Settings.SelectedCategory = value.category_id;
            }
        }

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

            await initSelectedCategory();            

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

        private async static Task initSelectedCategory()
        {
            if (AVILOT.App.Settings.SelectedCategory != "")
            {
                var category = await db.getCategoryById(AVILOT.App.Settings.SelectedCategory);
                if (category == null)
                {
                    AVILOT.App.Settings.SelectedCategory = "";
                }
                else
                {
                    _selectedCategory = category;
                }
            }
            if (AVILOT.App.Settings.SelectedCategory == "")
            {
                var categories = await db.getCategories();
                var firstCategory = categories.FirstOrDefault();
                if (firstCategory == null)
                {
                    throw new Exception("No categories found");
                }
                else
                {
                    _selectedCategory = firstCategory;
                    AVILOT.App.Settings.SelectedCategory = firstCategory.category_id;
                }
            }
        }

        
    }
}
