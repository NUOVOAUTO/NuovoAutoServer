using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Model.Dto
{
    public class BlobInfo
    {
        public string BlobUrl { get; set; }
        public string BlobContainer { get; set; }
        public string BlobFilePath { get; set; }
        public byte[] BlobContentByteArray { get; set; }
        public string VersionId { get; set; }
        public Uri Uri { get; set; }
    }
}
