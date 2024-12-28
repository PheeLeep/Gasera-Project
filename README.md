# Gasera-Project

# DISCLAIMER
This program is use for education purposes only, and it should not be use
beyond on what is supposed to, without a mutual permission. The author is not
responsible or not held liable to any damage or misuse that was made by using 
this program.

**USE AT YOUR OWN RISK.**
</br>

## Description
Gasera (TL from Tagalog: Kerosene Lamp) is a program that acquires hash 
and GUID used for password control in Shadow Defender, for password
recovery.

Shadow Defender has a password-protected feature, where the administrator
lock the program with a password to prevent unauthorized access and exiting
the drives out of the virualization mode (or "Shadow Mode").

## Download/Compile
You can download the latest Release binary [here.](https://github.com/PheeLeep/Gasera-Project/releases)
(the zip file contains both .NET and .NET Framework version of the program.)

Or, you can pull this repository by executing this command:
```cmd
git clone https://github.com/PheeLeep/Gasera-Project
```

And then, go to the folder where you store the pulled repository, and
compile by the following methods you want (both methods build the executables
to the respected 'bin' folders (like: **'Gasera\bin'** for .NET version,
and **'Gasera-netF\bin'** for .NET Framework version)):

In command prompt:

- Open the Command Prompt in the repository folder (usually by shift and right-click,
  and then click **Open in Terminal/Command Prompt**). Next, type the following:
```cmd
dotnet build
```

In Visual Studio 2022 or later:

- Double-click **Gasera-Project.sln**
- On the dropdown menu, go to **Build** > then click **Build Solution**

**NOTE:** Make sure you have .NET 7.0 SDK and .NET Framework 4.8 SDK (for Gasera-netF)
installed on your computer, in order to compile the program. The reason of having the
.NET Framework version is for alternative purpose if the .NET runtime is not installed
on the target machine, making the .NET version unable to run.

## Usage
**NOTE:** This also applies to Gasera-netF (.NET Framework version)
```cmd
Gasera [/out <file>] [options]
```

```
    /out <file>       - Writes the hash to the specified file path.                    
                        By default, the program writes two files. The MD5(UTF16LE()) hash
                        target, and the GUID that acts as a salt.
-------------------------------------------------------
    Options:

    /h                - Shows help.
    --use-novel-hash  - Use the new hash format, instead of writing two files. (hash and GUID salt). 
                        (see the GitHub repo documentation for more details about the format.)

    -i                - Don't exit if the hash is for blank password, and proceed to writing
                        the output anyway.

    -fO               - Force to overwrite the file output.
    -fA               - Force to append the file output with new value.
    --enforce-64      - Enforce the program to use 64-bit registry
    --enforce-32      - Enforce the program to use 32-bit registry
```

## How to Use for Password Recovery?

### For Separated GUID+Hash Files:
- In Hashcat:

    Use **'-a 1'** for dictionary attack, or **'-a 6'** for mask attack.
    
    (It is important to specify GUID file first before the wordlist file or mask pattern, 
to prepend it with each password to guess.)

    The hash type is **70** '(md5(utf16le($pass)))'.

    Example:

```
   hashcat -m 70 shadow-hash.txt -a 1 shadow-hash-guid.txt rockyou.txt
                       ^                       ^               ^
                [Target Hash File]     [GUID Salt File]    [Wordlist]
```


### For Novel Hash:
As of now, there is no password recovery tools that are supported the format for
Shadow Defender hash. If you're interested to build an algorithm for the said hash format,
checkout [here.](https://github.com/PheeLeep/Gasera-Project/blob/master/NovelHashFormat.md)

## License
This project is under [MIT License.](https://github.com/PheeLeep/Gasera-Project/blob/master/LICENSE.txt)
