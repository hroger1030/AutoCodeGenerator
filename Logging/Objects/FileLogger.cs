/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;

namespace Logging
{
    public class FileLogger : ILogManager
    {
        #region Constants

            protected string LOG_ENTRY_FORMAT = "{0:yy/MM/dd hh:mm:ss} Game:{1} {2}: {3}" + Environment.NewLine;

        #endregion

        #region Fields

            protected Guid _GameId;
            protected string _FilePath;
            protected eLoggingLevel _LoggingLevel;

        #endregion

        #region Properties

            public eLoggingLevel LoggingLevel
            {
                get { return _LoggingLevel; }
                set { _LoggingLevel = value; }
            }

        #endregion

        #region Methods


            public FileLogger(Guid game_id, string file_path) : this(game_id, file_path, eLoggingLevel.AllErrors) { }

            public FileLogger(Guid game_id, string file_path, eLoggingLevel logging_level)
            {
                _GameId         = game_id;
                _FilePath       = file_path;
                _LoggingLevel   = logging_level;
            }

            public void LogError(Exception ex)
            {
                File.AppendAllText(_FilePath, string.Format(LOG_ENTRY_FORMAT, DateTime.UtcNow, _GameId, eLoggingLevel.Error, ex));
            }

            public void LogMessage(string message)
            {
                LogMessage(message, eLoggingLevel.Info);
            }

            public void LogMessage(string message, eLoggingLevel message_type)
            {
                File.AppendAllText(_FilePath, string.Format(LOG_ENTRY_FORMAT, DateTime.UtcNow, _GameId, message_type, message));
            }

            //public string FormatLogEntry(string message)
            //{
            //    return string.Format("{0:yyyy/MM/dd HH:mm:ss}: {1}{2}", DateTime.Now, message, Environment.NewLine);
            //} 

            //public string GenerateLogfileName(string application_name, string file_type_extension)
            //{
            //    // generates filename:
            //    // MyApplication_log(2012-12-31 14:34:33).txt

            //    return string.Format("{0}_log ({1:yyyy-MM-dd HH.mm.ss}).{2}", application_name, DateTime.Now, file_type_extension);
            //} 

        #endregion
    }
}