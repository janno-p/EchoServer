module EchoServer.Program

open System
open System.IO
open System.Net
open System.Diagnostics
open System.Xml.Linq

[<EntryPoint>]
let main argv =
    use listener = new HttpListener()
    listener.Prefixes.Add("http://localhost:8001/")
    listener.Start()

    let rootPath = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)
    let fullPath n = Path.Combine(rootPath, "Responses", n + ".xml")

    let rec mainLoop () =
        let context = listener.GetContext()
        let request = context.Request
        let response = context.Response

        let fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".xml"

        let getRequestMessage () =
            use r = new StreamReader(request.InputStream)
            let doc = XDocument.Load(r)
            use w = new StreamWriter(File.Create(fileName))
            doc.Save(w)
            doc

        let doc = getRequestMessage()

        let elem name (parent: XElement) =
            match parent.Element(name) with
            | null -> None
            | element -> Some(element)

        Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", fileName) |> ignore

        let serviceName =
            doc.Root
            |> elem (XName.Get("Header", "http://schemas.xmlsoap.org/soap/envelope/"))
            |> Option.bind (fun x -> x |> elem (XName.Get("nimi", "http://x-tee.riik.ee/xsd/xtee.xsd")))
            |> Option.bind (fun x -> if File.Exists(fullPath x.Value) then Some(x.Value) else None)

        use w = new StreamWriter(response.OutputStream)

        match serviceName with
        | Some(name) ->
            use file = new StreamReader(File.OpenRead(fullPath name), Text.Encoding.UTF8)
            w.Write(file.ReadToEnd())
        | None ->
            w.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?><error />")

        w.Flush()
        response.Close()

        mainLoop()

    mainLoop()

    0
