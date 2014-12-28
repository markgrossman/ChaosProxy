using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChaosProxy
{
    public static class ChaosEngine
    {
        [Range(1, 100)]
        public static int FailRate { get; set; }

        public static bool RandomStatus { get; set; }
        public static int StatusCode { get; set; }

        public static bool ReturnError()
        {
            var rand = new Random();
            var num = rand.Next(1, 100);
            if (num < FailRate)
            {
                if (RandomStatus)
                {
                    StatusCode = RandomErrorToReturn();
                    return true;
                }
                return true;
            }
            return false;
        }

        public static int RandomErrorToReturn()
        {
            var statusCodes = new List<int> {400, 401, 403, 404, 500};

            var rand = new Random();

            return statusCodes[rand.Next(1, statusCodes.Count)];
        }
    }
}