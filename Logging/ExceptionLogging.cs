using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace CSharpGeneralLibraries.ExceptionLogging
{
    public class ExceptionLogging
    {
        private int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
        public ExceptionLogging(Application application) {        
            SetupExceptionHandling(application);
        }

        // Default Session ID - created on app start
        // When session has not been properly established or informations related to current session are missing

        // Pass current session informations object
        // To provide addional session informations

        //public class SessionIdentifierElementDefinitionBase(string name, Func<Exception, string> processingFunc)
        //    : Tuple<string, Func<Exception, string>>(name, processingFunc) 
        //{
        //    private string Name { get { return Item1; } }
        //    private Func<Exception , string> ProcessingFunc { get { return Item2; } }
        //    public SessionIdentifierElement GetIdentifierElement(Exception exception)
        //    {
        //        return new SessionIdentifierElement(Name, ProcessingFunc(exception));
        //    }
        //}
        //public class SessionIdentifierElementDefinition(string name, Func<Exception, string> processingFunc)
        //    : SessionIdentifierElementDefinitionBase(name, processingFunc)
        //{
        //    private string Name { get { return Item1; } }
        //    private Func<Exception, string> ProcessingFunc { get { return Item2; } }
        //    public SessionIdentifierElement GetIdentifierElement(Exception exception)
        //    {
        //        return new SessionIdentifierElement(Name, ProcessingFunc(exception));
        //    }
        //}
        //private static List<SessionIdentifierElementDefinition> sessionIdentifierDefinitions =
        //    new ();
        //public class SessionIdentifierElement(string name, string value) : Tuple<string, string>(name, value)
        //{
        //    public string Name { get { return Item1; } }
        //    public string Value { get { return Item2; } }
        //}
        //public class SessionIdentifier(IEnumerable<SessionIdentifierElement> elements) : List<SessionIdentifierElement>(elements)
        //{
        //}
        //public class SessionIdentifiers(IEnumerable<SessionIdentifier> identifiers) : List<SessionIdentifier>(identifiers)
        //{
        //    public SessionIdentifiers() : this(new List<SessionIdentifier>()) { }
        //}
        //private static SessionIdentifier PrepareIdentifier(Exception exception)
        //{
        //    return new SessionIdentifier(sessionIdentifierDefinitions.Select(x => x.GetIdentifierElement(exception)));
        //}
        //public static bool Initialized { get; private set; } = false;
        //public static void Initialize()
        //{
            

        //    Initialized = true;
        //}


        public static void LogException(Exception exception)
        {
            // 1. Get identifier (PrepareIdentifier)
            //var Identifier = PrepareIdentifier(exception);
            // 2. Find identifier in IdentifiersList
            // 2.1. Not found - register new identifier - add to List
            // 3. Add exception to log with identifier id (index of identifiers list)
        }

        [Conditional("DEBUG")]
        [Conditional("RELEASE")]
        public void SetupExceptionHandling(Application application )
        {

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
                LogHandledException((Exception)e.Exception, "AppDomain.CurrentDomain.UnhandledException");

            application.DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                //e.Handled = true;

            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                // e.SetObserved();

            };
        }
        // To view exception
        // ExceptionView is based on StackTrace
        // While preparing view, each logged exception's stack trace is compared to base trace list
        // If not found then add as new
        // If found, check extended trace list contains base reference, possible extended trace list reference (or base on reference type),
        // start and end indexes where it was detected and additional trace (question if its prefix or postfix)
        // Based on the reference each unique exception view is created and clasified

        static Exception hexception;

        private string GetHandledExceptionLogElement(Exception exception, bool rethrownException, string source)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            return
                new XElement(
                    rethrownException ? "rethrownException" : "exception",
                    new XAttribute("timestamp", DateTime.Now),
                    new XAttribute("source", source),
                    new XAttribute("assemblyName",
                                    assembly == null ? "Null" :
                                    String.Format("{0} v{1}", assembly.Name, assembly.Version)),
                    new XAttribute("message", exception.Message),
                    new XAttribute("stackTrace", exception.StackTrace ?? "Null")
                ).ToString();
        }
        private string GetUnhandledExceptionLogElement(Exception exception, bool rethrownException, string source)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            return
                new XElement(
                    rethrownException ? "rethrownException" : "exception",
                    new XAttribute("timestamp", DateTime.Now),
                    new XAttribute("type", exception.GetType().Name),
                    new XAttribute("source", source),
                    new XAttribute("assemblyName",
                                    assembly == null ? "Null" :
                                    String.Format("{0} v{1}", assembly.Name, assembly.Version)),
                    new XAttribute("message", exception.Message),
                    new XAttribute("stackTrace", exception.StackTrace ?? "Null")
                ).ToString();
        }

        private void LogHandledException(Exception exception, string source)
        {
            hexception = exception;
        }
        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = File.CreateText("newfile.txt"))
                {
                    writer.WriteLine("Exception in LogUnhandledException");
                }
            }
            finally
            {
                using (TextWriter writer = File.AppendText("exceptions.xml"))
                {
                    writer.WriteLine(GetUnhandledExceptionLogElement(exception, false, source));
                }
            }
        }
    }
public class ExceptionLogger
{
    bool processingException = false;
    private static readonly string EXCEPTIONS_FILE_NAME = "exceptions.xml";
    private static readonly string EXCEPTIONS_DATA_FILE_NAME = "exceptionsData.xml";
    public enum LocationType
    {
        LocalFileSystem,
        FTPServer
    };
    private class Location
    {
        private LocationType Type;
        private string DirectoryPath;
        private readonly string EXCEPTIONS_FILE_CONTENTS = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                                            <!DOCTYPE exceptions [
                                                              <!ENTITY exceptionsData SYSTEM ""exceptionsData.xml"">
                                                            ]>
                                                            <exceptions>
                                                              &exceptionsData;
                                                            </exceptions>";
        public string ExceptionsPath => DirectoryPath + Path.DirectorySeparatorChar + EXCEPTIONS_FILE_NAME;
        public string ExceptionsDataPath => DirectoryPath + Path.DirectorySeparatorChar + EXCEPTIONS_DATA_FILE_NAME;
        public Location(LocationType type, DirectoryInfo directoryPath) {
            Type = type;
            DirectoryPath = directoryPath.FullName;
        }
        public bool CheckIfItsPossibleToConnect()
        {
            bool serverAvailable = false;
            switch (Type)
            {
                case LocationType.FTPServer:
                {
                    FtpClient client = null;
                    string serverId = fileServer;
                    string serverUser = fileServerUser;
                    string serverPassword = fileUserPass;
                    try
                    {
                        if (client == null)
                            client = new FtpClient(serverId);
                        client.Credentials = new NetworkCredential(serverUser, serverPassword);
                        if (client.IsConnected == false)
                            client.Connect();
                        client.Dispose();
                        serverAvailable = true;
                    }
                    catch (Exception ex)
                    {// Failed to connect or something else
                        MainWindowViewModel.log.Error(String.Format("Failed to connect to FTP server to log exceptions.{0}\n{1}", ex.Message, ex.StackTrace));
                    }
                    finally
                    {
                        //context.Leave();//mfa
                        if (client != null)
                            client.Dispose();
                    }

                    }
                    break;
                default:
                    serverAvailable = true;
                    break;
            }
            return serverAvailable;
        }
        public FtpClient CreateLocationIfNotExistsKeepClientConnected()
        {
            FtpClient client = null;
            CreateLocationIfNotExists(ref client, true);
            return client;
        }
        public void CreateLocationIfNotExists()
        {
            FtpClient client = null;
            CreateLocationIfNotExists(ref client);
            if (client != null)
            {
                client.Dispose();
            }
        }
        private void CreateLocationIfNotExists(ref FtpClient client, bool keepConnected = false)
        {
            switch (Type)
            {
                case LocationType.LocalFileSystem:
                {
                    if (!Directory.Exists(DirectoryPath))
                    {
                        Directory.CreateDirectory(DirectoryPath);
                    }
                    if (!File.Exists(ExceptionsPath))
                    {
                        File.WriteAllText(ExceptionsPath, EXCEPTIONS_FILE_CONTENTS);
                    }
                    if (!File.Exists(ExceptionsDataPath))
                    {
                        using (File.Create(ExceptionsDataPath)) { };
                    }
                }
                break;
                case LocationType.FTPServer:
                {
                    string serverId = fileServer;
                    string serverUser = fileServerUser;
                    string serverPassword = fileUserPass;
                    try
                        {
                        if (client == null)
                            client = new FtpClient(serverId);
                        client.Credentials = new NetworkCredential(serverUser, serverPassword);
                        if (client.IsConnected == false)
                            client.Connect();
                        if (!client.DirectoryExists(DirectoryPath))
                        {
                            client.CreateDirectory(DirectoryPath);
                        }
                        if (!client.FileExists(ExceptionsPath))
                        {
                            using (var exceptionsFileStream = client.OpenWrite(ExceptionsPath, FtpDataType.ASCII))
                            {
                                StreamWriter writer = new StreamWriter(exceptionsFileStream);
                                writer.Write(EXCEPTIONS_FILE_CONTENTS);
                            }
                        }
                        if (!client.FileExists(ExceptionsDataPath))
                        {
                            using (var exceptionsDataFileStream = client.OpenWrite(ExceptionsDataPath, FtpDataType.ASCII)) { }
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        //context.Leave();//mfa
                        if(client != null && keepConnected == false)
                            client.Disconnect();
                    }                          
                }
                break;
            }
        }
        public void ClearLocation()
        {
            switch (Type)
            {
                case LocationType.LocalFileSystem:
                {
                    if (File.Exists(ExceptionsPath))
                    {
                        File.Delete(ExceptionsPath);
                    }
                    if (File.Exists(ExceptionsDataPath))
                    {
                        File.Delete(ExceptionsDataPath);
                    }
                }
                break;
                case LocationType.FTPServer:
                {
                    string serverId = fileServer;
                    string serverUser = fileServerUser;
                    string serverPassword = fileUserPass;
                    FtpClient client = null;
                    try
                    {
                        client = new FtpClient(serverId);
                        client.Credentials = new NetworkCredential(serverUser, serverPassword);
                        client.Connect();
                        if (client.FileExists(ExceptionsPath))
                        {
                            client.DeleteFile(ExceptionsPath);
                        }
                        if (client.FileExists(ExceptionsDataPath))
                        {
                            client.DeleteFile(ExceptionsDataPath);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        if (client != null)
                            client.Dispose();
                    }
                }
                break;
            }
        }
        public void SaveDataToLocation(string data)
        {
            int MaxAttemptsNumber = 3;
            int AttemptTimeout = 10; // [ms]
            FtpClient client = null;
            int attemptsCounter = 0;
            bool exceptionCatched = true;
            while (exceptionCatched == true)
            {
                exceptionCatched = false;
                try
                {
                    client = CreateLocationIfNotExistsKeepClientConnected();
                    switch (Type)
                    {
                        case LocationType.LocalFileSystem:
                            {
                                //// vvv Use below code to debug for blocked file
                                //using (var stream = File.OpenWrite(ExceptionsDataPath))
                                //{
                                //    Console.WriteLine();
                                //}
                                File.AppendAllText(ExceptionsDataPath, (data != null ? data : String.Empty) + "\n");
                            }
                            break;
                        case LocationType.FTPServer:
                            {
                                string serverId = fileServer;
                                string serverUser = fileServerUser;
                                string serverPassword = fileUserPass;
                                if (client == null)
                                    client = new FtpClient(serverId);
                                client.Credentials = new NetworkCredential(serverUser, serverPassword);
                                if(client.IsConnected == false)
                                    client.Connect();
                                using (var exceptionsDataFileStream = client.OpenAppend(ExceptionsDataPath, FtpDataType.ASCII)) {
                                    StreamWriter writer = new StreamWriter(exceptionsDataFileStream);

                                    writer.WriteLine(data != null ? data : String.Empty);
                                    writer.Flush();
                                }
                            }
                            break;
                    }
                }
                catch (Exception)
                {
                    exceptionCatched = true;
                    if (attemptsCounter == MaxAttemptsNumber)
                        throw;
                    attemptsCounter++;
                    System.Threading.Thread.Sleep(AttemptTimeout);
                }
                finally
                {
                    if (client != null && (exceptionCatched == false || attemptsCounter == MaxAttemptsNumber)){                            
                        client.Dispose();
                    }
                }
            }
        }
    }
    // Primary location
    private Location primaryLocation;
    // In case of error on primary location
    private readonly Location temporaryLocation = new Location(LocationType.LocalFileSystem, new DirectoryInfo( Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
    private Location currentLocation;

    private int ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
    private string AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    private static string fileServer = String.Empty;
    private static string fileServerUser = String.Empty;
    private static string fileUserPass = String.Empty;
    public ExceptionLogger(Application application, LocationType type, string path) { 
        lock (this)
        {
            // 0. Get server id
            fileServer = ConfigurationManager.AppSettings["file_server"];
            //if (fileServer.Equals("itvdbsrv") || fileServer.Equals("10.224.188.100"))
            //{
            //    fileServerUser = @"phase";
            //    fileUserPass = @"Ph@se1";
            //}
            //else

            // Temp force connection to dlclstyr2
            fileServer = "dlclstyr2";
            {
                fileServerUser = @"LogUpdater";
                fileUserPass = @"testUpdate123";
            }
            // 1. Setup location
            primaryLocation = new Location(type, new DirectoryInfo(path));
            // 1.0. Check if primary location is available - in case of no connection to primary location (mainly FTP server)
            if (primaryLocation.CheckIfItsPossibleToConnect() == true)
            {
                // 1.1 by default current location is primary. it changes to temporary on an exception
                currentLocation = primaryLocation;
                // 1.2 try to create locations
                primaryLocation.CreateLocationIfNotExists();
                temporaryLocation.CreateLocationIfNotExists();
                // 2. Move exceptions from temporary location to primary (if any exceptions exist and primary location is available)
                MoveExceptionsDataFromTemporaryToPrimaryLocation();
            }
            else
            {
                currentLocation = temporaryLocation;
                // 1.2 try to create locations
                temporaryLocation.CreateLocationIfNotExists();
            }
        }
        // 3. Setup exception handling
        SetupExceptionHandling(application);
    }

    private string ReadExceptionsDataFromTemporaryLocation()
    {
        if (File.Exists(temporaryLocation.ExceptionsDataPath))
        {
            using (TextReader reader = new StreamReader(temporaryLocation.ExceptionsDataPath))
            { return reader.ReadToEnd(); }
        }
        return string.Empty;
    }
      

    private void MoveExceptionsDataFromTemporaryToPrimaryLocation()
    {
        string exceptionsData = String.Empty;
        bool exceptionWhileSaving = false;
        try
        {
            // 1 read data from temporary location
            exceptionsData = ReadExceptionsDataFromTemporaryLocation();
        } catch (Exception ex) {
            MainWindowViewModel.log.Error(String.Format("Failed to get exceptions from temporary location.{0}\n{1}", ex.Message, ex.StackTrace));
        }

        if (string.IsNullOrEmpty(exceptionsData)) // Nothing to save or failed to get data
            return;
        
        // 3 try to save the data to primary location
        try 
        {
            primaryLocation.SaveDataToLocation(exceptionsData);                
        }
        catch (Exception ex)
        {
            currentLocation = temporaryLocation;
            MainWindowViewModel.log.Error(String.Format("Failed to save exceptions from temporary location in primary location.{0}\n{1}", ex.Message, ex.StackTrace));
            exceptionWhileSaving = true;
        }
        try
        {
            if (exceptionWhileSaving == false)
            {
                temporaryLocation.ClearLocation();
            }
        }
        catch (Exception ex)
        {
            MainWindowViewModel.log.Error(String.Format("Failed to clear exceptions in temporary location.{0}\n{1}", ex.Message, ex.StackTrace));
        }
    }

    // Default Session ID - created on app start
    // When session has not been properly established or informations related to current session are missing

    // Pass current session informations object
    // To provide addional session informations

    //public class SessionIdentifierElementDefinitionBase(string name, Func<Exception, string> processingFunc)
    //    : Tuple<string, Func<Exception, string>>(name, processingFunc) 
    //{
    //    private string Name { get { return Item1; } }
    //    private Func<Exception , string> ProcessingFunc { get { return Item2; } }
    //    public SessionIdentifierElement GetIdentifierElement(Exception exception)
    //    {
    //        return new SessionIdentifierElement(Name, ProcessingFunc(exception));
    //    }
    //}
    //public class SessionIdentifierElementDefinition(string name, Func<Exception, string> processingFunc)
    //    : SessionIdentifierElementDefinitionBase(name, processingFunc)
    //{
    //    private string Name { get { return Item1; } }
    //    private Func<Exception, string> ProcessingFunc { get { return Item2; } }
    //    public SessionIdentifierElement GetIdentifierElement(Exception exception)
    //    {
    //        return new SessionIdentifierElement(Name, ProcessingFunc(exception));
    //    }
    //}
    //private static List<SessionIdentifierElementDefinition> sessionIdentifierDefinitions =
    //    new ();
    //public class SessionIdentifierElement(string name, string value) : Tuple<string, string>(name, value)
    //{
    //    public string Name { get { return Item1; } }
    //    public string Value { get { return Item2; } }
    //}
    //public class SessionIdentifier(IEnumerable<SessionIdentifierElement> elements) : List<SessionIdentifierElement>(elements)
    //{
    //}
    //public class SessionIdentifiers(IEnumerable<SessionIdentifier> identifiers) : List<SessionIdentifier>(identifiers)
    //{
    //    public SessionIdentifiers() : this(new List<SessionIdentifier>()) { }
    //}
    //private static SessionIdentifier PrepareIdentifier(Exception exception)
    //{
    //    return new SessionIdentifier(sessionIdentifierDefinitions.Select(x => x.GetIdentifierElement(exception)));
    //}
    //public static bool Initialized { get; private set; } = false;
    //public static void Initialize()
    //{
        

    //    Initialized = true;
    //}


    //public static void LogException(Exception exception)
    //{
    //    // 1. Get identifier (PrepareIdentifier)
    //    //var Identifier = PrepareIdentifier(exception);
    //    // 2. Find identifier in IdentifiersList
    //    // 2.1. Not found - register new identifier - add to List
    //    // 3. Add exception to log with identifier id (index of identifiers list)
    //}

    public void SetupExceptionHandling(Application application )
    {

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

        AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            LogException((Exception)e.Exception,  "AppDomain.CurrentDomain.FirstChance");

        application.DispatcherUnhandledException += (s, e) =>
        {
            LogException(e.Exception, "Application.Current.DispatcherUnhandledException");
            //e.Handled = true;

        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            LogException(e.Exception,  "TaskScheduler.UnobservedTaskException");
            // e.SetObserved();

        };
    }
    // To view exception
    // ExceptionView is based on StackTrace
    // While preparing view, each logged exception's stack trace is compared to base trace list
    // If not found then add as new
    // If found, check extended trace list contains base reference, possible extended trace list reference (or base on reference type),
    // start and end indexes where it was detected and additional trace (question if its prefix or postfix)
    // Based on the reference each unique exception view is created and clasified

    //static Exception hexception;

    //private string GetHandledExceptionLogElement(Exception exception, bool rethrownException, string source)
    //{
    //    var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
    //    return
    //        new XElement(
    //            rethrownException ? "rethrownException" : "exception",
    //            new XAttribute("timestamp", DateTime.Now),
    //            new XAttribute("source", source),
    //            new XAttribute("assemblyName",
    //                            assembly == null ? "Null" :
    //                            String.Format("{0} v{1}", assembly.Name, assembly.Version)),
    //            new XAttribute("message", exception.Message),
    //            new XAttribute("stackTrace", exception.StackTrace ?? "Null")
    //        ).ToString();
    //}
    //private string GetUnhandledExceptionLogElement(Exception exception, bool rethrownException, string source)
    //{
    //    var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
    //    return
    //        new XElement(
    //            rethrownException ? "rethrownException" : "exception",
    //            new XAttribute("timestamp", DateTime.Now),
    //            new XAttribute("type", exception.GetType().Name),
    //            new XAttribute("source", source),
    //            new XAttribute("assemblyName",
    //                            assembly == null ? "Null" :
    //                            String.Format("{0} v{1}", assembly.Name, assembly.Version)),
    //            new XAttribute("message", exception.Message),
    //            new XAttribute("stackTrace", exception.StackTrace ?? "Null")
    //        ).ToString();
    //}

    //private void LogHandledException(Exception exception, string source)
    //{
    //    hexception = exception;
    //}
    private XElement GetLogEntryAsElement(Exception exception, string source)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        return
            new XElement("exception",
                new XAttribute("timestamp", DateTime.Now),
                new XAttribute("source", source),
                new XAttribute("process", ProcessId.ToString()),
                new XAttribute("appVersion", AppVersion),
                new XAttribute("assemblyName",
                                assembly == null ? "Null" :
                                String.Format("{0} v{1}", assembly.Name, assembly.Version)),
                new XAttribute("message", exception.Message),
                new XAttribute("targetSite", exception.TargetSite != null ? exception.TargetSite.ToString() : "Null"),
                new XAttribute("stackTrace", exception.StackTrace ?? "Null"),
                exception.InnerException != null ? GetLogEntryAsElement(exception.InnerException, source).ToString() : null
            );
    }
    private string GetLogEntry(Exception exception, string source)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        return
            new XElement("exception",
                new XAttribute("timestamp", DateTime.Now),
                new XAttribute("source", source),
                new XAttribute("process", ProcessId.ToString()),
                new XAttribute("appVersion", AppVersion),
                new XAttribute("assemblyName",
                                assembly == null ? "Null" :
                                String.Format("{0} v{1}", assembly.Name, assembly.Version)),
                new XAttribute("message", exception.Message),
                new XAttribute("targetSite", exception.TargetSite != null? exception.TargetSite.ToString()  : "Null"),
                new XAttribute("stackTrace", exception.StackTrace ?? "Null"),
                exception.InnerException != null? GetLogEntryAsElement(exception.InnerException, source) : null
            ).ToString();
    }
    
    private void LogException(Exception exception, string source)
    {
        lock (this)
        {
            if (processingException == true) // To avoid loops
                return;
            processingException = true;
            string logEntry = String.Empty;
            bool changedLocation = true;
            while (changedLocation)
            {
                changedLocation = false;
                try
                {
                    logEntry = GetLogEntry(exception, source);
                    currentLocation.SaveDataToLocation(logEntry);

                }
                catch (Exception ex)
                {
                    if (currentLocation != temporaryLocation)
                    {
                        currentLocation = temporaryLocation;
                        changedLocation = true;
                    }
                    else
                    {
                        MainWindowViewModel.log.Error(String.Format("Failed to log exception:{0}\nDue to exception:{1}\n{2}", logEntry, ex.Message, ex.StackTrace));
                    }
                }
            }
            processingException = false;
        }
    }
}
public partial class App : Application
{
    private ExceptionLogger exceptionLogger;
    protected override void OnStartup(StartupEventArgs e)
    {            
        base.OnStartup(e);
        string path = string.Empty;mbly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "Exceptions");

        exceptionLogger = new ExceptionLogger(this, ExceptionLogger.LocationType.FTPServer,
            @"\\Space" + Path.DirectorySeparatorChar + "Exceptions");
    }
}
}
