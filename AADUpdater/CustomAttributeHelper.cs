using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADUpdater
{
    internal class CustomAttributeHelper
    {
        internal readonly string _extensionAppClientId;

        internal CustomAttributeHelper(string extensionAppClientId)
        {
            _extensionAppClientId = extensionAppClientId.Replace("-", "");
        }

        internal string GetCompleteAttributeName(string attributeName)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new System.ArgumentException("Parameter cannot be null", nameof(attributeName));
            }

            return $"extension_{_extensionAppClientId}_{attributeName}";
        }
    }
}
