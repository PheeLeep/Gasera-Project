using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static GaseraLib.IOClass;
using static GaseraLib.WinNatives;

namespace GaseraLib {

    // Some of SonarLint warnings were disable.
#pragma warning disable S2583, S2589

    public static class Gasera {

        private static string prevTitle = "";
        private static FileExistAction fca_outFile = FileExistAction.None;
        private static FileExistAction fca_outGUIDSalt = FileExistAction.None;
        private static bool fileConflictSet = false;
        private static bool forceWriteOnBlankPassword = false;

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

                // I'll split the codeflow into regions, for better view and organized.
                #region Stage 1 (Parameter and File Checks)

                // Shadow Defender is for Windows-only, so we should target Windows.
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    throw new PlatformNotSupportedException("[!]: This program is for Windows only.");

                // Parameter checks.
                if (args.Length == 0 || args[0].Equals("/h")) {
                    ShowHelp();
                    return;
                }

                FileInfo outFile = null;
                bool write_as_novel_hash_format = false;

                for (int i = 0; i < args.Length; i++) {
                    switch (args[i]) {
                        case "/out":
                            if ((i + 1) >= args.Length)
                                throw new InvalidOperationException("[!]: No output file specified.");
                            i++;
                            outFile = new FileInfo(args[i]);
                            break;
                        case "--use-novel-hash":
                            if (write_as_novel_hash_format) break;
                            write_as_novel_hash_format = true;
                            break;
                        case "-fO":
                            SetFileConflictAction(FileExistAction.Overwrite);
                            break;
                        case "-fA":
                            SetFileConflictAction(FileExistAction.Append);
                            break;
                        case "-i":
                            forceWriteOnBlankPassword = true;
                            break;
                        default:
                            throw new InvalidOperationException("[!]: No parameter exist to type '" + args[0] + "'.");
                    }
                }

                if (outFile == null)
                    throw new InvalidOperationException("[!]: No output file specified.");
                string safeFileName = outFile.Name.Replace(outFile.Extension, "");
                FileInfo guidFile = new FileInfo(Path.Combine(outFile.Directory.FullName, safeFileName + "-guid" + outFile.Extension));

                // Check for file outputs' existence first.
                if (fca_outFile == FileExistAction.None && outFile.Exists && !FileExistChoice(outFile.Name, out fca_outFile))
                    return;
                if (!write_as_novel_hash_format && fca_outGUIDSalt == FileExistAction.None
                    && guidFile.Exists && !FileExistChoice(guidFile.Name, out fca_outGUIDSalt))
                    return;
                #endregion
                #region Stage 2 (Program Lookup and Hash Acquire)

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
                if (!Regex.IsMatch(hash, "^[0-9a-fA-F]{32}$"))
                    throw new InvalidOperationException("[x]: Invalid hash format found.");

                Console.WriteLine("[i]: Necessary data has been acquired.");
                #endregion
                #region Stage 3 (Output)
                string outputStr = write_as_novel_hash_format ? "$shadow_defender$*" + guid + "*" + hash : hash;

                Console.WriteLine("\nResult:\n" + new string('-', 20));
                Console.WriteLine("Shadow Defender GUID: " + guid);
                Console.WriteLine("MD5 UTF-16 Password Hash: " + hash);
                Console.WriteLine(new string('-', 20));

                // Checks if the hash is the same as the blank password's hash.
                // If yes and the 'forceWriteOnBlankPassword' is false,
                // exit the program for obvious reason.
                if (CheckForBlankPassword(guid, hash) && !forceWriteOnBlankPassword) return;

                if (!WriteTextToFile(outFile, outputStr, fca_outFile)) return;
                if (!write_as_novel_hash_format) {
                    if (!WriteTextToFile(guidFile, guid, fca_outGUIDSalt)) 
                        return;
                    Console.WriteLine("\nIf you're using Hashcat, use '-a 1' or '-a 6', and make sure");
                    Console.WriteLine("the GUID salt file is specified first before the wordlist or mask pattern.");
                    Console.WriteLine("\nRun this program with '/h' for help and more details.\n");
                }
                #endregion
                Console.WriteLine("[i]: Done.");
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            } finally {
                // Revert to the previous title.
                Console.Title = prevTitle;
            }
        }

        /// <summary>
        /// Sets the values of <see cref="fca_outFile"/> and <see cref="fca_outGUIDSalt"/>
        /// </summary>
        /// <param name="fca">The <see cref="FileExistAction"/> value to be set.</param>
        /// <remarks>This method will work once. Calling again will never set the two values.</remarks>
        private static void SetFileConflictAction(FileExistAction fca) {
            if (fileConflictSet || fca == FileExistAction.None) return;
            // Set chaining.
            fca_outFile = fca_outGUIDSalt = fca;
            fileConflictSet = true;
        }

        /// <summary>
        /// Retrieves Shadow Defender's Password Hash from the specified path.
        /// </summary>
        /// <param name="userDatPath">A specified path pointing to the user.dat file</param>
        /// <param name="outHash">An output string of MD5-UTF16LE hash.</param>
        /// <returns>Returns true if found, and outputs the value to <paramref name="outHash"/>; otherwise, false.</returns>
        private static bool GetDefenderMD5Hash(string userDatPath, out string outHash) {
            outHash = "";
            Console.WriteLine("[*]: Acquiring hash...");
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

            StringBuilder hash = new StringBuilder();
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
    }
}
