// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"
#r "System.IO.Compression.FileSystem"

open System
open System.IO
open Fake
open Fake.NpmHelper

let yarn = 
    if EnvironmentHelper.isWindows then "yarn.cmd" else "yarn"
    |> ProcessHelper.tryFindFileOnPath
    |> function
       | Some yarn -> yarn
       | ex -> failwith ( sprintf "yarn not found (%A)\n" ex )

// Directories
let buildDir  = "./build/"

// Filesets
let projects  =
      !! "src/*.fsproj"

// Artifact packages
let packages  =
      !! "src/package.json"

let dotnetcliVersion = "1.0.1"
let mutable dotnetExePath = "dotnet"

let runDotnet workingDir =
    DotNetCli.RunCommand (fun p -> { p with ToolPath = dotnetExePath
                                            WorkingDir = workingDir } )

Target "InstallDotNetCore" (fun _ ->
   dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

Target "Install" (fun _ ->
    projects
    |> Seq.iter (fun s -> 
        let dir = IO.Path.GetDirectoryName s
        printf "Installing: %s\n" dir
        Npm (fun p ->
            { p with
                NpmFilePath = yarn
                Command = Install Standard
                WorkingDirectory = dir
            })
        runDotnet dir "restore"
    )
)


// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir]
)

Target "Build" (fun _ ->
    projects
    |> Seq.iter (fun s -> 
        let dir = IO.Path.GetDirectoryName s
        runDotnet dir "build")
)


// Build order
"Clean"
  ==> "InstallDotNetCore"
  ==> "Install"
  ==> "Build"
  
// start build
RunTargetOrDefault "Build"
