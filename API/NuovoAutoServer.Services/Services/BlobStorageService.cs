using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using EllipticCurve.Utils;

using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Extensions.Msal;

using NuovoAutoServer.Model.Dto;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BlobInfo = NuovoAutoServer.Model.Dto.BlobInfo;

namespace NuovoAutoServer.Services.Services
{
    public class BlobStorageService
    {

        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<bool> BlobExists(string blobContainer, string blobPath)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(blobContainer);
            BlobClient blobClient = containerClient.GetBlobClient(blobPath);
            return await blobClient.ExistsAsync();
        }

        public static async Task<string> GetBlobContentAsString(Uri blobUri)
        {
            BlobClient blobClient = GetBlobClient(blobUri);

            if (!await blobClient.ExistsAsync())
                return null;
            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            var blobContent = downloadResult.Content.ToString();
            return blobContent;
        }

        public async Task<Stream?> GetBlobContentAsStream(string blobContainer, string blobPath)
        {
            // Get a credential and create a service client object for the blob service client.
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(blobContainer);
            BlobClient blobClient = containerClient.GetBlobClient(blobPath);
            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            MemoryStream memoryStream = new MemoryStream();

            //downloads blob's content to a stream 
            await blobClient.DownloadToAsync(memoryStream);
            return memoryStream;
        }

        public async Task<byte[]?> GetBlobContentAsBytes(string blobContainer, string blobPath)
        {
            // Get a credential and create a service client object for the blob service client.
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(blobContainer);
            BlobClient blobClient = containerClient.GetBlobClient(blobPath);
            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            // Get the blob's content as bytes
            byte[] contentBytes;
            // Download the blob's content as a byte array
            // Initialize a memory stream where the blob's content will be downloaded
            using (var memoryStream = new MemoryStream())
            {
                // Download the blob's content to the memory stream
                await blobClient.DownloadToAsync(memoryStream);

                // Convert the memory stream to a byte array
                contentBytes = memoryStream.ToArray();
            }

            return contentBytes;
        }

        public async Task<byte[]?> GetBlobContentAsBytes(string url)
        {
            // Create a BlobClient object from the URL
            BlobClient blobClient = new BlobClient(new Uri(url));
            var contentBytes = await GetBlobContentAsBytes(blobClient.BlobContainerName, blobClient.Name);
            return contentBytes;
        }

        public async Task<string> UploadToBlob(string blobContainer, string blobName, byte[] byteArray, string contentType = null)
        {
           var blobInfo = await UploadToBlobAsByteArray(blobContainer, blobName, byteArray, contentType);

            return blobInfo.BlobUrl;
        }

       

        public async Task<BlobContainerClient> GetBlobContainerClient(string container, bool createContianerIfNotExists = false)
        {
            // Get a reference to the blob container.
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(container);
            if (createContianerIfNotExists)
                await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);
            await Task.CompletedTask;
            return containerClient;
        }

        public async Task<BlobClient> GetBlobClient(string container, string blobPath, bool createContianerIfNotExists = false)
        {
            AdjustBlobNames(ref container, ref blobPath);

            // Get a reference to the blob container.
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(container);
            if (createContianerIfNotExists)
                await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(blobPath);
            await Task.CompletedTask;
            return blobClient;
        }

        public static BlobClient GetBlobClient(Uri blobUri)
        {
            return new BlobClient(blobUri, new DefaultAzureCredential());
        }

        /// <summary>
        /// Adjusts the values of blobContainer and blobName based on the presence of a slash in blobContainer.
        /// If a slash is found in blobContainer, the text following the slash is moved to the beginning of blobName,
        /// prefixed with a slash, and blobContainer is updated to contain only the text preceding the slash.
        /// </summary>
        /// <param name="blobContainer">The container string to be adjusted. Pass by reference.</param>
        /// <param name="blobName">The name string to be prefixed. Pass by reference.</param>
        private static void AdjustBlobNames(ref string blobContainer, ref string blobName)
        {
            int slashIndex = blobContainer.IndexOf('/');
            if (slashIndex >= 0)
            {
                string prefix = blobContainer.Substring(slashIndex + 1);
                blobContainer = blobContainer.Substring(0, slashIndex);
                if (!string.IsNullOrEmpty(prefix))
                {
                    blobName = prefix + "/" + blobName;
                }
            }
        }

        private async Task<BlobInfo> UploadToBlobAsByteArray(string blobContainer, string blobName, byte[] byteArray, string contentType)
        {
            BlobClient blobClient = await GetBlobClient(blobContainer, blobName, true);

            BinaryData binaryData = new BinaryData(byteArray);
            Azure.Response<BlobContentInfo> uploadResponse = await blobClient.UploadAsync(binaryData, overwrite: true);
            if (contentType != null)
                await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
                {
                    ContentType = contentType
                });
            return new BlobInfo() { VersionId = uploadResponse.Value.VersionId, BlobUrl = blobClient.Uri.AbsoluteUri };
        }
    }
}
