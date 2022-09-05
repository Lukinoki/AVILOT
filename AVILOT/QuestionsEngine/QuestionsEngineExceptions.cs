using System;
using System.Collections.Generic;
using System.Text;

namespace AVILOT.AVQuestionsEngine
{
    public class QuestionEngineException : Exception
    {
        internal QuestionEngineException(string msg) : base(msg) { }
        internal QuestionEngineException() { }

        internal QuestionEngineException(string msg, Exception innerException) : base(msg, innerException) { }

    }
    public class QuestionEngineNotInitializedException : QuestionEngineException
    {
            
        internal QuestionEngineNotInitializedException() : base("Question engine has not been initialized yet.Please call QuestionEngine.Initialize() before using")
        {
            //wow such empty
        }
    }

    #region CsvExceptions
    public class QuestionsEngineCsvException : QuestionEngineException
    {
        internal QuestionsEngineCsvException(string msg) : base(msg) { }
        internal QuestionsEngineCsvException() { }
        internal QuestionsEngineCsvException(string msg, Exception innerException) : base(msg, innerException) { }
    }
    public class UnknownTransferVersionException : QuestionEngineException
    {
        readonly string Version;
        internal UnknownTransferVersionException(string version) : base($"Unknown avilottransfer file version {version}.")
        {
            Version = version;
        }
    }
    public class UnreadableFileException : QuestionEngineException
    {
        public readonly string Filename;
        internal UnreadableFileException(string filename) : base ($"Cannot read file: {filename}")
        {
            Filename = filename;
        }
        internal UnreadableFileException() : base($"Cannot read file")
        {
            Filename = "";
        }
    }
    public class UnreadableV1FileException : UnreadableFileException
    {
        internal UnreadableV1FileException(string filename) : base(filename) { }
        internal UnreadableV1FileException() : base() { }
    }
    public class UnreadableV2FileException : UnreadableFileException
    {
        internal UnreadableV2FileException(string filename) : base(filename) { }
        internal UnreadableV2FileException() : base() { }
    }
    #endregion
}
