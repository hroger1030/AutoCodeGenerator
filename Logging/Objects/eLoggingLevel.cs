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
    [Flags]
    public enum eLoggingLevel
    {
        None = 0,

        /// <summary>
        /// A critical error is anything that will cause an interuption to the game.
        /// </summary>
        Error = 1,

        /// <summary>
        /// Any sort of error message flagging odd values or behavior.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Timers and counters.
        /// </summary>
        Metric = 4,

        /// <summary>
        /// Informational messages and events.
        /// </summary>
        Info = 8,

        /// <summary>
        /// all logging is turned on
        /// </summary>
        All = Error | Warning | Metric | Info,

        /// <summary>
        /// all error logging is turned on
        /// </summary>
        AllErrors = Error | Warning,
    } 
}
