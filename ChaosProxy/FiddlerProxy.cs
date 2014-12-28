using System;
using System.Threading;
using Fiddler;

namespace ChaosProxy
{
    public class FiddlerProxy
    {
        private static string _serviceHostName, _siteUrl;
        private readonly int _port;

        private readonly SessionStateHandler _sessionHandler = delegate(Session oSession)
        {
            if (VerifyVerbAndHostname(oSession, _serviceHostName) && ChaosEngine.ReturnError())
            {
                oSession.utilCreateResponseAndBypassServer();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(oSession.fullUrl + " ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ChaosEngine.StatusCode);

                oSession.oResponse.headers["Access-Control-Allow-Origin"] = _siteUrl;
                oSession.oResponse.headers.HTTPResponseCode = ChaosEngine.StatusCode;
                oSession.responseCode = ChaosEngine.StatusCode;

                Console.ResetColor();
            }
        };

        public FiddlerProxy(Site exSite, Site site, int portNumber = 0)
        {
            _serviceHostName = exSite.Hostname;
            _siteUrl = site.Url;
            _port = portNumber;
        }

        public void StartFiddler()
        {
            FiddlerApplication.Startup(_port, FiddlerCoreStartupFlags.Default);
            FiddlerApplication.BeforeRequest += _sessionHandler;
        }

        public void StopFiddler()
        {
            FiddlerApplication.BeforeRequest -= _sessionHandler;
            FiddlerApplication.Shutdown();
            //Give the proxy some time to end
            Thread.Sleep(500);
        }

        private static bool VerifyVerbAndHostname(Session oSession, string serviceHostName)
        {
            return !oSession.HTTPMethodIs("CONNECT") && !oSession.HTTPMethodIs("OPTIONS") &&
                   oSession.HostnameIs(serviceHostName);
        }
    }
}