using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AADUpdater
{
    internal class UserService
    {
        public static async Task<object> GetUserByIdWithCustomAttributes(string userId, string extensionAppClientId, string customAttributeNames, GraphServiceClient graphClient)
        {
            if (string.IsNullOrWhiteSpace(extensionAppClientId))
            {
                throw new ArgumentException("ExtensionAppClientId (Application ID) is missing from appsettings.json. Find it in the App registrations pane in the Azure portal.", nameof(extensionAppClientId));
            }

            // Get the complete name of the custom attribute (Azure AD extension)
            CustomAttributeHelper helper = new CustomAttributeHelper(extensionAppClientId);

            var completeCustomAttributeNamesList = "";
            if (!string.IsNullOrEmpty(customAttributeNames))
            {
                var completeCustomAttributeNames = customAttributeNames.Split(",").Select(x => helper.GetCompleteAttributeName(x.Trim()));
                completeCustomAttributeNamesList = string.Join(",", completeCustomAttributeNames);
                completeCustomAttributeNamesList = !string.IsNullOrEmpty(completeCustomAttributeNamesList) ? "," + completeCustomAttributeNamesList : "";
            }

            Console.WriteLine($"Getting user with ID {userId} and the custom attributes '{completeCustomAttributeNamesList}");

            // Get user
            var result = await graphClient.Users[userId]
                .Request()
                .Select($"id,givenName,surName,displayName,identities{completeCustomAttributeNamesList}")
                .GetAsync();

            return result;
        }

        public static async Task<object> UpdateUserCustomAttributes(string userId, string extensionAppClientId, IDictionary<string, object> customAttributes, GraphServiceClient graphClient)
        {
            if (string.IsNullOrWhiteSpace(extensionAppClientId))
            {
                throw new ArgumentException("ExtensionAppClientId (ApplicationId) is missing in the appsettings.json. Get it from the App Registrations blade in the Azure portal.", nameof(extensionAppClientId));
            }

            // Get the complete name of the custom attribute (Azure AD extension)
            CustomAttributeHelper helper = new CustomAttributeHelper(extensionAppClientId);

            var completeCustomAttributeNames = customAttributes.Select(kv => helper.GetCompleteAttributeName(kv.Key));
            var extensionInstance = customAttributes.ToDictionary(kv => helper.GetCompleteAttributeName(kv.Key), kv => kv.Value);
            var completeCustomAttributeNamesList = string.Join(",", completeCustomAttributeNames);
            completeCustomAttributeNamesList = !string.IsNullOrEmpty(completeCustomAttributeNamesList) ? "," + completeCustomAttributeNamesList : "";
            Console.WriteLine($"Update user {userId} with the custom attributes {completeCustomAttributeNamesList}");


            try
            {
                // Update user
                var result = await graphClient.Users[userId]
                .Request()
                .UpdateAsync(new User
                {
                    AdditionalData = extensionInstance
                });
                Console.WriteLine($"Updated the user {userId}. Now getting the user with object ID '{userId}'...");

                // Get created user by object ID
                result = await graphClient.Users[userId]
                    .Request()
                    .Select($"id,givenName,surName,displayName,identities{completeCustomAttributeNamesList}")
                    .GetAsync();

                return result;
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Have you created the custom attributes {completeCustomAttributeNamesList} in your tenant?");
                    Console.WriteLine();
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                throw;
            }
        }
    }
}
