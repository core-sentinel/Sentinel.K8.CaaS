using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Sentinel.NetworkUtils.Models;

namespace Sentinel.NetworkUtils.Helpers;

public static class TestKeyVaultConnection
{

    public static async Task<TestNetConnectionResponse> TestConnection(string keyVaultName, ServicePrincipal principal = null)
    {
        var sw = Stopwatch.StartNew();
        var keyVaultUrl = $"https://{keyVaultName}.vault.azure.net";
        SecretClient client = null;
        if (principal != null && principal.ClientId != null && principal.ClientSecret != null && principal.TenantId != null)
        {
            var credential = new ClientSecretCredential(principal.TenantId, principal.ClientId, principal.ClientSecret);
            client = new SecretClient(new Uri(keyVaultUrl), credential);
        }
        else
        {
            client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        }


        try
        {

            AsyncPageable<SecretProperties> allSecrets = client.GetPropertiesOfSecretsAsync();

            await foreach (SecretProperties secret in allSecrets)
            {
                Console.WriteLine($"IterateSecretsWithAwaitForeachAsync: {secret.Name}");
            }
            return new TestNetConnectionResponse(CheckAccessRequestResourceType.KeyVault, true, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            return new TestNetConnectionResponse(CheckAccessRequestResourceType.KeyVault, false, ex.Message, sw.ElapsedMilliseconds);
        }
        finally { sw.Stop(); }
    }
}