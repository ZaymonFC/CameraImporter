# Camera Importer

This is a quick a dirty camera importer written in F# for MAC-OS

### Getting Started
Change configuration at the top of `Program.fs` to suit your needs.

```fsharp
module private Configuration =
    let onCameraImageLocation = "DCIM/100EOS7D"
    let targetDirectory = "/Users/zaymonfoulds-cook/_Photos/Raws"
    let supportedFileExtensions = [ ".cr2"; ".jpg"; ".mp4"; ".png" ]

    let dateFormat = "yyyy-MM" // .NET DateTime Format String

    let descriptionCasing : DescriptionCasing = UpperCase
    let descriptionSpaceReplacement : String = "_"
```

Camera importer will copy all files matching the supported file extension into a folder within `targetDirectory` in the format:

```
folderNumber_<custom-date-pattern>_DESCRIPTION_IN_SPECIFIED_FORMAT
```

### Running
- Requires any .NET Core SDK

`dotnet run` will give you an interactive prompt to select the volume where the files are located. Name them and copy.
