using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GaseraLib {
    internal static class WinNatives {
        /// <summary>
        /// Retrieves a string from the specified section in an initialization file.
        /// </summary>
        /// <returns>The return value is the number of characters copied to the buffer.</returns>
        /// <remarks>
        /// Documentation found on: <see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring"/>
        /// </remarks>

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault,
            StringBuilder lpReturnedString, int nSize, string lpFileName);

        /// <summary>
        /// Copies a string into the specified section of an initialization file.
        /// </summary>
        /// <returns>If the function successfully copies the string to the initialization file, the return value is nonzero.</returns>
        /// <remarks>
        /// Documentation found on: <see href="https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-writeprivateprofilestringa"/>
        /// </remarks>
        [DllImport("kernel32", SetLastError = true)]
        internal static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    }
}
