// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace AADUpdater;

class Program
{

    const string Help = "help";
    const string Get = "get";
    const string Set = "set";
    const string HelpMsg = "Usage: aadupdater get|set|help --graph-app-secret <MS Graph App secret> --user-name <username> --attr-name <attr-name> [--attr-value <attr-value>]";

    public static async Task Main(string[] args)
    {

        if(args.Length < 1)
        {
            Console.WriteLine(HelpMsg);
            return;
        }
        try
        {
            var arguments = ParseArguments(args);
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var tenant = config["AzureAd:Tenant"];
            var graphAppSecret = arguments.Parameters["--graph-app-secret"];
            var userName = $"{arguments.Parameters["--user-name"]}@{tenant}.onmicrosoft.com";
            var attrName = arguments.Parameters["--attr-name"];

            switch (arguments.Command.ToLower())
            {
                case Get:
                    var result = await GetUserByIdWithCustomAttributes(userName, attrName, config, graphAppSecret);
                    var user = ((dynamic)result).user as User;
                    if (user != null)
                    {
                        var extensionClientId = config["AzureAd:ExtensionAppClientId"];
                        var helper = new CustomAttributeHelper(extensionClientId);
                        Console.WriteLine($"{user.DisplayName} {user.AdditionalData[helper.GetCompleteAttributeName(attrName)]}");
                    }
                    break;
                case Set:
                        var attrValue = arguments.Parameters["--attr-value"];
                        var result2 = await UpdateUserCustomCode(userName, attrValue, config, graphAppSecret);
                        var user2 = ((dynamic)result2).user as User;
                        if (user2 != null)
                        {
                            var extensionClientId = config["AzureAd:ExtensionAppClientId"];
                            var helper = new CustomAttributeHelper(extensionClientId);
                            Console.WriteLine($"{user2.DisplayName} {user2.AdditionalData[helper.GetCompleteAttributeName(attrName)]}");
                        }
                    break;
                case Help:
                default:
                    Console.WriteLine(HelpMsg);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(HelpMsg);
            Console.WriteLine(ex.ToString());
        }
    }

    private static async Task<object> GetUserByIdWithCustomAttributes(string userId, string customAttributeNames, IConfiguration config, string graphAppSecret)
    {
        var graphClient = CreateClient(config, graphAppSecret);
        var extensionClientId = config["AzureAd:ExtensionAppClientId"];
        var user = await UserService.GetUserByIdWithCustomAttributes(userId, extensionClientId, customAttributeNames, graphClient);
        var result = new { user };
        return result;
    }

    private static async Task<object> UpdateUserCustomCode(string userId, string customCodeValue, IConfiguration config, string graphAppSecret)
    {
        var graphClient = CreateClient(config, graphAppSecret);
        var extensionClientId = config["AzureAd:ExtensionAppClientId"];
        var customAttributes = new Dictionary<string, object>();
        var propName = "CustomCode";
        customAttributes.Add(propName, customCodeValue);
        var user = await UserService.UpdateUserCustomAttributes(userId, extensionClientId, customAttributes, graphClient);
        var result = new { user };
        return result;
    }

    private static GraphServiceClient CreateClient(IConfiguration config, string graphAppSecret)
    {
        var tenant = config["AzureAd:Tenant"];
        var clientId = config["AzureAd:ClientId"];
        var tenantId = config["AzureAd:TenantId"];
        var clientSecret = graphAppSecret;
        IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(clientId)
            .WithTenantId(tenantId)
            .WithClientSecret(clientSecret)
            .Build();
        ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

        // Set up the Microsoft Graph service client with client credentials
        GraphServiceClient graphClient = new GraphServiceClient(authProvider);
        return graphClient;
    }

    private static Arguments ParseArguments(string[] args)
    {

        var arguments = new Arguments();
        if (args.Length == 0)
            arguments.Command = Help;
        else
            arguments.Command = args[0].ToLower();
        int i = 1;
        while (i < args.Length)
        {
            if (args[i].StartsWith("--"))
            {
                if (i <= args.Length - 2 && !args[i + 1].StartsWith("--"))
                {
                    arguments.Parameters[args[i].ToLower()] = args[i + 1];
                    i += 2;
                }
                else
                {
                    arguments.Parameters[args[i].ToLower()] = "1";
                    i++;
                }
            }
            else
            {
                i++;
            }
        }
        return arguments;
    }



}