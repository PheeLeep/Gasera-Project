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
and GUID used for password-locking Shadow Defender from its initialization 
file located in its installed folder path, and its registry key.

Shadow Defender has a password-protected feature, where the administrator
lock the program with a password to prevent unauthorized access and exiting
the drives out of the virualization mode (or "Shadow Mode").

## Download/Compile
You can download the Release binary [here.](https://github.com/PheeLeep/Gasera-Project/releases)
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
installed on your computer, in order to compile the program. 

## Usage
**NOTE:** This also applies to Gasera-netF (.NET Framework version)
```cmd
Gasera [/h] [/out <file>]
```

Arguments:
```
/h              - Shows help.
/out <file>     - Outputs the hash to the specified file.
                  (it will not output if the existing hash is the
                  same hash for blank password.)
```

## Format
Gasera outputs the format and its parameters for Hashcat and John the Ripper:

```
$shadow_defender$*[GUID]*[Hash]
```
Parameters:
```
      $shadow_defender$  : The target name. (which in case, Shadow Defender)
      [GUID]             : A unique GUID generated during Shadow Defender
                           installation. Acts as a salt.
      [Hash]             : A UTF-16 Little Endian encoded, MD5-digested
                           password hash.
```

The MD5 hash calculation algorithm is: **md5(utf16le([GUID].$Pass))** => [Hash]
## License
[MIT License](https://github.com/PheeLeep/Gasera-Project/blob/master/LICENSE.txt)