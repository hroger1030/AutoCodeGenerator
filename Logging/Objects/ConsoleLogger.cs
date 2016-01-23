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

namespace Logging
{
    public class ConsoleLogger : ILogManager
    {
        #region Constants

            protected string LOG_ENTRY_FORMAT = "{0:yy/MM/dd hh:mm:ss} Game:{1} {2}:{3}" + Environment.NewLine;

        #endregion

        #region Fields

            protected static readonly object _LockObject = new object();

            protected Guid _GameId;
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

            public ConsoleLogger(Guid game_id, eLoggingLevel logging_level)
            {
                _GameId         = game_id;
                _LoggingLevel   = logging_level;
            }

            public void LogError(Exception ex)
            {
                lock (_LockObject)
                {
                    Console.ForegroundColor = SetColorForMessageType(eLoggingLevel.Error);
                    Console.WriteLine(string.Format(LOG_ENTRY_FORMAT, DateTime.UtcNow, _GameId, eLoggingLevel.Error, ex));
                    Console.ResetColor();
                }
            }

            public void LogMessage(string message)
            {
                LogMessage(message, eLoggingLevel.Info);
            }

            public void LogMessage(string message, eLoggingLevel message_type)
            {
                lock (_LockObject)
                {
                    Console.ForegroundColor = SetColorForMessageType(message_type);
                    Console.WriteLine(string.Format(LOG_ENTRY_FORMAT, DateTime.UtcNow, _GameId, message_type, message));
                    Console.ResetColor();
                }
            }

            protected ConsoleColor SetColorForMessageType(eLoggingLevel logging_level)
            {
                switch (logging_level)
                {
                    case eLoggingLevel.Error:           return ConsoleColor.Red;
                    case eLoggingLevel.Info:            return ConsoleColor.Green;
                    case eLoggingLevel.Metric:          return ConsoleColor.Cyan;                      
                    case eLoggingLevel.Warning:         return ConsoleColor.Yellow;

                    default:
                        throw new Exception(string.Format("Unknown logging level ({0}) encountered", logging_level));
                }
            }

        #endregion
    }
}