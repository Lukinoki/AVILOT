using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AVILOT.Backend;
using AVILOT.Backend.Models;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using SQLite_dev.backend;
using System.Runtime.ConstrainedExecution;
using AVILOT.backend;

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

        public async static Task InitializeAsync(string dbPath, string? defDatasetPath = null)
        {
            if (!File.Exists(dbPath)) // check if this is the first run
            {
                AVILOT.App.Settings.DBInitialized = false;
            }
            Console.WriteLine("initializing backend");
            Stopwatch sw = Stopwatch.StartNew();

            Console.WriteLine("create db object");
            db = new Database(dbPath);
            Console.WriteLine($"created db obj in {sw.ElapsedMilliseconds}");
            sw.Restart();
            await db.InitializeAsync();
            Console.WriteLine($"initialized db in {sw.ElapsedMilliseconds}");

            if (!AVILOT.App.Settings.DBInitialized)
            {
                try
                {
                    await loadDefaultDataset(defDatasetPath);

                }
                catch (Exception e)
                {
                    throw new Exception("Failed to load default dataset", e);
                }

                AVILOT.App.Settings.DBInitialized = true;
            }

            await initSelectedCategory();            

        }
        private async static Task loadDefaultDataset(string? defDatasetPath = null)
        {
            var sw = Stopwatch.StartNew();
            if (defDatasetPath != null)
            {
                Console.WriteLine("loading dataset from file");
                await BackendService.db.updateDataset(defDatasetPath);
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "AVILOT.dataset_default.zip";
                var resource = assembly.GetManifestResourceStream(resourceName);
                Console.WriteLine("loading default dataset from default resource");
                await BackendService.db.updateDataset(resource, resourceName);
                if (!(await db.getCategories()).Contains(selectedCategory))
                {
                    var cats = await db.getCategories();
                    if (cats.Count > 0)
                    {
                        selectedCategory = cats.First();
                    }
                }
            }
            Console.WriteLine($"updated dataset dataset in {sw.ElapsedMilliseconds}");
            
        } 

        public async static Task<bool> checkDatasetUpdate() {

            var response = await Sync.requestCheckDatasetUpdate(AVILOT.App.Settings.AppVersion, AVILOT.App.Settings.DatasetVersion);
            if (response == null)
            {
                Console.WriteLine("dataset up to date");
                return false;
            }
            Console.WriteLine("downloading new dataset");
            var datasetPath = await Sync.downloadDatasetFile(response.url, response.filename);
            Console.WriteLine("loading new dataset");
            await loadDefaultDataset(datasetPath);
            Sync.deleteDatasetFile(datasetPath);
            Console.WriteLine("done updating dataset");
            AVILOT.App.Settings.DatasetVersion = response.filename;
            return true;
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
