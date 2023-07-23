# Novel Hash Format

## Description
When Gasera acquires the hash and GUID from Shadow Defender's registry and ini file,
there are two ways to output, depending on user's provided command argument:

- **Split Format:** Writes the hash and GUID salt file (usually, it has '-guid' after
  the user-specific filename.)
- **Novel Hash Format:** Combines the GUID salt and the hash, then outputs into a single
  file. (If user add '--use-novel-hash' to the commandline argument.)

The Novel Hash Format follows the format that is commonly use for compatibility
with known password recovery tools like Hashcat, and it is possible to have
number of different hashes that has the same format, in a single file by
adding '-fA' on the commandline argument or press 'A' when the specified
file exist, in order to append.

Format:
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

## Note

Shadow Defender concatenates the unique GUID with a password provided by an
administrator when setting up the Password Control. The concatenated string
will then encoded with UTF16 Little Endian before hashing with MD5.

The example algorithm is ```HashPassword(string, string)```, located in 
[Gasera.cs](https://github.com/PheeLeep/Gasera-Project/blob/master/GaseraLib/Gasera.cs#L209)
file.
