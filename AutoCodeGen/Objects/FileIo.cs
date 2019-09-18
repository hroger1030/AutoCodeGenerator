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
using System.Collections.Generic;
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

            FileIo.WriteToFile(filePath, outputData);
        }

        /// <summary>
        /// This method writes a collection of strings to disk. Overwrites any files with same name an path that already exist.
        /// </summary>
        public static void WriteToFile(string filePath, List<string> outputData)
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

            File.WriteAllLines(filePath, outputData);
        }

        /// <summary>
        /// This method writes a byte array to disk. Overwrites any files with same name an path that already exist.
        /// </summary>
        public static void WriteToFile(string filePath, byte[] outputData)
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

            File.WriteAllBytes(filePath, outputData);
        }

        /// <summary>
        /// Method to move files that overwrites any existing files.
        /// </summary>
        public static void MoveFile(string filePath, string fileDestination)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty");

            if (!File.Exists(filePath))
                throw new ArgumentException("File does not exist");

            if (string.IsNullOrWhiteSpace(fileDestination))
                throw new ArgumentException("Destination path cannot be null or empty");

            string directory_name = Path.GetDirectoryName(fileDestination);

            if (!Directory.Exists(directory_name))
                Directory.CreateDirectory(directory_name);

            if (File.Exists(fileDestination))
            {
                if (File.GetAttributes(fileDestination) != FileAttributes.Normal)
                    File.SetAttributes(fileDestination, FileAttributes.Normal);

                File.Delete(fileDestination);
            }

            Directory.Move(filePath, fileDestination);
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        public static void DeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new ArgumentException("File does not exist");

            if (File.GetAttributes(filePath) != FileAttributes.Normal)
                File.SetAttributes(filePath, FileAttributes.Normal);

            File.Delete(filePath);
        }

        /// <summary>
        /// Mimics renaming a file in the file system. 
        /// </summary>
        /// <param name="oldFilePath">source file name</param>
        /// <param name="newFilePath">new file name</param>
        public static void RenameFile(string oldFilePath, string newFilePath)
        {
            if (string.IsNullOrWhiteSpace(oldFilePath))
                throw new ArgumentException("Old path cannot be null or empty");

            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentException("New path cannot be null or empty");

            // changes that are only filename case related need special treatment
            if (oldFilePath.ToLower() == newFilePath.ToLower())
            {
                string temp_name = GenerateTemporaryFilename(oldFilePath);

                File.Move(oldFilePath, temp_name);
                File.Move(temp_name, newFilePath);
            }
            else
            {
                File.Move(oldFilePath, newFilePath);
            }
        }

        /// <summary>
        /// Renames a directory the file system. 
        /// </summary>
        public static void RenameDirectory(string oldDirectoryPath, string newDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(oldDirectoryPath))
                throw new ArgumentException("Old directory name cannot be null or empty");

            if (string.IsNullOrWhiteSpace(newDirectoryPath))
                throw new ArgumentException("New directory name cannot be null or empty");

            if (!Directory.Exists(oldDirectoryPath))
                throw new ArgumentException($"No directory named '{oldDirectoryPath}' exists");

            if (Directory.Exists(newDirectoryPath))
                throw new ArgumentException($"A directory named '{newDirectoryPath}' already exists");

            if (oldDirectoryPath.ToLower() == newDirectoryPath.ToLower())
            {
                string temp_name = GenerateTemporaryDirectoryname(oldDirectoryPath);

                Directory.Move(oldDirectoryPath, temp_name);
                Directory.Move(temp_name, newDirectoryPath);
            }
            else
            {
                Directory.Move(oldDirectoryPath, newDirectoryPath);
            }
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

        /// <summary>
        /// Determines if a full path contains characters that are invalid in either the path or
        /// file name.
        /// </summary>
        public static bool ContainsInvalidCharacters(string input)
        {
            string file_path = Path.GetDirectoryName(input);
            string file_name = Path.GetFileName(input);

            if (ContainsInvalidPathCharacters(file_path))
                return true;

            if (ContainsInvalidFileNameCharacters(file_name))
                return true;

            return false;
        }

        /// <summary>
        /// Generates a random unique name for a file in a given directory.
        /// </summary>
        private static string GenerateTemporaryFilename(string filePath)
        {
            string temp_name = string.Empty;

            while (File.Exists(temp_name))
                temp_name = Path.Combine(filePath, Path.GetRandomFileName());

            return temp_name;
        }

        /// <summary>
        /// Generates a random unique name for a given directory.
        /// </summary>
        private static string GenerateTemporaryDirectoryname(string path)
        {
            string temp_name = string.Empty;

            while (File.Exists(temp_name))
                temp_name = path + Guid.NewGuid().ToString("N");

            return temp_name;
        }
    }
}
