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
using System.Diagnostics;
using System.Windows.Forms;

namespace Logging
{
    public class ApplicationLogger : ILogManager
    {
        #region Fields

            protected string _ApplicationName;
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

            public ApplicationLogger(eLoggingLevel logging_level) : this(Application.ProductName, logging_level) { }

            public ApplicationLogger(string application_name, eLoggingLevel logging_level)
            {
                _ApplicationName    = application_name;
                _LoggingLevel       = logging_level;
            }

            public void LogError(Exception ex)
            {
                WriteToApplicationEventLog(ex.ToString(), eLoggingLevel.Error);
            }

            public void LogMessage(string message)
            {
                WriteToApplicationEventLog(message, eLoggingLevel.Info);
            }

            public void LogMessage(string message, eLoggingLevel logging_level)
            {
                WriteToApplicationEventLog(message, logging_level);
            }

            protected void WriteToApplicationEventLog(string message, eLoggingLevel logging_level)
            {
                if (!EventLog.SourceExists(_ApplicationName))
                    EventLog.CreateEventSource(_ApplicationName, "Application");

                EventLog.WriteEntry(_ApplicationName, message, MapLoggingLevelEnum(logging_level));
            }

            protected EventLogEntryType MapLoggingLevelEnum(eLoggingLevel logging_level)
            {
                switch (logging_level)
                {
                    case eLoggingLevel.Error:
                        return EventLogEntryType.Error;

                    case eLoggingLevel.Warning:
                        return EventLogEntryType.Warning;

                    default: 
                        return EventLogEntryType.Information;
                }
            }

        #endregion
     }
}