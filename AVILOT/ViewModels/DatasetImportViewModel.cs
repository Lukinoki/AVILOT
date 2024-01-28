using AVILOT.Models;
using AVILOT.Services;
using AVILOT.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.IO;
using System.Threading.Tasks;

namespace AVILOT.ViewModels
{
    public class DatasetImportViewModel : BaseViewModel
    {
        public Command ImportNewCommand;
        public Command PrintCollection = new Command(async () =>
        {
            var categories = await BackendService.db.getCategories();
            foreach (var category in categories)
            {
                Console.WriteLine($"Name: {category.headline} Id: {category.category_id}");
            };
        });
        public Command DeleteDatabaseCommand { get; }

        public Command AnswerQuestionCommand { get; }

        public Command PrintAllCollections { get; }

        public Command PrintPages { get; }
        public string ImportStatus { get; set; }
        public DatasetImportViewModel()
        {

            ImportNewCommand = new Command(async () =>
            {
                await ImportNewCollection();
            });


        }

        public static async Task ImportNewCollection()
        {
            var pickResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Pick New Dataset"
            });
            
            if (pickResult != null)
            {
                Console.WriteLine($"Loading {pickResult.FileName}");
                try
                {
                    await BackendService.db.updateDataset(pickResult.FullPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                Console.WriteLine("Loaded");
            }
            else
            {
                Console.WriteLine("Canceled");
            }

        }

    }
}
