using System;
using System.IO;

namespace GaseraLib {
    internal static class IOClass {

        /// <summary>
        /// Indicates the action when the file exist.
        /// </summary>
        internal enum FileExistAction {

            /// <summary>
            /// No action required.
            /// </summary>
            None,

            /// <summary>
            /// Overwrite the existing file.
            /// </summary>
            Overwrite,

            /// <summary>
            /// Append the existing file.
            /// </summary>
            Append
        }

        /// <summary>
        /// Writes the file.
        /// </summary>
        /// <param name="f">The target file to be written.</param>
        /// <param name="text">A value to write on a file.</param>
        /// <param name="fca">The action if the target file exists.</param>
        /// <returns>Returns true if the operation succeed; otherwise, false.</returns>
        internal static bool WriteTextToFile(FileInfo f, string text, FileExistAction fca) {
            try {
                if (f == null) throw new ArgumentNullException(nameof(f));
                if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

                if (f.Exists) {
                    switch (fca) {
                        case FileExistAction.Overwrite:
                            f.Delete();
                            break;
                        case FileExistAction.Append:
                            break;
                        default:
                            throw new InvalidOperationException("A file was exist. Overwrite or append it.");
                    }
                }

                if (!f.Exists && fca== FileExistAction.Append)
                    f.Open(FileMode.Create).Close(); // Create a file, if append was specified but the file doesn't exist.

                using (StreamWriter sw = new StreamWriter(f.FullName, fca == FileExistAction.Append)) {
                    if (fca== FileExistAction.Append && f.Length > 0)
                        sw.Write("\n");
                    sw.Write(text);
                    sw.Flush();
                    Console.WriteLine("[i]: File '" + f.Name + "' has been written.");
                }

                return true;
            } catch (Exception ex) {
                Console.WriteLine("[x]: Couldn't write the file. (" + ex.Message + ")");
                return false;
            }
        }

        /// <summary>
        /// Retrieves the result of user's decision.
        /// </summary>
        /// <param name="fileName">A safe file name.</param>
        /// <param name="fca">Contains an enum value on what program will do to the existing file later.</param>
        /// <returns>
        /// Returns true if the user wants to overwrite or append the file, and the
        /// value of '<paramref name="fca"/>' will be set on what action will be; otherwise, false and the
        /// value of '<paramref name="fca"/>' will be <see cref="FileExistAction.None"/>.
        /// </returns>
        internal static bool FileExistChoice(string fileName, out FileExistAction fca) {
            fca = FileExistAction.None;
            ConsoleKey response;
            do {
                Console.WriteLine($"Do you want to overwrite or append '{fileName}'?");
                Console.Write("[O: Overwrite | A: Append | C: Cancel]: ");
                response = Console.ReadKey().Key;
                if (response != ConsoleKey.Enter)
                    Console.WriteLine();
            } while (response != ConsoleKey.O && response != ConsoleKey.A && response != ConsoleKey.C);

            switch (response) {
                case ConsoleKey.O:
                    fca = FileExistAction.Overwrite; break;
                case ConsoleKey.A:
                    fca = FileExistAction.Append; break;
                case ConsoleKey.C:
                    Console.WriteLine("[*]: User abort.");
                    break;
            }
            return response != ConsoleKey.C;
        }
    }
}
