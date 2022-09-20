using AVILOT.Models;
using AVILOT.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.IO;
using AVILOT.AVQuestionsEngine;
using System.Threading.Tasks;

namespace AVILOT.ViewModels
{
    public class DatasetImportViewModel : BaseViewModel
    {
        
        public Command ImportNewCommand { get; }
        public Command PrintCollection { get; }
        public Command DeleteDatabaseCommand { get; }

        public Command AnswerQuestionCommand { get; }
        
        public Command PrintAllCollections { get; }

        public Command PrintPages { get; }
        public string ImportStatus { get; set; }

        public Command OpenFirstQuestion { get; }
        public DatasetImportViewModel()
        {
            ImportNewCommand = new Command(ImportNewCollection);
            DeleteDatabaseCommand = new Command(async () =>
            {
                await QuestionsEngine.GlobalQuestionsDatabase.ClearDatabase();
            });
            PrintCollection = new Command(async () =>
            {
                Console.WriteLine("STARTING PRINT");
                //var collections = await QuestionsEngine.QuestionsEngine.getAllColections();
                var collection = QuestionsEngine.allQuestionsCollection;
                var allQuestions = await collection.GetAllQuestions();
                foreach (var question in allQuestions)
                {
                    Console.WriteLine($"{question.Text} Id: {question.Id}");
                    Console.WriteLine($"correct: {question.AnsweredCorrectCount} wrong: {question.AnsweredWrongCount} lastcorrect: {question.LastCorrectAnswer} lastwrong: {question.LastWrongAnswer}");
                    foreach (var answer in question.Answers)
                    {
                        Console.WriteLine($"ANS: {answer.Text}");
                    }
                }
            });
            PrintAllCollections = new Command(async () =>
            {
                var collections = await QuestionsEngine.GetAllColections();
                foreach (var collection in collections)
                {
                    Console.WriteLine($"Name: {collection.Name} Id: {collection.Id}");
                }

            });
            AnswerQuestionCommand = new Command(async () =>
            {
                var collections = await QuestionsEngine.GetAllColections();
                var collection = collections[0];
                for (int i = 0; i < 10; i++)
                {
                    var question = await collection.GetMostValuableQuestionAsync();
                    Console.WriteLine(question.Text);
                    await question.Select(question.CorrectAnswerIndex);
                }
                Console.WriteLine(await collection.GetAnsweredCorrectCount());
                Console.WriteLine(await collection.GetAnsweredCorrectPercentage());
                
            });
            PrintPages = new Command(async () =>
            {
                Console.WriteLine("STARTING PRINT");
                //var collections = await QuestionsEngine.QuestionsEngine.getAllColections();
                var collection = QuestionsEngine.allQuestionsCollection;
                var allQuestions = collection.IterateOverQuestionPagesAsync(10);
                await foreach (var page in allQuestions)
                {
                    Console.WriteLine("new page");
                    foreach(var question in page)
                    {
                        Console.WriteLine($"{question.Text} Id: {question.Id}");
                        Console.WriteLine($"correct: {question.AnsweredCorrectCount} wrong: {question.AnsweredWrongCount} lastcorrect: {question.LastCorrectAnswer} lastwrong: {question.LastWrongAnswer}");
                        foreach (var answer in question.Answers)
                        {
                            Console.WriteLine($"ANS: {answer.Text}");
                        }
                    }
                }
            });

            ImportStatus = "Idle";
        }

        private async void ImportNewCollection(object obj)
        {
            var pickResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Pick New Dataset"
            });
            Console.WriteLine("Canceled");
            if (pickResult != null)
            {
                Console.WriteLine("Reading");
                var stream = await pickResult.OpenReadAsync();
                if (stream != null)
                {
                    Console.WriteLine($"Loading {pickResult.FileName}");
                    var streamreader = new StreamReader(stream);
                    await QuestionsEngine.ImportQuestionsFromCsv(streamreader, pickResult.FileName);
                    Console.WriteLine("Loaded");
                }
                
            }
            
        }

        private Command testJoins;
        public ICommand TestJoins => testJoins ??= new Command(PerformTestJoins);

        private async void PerformTestJoins()
        {
            var collections = await QuestionsEngine.GetAllColections();
            var join = new JoinedQuestionsCollection(collections);
            var questions = await join.GetAllQuestions();
            Console.WriteLine(questions.Length);
        }
    }
}
