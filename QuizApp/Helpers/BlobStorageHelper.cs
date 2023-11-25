using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace QuizApp.Helpers
{
    public class BlobStorageHelper
    {
        private readonly static string _blobStorageAccount = ConfigurationManager.AppSettings["BlobStorageAccount"].ToString();// GlobalSettings.BlobStorageAccount;
        private readonly static string _blobContainerName = ConfigurationManager.AppSettings["BlobContainerName"].ToString();

        public static FileUploadResponseModel UploadFileToBlob(string fileName, byte[] fileData)
        {
            FileUploadResponseModel response = null;
            var fileIdentifier = GetUniqueFileName(fileName);

            string uri = UploadFileToAzureBlob(fileIdentifier, fileData);
            if (!string.IsNullOrWhiteSpace(uri))
            {
                response = new FileUploadResponseModel();
                response.FileName = fileName;
                response.FileIdentifier = fileIdentifier;
                response.FileLink = uri;
            }
            return response;
        }
        private static string GenerateSlug(string phrase)
        {
            var s = phrase.ToLower();
            s = Regex.Replace(s, @"[^a-z0-9\s-.]", "");                      // remove invalid characters
            s = Regex.Replace(s, @"\s+", " ").Trim();                       // single space
            s = Regex.Replace(s, @"\s", "-");                               // insert hyphens
            s = Regex.Replace(s, @"\-+", "-");
            return s.ToLower();
        }

        public static string UploadFileToAzureBlob(string fileIdentifier, byte[] fileData)
        {
            try
            {
                string uri = null;
                var directoryName = "";
                // Retrieve storage account from connection string.
                var storageAccount = CloudStorageAccount.Parse(_blobStorageAccount);

                // Create the blob client.
                var blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                var container = blobClient.GetContainerReference(_blobContainerName);
                var resourceContainer = !string.IsNullOrWhiteSpace(directoryName)
                    ? directoryName + "/" + fileIdentifier
                    : fileIdentifier;

                var blockBlob = container.GetBlockBlobReference(resourceContainer);

                blockBlob.Properties.ContentType = MimeMapping.GetMimeMapping(fileIdentifier);
                blockBlob.UploadFromByteArray(fileData, 0, fileData.Length);

                uri = blockBlob.Uri.AbsoluteUri;
                return uri;
            }
            catch (Exception ex) { return null; }
        }

        public static string GetBlobLinkByFileIdentifier(string fileIdentifier)
        {
            var directoryName = "";

            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(_blobStorageAccount);

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            var container = blobClient.GetContainerReference(_blobContainerName);
            var resourceContainer = !string.IsNullOrWhiteSpace(directoryName)
                ? directoryName + "/" + fileIdentifier
                : fileIdentifier;

            var blockBlob = container.GetBlockBlobReference(resourceContainer);
            return blockBlob.Uri.AbsoluteUri;
        }

        public static IEnumerable<FileUploadResponseModel> GetAllBlobFiles(string searchText = "")
        {
            var directoryName = "";

            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(_blobStorageAccount);

            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve reference to a previously created container.
            var container = blobClient.GetContainerReference(_blobContainerName);

            var directory = container.GetDirectoryReference(directoryName);
            var list = directory.ListBlobs();
            var newList = list.OfType<CloudBlockBlob>().Select(b => new FileUploadResponseModel
            {
                FileName = ExtractFileName(b.Name),
                FileIdentifier = b.Name,
                FileLink = b.Uri.AbsoluteUri
            }).ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                newList = newList.Where(a => a.FileIdentifier.ToLower().Contains(searchText.ToLower())).ToList();
            }
            return newList;
        }

        public static ActionMessageResponseModel DeleteBlobFile(string fileIdentifier)
        {
            try
            {
                var directoryName = "";

                // Retrieve storage account from connection string.
                var storageAccount = CloudStorageAccount.Parse(_blobStorageAccount);

                // Create the blob client.
                var blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                var container = blobClient.GetContainerReference(_blobContainerName);

                var resourceContainer = !string.IsNullOrWhiteSpace(directoryName)
                    ? directoryName + "/" + fileIdentifier
                    : fileIdentifier;

                var blockBlob = container.GetBlockBlobReference(resourceContainer);
                if (blockBlob.Exists())
                    blockBlob.Delete();
                return new ActionMessageResponseModel { Success = true, Message = "File Deleted" };
            }
            catch (Exception ex) { return new ActionMessageResponseModel { Success = false, Message = ex.Message }; }
        }

        public static Tuple<string, byte[]> DownloadBlobFile(string containerName, string directoryName, string fileName) {
            // Retrieve storage account from connection string.
            var storageAccount = CloudStorageAccount.Parse(_blobStorageAccount);
            // Create the blob client.
            var blobClient = storageAccount.CreateCloudBlobClient();
            // Retrieve reference to a previously created container.
            var container = blobClient.GetContainerReference(containerName);
            var resourceContainer = (!string.IsNullOrEmpty(directoryName)) ? (directoryName + "/" + fileName) : fileName;
            var blockBlob = container.GetBlockBlobReference(resourceContainer);
            var memStream = new MemoryStream();
            blockBlob.DownloadToStream(memStream);
            return new Tuple<string, byte[]>(blockBlob.Properties.ContentType, memStream.ToArray());
        }

        public static Tuple<string, byte[]> DownloadEmailAttachmentBlobFile(string fileLink) {
            // Retrieve storage account from connection string.
            var segments = fileLink.Split(new char[] { '/' });
            var fileName = segments[segments.Length - 1];
            return DownloadBlobFile(_blobContainerName, null, fileName);
        }

        #region Private Utils

        private static string GetUniqueFileName(string fileName)
        {
            var rand = new Random();
            var firstGuid = Guid.NewGuid().ToString().Split('-')[rand.Next(0, 4)];
            var secondGuid = Guid.NewGuid().ToString().Split('-')[rand.Next(0, 4)];
            fileName = GenerateSlug(fileName);
            fileName = Path.GetFileNameWithoutExtension(fileName) + Path.GetExtension(fileName);
            return $"{firstGuid}-{secondGuid}-{fileName}";
        }

        private static string ExtractFileName(string fileidentifier)
        {
            var array = fileidentifier.Split('-');
            if (array.Length == 3) { return array[2].Trim(); }
            return fileidentifier;
        }

        #endregion

        //public string GetBlobFileDownloadLink(string clientName, string fileName, bool isShared)
        //{
        //    return GetDownloadLinkForFileBlob(new FileUploadRequestModel
        //    {
        //        RootContainerName = $"{clientName}leadcvs{_environmentName}",
        //        FileName = fileName,
        //    }, isShared);
        //}

        //public string GetDownloadLinkForFileBlob(FileUploadRequestModel uploadFileRequest, bool isShared = false)
        //{
        //    // Retrieve storage account from connection string.
        //    var storageAccount = CloudStorageAccount.Parse(_blobStorageAccount);

        //    // Create the blob client.
        //    var blobClient = storageAccount.CreateCloudBlobClient();

        //    // Retrieve reference to a previously created container.
        //    var container = blobClient.GetContainerReference(uploadFileRequest.RootContainerName);

        //    var resourceContainer = (!string.IsNullOrEmpty(uploadFileRequest.DirectoryName)) ? (uploadFileRequest.DirectoryName + "/" + uploadFileRequest.FileName) : uploadFileRequest.FileName;
        //    var blockBlob = container.GetBlockBlobReference(resourceContainer);
        //    var builder = new UriBuilder(blockBlob.Uri);

        //    DateTime expiry = DateTime.UtcNow.AddHours(4);
        //    if (isShared)
        //        expiry = expiry.AddYears(1);
        //    builder.Query = blockBlob.GetSharedAccessSignature(
        //        new SharedAccessBlobPolicy
        //        {
        //            Permissions = SharedAccessBlobPermissions.Read,
        //            SharedAccessStartTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)),
        //            SharedAccessExpiryTime = new DateTimeOffset(expiry)

        //        }).TrimStart('?');

        //    var signedBlobUrl = builder.Uri;
        //    return signedBlobUrl.ToString();
        //}

        //public FileDto DownloadBlobFile(DownloadFileRequest uploadFileRequest)
        //{
        //    var result = new FileDto();
        //    try
        //    {
        //        // Retrieve storage account from connection string.
        //        var storageAccount = CloudStorageAccount.Parse(uploadFileRequest.CloudStoragePath);
        //        // Create the blob client.
        //        var blobClient = storageAccount.CreateCloudBlobClient();
        //        // Retrieve reference to a previously created container.
        //        var container = blobClient.GetContainerReference(uploadFileRequest.RootContainerName);
        //        var resourceContainer = (!string.IsNullOrEmpty(uploadFileRequest.DirectoryName)) ? (uploadFileRequest.DirectoryName + "/" + uploadFileRequest.FileName) : uploadFileRequest.FileName;
        //        var blockBlob = container.GetBlockBlobReference(resourceContainer);
        //        var memStream = new MemoryStream();
        //        blockBlob.DownloadToStream(memStream);
        //        result.FileName = uploadFileRequest.FileName;
        //        result.ContentType = blockBlob.Properties.ContentType;
        //        result.Data = memStream.ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    return result;
        //}
    }
}
