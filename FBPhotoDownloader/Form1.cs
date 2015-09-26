using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FBPhotoDownloader
{
    public partial class FBPhotoDownloader : Form
    {
        private string outputLocation;
        private BackgroundWorker bw = new BackgroundWorker();

        public class album
        {
            public string name;
            public string id;
            public List<photo> photos;
        }

        public class photo
        {
            public string name;
            public string id;
        }

        public struct progressData
        {
            public string labelText;
            public string curPhotoPath;
        }

        public FBPhotoDownloader()
        {
            InitializeComponent();
            outputText.Text = "C:\\fb";
            label1.Text = "";
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork); 
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            progressBar1.Maximum = 100;
        }

        public static List<album> getAlbumData(string url)
        {
            JObject response = JObject.Parse(performWebRequest(url));
            List<album> albums = new List<album>();

            Console.WriteLine("json:" + response);
            foreach (JToken token in response["data"])
            {
                album curAlbum = new album();
                curAlbum.id = token["id"].ToString();
                JEnumerable<JToken> children = token.Children();
                curAlbum.name = token.Value<string>("name") ?? "";
                curAlbum.photos = new List<photo>();
                albums.Add(curAlbum);
            }

            if (response["paging"].Value<string>("next") != null)
                albums.AddRange(getAlbumData(response["paging"]["next"].ToString()));

            return albums;
        }

        public static List<photo> getPhotoData(string url)
        {
            JObject response = JObject.Parse(performWebRequest(url));
            List<photo> photos = new List<photo>();

            Console.WriteLine("json:" + response);
            foreach (JToken token in response["data"])
            {
                photo curphoto = new photo();
                curphoto.id = token["id"].ToString();
                JEnumerable<JToken> children = token.Children();
                curphoto.name = token.Value<string>("name") ?? "";
                photos.Add(curphoto);
            }

            string next = response["paging"].Value<string>("next");
            Console.WriteLine(next);

            if (response["paging"].Value<string>("next") != null)
                photos.AddRange(getPhotoData(response["paging"]["next"].ToString()));

            return photos;
        }

        public static string performWebRequest(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            Stream response = webRequest.GetResponse().GetResponseStream();
            StreamReader reader = new StreamReader(response);

            return reader.ReadToEnd();
        }

        public static string getPhotoLink(string url)
        {
            JObject response = JObject.Parse(performWebRequest(url));
            return response["images"][0]["source"].ToString();
        }

        public static void downloadPhoto(string url, string fileName)
        {
            WebClient wc = new WebClient();
            try
            {
                wc.DownloadFile(url, fileName);
            }
            catch (WebException e)
            {
                string shortenedFilename = fileName.Substring(0, 252);
                shortenedFilename += ".jpg";
                wc.DownloadFile(url, shortenedFilename);
            }
        }

        public static string stripIllegalCharacters(string input)
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();
            char[] invalidFileChars = Path.GetInvalidFileNameChars();
            List<char> allInvalidChars = new List<char>();
            allInvalidChars.AddRange(invalidPathChars);
            allInvalidChars.AddRange(invalidFileChars);

            foreach (char badChar in allInvalidChars)
            {
                input = input.Replace("" + badChar, "");
            }

            return input;
        }

        private void outputSelect_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult dr = fbd.ShowDialog();
            if(dr.Equals(DialogResult.OK))
            {
                outputLocation = fbd.SelectedPath;
                outputText.Text = fbd.SelectedPath;
            }
        }

        public bool ThumbnailCallback()
        {
            return false;
        }

        private void downloadBtn_Click(object sender, EventArgs e)
        {
            //TODO:
            //Handle access token error
            //Handle rate limit error?
            /*
            make successive runs skip existing photos (how to deal with same name?)
            write readme*/

            //"https://graph.facebook.com/v2.4/10153371789554079?fields=images&access_token=" + accessToken
            // /v2.4/me/albums -> get album ids,names (for folder names)
            //loop on albums
            // /v2.4/<album-id>/photos -> get photo ids, names (for photo names -> transform to no spaces?)
            // e.g. /v2.4/10150145047609079/photos
            //loop on photos
            // /v2.4/<photo-id>?fields=images -> get image link
            // e.g. /v2.4/10153371789554079?fields=images
            // download image -> store in folder with name of album and name name of photo
            label1.Text = "download clicked!";
            if (bw.IsBusy != true)
            {
                outputSelect.Enabled = false;
                outputText.ReadOnly = true;
                bw.RunWorkerAsync();
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressData pd = (progressData)e.UserState;
            Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);

            label1.Text = pd.labelText;

            progressBar1.Value = e.ProgressPercentage;
            //System.Threading.Thread.Sleep(500);
            if (pd.curPhotoPath != "")
                pictureBox1.Image = Image.FromFile(pd.curPhotoPath).GetThumbnailImage(pictureBox1.Width, pictureBox1.Height, myCallback, IntPtr.Zero);
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            label1.Text = "Done!";
            outputSelect.Enabled = true;
            outputText.ReadOnly = false;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (outputLocation == null)
                outputLocation = "C:\\fb";

            string[] input = File.ReadAllLines("user.txt");
            string accessToken = input[0];
            int totalPhotos = 0;

            progressData pd;
            pd.curPhotoPath = "";
            pd.labelText = "Getting album data";
            worker.ReportProgress(0, pd);

            string albumURL = "https://graph.facebook.com/v2.4/me/albums?access_token=" + accessToken;
            List<album> albumIDs = getAlbumData(albumURL);
            /*List<album> albumIDs = new List<album>();
            album test = new album();
            test.name = "test";
            test.id = "10153198026299079";
            test.photos = new List<photo>();
            albumIDs.Add(test);*/

            pd.labelText = "Getting list of photos";
            worker.ReportProgress(0, pd);

            for (int i = 0; i < albumIDs.Count; i++)
            {
                album curAlbum = albumIDs[i];
                string photoIDsURL = "https://graph.facebook.com/v2.4/" + curAlbum.id + "/photos?access_token=" + accessToken;

                List<photo> photoIDs = getPhotoData(photoIDsURL);
                curAlbum.photos = photoIDs;
                totalPhotos += photoIDs.Count;
                pd.labelText = "Photos found so far: " + totalPhotos;
                worker.ReportProgress(0, pd);
            }

            downloadPhotos(albumIDs, accessToken, totalPhotos, worker);
        }

        private void downloadPhotos(List<album> albumIDs, string accessToken, int totalPhotos, BackgroundWorker worker)
        {
            int curPhotoNum = 0;

            foreach (album curAlbum in albumIDs)
            {
                string albumDirectory = outputLocation + "\\" + stripIllegalCharacters(curAlbum.name);
                Directory.CreateDirectory(albumDirectory);

                foreach (photo curPhoto in curAlbum.photos)
                {
                    string photoURL = "https://graph.facebook.com/v2.4/" + curPhoto.id + "?fields=images&access_token=" + accessToken;
                    string photoLink = getPhotoLink(photoURL);
                    string outputPath = getOutputPath(albumDirectory, curPhoto);

                    progressData pd;
                    pd.curPhotoPath = outputPath;
                    pd.labelText = "(" + curPhotoNum + "/" + totalPhotos + ") " + outputPath;

                    downloadPhoto(photoLink, outputPath);
                    curPhotoNum++;
                    int progress = (int)(((double)curPhotoNum / totalPhotos) * 100);
                    worker.ReportProgress(progress, pd);
                }
            }
        }

        public string getOutputPath(string albumDirectory, photo curPhoto)
        {
            string outputPath;

            if (curPhoto.name == "")
                outputPath = albumDirectory + "\\" + curPhoto.id;
            else
                outputPath = albumDirectory + "\\" + stripIllegalCharacters(curPhoto.name);

            while (File.Exists(outputPath + ".jpg"))
                outputPath += "_";

            //Make sure we don't go over windows path limits
            if (outputPath.Length > 255)
                outputPath = outputPath.Substring(0, 255);

            outputPath += ".jpg";

            return outputPath;
        }
    }
}
