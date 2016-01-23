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

namespace DAL
{
    public static partial class FileIo
    {
        #region Methods

            /// <summary>
            /// This method writes a string to disk. Overwrites any files with same name an path that already exist.
            /// </summary>
            /// <param name="file_path">full path of output file</param>
            /// <param name="output_data">data to be written to the output file</param>
            /// <returns>bool flag indicating sucess or failure of operation.</returns>
            public static void WriteToFile(string file_path, string output_data)
            {
                if (string.IsNullOrWhiteSpace(file_path))
                    throw new ArgumentException("File path cannot be null or empty");

                if (output_data == null)
                    throw new ArgumentException("File path cannot be null");

                string directory_name = Path.GetDirectoryName(file_path);

                if (!Directory.Exists(directory_name))
                    Directory.CreateDirectory(directory_name);
                    
                // Are attributes set properly?
                if (File.Exists(file_path) && File.GetAttributes(file_path) != FileAttributes.Normal)
                    File.SetAttributes(file_path, FileAttributes.Normal);

                File.WriteAllText(file_path, output_data);
            }

            /// <summary>
            /// This method writes a collection of strings to disk. Overwrites any files with same name an path that already exist.
            /// </summary>
            /// <param name="file_path">full path of output file</param>
            public static void WriteToFile(string file_path, List<string> output_data)
            {
                if (string.IsNullOrWhiteSpace(file_path))
                    throw new ArgumentException("File path cannot be null or empty");

                if (output_data == null)
                    throw new ArgumentException("File path cannot be null");

                string directory_name = Path.GetDirectoryName(file_path);

                if (!Directory.Exists(directory_name))
                    Directory.CreateDirectory(directory_name);
                    
                if (File.Exists(file_path) && File.GetAttributes(file_path) != FileAttributes.Normal)
                    File.SetAttributes(file_path, FileAttributes.Normal);

                File.WriteAllLines(file_path, output_data);
            }

            /// <summary>
            /// This method writes a byte array to disk. Overwrites any files with same name an path that already exist.
            /// </summary>
            /// <param name="file_path">full path of output file</param>
            public static void WriteToFile(string file_path, byte[] output_data)
            {
                if (string.IsNullOrWhiteSpace(file_path))
                    throw new ArgumentException("File path cannot be null or empty");

                if (output_data == null)
                    throw new ArgumentException("File path cannot be null");

                string directory_name = Path.GetDirectoryName(file_path);

                if (!Directory.Exists(directory_name))
                    Directory.CreateDirectory(directory_name);
                    
                if (File.Exists(file_path) && File.GetAttributes(file_path) != FileAttributes.Normal)
                    File.SetAttributes(file_path, FileAttributes.Normal);

                File.WriteAllBytes(file_path, output_data);
            }

            /// <summary>
            /// Method to move files that overwrites any existing files.
            /// </summary>
            /// <param name="file_path">full file name an path of file to be moved</param>
            /// <param name="file_destination">full name and path of destination.</param>
            public static void MoveFile(string file_path, string file_destination)
            {
                if (string.IsNullOrWhiteSpace(file_path))
                    throw new ArgumentException("File path cannot be null or empty");

                if (!File.Exists(file_path))
                    throw new ArgumentException("File does not exist");

                if (string.IsNullOrWhiteSpace(file_destination))
                    throw new ArgumentException("Destination path cannot be null or empty");

                string directory_name = Path.GetDirectoryName(file_destination);

                if (!Directory.Exists(directory_name))
                    Directory.CreateDirectory(directory_name);

                if (File.Exists(file_destination))
                {
                    if (File.GetAttributes(file_destination) != FileAttributes.Normal)
                        File.SetAttributes(file_destination, FileAttributes.Normal);

                    File.Delete(file_destination);
                }

                Directory.Move(file_path, file_destination);
            }

            /// <summary>
            /// Deletes a file.
            /// </summary>
            /// <param name="file_path">full path of file to delete</param>
            public static void DeleteFile(string file_path)
            {
                if (!File.Exists(file_path))
                    throw new ArgumentException("File does not exist");

                if (File.GetAttributes(file_path) != FileAttributes.Normal)
                    File.SetAttributes(file_path, FileAttributes.Normal);

                File.Delete(file_path);
            }

            /// <summary>
            /// Mimics renaming a file in the file system. 
            /// </summary>
            /// <param name="old_file_path">source file name</param>
            /// <param name="new_file_path">new file name</param>
            public static void RenameFile(string old_file_path, string new_file_path)
            {
                if (string.IsNullOrWhiteSpace(old_file_path))
                    throw new ArgumentException("Old path cannot be null or empty");

                if (string.IsNullOrWhiteSpace(new_file_path))
                    throw new ArgumentException("New path cannot be null or empty");

                // changes that are only filename case related need special treatment
                if (old_file_path.ToLower() == new_file_path.ToLower())
                {
                    string temp_name = GenerateTemporaryFilename(old_file_path);

                    File.Move(old_file_path, temp_name);
                    File.Move(temp_name, new_file_path);
                }
                else
                {
                    File.Move(old_file_path, new_file_path);
                }
            }            

            /// <summary>
            /// Renames a directory the file system. 
            /// </summary>
            /// <param name="old_directory_path">old directory name</param>
            /// <param name="new_directory_path">new directory name</param>
            public static void RenameDirectory(string old_directory_path, string new_directory_path)
            {
                if (string.IsNullOrWhiteSpace(old_directory_path))
                    throw new ArgumentException("Old directory name cannot be null or empty");

                if (string.IsNullOrWhiteSpace(new_directory_path))
                    throw new ArgumentException("New directory name cannot be null or empty");

                if (!Directory.Exists(old_directory_path))
                    throw new ArgumentException(string.Format("No directory named '{0}' exists", old_directory_path));

                if (Directory.Exists(new_directory_path))
                    throw new ArgumentException(string.Format("A directory named '{0}' already exists", new_directory_path));

                if (old_directory_path.ToLower() == new_directory_path.ToLower())
                {
                    string temp_name = GenerateTemporaryDirectoryname(old_directory_path);

                    Directory.Move(old_directory_path, temp_name);
                    Directory.Move(temp_name, new_directory_path);
                }
                else
                {
                    Directory.Move(old_directory_path, new_directory_path);
                }
            }    
        
            /// <summary>
            /// Function copies a entire directory's content into a new directory, creating it if it does not exist.
            /// </summary>
            /// <param name="source_directory">full path of source directory</param>
            /// <param name="destination_directory">full path of destination directory</param>
            public static void CopyDirectory(string source_directory, string destination_directory)
            {
                if (string.IsNullOrWhiteSpace(source_directory))
                    throw new ArgumentException("Source directory name cannot be null or empty");

                if (string.IsNullOrWhiteSpace(destination_directory))
                    throw new ArgumentException("New directory name cannot be null or empty");

                if (!Directory.Exists(source_directory))
                    throw new ArgumentException(string.Format("No directory named '{0}' exists", source_directory));

                if (Directory.Exists(destination_directory))
                    throw new ArgumentException(string.Format("A directory named '{0}' already exists", destination_directory));

                if (!Directory.Exists(destination_directory))
                    Directory.CreateDirectory(destination_directory);

                // Recursively add subdirectories
                foreach (string subdirectory_name in Directory.GetDirectories(source_directory))
                {
                    string destination_path = subdirectory_name.Replace(source_directory, destination_directory);
                    CopyDirectory(subdirectory_name, destination_path);
                }

                // add all files 
                foreach (string existing_file_path in Directory.GetFiles(source_directory))
                {
                    string new_file_name = existing_file_path.Replace(source_directory, destination_directory);

                    // Are attributes set properly?
                    if (File.Exists(new_file_name) && File.GetAttributes(new_file_name) != FileAttributes.Normal)
                        File.SetAttributes(new_file_name, FileAttributes.Normal);

                    File.Copy(existing_file_path, new_file_name, true);
                }
            }

            /// <summary>
            /// Deletes all files and subdirectories in a tree.
            /// </summary>
            /// <param name="directory">full path of directory to delete</param>
            public static void DeleteDirectoryTree(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new ArgumentException("Directory does not exist");

                // Nuke any files in the directory
                foreach (string file_name in Directory.GetFiles(directory))
                {
                    // get rid of pesky 'read only' flags...
                    if (File.GetAttributes(file_name) != FileAttributes.Normal)
                        File.SetAttributes(file_name, FileAttributes.Normal); 

                    File.Delete(file_name);
                }

                Directory.Delete(directory, true);
            }
        
            /// <summary>
            /// Retrieves a list of all the files in a directory and all its children.
            /// </summary>
            /// <param name="directory">full path of directory to search</param>
            /// <returns>string list of full path of all files under directory branch</returns>
            public static List<string> GetDirectoryFileList(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new ArgumentException("Directory does not exist");

                var file_list = new List<string>();

                foreach (var file_name in Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories))
                    file_list.Add(file_name);

                return file_list;
            }

            /// <summary>
            /// Retrieves a list of all the subdirectorys in a directory.
            /// </summary>
            /// <param name="directory">full path of directory to search</param>
            /// <returns>string list of full path of all directories under directory branch</returns>
            public static List<string> GetSubdirectoryList(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new ArgumentException("Directory does not exist");

                var file_list = new List<string>();
            
                foreach (string file_name in Directory.GetDirectories(directory,"*.*", SearchOption.AllDirectories))
                    file_list.Add(file_name);

                return file_list;
            }

            /// <summary>
            /// Determines if a string contains characters are invalid to be used as a path.
            /// </summary>
            /// <param name="input">The path to be checked</param>
            /// <returns>bool flag indicating if the string contains invalid characters</returns>
            private static bool ContainsInvalidPathCharacters(string input)
            {
                return input.IndexOfAny(Path.GetInvalidPathChars()) != -1;
            }

            /// <summary>
            /// Determines if a string contains characters that are invalid to be used as a file name.
            /// </summary>
            /// <param name="input">The file name to be checked</param>
            /// <returns>bool flag indicating if the string contains invalid characters</returns>
            private static bool ContainsInvalidFileNameCharacters(string input)
            {
                return input.IndexOfAny(Path.GetInvalidFileNameChars()) != -1;
            }

            /// <summary>
            /// Determines if a full path contains characters that are invalid in either the path or
            /// file name.
            /// </summary>
            /// <param name="input">The file name and path to be cleaned</param>
            /// <returns>bool flag indicating if the string contains invalid characters</returns>
            private static bool ContainsInvalidCharacters(string input)
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
            /// <param name="file_path">directory that file name is to be used in.</param>
            /// <returns>A random unique name</returns>
            private static string GenerateTemporaryFilename(string file_path)
            {
                if (file_path[file_path.Length-1] != Path.PathSeparator)
                    file_path += Path.PathSeparator;

                string temp_name = string.Empty;

                while (File.Exists(temp_name) || string.IsNullOrWhiteSpace(temp_name))
                    temp_name = file_path + Path.GetRandomFileName();

                return temp_name;
            }

            /// <summary>
            /// Generates a random unique name for a given directory.
            /// </summary>
            /// <param name="path">Current directory path.</param>
            /// <returns></returns>
            private static string GenerateTemporaryDirectoryname(string path)
            {
                string temp_name = string.Empty;

                while (File.Exists(temp_name) || string.IsNullOrWhiteSpace(temp_name))
                    temp_name = path + "Temp";                

                return temp_name;
            }

        #endregion
    }
}
