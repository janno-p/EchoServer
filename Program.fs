module EchoServer.Program

open System
open System.IO
open System.Net
open System.Diagnostics

[<EntryPoint>]
let main argv =
    use listener = new HttpListener()
    listener.Prefixes.Add("http://localhost:8001/")
    listener.Start()

    let rec mainLoop () =
        let context = listener.GetContext()
        let request = context.Request
        let response = context.Response

        let fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xml"

        let processRequest () =
            use r = new StreamReader(request.InputStream)
            use w = new StreamWriter(File.Create(fileName))
            w.Write(r.ReadToEnd())

        processRequest()

        Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", fileName) |> ignore

        use w = new StreamWriter(response.OutputStream)
        w.WriteLine("Hello, World!")

        mainLoop()

    mainLoop()

    0
