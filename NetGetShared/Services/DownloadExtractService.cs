using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetGetShared
{
    public class DownloadExtractService
    {
        public WebClient WebClient { get; set; }

        public DownloadExtractService()
        {
            WebClient = new WebClient();
        }
        public void Download(string downloadFrom, string downloadTo, string fileName, DownloadProgressChangedEventHandler progressChangedEventHandler, AsyncCompletedEventHandler asyncCompletedEventHandler)
        {
            if (File.Exists(downloadTo))
            {
                File.Delete(downloadTo);
            }
            if (Directory.Exists(downloadTo))
            {
                Directory.Delete(downloadTo, true);
            }

            Directory.CreateDirectory(downloadTo);

            WebClient.DownloadProgressChanged += progressChangedEventHandler;
            WebClient.DownloadFileCompleted += asyncCompletedEventHandler;
            WebClient.DownloadFileAsync(new Uri(downloadFrom), Path.Combine(downloadTo, fileName));
        }
        public void Extract(string extractFrom, string extractTo, Progress<ZipProgress> zipProgress)
        {
            using (ZipArchive zip = new ZipArchive(new FileStream(extractFrom, FileMode.Open)))
            {
                zip.ExtractToDirectory(extractTo, zipProgress, true);
            }
        }
    }
}
