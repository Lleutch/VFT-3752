namespace Library2

open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Interactive.Shell

open System.Text
open System.IO


module test =

    let sbOut = new StringBuilder()
    let sbErr = new StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    // Build command line arguments & start FSI session
    let argv = [| "C:\\fsi.exe" |]
    let allArgs = Array.append argv [|"--noninteractive"|]

    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream) 

    
    // GetOrAdd should in this case return a warning -> it doesn't in FCS version :
    // 10.0.3 + 11.0.1 + 13.0.0 ...
    // works on 8.0.0 + 9.0.1
    let fnTest1 = """
    let internal memoizeBy (getKey : 'a -> 'key) (f: 'a -> 'b) : 'a -> 'b =
        let cache = System.Collections.Concurrent.ConcurrentDictionary<'key, 'b>()
        fun (x: 'a) ->
            cache.GetOrAdd(getKey x, f)
    """

    // match should in this case return a warning -> it does 
    let fnTest2 = """
    let test isSome = 
        match isSome with
        | None -> () 
    """


    let parseAndCheckSingleFile (input) = 
        let file = Path.ChangeExtension(System.IO.Path.GetTempFileName(), "fsx")
        File.WriteAllText(file, input)
        let checker = SourceCodeServices.FSharpChecker.Create(keepAssemblyContents=true)

        let projOptions = 
            checker.GetProjectOptionsFromScript(file, input)
            |> Async.RunSynchronously

        checker.ParseAndCheckProject(projOptions) //|> fst) 
        |> Async.RunSynchronously


    let generateFooAndGetArgsInfos fn =
        let input = 
            """
module MyLibrary 
open System
            """ 
        let singleFile = input + fn

        let checkProjectResults = parseAndCheckSingleFile(singleFile)
        let err = checkProjectResults.Errors
        err

    let listOfErrorsAndWarnings1 = generateFooAndGetArgsInfos fnTest1
    let listOfErrorsAndWarnings2 = generateFooAndGetArgsInfos fnTest2

    let printing () =
        printfn "Should Have 1 error :\n %A" listOfErrorsAndWarnings1
        printfn "Should Have 1 error :\n %A" listOfErrorsAndWarnings2

    [<EntryPoint>]
    let main args =
        printing ()
        0

