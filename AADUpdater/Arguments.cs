using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADUpdater
{
    internal class Arguments
    {
        public string Command { get; set; }
        public IDictionary<string, string> Parameters { get; private set; }

        public Arguments()
        {
            Command = "";
            Parameters = new Dictionary<string, string>();
        }
        public bool HasArg(string key)
        {
            return Parameters.ContainsKey(key);
        }

        public string GetArg(string key, bool throwError = false, string errorMessage = "")
        {
            if (!HasArg("--uri"))
            {
                if (throwError)
                {
                    var msg = string.IsNullOrEmpty(errorMessage) ? $"The command {Command} requires the {key} argument." : errorMessage;
                    throw new KeyNotFoundException(msg);
                }
                return string.Empty;
            }
            return Parameters[key];
        }
    }
}
