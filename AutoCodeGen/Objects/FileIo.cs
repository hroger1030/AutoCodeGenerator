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

namespace AutoCodeGenLibrary
{
    public static class FileIo
    {
        /// <summary>
        /// This method writes a string to disk. Overwrites any files with same name an path that already exist.
        /// </summary>
        public static void WriteToFile(string filePath, string outputData)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty");

            if (outputData == null)
                throw new ArgumentException("File path cannot be null");

            string directory_name = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directory_name))
                Directory.CreateDirectory(directory_name);

            if (File.Exists(filePath) && File.GetAttributes(filePath) != FileAttributes.Normal)
                File.SetAttributes(filePath, FileAttributes.Normal);

            File.WriteAllText(filePath, outputData);
        }



        /// <summary>
        /// Function copies a entire directory's content into a new directory, creating it if it does not exist.
        /// </summary>
        public static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            if (string.IsNullOrWhiteSpace(sourceDirectory))
                throw new ArgumentException("Source directory name cannot be null or empty");

            if (string.IsNullOrWhiteSpace(destinationDirectory))
                throw new ArgumentException("New directory name cannot be null or empty");

            if (!Directory.Exists(sourceDirectory))
                throw new ArgumentException($"No directory named '{sourceDirectory}' exists");

            if (Directory.Exists(destinationDirectory))
                throw new ArgumentException($"A directory named '{destinationDirectory}' already exists");

            if (!Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            // Recursively add subdirectories
            foreach (string subdirectory_name in Directory.GetDirectories(sourceDirectory))
            {
                string destination_path = subdirectory_name.Replace(sourceDirectory, destinationDirectory);
                CopyDirectory(subdirectory_name, destination_path);
            }

            // copy all files 
            foreach (string existing_file_path in Directory.GetFiles(sourceDirectory))
            {
                string new_file_name = existing_file_path.Replace(sourceDirectory, destinationDirectory);

                if (File.Exists(new_file_name) && File.GetAttributes(new_file_name) != FileAttributes.Normal)
                    File.SetAttributes(new_file_name, FileAttributes.Normal);

                File.Copy(existing_file_path, new_file_name, true);
            }
        }

        /// <summary>
        /// Deletes all files and subdirectories in a tree.
        /// </summary>
        public static void DeleteDirectoryTree(string directory)
        {
            if (!Directory.Exists(directory))
                throw new ArgumentException("Directory does not exist");

            foreach (string file_name in Directory.GetFiles(directory))
            {
                // get rid of 'read only' flags...
                if (File.GetAttributes(file_name) != FileAttributes.Normal)
                    File.SetAttributes(file_name, FileAttributes.Normal);

                File.Delete(file_name);
            }

            Directory.Delete(directory, true);
        }

        /// <summary>
        /// Determines if a string contains characters are invalid to be used as a path.
        /// </summary>
        public static bool ContainsInvalidPathCharacters(string input)
        {
            return input.IndexOfAny(Path.GetInvalidPathChars()) != -1;
        }

        /// <summary>
        /// Determines if a string contains characters that are invalid to be used as a file name.
        /// </summary>
        public static bool ContainsInvalidFileNameCharacters(string input)
        {
            return input.IndexOfAny(Path.GetInvalidFileNameChars()) != -1;
        }
    }
}
