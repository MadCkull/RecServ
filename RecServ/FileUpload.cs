using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace RecServ
{
    public class FileUpload
    {
        private static readonly string ClientId = "0b3f6393-7b64-42bc-aaab-db94a30cb5bf"; // Replace with your client ID
        private static readonly string ClientSecret = "tn98Q~SalvV0Ij2p_dcxztDP.YjXD233fRc4MbV8"; // Replace with your client secret
        private static readonly string TenantId = "f8cdef31-a31e-4b4a-93e4-5f571e91255a"; // Replace with your tenant ID

        private static readonly string RedirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        private static readonly string Authority = $"https://login.microsoftonline.com/{TenantId}";

        private GraphServiceClient graphClient;

        public async Task InitializeGraphClient()
        {
            graphClient = await GetGraphClient();
        }

        public async Task Upload(string filePath, string folderName)
        {
            if (graphClient == null)
            {
                throw new InvalidOperationException("Graph client has not been initialized. Call InitializeGraphClient() before uploading.");
            }

            // Get or create the folder
            var userDataFolder = await GetOrCreateFolder(folderName);

            // Get the file name from the file path
            var fileName = Path.GetFileName(filePath);

            // Upload the file to the folder
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var uploadSession = await graphClient.Drive.Items[userDataFolder.Id].ItemWithPath(fileName).CreateUploadSession().Request().PostAsync();
                var maxChunkSize = 320 * 1024; // 320 KB
                var largeFileUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, maxChunkSize);

                // Upload the file in chunks
                var uploadResult = await largeFileUploadTask.UploadAsync();
                if (uploadResult.UploadSucceeded)
                {
                    Console.WriteLine("File uploaded successfully.");
                }
                else
                {
                    Console.WriteLine("File upload failed.");
                }
            }
        }

        private static async Task<GraphServiceClient> GetGraphClient()
        {
            var publicClientApplication = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithRedirectUri(RedirectUri)
                .Build();

            var authResult = await publicClientApplication.AcquireTokenInteractive(new[] { "Files.ReadWrite" }).ExecuteAsync();
            var authProvider = new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                return Task.CompletedTask;
            });

            return new GraphServiceClient(authProvider);
        }


        private async Task<DriveItem> GetOrCreateFolder(string folderName)
        {
            var drive = await graphClient.Me.Drive.Root.Request().GetAsync();
            var folder = await graphClient.Me.Drive.Root.Children.Request().Filter($"name eq '{folderName}'").GetAsync();

            if (folder.Count > 0)
            {
                return folder[0];
            }
            else
            {
                var newFolder = new DriveItem
                {
                    Name = folderName,
                    Folder = new Folder()
                };
                return await graphClient.Me.Drive.Root.Children.Request().AddAsync(newFolder);
            }
        }
    }
}