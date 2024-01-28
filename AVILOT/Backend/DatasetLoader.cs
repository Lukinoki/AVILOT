using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using SQLite;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Threading.Tasks;
using AVILOT.Backend.Models;

namespace AVILOT.Backend
{
    public class InvalidFileExeption : Exception
    {
        public InvalidFileExeption(string reason) : base(reason) { }
        public InvalidFileExeption(string reason, Exception innerException) : base(reason, innerException) { }

    }

    internal class DatasetLoader
    {
        readonly private SQLiteAsyncConnection conn;
        readonly private CsvConfiguration csvConfig;

        public DatasetLoader(SQLiteAsyncConnection conn)
        {
            this.conn = conn;
            csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Encoding = Encoding.UTF8,
                Delimiter = ","
            };
        }

        public async Task updateDataset(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"File not found", filepath);
            }
            if (Path.GetExtension(filepath) != ".zip")
            {
                throw new InvalidFileExeption(filepath);
            }
            using (var file = File.OpenRead(filepath))
            {
                await updateDataset(file, Path.GetFileName(filepath));
            }
        }

        public async Task updateDataset(Stream stream, string filename)
        {
            
            if (Path.GetExtension(filename) != ".zip")
            {
                throw new InvalidFileExeption(filename);
            }
            using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                string[] requiredFiles =
                {
                    "Question.csv",
                    "Category.csv",
                    "TestTemplate.csv",
                    "Answer.csv",
                    "Template_Question.csv"
                };

                foreach (var requiredFile in requiredFiles)
                {
                    if (zip.GetEntry(requiredFile) == null)
                    {
                        throw new InvalidFileExeption($"'{requiredFile}' is missing from the archive");
                    }
                }

                await conn.RunInTransactionAsync((SQLiteConnection sConn) =>
                {
                    sConn.DeleteAll<Question>();
                    sConn.DeleteAll<Category>();
                    sConn.DeleteAll<TestTemplate>();
                    sConn.DeleteAll<Answer>();
                    sConn.DeleteAll<Template_Question>();

                    loadTableCSV<Question>(zip.GetEntry("Question.csv").Open(), sConn);
                    loadTableCSV<Category>(zip.GetEntry("Category.csv").Open(), sConn);
                    loadTableCSV<TestTemplate>(zip.GetEntry("TestTemplate.csv").Open(), sConn);
                    loadTableCSV<Answer>(zip.GetEntry("Answer.csv").Open(), sConn);
                    loadTableCSV<Template_Question>(zip.GetEntry("Template_Question.csv").Open(), sConn);
                });

            }
        }

        void loadTableCSV<T>(Stream stream, SQLiteConnection sCon, string? filename = null) where T : class
        {
            StreamReader reader = new StreamReader(stream);
            try
            {
                using var csv = new CsvReader(reader, csvConfig);
                IEnumerable<T> recordsReader = csv.GetRecords<T>();
                T[] records = recordsReader.ToArray();
                sCon.InsertAll(records);
            }
            catch (Exception ex)
            {
                if (filename != null)
                {
                    throw new InvalidFileExeption($"File '{filename}' in archive is invalid", ex);
                }
                else
                {
                    throw new InvalidFileExeption($"File in archive loading into table '{typeof(T).Name}' is invalid", ex);
                }
            }

        }
    }
}
