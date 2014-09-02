// --------------------------------------------------------------------------------------
// FAKE build script 
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/NuGet.Core.dll"
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open System
open System.IO
#if MONO
#else
#load "packages/SourceLink.Fake/Tools/Fake.fsx"
open SourceLink
#endif

let nugetPath = Path.Combine(__SOURCE_DIRECTORY__,@"tools\NuGet\NuGet.exe")


let projectName, version = "FSharp.Enterprise",  if isLocalBuild then ReadFileAsString "local_build_number.txt" else tcBuildNumber

let buildDir, testDir, deployDir, docsDir, nugetDir = @"build\artifacts", @"build\test", @"build\deploy", @"build\docs", @"build\nuget"
let nugetDocsDir = nugetDir @@ "docs"
let nugetKey = if System.IO.File.Exists "./nuget-key.txt" then ReadFileAsString "./nuget-key.txt" else ""

let appReferences = !! @"src\**\FSharp.Enterprise.fsproj"
let testReferences = !! @"tests\**\*.Tests.*sproj"

Target "RestorePackages" RestorePackages

Target "Clean" (fun _ -> 
    CleanDirs [buildDir; testDir; deployDir; docsDir]
)

Target "AssemblyInfo" (fun _ -> 
       
        CreateFSharpAssemblyInfo "src/FSharp.Enterprise/AssemblyInfo.fs" 
            [
                Attribute.Version version
                Attribute.Title projectName
                Attribute.Guid "207C7E5B-DFFF-41DC-849A-53D10A0FF644"
            ]
       
        CreateFSharpAssemblyInfo "src/FSharp.Enterprise.RabbitMq/AssemblyInfo.fs" 
            [
                Attribute.Version version
                Attribute.Title (projectName + ".RabbitMq")
                Attribute.Guid "D93F9436-D1DD-4FB1-9C0A-52298F9F0215"
            ]
       
        CreateFSharpAssemblyInfo "src/FSharp.Enterprise.Web/AssemblyInfo.fs" 
            [
                Attribute.Version version
                Attribute.Title (projectName + ".Web")
                Attribute.Guid "C4855501-6D39-44CF-B55E-DE8EE16516AC"
            ]
)

Target "BuildApp" (fun _ ->
    MSBuild buildDir "Build" ["Configuration","Release"; "Platform", "anycpu"] appReferences |> Log "BuildApp: "
)

Target "BuildTest" (fun _ ->
    MSBuildDebug testDir "Build" testReferences
        |> Log "TestBuild-Output: "
)

Target "Test" (fun _ ->
    !! (testDir + "/*.Tests.dll")
        |> NUnit (fun p ->
            {p with
                DisableShadowCopy = true
                OutputFile = testDir + "\TestResults.xml" })
)

Target "Deploy" (fun _ ->
    !! (buildDir + "/**/FSharp.Enterprise*.dll")
        ++ (buildDir + "/**/FSharp.Enterprise.pdb")
        ++ (buildDir + "/**/FSharp.Enterprise.xml")
        -- "*.zip"
        |> Zip buildDir (deployDir + sprintf "\%s-%s.zip" projectName version)
)

Target "BuildNuGet" (fun _ ->
    CleanDirs [nugetDir; nugetDocsDir]
    XCopy docsDir nugetDocsDir
    printfn "%s" nugetPath
    [
        "lib", buildDir + "\FSharp.Enterprise.dll"
        "lib", buildDir + "\FSharp.Enterprise.pdb"
        "lib", buildDir + "\FSharp.Enterprise.xml"
    ] |> Seq.iter (fun (folder, path) -> 
                    let dir = nugetDir @@ folder @@ "net40"
                    CreateDir dir
                    CopyFile dir path)
    NuGet (fun p ->
        {p with               
            Authors = ["Simon Cousins"]
            Project = projectName
            Description = "F# Enterprise Library collection"
            Version = version
            OutputPath = nugetDir
            WorkingDir = nugetDir
            AccessKey = nugetKey
           // ToolPath = "tools\Nuget\Nuget.exe"
            PublishUrl = "http://hp20006551:8082/httpAuth/app/nuget/v1/FeedService.svc/"       
            Publish = false})
        ("./FSharp.Enterprise.nuspec")
    [
       (nugetDir) + sprintf "\FSharp.Enterprise.%s.nupkg" version
    ] |> CopyTo deployDir
)

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

"Clean"
    ==> "RestorePackages"
    ==> "AssemblyInfo"
    ==> "BuildApp" <=> "BuildTest"
    ==> "Test" 
    ==> "BuildNuGet"
    ==> "Deploy"

RunTargetOrDefault "Deploy"
