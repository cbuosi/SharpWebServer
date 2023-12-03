using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.Globalization;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;
using System.Web;
using System.Diagnostics;

public class SocketListener
{
    const string VERSAO = "2.03";

    enum eTipoReqHTTP
    {
        DESCONHECIDO = 0,
        GET,
        POST,
        PUT,
        PATCH,
        DELETE,
        HEAD,
        OPTIONS
    }

    enum eTipoArquivo
    {
        DESCONHECIDO = 0,
        TEXTO = 1,
        IMAGEM = 2,
        AUDIO = 3,
        VIDEO = 4,
        FONTE = 5,
        APLICACAO = 6
    }

    enum eStatusHTTP
    {
        DESCONHECIDO = 0,
        //Informativo = 1xx,
        Continue = 100,
        Switching_Protocols = 101,
        Processing = 102,
        //Sucesso = 2xx,
        OK = 200,
        Created = 201,
        Accepted = 202,
        Non_authoritative_Information = 203,
        No_Content = 204,
        Reset_Content = 205,
        Partial_Content = 206,
        Multi_Status = 207,
        Already_Reported = 208,
        IM_Used = 226,
        //Redirecionamento = 3xx,
        Multiple_Choices = 300,
        Moved_Permanently = 301,
        Found = 302,
        See_Other = 303,
        Not_Modified = 304,
        Use_Proxy = 305,
        Temporary_Redirect = 307,
        Permanent_Redirect = 308,
        //Erro_no_Cliente = 4xx,
        Bad_Request = 400,
        Unauthorized = 401,
        Payment_Required = 402,
        Forbidden = 403,
        Not_Found = 404,
        Method_Not_Allowed = 405,
        Not_Acceptable = 406,
        Proxy_Authentication_Required = 407,
        Request_Timeout = 408,
        Conflict = 409,
        Gone = 410,
        Length_Required = 411,
        Precondition_Failed = 412,
        Payload_Too_Large = 413,
        Request_URI_Too_Long = 414,
        Unsupported_Media_Type = 415,
        Requested_Range_Not_Satisfiable = 416,
        Expectation_Failed = 417,
        Im_a_teapot = 418,
        Misdirected_Request = 421,
        Unprocessable_Entity = 422,
        Locked = 423,
        Failed_Dependency = 424,
        Upgrade_Required = 426,
        Precondition_Required = 428,
        Too_Many_Requests = 429,
        Request_Header_Fields_Too_Large = 431,
        Connection_Closed_Without_Response = 444,
        Unavailable_For_Legal_Reasons = 451,
        Client_Closed_Request = 499,
        //Erro_no_Servidor = 5xx,
        Internal_Server_Error = 500,
        Not_Implemented = 501,
        Bad_Gateway = 502,
        Service_Unavailable = 503,
        Gateway_Timeout = 504,
        HTTP_Version_Not_Supported = 505,
        Variant_Also_Negociates = 506,
        Insufficient_Storage = 507,
        Loop_Detected = 508,
        Not_Extended = 510,
        Network_Authentication_Required = 511,
        Network_Connection_Timeout_Error = 599,
    }

    const string HTTP_VER = "HTTP/1.1";
    const string ARQUIVO_DATABASE_CONFIG = "Config.xml";
    static string ROOT_SERVIDOR = ""; //Config.xml
    static string DESCRICAO_SERVIDOR = ""; //Config.xml
    static string ARQUIVO_PADRAO = ""; //Config.xml
    static string CACHE_MAX_AGE = ""; //Config.xml


    static Stopwatch? oReloginho = null;

    public static int Main(String[] args)
    {
        StartServer();
        return 0;
    }

    private static string ObterMime(string _ext)
    {
        switch (_ext.ToUpper().Trim())
        {
            case ".AAC": return "audio/aac";//AAC audio											
            case ".ABW": return "application/x-abiword";//AbiWord document									
            case ".ARC": return "application/x-freearc";//Archive document (multiple files embedded)			
            case ".AVIF": return "image/avif";//AVIF image											
            case ".AVI": return "video/x-msvideo";//AVI": Audio Video Interleave							
            case ".AZW": return "application/vnd.amazon.ebook";//Amazon Kindle eBook format							
            case ".BIN": return "application/octet-stream";//Any kind of binary data								
            case ".BMP": return "image/bmp";//Windows OS/2 Bitmap Graphics						
            case ".BZ": return "application/x-bzip";//BZip archive										
            case ".BZ2": return "application/x-bzip2";//BZip2 archive										
            case ".CDA": return "application/x-cdf";//CD audio											
            case ".CSH": return "application/x-csh";//C-Shell script										
            case ".CSS": return "text/css";//Cascading Style Sheets (CSS)						
            case ".CSV": return "text/csv";//Comma-separated values (CSV)						
            case ".DOC": return "application/msword";//Microsoft Word										
            case ".DOCX": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";//Microsoft Word (OpenXML)							
            case ".EOT": return "application/vnd.ms-fontobject";//MS Embedded OpenType fonts							
            case ".EPUB": return "application/epub+zip";//Electronic publication (EPUB)						
            case ".GZ": return "application/gzip";//GZip Compressed Archive								
            case ".GIF": return "image/gif";//Graphics Interchange Format (GIF)					
            case ".HTM": return "text/html";//HyperText Markup Language (HTML)					
            case ".HTML": return "text/html";//HyperText Markup Language (HTML)					
            case ".ICO": return "image/vnd.microsoft.icon";//Icon format											
            case ".ICS": return "text/calendar";//iCalendar format									
            case ".JAR": return "application/java-archive";//Java Archive (JAR)									
            case ".JPEG": return "image/jpeg";//JPEG images											
            case ".JPG": return "image/jpeg";//JPEG images											
            case ".JS": return "text/javascript"; //(Specifications: HTML and RFC 9239)";//JavaScript											
            case ".JSON": return "application/json";//JSON format											
            case ".JSONLD": return "application/ld+json";//JSON-LD format										
            case ".MID": return "audio/midi, audio/x-midi";//Musical Instrument Digital Interface (MIDI)			
            case ".MIDI": return "audio/midi, audio/x-midi";//Musical Instrument Digital Interface (MIDI)			
            case ".MJS": return "text/javascript";//JavaScript module									
            case ".MP3": return "audio/mpeg";//MP3 audio											
            case ".MP4": return "video/mp4";//MP4 video											
            case ".MPEG": return "video/mpeg";//MPEG Video											
            case ".MPKG": return "application/vnd.apple.installer+xml";//Apple Installer Package								
            case ".ODP": return "application/vnd.oasis.opendocument.presentation";//OpenDocument presentation document					
            case ".ODS": return "application/vnd.oasis.opendocument.spreadsheet";//OpenDocument spreadsheet document					
            case ".ODT": return "application/vnd.oasis.opendocument.text";//OpenDocument text document							
            case ".OGA": return "audio/ogg";//OGG audio											
            case ".OGV": return "video/ogg";//OGG video											
            case ".OGX": return "application/ogg";//OGG													
            case ".OPUS": return "audio/opus";//Opus audio											
            case ".OTF": return "font/otf";//OpenType font										
            case ".PNG": return "image/png";//Portable Network Graphics							
            case ".PDF": return "application/pdf";//Adobe Portable Document Format (PDF)				
            case ".PHP": return "application/x-httpd-php";//Hypertext Preprocessor (Personal Home Page)			
            case ".PPT": return "application/vnd.ms-powerpoint";//Microsoft PowerPoint								
            case ".PPTX": return "application/vnd.openxmlformats-officedocument.presentationml.presentation";//Microsoft PowerPoint (OpenXML)						
            case ".RAR": return "application/vnd.rar";//RAR archive											
            case ".RTF": return "application/rtf";//Rich Text Format (RTF)								
            case ".SH": return "application/x-sh";//Bourne shell script									
            case ".SVG": return "image/svg+xml";//Scalable Vector Graphics (SVG)						
            case ".TAR": return "application/x-tar";//Tape Archive (TAR)									
            case ".TIF": return "image/tiff";//Tagged Image File Format (TIFF)						
            case ".TIFF": return "image/tiff";//Tagged Image File Format (TIFF)						
            case ".TS": return "video/mp2t";//MPEG transport stream								
            case ".TTF": return "font/ttf";//TrueType Font										
            case ".TXT": return "text/plain";//Text, (generally ASCII or ISO 8859-n)				
            case ".VSD": return "application/vnd.visio";//Microsoft Visio										
            case ".WAV": return "audio/wav";//Waveform Audio Format								
            case ".WEBA": return "audio/webm";//WEBM audio											
            case ".WEBM": return "video/webm";//WEBM video											
            case ".WEBP": return "image/webp";//WEBP image											
            case ".WOFF": return "font/woff";//Web Open Font Format (WOFF)							
            case ".WOFF2": return "font/woff2";//Web Open Font Format (WOFF)							
            case ".XHTML": return "application/xhtml+xml";//XHTML												
            case ".XLS": return "application/vnd.ms-excel";//Microsoft Excel										
            case ".XLSX": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";//Microsoft Excel (OpenXML)							
            case ".XML": return "application/xml";//XML													
            case ".XUL": return "application/vnd.mozilla.xul+xml";//XUL													
            case ".ZIP": return "application/zip";//ZIP archive											
            case ".3GP": return "video/3gpp; audio/3gpp if it doesn't contain video";//3GPP audio/video container							
            case ".3G2": return "video/3gpp2; audio/3gpp2 if it doesn't contain video";//3GPP2 audio/video container							
            case ".7Z": return "application/x-7z-compressed";//7-zip archive			
            default: return "";
        }

    }

    public static void StartServer()
    {

        Encoding encoder = Encoding.UTF8;

        eTipoReqHTTP TipoReq = eTipoReqHTTP.DESCONHECIDO;
        eStatusHTTP SttusHTTP = eStatusHTTP.DESCONHECIDO;
        eTipoArquivo tipoArquivo = eTipoArquivo.DESCONHECIDO;

        IPAddress ipAddress;
        IPEndPoint localEndPoint;

        int PortaServico = 8181;
        int ReqN = 0;
        int MaxConSimultaneas = 10;

        string strContent = "text/html";
        string strDataHora = "Thu, 09 Dec 2004 12:07:48 GMT";
        int tam_conteudo = 0;
        Dictionary<string, string> Cabecalhos;

        string arquivo_solicitado = "";
        string conteudo_arquivo_texto = "";
        string dado_recebido = string.Empty;
        string[] data_v = { string.Empty };
        string dado_retorno = string.Empty;
        FileInfo oFileInfo;
        //---------------------------------------------------------
        byte[] msg_resp = { };
        byte[] msg_resp_texto = { };
        byte[] msg_resp_binario = { };


        byte[] bytes = { };
        int bytesRec = 0;

        try
        {

            DesenhaLogo();

            ROOT_SERVIDOR = ObterConfig("ROOT_SERVIDOR");
            DESCRICAO_SERVIDOR = ObterConfig("DESCRICAO_SERVIDOR") + " (" + VERSAO + ")"; //"IBM_CICS_Transaction_Server / 3.1.0(zOS)";
            ARQUIVO_PADRAO = ObterConfig("ARQUIVO_PADRAO");
            CACHE_MAX_AGE = ObterConfig("CACHE_MAX_AGE");


            Console.WriteLine($"Nome Servidor..............: [{DESCRICAO_SERVIDOR}]");
            Console.WriteLine($"Caminho raiz servidor......: [{ROOT_SERVIDOR}]");
            Console.WriteLine($"Arquivo raiz padrão........: [{ARQUIVO_PADRAO}]");
            Console.WriteLine($"Tempo Cache (seg.).........: [{CACHE_MAX_AGE}]");


            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses

            Console.Write("Obtendo endereço(s) rede...:");
            IPHostEntry host = Dns.GetHostEntry("localhost");
            Console.Write(" [" + host.AddressList.Count().ToString() + "]");

            ipAddress = host.AddressList[0];

            foreach (IPAddress address in host.AddressList)
            {
                Console.Write(" - IP: [" + address.ToString() + "]"); ;
                if (address.ToString().Contains("127."))
                {
                    ipAddress = address;
                }
            }

            Console.WriteLine(""); ;

            Console.WriteLine($"Utilizando IP..............: [Any]");
            Console.WriteLine($"Utilizando Porta...........: [{PortaServico}] ");
            localEndPoint = new IPEndPoint(IPAddress.Any, PortaServico);



            // Create a Socket that will use Tcp protocol
            Console.Write($"Criando 'Listener'.........: ");
            Socket listener = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine($"[OK]");

            // A Socket must be associated with an endpoint using the Bind method
            Console.Write($"Conectando 'Listener'......: "); // para o IP e Porta configurados.'");
            listener.Bind(localEndPoint);
            Console.WriteLine($"[OK]");

            // Specify how many requests a Socket can listen before it gives Server busy response.
            // We will listen 10 requests at a time
            Console.Write($"Listener Ativado...........: ");
            listener.Listen(MaxConSimultaneas);
            Console.WriteLine($"[OK] - {MaxConSimultaneas.ToString()} conexoes simultaneas.");


            while (true)
            {
                ReqN += 1;
                Console.WriteLine($"- {ReqN.ToString()} -- Aguardando conexão #########################################################################");
                Socket handler = listener.Accept();
                oReloginho = new Stopwatch();
                oReloginho.Start();
                // Incoming data from the client.
                //while (true)
                //{
                dado_recebido = string.Empty;
                dado_retorno = string.Empty;
                bytes = new byte[1024];
                bytesRec = handler.Receive(bytes);
                dado_recebido = encoder.GetString(bytes, 0, bytesRec);

                arquivo_solicitado = "";
                conteudo_arquivo_texto = "";

                data_v = Array.Empty<string>();
                msg_resp = Array.Empty<byte>();
                msg_resp_texto = Array.Empty<byte>();
                msg_resp_binario = Array.Empty<byte>();


                strDataHora = DateTime.Now.ToString("dddd", CultureInfo.CreateSpecificCulture("en-US")).Substring(0, 3) +
                              DateTime.Now.ToString(", dd MMM yyyy mm:HH:ss", CultureInfo.CreateSpecificCulture("en-US")) + " GMT";

                strContent = "text/plain";
                tam_conteudo = 0;

                Console.WriteLine($"RECEBEU: <<<<<<<<<< <<<<<<<<<< <<<<<<<<<< <<<<<<<<<<\n[{dado_recebido}]");

                dado_recebido = dado_recebido.Replace("\r", ""); //Retira os \r (\n = CR (Carriage Return) // Used as a new line character in Unix)
                data_v = dado_recebido.Split("\n");

                if (data_v[0].Contains(HTTP_VER) == false)
                {
                    SttusHTTP = eStatusHTTP.Bad_Request;
                    goto ENVIO_RESPOSTA;
                }

                TipoReq = ObterTipoReq(data_v[0]);


                if (TipoReq == eTipoReqHTTP.DESCONHECIDO)
                {
                    SttusHTTP = eStatusHTTP.Method_Not_Allowed;
                    goto ENVIO_RESPOSTA;
                }

                arquivo_solicitado = ObterArquivoSolicitado(data_v[0]);

                if (arquivo_solicitado == "")
                {
                    SttusHTTP = eStatusHTTP.Precondition_Failed;
                    goto ENVIO_RESPOSTA;
                }

                Cabecalhos = ProcessaCabecalhos(dado_recebido);



                //1 - verifica se eh GET
                if (TipoReq == eTipoReqHTTP.GET || TipoReq == eTipoReqHTTP.POST)
                {

                    arquivo_solicitado = ROOT_SERVIDOR + arquivo_solicitado;
                    oFileInfo = new FileInfo(arquivo_solicitado);
                    strContent = ObterMime(oFileInfo.Extension);
                    tipoArquivo = ObterTipoArquivo(strContent);

                    if (oFileInfo.Name == "TratarRegistro.json")
                    {
                        SttusHTTP = eStatusHTTP.OK;
                        strContent = ObterMime(oFileInfo.Extension);
                        //conteudo_arquivo_texto = "[{'name':'Nicolas1','age':41},{'name':'Nicolas2','age':42},{'name':'Nicolas3','age':43},{'name':'Nicolas4','age':44}]";
                        //conteudo_arquivo_texto = "{\"success\":\"true\"}";
                        conteudo_arquivo_texto = "[{\"name\":\"Nicolas1\",\"age\":41},{\"name\":\"Nicolas2\",\"age\":42},{\"name\":\"Nicolas3\",\"age\":43},{\"name\":\"Nicolas4\",\"age\":44}]";


                        goto ENVIO_RESPOSTA;
                    }

                    if (oFileInfo.Exists == false)
                    {
                        SttusHTTP = eStatusHTTP.Not_Found;
                        goto ENVIO_RESPOSTA;
                    }


                    //-------------------------------------------
                    SttusHTTP = eStatusHTTP.OK;
                    //-------------------------------------------
                    if (tipoArquivo == eTipoArquivo.TEXTO)
                    {
                        conteudo_arquivo_texto = File.ReadAllText(arquivo_solicitado, Encoding.Default);
                        goto ENVIO_RESPOSTA;
                    }
                    else if (tipoArquivo == eTipoArquivo.IMAGEM)
                    {
                        msg_resp_binario = File.ReadAllBytes(arquivo_solicitado);
                        goto ENVIO_RESPOSTA;
                    }
                    else if (tipoArquivo == eTipoArquivo.FONTE)
                    {
                        msg_resp_binario = File.ReadAllBytes(arquivo_solicitado);
                        goto ENVIO_RESPOSTA;
                    }
                    else
                    {
                        SttusHTTP = eStatusHTTP.Bad_Request;
                        conteudo_arquivo_texto = "???";
                    }

                    //byte[] binaryImage = File.ReadAllBytes(arquivo_solicitado);
                    //conteudo_arquivo_solicitado = conteudo_arquivo_solicitado.ToString();

                }
                else
                {
                    SttusHTTP = eStatusHTTP.Im_a_teapot;
                    conteudo_arquivo_texto = TipoReq.ToString() + " - Ainda nao implementado!";
                    goto ENVIO_RESPOSTA;
                }

            //2 - le arquivo

            //3 monta classe retorno com conteudo do arquivo

            //4 retorna 
            //headers["GET"]
            //WebHeaderCollection headers = new WebHeaderCollection(). .Parse(data);

            ENVIO_RESPOSTA:

                if (conteudo_arquivo_texto.Length > 0)
                {
                    //if (tipoArquivo == eTipoArquivo.TEXTO)
                    //{
                    //    tam_conteudo = conteudo_arquivo_texto.Length + 2;
                    //}
                    //else
                    //{
                    tam_conteudo = conteudo_arquivo_texto.Length;
                    //}
                }
                else if (msg_resp_binario.Length > 0)
                {
                    tam_conteudo = msg_resp_binario.Length;
                }

                //data = "HTTP/1.1 404 OK\nDate: Thu, 09 Dec 2004 12:07 : 48 GMT\nServer: IBM_CICS_Transaction_Server / 3.1.0(zOS)";
                dado_retorno = $"{HTTP_VER} {((int)SttusHTTP).ToString()} {SttusHTTP.ToString()}\n" +
                               $"Date: {strDataHora}\n" +
                               $"Server: {DESCRICAO_SERVIDOR}\n" +
                               $"Content-type: {strContent}\n" +
                               $"Cache-Control: public, max-age={CACHE_MAX_AGE}\n" +
                               $"Content-Length: {tam_conteudo}\n\n";
                //Set-Cookie:bbscoito=gostso2

                Console.WriteLine($"------------------------------------------------------------------------------------------------");
                Console.WriteLine($"ENVIOU (CONTEÚDO OMITIDO): >>> >>>>>>>>>> >>>>>>>>>>\n[{dado_retorno}]");

                msg_resp = encoder.GetBytes(dado_retorno);

                if (conteudo_arquivo_texto.Length > 0)
                {
                    msg_resp_texto = encoder.GetBytes(conteudo_arquivo_texto);
                    msg_resp = msg_resp.Concat(msg_resp_texto).ToArray();
                }

                if (msg_resp_binario.Length > 0)
                {
                    msg_resp = msg_resp.Concat(msg_resp_binario).ToArray();
                }

                handler.Send(msg_resp);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                Console.WriteLine($"################################################################################################### Tempo de resposta: [{(decimal.Parse(oReloginho.ElapsedMilliseconds.ToString()) / 1000).ToString("0.####")}] segundos");

                oReloginho.Stop();

            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }

    private static void DesenhaLogo()
    {

        int seed = 42;
        Random random = new Random(seed);

        // Gerando números aleatórios usando a instância com a semente
        int numeroAleatorio1 = random.Next(1, 5);

        //numeroAleatorio1 = 99; teste

        if (numeroAleatorio1 == 1)
        {
            Console.WriteLine($@"==========================================================================");
            Console.WriteLine($@" __ _                     __    __     _     __                          ");
            Console.WriteLine($@"/ _\ |__   __ _ _ __ _ __/ / /\ \ \___| |__ / _\ ___ _ ____   _____ _ __ ");
            Console.WriteLine($@"\ \| '_ \ / _` | '__| '_ \ \/  \/ / _ \ '_ \\ \ / _ \ '__\ \ / / _ \ '__|");
            Console.WriteLine($@"_\ \ | | | (_| | |  | |_) \  /\  /  __/ |_) |\ \  __/ |   \ V /  __/ |   ");
            Console.WriteLine($@"\__/_| |_|\__,_|_|  | .__/ \/  \/ \___|_.__/\__/\___|_|    \_/ \___|_|   ");
            Console.WriteLine($@"                    |_|                                             v.{VERSAO}");
            Console.WriteLine($@"==========================================================================");
            return;
        }

        if (numeroAleatorio1 == 2)
        {
            Console.WriteLine($@"=================================================================================");
            Console.WriteLine($@"   _____ __                   _       __     __   _____                          ");
            Console.WriteLine($@"  / ___// /_  ____ __________| |     / /__  / /_ / ___/___  ______   _____  _____");
            Console.WriteLine($@"  \__ \/ __ \/ __ `/ ___/ __ \ | /| / / _ \/ __ \\__ \/ _ \/ ___/ | / / _ \/ ___/");
            Console.WriteLine($@" ___/ / / / / /_/ / /  / /_/ / |/ |/ /  __/ /_/ /__/ /  __/ /   | |/ /  __/ /    ");
            Console.WriteLine($@"/____/_/ /_/\__,_/_/  / .___/|__/|__/\___/_.___/____/\___/_/    |___/\___/_/     ");
            Console.WriteLine($@"                     /_/                                                   v.{VERSAO} ");
            Console.WriteLine($@"=================================================================================");
            return;
        }

        if (numeroAleatorio1 == 3)
        {

            Console.WriteLine($@"===============================================================================");
            Console.WriteLine($@"  ____  _                   __        __   _    ____                           ");
            Console.WriteLine($@" / ___|| |__   __ _ _ __ _ _\ \      / /__| |__/ ___|  ___ _ ____   _____ _ __ ");
            Console.WriteLine($@" \___ \| '_ \ / _` | '__| '_ \ \ /\ / / _ \ '_ \___ \ / _ \ '__\ \ / / _ \ '__|");
            Console.WriteLine($@"  ___) | | | | (_| | |  | |_) \ V  V /  __/ |_) |__) |  __/ |   \ V /  __/ |   ");
            Console.WriteLine($@" |____/|_| |_|\__,_|_|  | .__/ \_/\_/ \___|_.__/____/ \___|_|    \_/ \___|_|   ");
            Console.WriteLine($@"                        |_|                                              v.{VERSAO}");
            Console.WriteLine($@"===============================================================================");
            return;
        }

        if (numeroAleatorio1 == 4)
        {
            Console.WriteLine($@"========================================================================");
            Console.WriteLine($@" ___  _                   _ _ _       _    ___                          ");
            Console.WriteLine($@"/ __>| |_  ___  _ _  ___ | | | | ___ | |_ / __> ___  _ _  _ _  ___  _ _ ");
            Console.WriteLine($@"\__ \| . |<_> || '_>| . \| | | |/ ._>| . \\__ \/ ._>| '_>| | |/ ._>| '_>");
            Console.WriteLine($@"<___/|_|_|<___||_|  |  _/|__/_/ \___.|___/<___/\___.|_|  |__/ \___.|_|  ");
            Console.WriteLine($@"                    |_|                                           v.{VERSAO} ");
            Console.WriteLine($@"========================================================================");
            return;
        }

        Console.WriteLine($"===================");
        Console.WriteLine($"SharpWebServer {VERSAO}");
        Console.WriteLine($"===================");


    }

    private static string ObterArquivoSolicitado(string _strReq)
    {
        string[] strParte;

        strParte = _strReq.Split(" ");

        if (strParte.Count() != 3)
        {
            return "";
        }

        if (strParte[1] == "/")
        {
            strParte[1] = "/" + ARQUIVO_PADRAO;
        }

        return strParte[1];

    }

    private static eTipoReqHTTP ObterTipoReq(string _data)
    {
        if (_data.Substring(0, 3).ToUpper() == "GET") return eTipoReqHTTP.GET;
        else if (_data.Substring(0, 4).ToUpper() == "POST") return eTipoReqHTTP.POST;
        else if (_data.Substring(0, 3).ToUpper() == "PUT") return eTipoReqHTTP.PUT;
        else if (_data.Substring(0, 5).ToUpper() == "PATCH") return eTipoReqHTTP.PATCH;
        else if (_data.Substring(0, 6).ToUpper() == "DELETE") return eTipoReqHTTP.DELETE;
        else if (_data.Substring(0, 4).ToUpper() == "HEAD") return eTipoReqHTTP.HEAD;
        else if (_data.Substring(0, 7).ToUpper() == "OPTIONS") return eTipoReqHTTP.OPTIONS;
        else return eTipoReqHTTP.DESCONHECIDO;
    }

    private static eTipoArquivo ObterTipoArquivo(string _strContent)
    {

        if (_strContent.ToUpper().Trim().Contains("TEXT/")) return eTipoArquivo.TEXTO;
        if (_strContent.ToUpper().Trim().Contains("IMAGE/")) return eTipoArquivo.IMAGEM;
        if (_strContent.ToUpper().Trim().Contains("AUDIO/")) return eTipoArquivo.AUDIO;
        if (_strContent.ToUpper().Trim().Contains("VIDEO/")) return eTipoArquivo.VIDEO;
        if (_strContent.ToUpper().Trim().Contains("FONT/")) return eTipoArquivo.FONTE;
        if (_strContent.ToUpper().Trim().Contains("APPLICATION/")) return eTipoArquivo.APLICACAO;

        return eTipoArquivo.DESCONHECIDO;
    }

    static Dictionary<string, string> ProcessaCabecalhos(string headerString)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Divide a string de cabeçalho em linhas
        string[] headerLines = headerString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        // Para cada linha, divide a chave e o valor
        foreach (string line in headerLines)
        {

            int separatorIndex = line.IndexOf(':');

            if (separatorIndex > 0)
            {
                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();

                headers[key] = value;
            }
        }

        return headers;
    }


    public static string ObterConfig(string chave)
    {
        XmlDocument xmlConfig = new XmlDocument();
        string strPath;

        try
        {
            strPath = ""; // HttpContext.Current.Server.MapPath("~");

            xmlConfig = new XmlDocument();
            xmlConfig.Load(Path.Combine(strPath, ARQUIVO_DATABASE_CONFIG));

#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
            return xmlConfig.DocumentElement.SelectSingleNode("//CONFIG").SelectSingleNode("//" + chave).Attributes["valor"].InnerText.ToString();
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.

            // Se estiver usando configurações de aplicativo (app settings) em vez de um arquivo XML
            // return System.Configuration.ConfigurationManager.AppSettings[chave];
        }
        catch (Exception ex)
        {
            LogaErro("Erro em " + NomeMetodo("Util") + ": " + ex.Message);
            return "";
        }
    }

    private static void LogaErro(string mensagem)
    {
        // Implemente a lógica para registrar o erro, por exemplo, usando log ou impressão no console
        Console.WriteLine(mensagem);
    }

    private static string NomeMetodo(string classe)
    {
        // Implemente a lógica para obter o nome do método, por exemplo, usando reflexão
        return "NomeDoMetodo"; // Substitua pelo nome correto do método
    }



}