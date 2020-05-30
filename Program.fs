// Learn more about F# at http://fsharp.org

open System
open System.IO

type Volume = string
type File = string

module private Constants =
    let onCameraImageLocation = "DCIM/100EOS7D"
    let targetDirectory = "/Users/zaymonfoulds-cook/_Photos/Raws"
    let supportedFileExtensions = [ ".cr2"; ".jpg"; ".mp4"; ".png" ]
    let dateFormat = "yyyy-MM" // .NET DateTime Format String


module Helpers =
    let readInt = Console.ReadLine >> Int32.Parse
    let ( >=> ) (xo: 'a option) (f: 'a -> 'b option) = xo |> Option.bind f

    let iterWithProgress (f: 'a -> 'b) (operationName: string) (l: 'a list) =
        let totalLength = l |> List.length

        l |> List.iteri (fun index element ->
            printfn "%s %d out of %d" operationName (index + 1) totalLength
            f element |> ignore)


module Comparisons =
    let (|InvariantEquals|_|) s s2 =
        match String.Equals(s, s2, StringComparison.InvariantCultureIgnoreCase) with
        | true -> Some ()
        | false -> None


module Printing =
    let printAnyThing thing = printfn "%A" thing
    let say s = printfn "%s" s

    let printHeading version =
        printfn "Camera Importer Version: %f" version

    let printIndexedVolumes (vs: (int * string) list) : unit =
        vs |> List.iter(fun (i, v) -> printfn "%d: %s" i v)
        printfn ""


module FileHelpers =
    let getVolumes () : Volume list =
        System.IO.Directory.EnumerateDirectories("/Volumes")
        |> List.ofSeq

    let loadFiles (v: Volume) : File list =
        Directory.GetFiles(sprintf "%s/%s" v Constants.onCameraImageLocation)
        |> List.ofSeq

    let copyFile directoryToCopy (file: FileInfo) =
        file.CopyTo(Path.Combine(directoryToCopy, file.Name)) |> ignore

    let copyFiles (x: {| Directory: string; FilesToCopy: FileInfo list |}) =
        Directory.CreateDirectory(x.Directory) |> ignore

        x.FilesToCopy |> Helpers.iterWithProgress (copyFile x.Directory) "Copying"

    let enrichWithFolderNumber (description, fs) =
        // Get the Current Directories
        let directoryNumber =
            Directory.EnumerateDirectories("/Users/zaymonfoulds-cook/_Photos/Raws")
            |> List.ofSeq
            |> List.length
            |> fun x -> x + 1

        let dateSegment = DateTime.Now.ToString(Constants.dateFormat)

        {|
            Directory = sprintf "%s/%d_%s_%s" Constants.targetDirectory directoryNumber dateSegment description
            FilesToCopy = fs
        |}


module Interaction =
    open Printing
    open Helpers
    open Comparisons

    let pickVolume (volumes: Volume list) : Volume =
        say "Please select a volume to transfer from:"
        let indexedVolumes = volumes |> List.mapi (fun i v -> i, v)

        indexedVolumes |> printIndexedVolumes

        readInt() |> fun choice -> indexedVolumes.[choice] |> snd

    type Decision =
        | Yes
        | No

    let getYesNo () : Decision =
        printf "Choice: "
        match Console.ReadLine() with
        | InvariantEquals "Y" | InvariantEquals "Yes" -> Yes
        | _ -> say "Goodbye"; No

    let confirm (fs: File list) : FileInfo list option =
        let supportedExtension (s: string) =
            Constants.supportedFileExtensions
            |> List.exists (fun fileExtension ->
                s.Contains(fileExtension, StringComparison.InvariantCultureIgnoreCase))

        let listToOption (l) =
            match l with
            | [] -> say "No images or videos found in selected volume!"; None
            | elements -> Some elements

        fs
        |> List.filter supportedExtension
        |> listToOption
        |> Option.bind (fun mediaFiles ->
            printfn "Would you like to import %d image and video files? [Y/N]" mediaFiles.Length
            match getYesNo() with
            | Yes -> Some (mediaFiles |> List.map FileInfo)
            | No -> None)

    let getMediaSetDescription (fs: FileInfo list) =
        say "Please describe the set of images:"
        Console.ReadLine().Replace(" ", "_").ToUpperInvariant(), fs


let workflow =
    FileHelpers.getVolumes
    >> Interaction.pickVolume
    >> FileHelpers.loadFiles
    >> Interaction.confirm
    >> Option.map (
        Interaction.getMediaSetDescription
        >> FileHelpers.enrichWithFolderNumber
        >> FileHelpers.copyFiles)
    >> ignore

[<EntryPoint>]
let main argv =
    Printing.printHeading 0.1

    workflow()

    0 // return an integer exit code
