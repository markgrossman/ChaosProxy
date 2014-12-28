using System;
using System.Configuration;

namespace ChaosProxy
{
    internal class ChaosProxy
    {
        private static FiddlerProxy _fiddle;
        private static readonly Site SiteUnderTest = new Site();
        private static readonly Site ExternalService = new Site();

        private static void Main()
        {
            Console.Clear();
            Console.WriteLine("Welcome to Chaos Proxy. CTRL C to exit");

            GetStartInfo();
            StartProxy();

            Console.CancelKeyPress += delegate
            {
                _fiddle.StopFiddler();
                Console.WriteLine("Exiting...");
            };

            var input = Console.ReadLine();

            while (true)
            {
                switch (input)
                {
                    case "cls":
                        Console.Clear();
                        break;
                    case "reset":
                        Console.Clear();
                        _fiddle.StopFiddler();
                        GetPercent();
                        GetStatusCode();
                        StartProxy();
                        ChaosEngine.RandomStatus = false;
                        break;
                }

                input = Console.ReadLine();
            }
        }

        private static void GetStartInfo()
        {
            Console.WriteLine("Which site are you testing?");
            Console.WriteLine("1: localparabotype");
            Console.WriteLine("2: devparabotype");
            Console.WriteLine("3: qaparabotype");
            Console.WriteLine("4: other");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    SiteUnderTest.Url = ConfigurationManager.AppSettings["LocalParabotypeBaseUrl"];
                    ExternalService.Hostname = ConfigurationManager.AppSettings["QAApiHostName"];
                    break;
                case "2":
                    SiteUnderTest.Url = ConfigurationManager.AppSettings["DevParabotypeBaseUrl"];
                    ExternalService.Hostname = ConfigurationManager.AppSettings["DevHostName"];
                    break;
                case "3":
                    SiteUnderTest.Url = ConfigurationManager.AppSettings["QAParabotypeBaseUrl"];
                    ExternalService.Hostname = ConfigurationManager.AppSettings["QAApiHostName"];
                    break;
                case "4":
                    GetSiteUnderTest();
                    GetExternalServiceHostName();
                    break;
            }

            GetPercent();
            GetStatusCode();
        }

        private static void GetPercent()
        {
            Console.WriteLine("What percent of calls should fail?");
            var failRate = 0;
            
            try
            {
                failRate = int.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("Percentage must be between 0 and 100");
                GetPercent();
            }

            if (failRate <= 100 && failRate >= 0)
            {
                ChaosEngine.FailRate = failRate;
                return;
            }
             
            Console.WriteLine("Percentage must be between 0 and 100");
            GetPercent();
        }

        private static void GetStatusCode()
        {
            ChaosEngine.RandomStatus = false;
            
            if (ChaosEngine.RandomStatus)
            {
                Console.WriteLine("Should the error code be random? (y/n)");
                ChaosEngine.RandomStatus = Console.ReadLine() == "y";
            }

            if (ChaosEngine.RandomStatus) return;
            var statusCode = 0;
            Console.WriteLine("What status code should be used to respond with?");

            try
            {
                statusCode = int.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("Please enter a valid status code");
                GetStatusCode();
            }

            ChaosEngine.StatusCode = statusCode;
        }

        private static void GetExternalServiceHostName()
        {
            Console.WriteLine("What is the external service hostname you want to intercept?");
            ExternalService.Hostname = Console.ReadLine();
        }

        private static void GetSiteUnderTest()
        {
            Console.WriteLine("What is the full url of the site you are testing?");
            var url = Console.ReadLine();

            if (Uri.IsWellFormedUriString(url,UriKind.Absolute))
            {
                SiteUnderTest.Url = url;
                return;
            }

            Console.WriteLine("Not a valid url");
            GetSiteUnderTest();
        }

        private static void StartProxy()
        {
            Console.WriteLine("Starting proxy...");

            _fiddle = new FiddlerProxy(ExternalService, SiteUnderTest);

            _fiddle.StartFiddler();

            Console.Clear();

            if (ChaosEngine.RandomStatus)
            {
                Console.WriteLine(
                    "Proxy started - {0} percent of calls being intercepted with a random status being returned",
                    ChaosEngine.FailRate);
            }
            else
            {
                Console.WriteLine("Proxy started - {0} percent of calls being intercepted with {1}s being returned",
                    ChaosEngine.FailRate, ChaosEngine.StatusCode);
            }
        }
    }
}