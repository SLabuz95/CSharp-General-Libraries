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
}
