using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Logic;
using Azure.ResourceManager.Logic.Models;
using System.Security.Cryptography.X509Certificates;
using System.Text;

class Program
    {
        private static ArmClient ArmClient;
        static async Task Main(string[] args)
        {
            ArmClient = new ArmClient(new DefaultAzureCredential());
            await AddCertificate();
        }
        private static async Task AddCertificate()
        {
            try
            {
                // Hardcoded values
                string subscriptionId = "";
                string resourceGroupName = "";
                string integrationAccountName = "";
                string base64FileContent = "MIIDOTCCAiGgAwIBAgIQVFDtoo+7S3CtSzBvX1ZbeDANBgkqhkiG9w0BAQsFADAZMRcwFQYDVQQDEw5kb21haW5uYW1lLmNvbTAeFw0yNDA0MzAxMDAyMjFaFw0yOTA0MzAxMDEyMjFaMBkxFzAVBgNVBAMTDmRvbWFpbm5hbWUuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAqlQ1Ck3vv9pjG3U9vCxmzu2ewT3or77HAXfG6U4brrAoZ3ltt1Rj/6raPtGB1AvHyxuO+R+8s61M94kiMONcVWFXkt9j07JxxJbEq2gM2rrTNoUme3DebDtsIGfXdSJW+x9tTzRp4AeV8R5K+Hdxt3dcvg+E6BmAstjcs/zalrQIqnQHDSVxl+pa6QXUOo4CI1ffEL9E1p68vJqUUgdFVuJXsm2MHNfI6Uf/HVZumZ8mvX34EQfQXlYLGgPqV2AGsO7GTMY8p7/l/N7RxaMdf2UEstaj96UnBVc+DKXtXkDzO0iv2Gw5r782BAUBvRY3njU7AxZuJ3b2lLGSf3HiOQIDAQABo30wezAPBgNVHQ8BAf8EBQMDB/+AMAkGA1UdEwQCMAAwHQYDVR0lBBYwFAYIKwYBBQUHAwEGCCsGAQUFBwMCMB8GA1UdIwQYMBaAFCfar5wUeNqW4i8cr5TmD06cKaSsMB0GA1UdDgQWBBQn2q+cFHjaluIvHK+U5g9OnCmkrDANBgkqhkiG9w0BAQsFAAOCAQEAdDp7hYdvh50yb2N9YRzgyz4e/ch+G5u207QsM+XIqa36WFKx5LdztTUUasFwq04qx/WgOYA4kzgq6R9at6/xgF+Cze8xVCOJeu2CJSPDOXn4EAcrzIBlVMAI8Cjqe2nGSi6RGTrminv6+/BHD7QCHeYj0SNCsuSmKlm+14mcQQy2gnxKEZkKGq6/WadO3fkDmHvNUozhpzkeS3BbILJh9Syh0ueudLWuciRu0zzIGS3sZVlt7rgh7zxLbiyDttP7oakiv9qYss0q9BoAFD8PC2Dw6q1a8cD245v6VYUiclxqcZmNKVTfpqwZGB71UQKK6QqccJB8PwOPRrqx4zqqkg==";
                string keyType = "private";
                string keyName = "Test";
                string certName = "Cert";
                string resourceGroup = "";
                string keyVaultResourceName = "";

            // Create resource identifier
            var resourceIdentifier = IntegrationAccountResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, integrationAccountName);
            IntegrationAccountResource integrationAccountResource = ArmClient.GetIntegrationAccountResource(resourceIdentifier);
            IntegrationAccountCertificateCollection certificateCollection = integrationAccountResource.GetIntegrationAccountCertificates();

            byte[] content = Encoding.UTF8.GetBytes(base64FileContent);
            X509Certificate2 certificate = new X509Certificate2(content);
            string notAfterString = certificate.NotAfter.ToString(); 

            // Proper JSON format for metadata
            IntegrationAccountCertificateData data = new IntegrationAccountCertificateData(new AzureLocation())
            {
                PublicCertificate = BinaryData.FromString("\""+base64FileContent+"\"")
            };

            if (keyType == "private")
            {
                data.Key = new IntegrationAccountKeyVaultKeyReference(keyName)
                {
                    ResourceId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.KeyVault/vaults/{keyVaultResourceName}"),
                    KeyName = keyName
                };
            }
            var response =await certificateCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, certName, data,CancellationToken.None);
            Console.WriteLine("Successfully saved");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}
