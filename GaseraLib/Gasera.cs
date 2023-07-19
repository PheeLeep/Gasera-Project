using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static GaseraLib.WinNatives;

namespace GaseraLib {

    public static class Gasera {

        private static string prevTitle = "";

        /// <summary>
        /// Prints the welcome page of the program, and the corresponding .NET version.
        /// </summary>
        /// <param name="netType">The type of the .NET variant that a program was used to compile.</param>
        /// <param name="netVersion">The .NET version.</param>
        public static void PrintWelcome(string netType, string netVersion) {
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("Gasera | Shadow Defender Password Hash Acquirer.");
            Console.WriteLine("Running in " + netType + " " + netVersion);
            Console.WriteLine("\nAuthor: PheeLeep");
            Console.WriteLine("GitHub: https://github.com/PheeLeep/Gasera-Project");
            Console.WriteLine(new string('-', 50));
            prevTitle = Console.Title;
            Console.Title = "Gasera | Shadow Defender Password Hash Acquirer for " + netType;
        }

        /// <summary>
        /// Executes a program.
        /// </summary>
        /// <param name="args">The arguments that a program provided.</param>
        public static void Execute(string[] args) {
            try {

                // Shadow Defender is for Windows-only, so we should target Windows.
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    throw new PlatformNotSupportedException("[!]: This program is for Windows only.");

                FileInfo filePath = null;

                // Loop out for the arguments.
                if (args.Length > 0) {
                    switch (args[0]) {
                        case "/out":
                            if (args.Length < 2)
                                throw new InvalidOperationException("[!]: No output file specified.");
                            filePath = new FileInfo(args[1]);
                            break;
                        case "/h":
                            ShowHelp();
                            return;
                        default:
                            throw new InvalidOperationException("[!]: No parameter exist to type '" + args[0] + "'.");
                    }
                }

                // Shadow Defender stores unique GUID and the path, to the registry.
                Console.WriteLine("[*]: Harvesting required data from Shadow Defender's registry...");
                if (!GetValueFromRegistry(@"SYSTEM\CurrentControlSet\Services\diskpt", "GUID", out string guid) ||
                    !GetValueFromRegistry(@"SOFTWARE\Shadow Defender", "Path", out string path))
                    return;

                if (!Guid.TryParse(guid, out _))
                    throw new InvalidOperationException("[x]: Malformed GUID parse found.");
                Console.WriteLine("[i]: Shadow Defender GUID found!");

                if (!Directory.Exists(path) || !File.Exists(path + "\\user.dat"))
                    throw new DirectoryNotFoundException("[x]: Specified path not found. Shadow Defender wasn't installed to this machine.");

                // Acquire the hash from the specified path, then output the result.
                if (!GetDefenderMD5Hash(path + "\\user.dat", out string hash)) return;

                Console.WriteLine("[i]: Necessary data has been acquired.");

                string outputStr = "$shadow_defender$*" + guid + "*" + hash;
                Console.WriteLine("\nResult:\n" + new string('-', 20));
                Console.WriteLine("Shadow Defender GUID: " + guid);
                Console.WriteLine("MD5 UTF-16 Password Hash: " + hash);
                Console.WriteLine("\nFormat hash: " + outputStr + "\n");
                Console.WriteLine(new string('-', 20));

                // Checks if the hash is the same as the blank password's hash.
                // If not, write out the format hash to the file.
                if (!CheckForBlankPassword(guid, hash)) {
                    try {
                        if (filePath != null) {
                            if (filePath.Exists) {
                                if (!Choice("Do you want to overwrite the specified file?")) return;
                                filePath.Delete();
                            }

                            using (StreamWriter sw = new StreamWriter(filePath.FullName)) {
                                sw.Write(outputStr);
                                sw.Flush();
                                Console.WriteLine("[i]: Data has been written.");
                            }
                        }
                    } catch (Exception ex) {
                        throw new InvalidOperationException("[x]: Couldn't write the output. (" + ex.Message + ")");
                    }
                }

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            } finally {
                // Revert to the previous title.
                Console.Title = prevTitle;
            }
        }
        
        /// <summary>
        /// Retrieves Shadow Defender's Password Hash from the specified path.
        /// </summary>
        /// <param name="userDatPath">A specified path pointing to the user.dat file</param>
        /// <param name="outHash">An output string of MD5-UTF16LE hash.</param>
        /// <returns>Returns true if found, and outputs the value to <paramref name="outHash"/>; otherwise, false.</returns>
        private static bool GetDefenderMD5Hash(string userDatPath, out string outHash) {
            outHash = "";
            Console.WriteLine("[i]: Acquiring hash...");
            StringBuilder hash = new StringBuilder(255);
            GetPrivateProfileString("common", "hash", "", hash, 32768, userDatPath);
            if (string.IsNullOrEmpty(hash.ToString())) {
                Console.WriteLine("[x]: Couldn't acquire a hash. (" + new Win32Exception(Marshal.GetLastWin32Error()).Message + ")");
                return false;
            }
            outHash = hash.ToString();
            return true;
        }

        /// <summary>
        /// Checks if Shadow Defender's Password Hash is the same hash used for blank password.
        /// </summary>
        /// <param name="guid">A unique GUID of Shadow Defender.</param>
        /// <param name="hash">The hash provided.</param>
        /// <returns>Returns true if the hash is a blank password; otherwise, false.</returns>
        private static bool CheckForBlankPassword(string guid, string hash) {
            string blankpwdHash = HashPassword(guid);
            if (blankpwdHash.Equals(hash.ToString())) {
                Console.WriteLine("[i]: Password was not set.");
                Console.WriteLine("Key: @BLANKPWD >> Hash: " + blankpwdHash);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs a password hashing using Shadow Defender's hashing technique.
        /// </summary>
        /// <param name="guid">A unique GUID of Shadow Defender.</param>
        /// <param name="password">A password to use for concatenate with <paramref name="guid"/>.</param>
        /// <returns>Returns the MD5-UTF16LE digested hash.</returns>
        private static string HashPassword(string guid, string password = "@BLANKPWD") {

            // Shadow Defender concatenates a unique GUID with the password provided
            // by the administrator, encoding into bytes using UTF-16LE (little endian)
            // before hashing it with MD5.

            StringBuilder hash = new StringBuilder(255);
            hash.Append(guid + password);
            using (MD5 md5 = MD5.Create()) {
                byte[] utf16data = Encoding.Unicode.GetBytes(hash.ToString());
                byte[] outputData = md5.ComputeHash(utf16data);
                return BitConverter.ToString(outputData).ToLower().Replace("-", "");
            }
        }

        /// <summary>
        /// Retrieves value from the specified registry subkey.
        /// </summary>
        /// <param name="regPath"></param>
        /// <param name="valName"></param>
        /// <param name="value"></param>
        /// <returns>
        /// Returns true if succeed, with the result on the <paramref name="value"/>; 
        /// otherwise, false and the value is <paramref name="value"/>.
        /// </returns>
        private static bool GetValueFromRegistry(string regPath, string valName, out string value) {
            value = "";
            RegistryKey defenderKey = null;
            try {
                defenderKey = Registry.LocalMachine.OpenSubKey(regPath);
                if (defenderKey == null)
                    throw new InvalidOperationException("Registry path of '" + regPath + "' is not exist or not accessible.");
                object obj = defenderKey.GetValue(valName) ??
                    throw new InvalidOperationException("Registry value of '" + valName + "' is not found.");
                value = obj.ToString();
                return true;
            } catch (Exception ex) {
                Console.WriteLine("[x]: Failed to get a registry value. (" + ex.Message + ")");
                return false;
            } finally {
                defenderKey?.Close();
            }

        }

        /// <summary>
        /// Prints the help from the resource.
        /// </summary>
        private static void ShowHelp() {
            Console.WriteLine(Properties.Resources.Help);
        }

        /// <summary>
        /// Gets the respond from the user.
        /// </summary>
        /// <param name="message">A string message to print before user-decision.</param>
        /// <returns>Returns a boolean value representing user's decision.</returns>
        private static bool Choice(string message) {
            ConsoleKey response;
            do {
                Console.Write($"{message} [y/n]: ");
                response = Console.ReadKey().Key;
                if (response != ConsoleKey.Enter)
                    Console.WriteLine();
            } while (response != ConsoleKey.Y && response != ConsoleKey.N);

            return response == ConsoleKey.Y;
        }
    }
}
