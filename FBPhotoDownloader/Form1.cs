using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;
using Meta.Core.ImageMeta;
using System.Windows.Media.Imaging;

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

            foreach (JToken token in response["data"])
            {
                photo curphoto = new photo();
                curphoto.id = token["id"].ToString();
                JEnumerable<JToken> children = token.Children();
                curphoto.name = token.Value<string>("name") ?? "";
                photos.Add(curphoto);
            }

            string next = response["paging"].Value<string>("next");

            if (response["paging"].Value<string>("next") != null)
                photos.AddRange(getPhotoData(response["paging"]["next"].ToString()));

            return photos;
        }

        public static string performWebRequest(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                Stream response = webRequest.GetResponse().GetResponseStream();
                StreamReader reader = new StreamReader(response);

                return reader.ReadToEnd();
            }
            catch(WebException)
            {
                MessageBox.Show("Facebook access token in invalid/expired.  Please update user.txt with a new access token.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return "";
        }

        public static string getPhotoLink(string url)
        {
            JObject response = JObject.Parse(performWebRequest(url));
            return response["images"][0]["source"].ToString();
        }

        public static void ConvertToBitmap(string fileName)
        {
            Bitmap bitmap;
            using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);
                bitmap = new Bitmap(image);
            }
            bitmap.Save(fileName, ImageFormat.Bmp);
        }

        public static void downloadPhoto(string url, string fileName, string description)
        {
            WebClient wc = new WebClient();
            try
            {
                StreamWriter sw = new StreamWriter(@"errors.txt", true);
                wc.DownloadFile(url, fileName);
                ImageProcessor processor = new ImageProcessor(fileName);
                ImageProperties properties = new ImageProperties();
                properties.Comments = description;
                if (!processor.TryWrite(properties))
                    sw.WriteLine("couldn't write properties to image: " + fileName);
                sw.Flush();
                sw.Close();
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
            //Handle rate limit error?
            //Add error logging?
            //Scale thumbnail and keep aspect ration
            //Figure out why metadata can't be written on some images
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
            if (pd.curPhotoPath != "")
            {
                Image currentPhoto = Image.FromFile(pd.curPhotoPath);
                Image thumbnail = currentPhoto.GetThumbnailImage(pictureBox1.Width, pictureBox1.Height, myCallback, IntPtr.Zero);
                pictureBox1.Image = thumbnail;
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            label1.Text = "Done!";
            outputSelect.Enabled = true;
            outputText.ReadOnly = false;
        }

        private void updateProgressText(string message,BackgroundWorker worker)
        {
            progressData pd;
            pd.curPhotoPath = "";
            pd.labelText = message;

            worker.ReportProgress(0, pd);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (outputLocation == null)
                outputLocation = "C:\\fb";

            string[] input = File.ReadAllLines("user.txt");
            string accessToken = input[0];
            int totalPhotos = 0;

            updateProgressText("Getting album data",worker);

            string albumURL = "https://graph.facebook.com/v2.4/me/albums?access_token=" + accessToken;
            List<album> albumIDs = getAlbumData(albumURL);

            updateProgressText("Getting list of photos",worker);
            
            var potentialExistingAlbums = Directory.EnumerateDirectories(outputLocation).Select(Path.GetFileName);
            bool foundExistingPhotos = false;
            Application.UseWaitCursor = true;

            for (int i = 0; i < albumIDs.Count; i++)
            {
                album curAlbum = albumIDs[i];
                string photoIDsURL = "https://graph.facebook.com/v2.4/" + curAlbum.id + "/photos?access_token=" + accessToken;

                List<photo> photoIDs = getPhotoData(photoIDsURL);

                //Make sure we aren't duplicating work by redownloading existing photos
                if (potentialExistingAlbums.Count() > 0)
                {
                    if (potentialExistingAlbums.Contains(stripIllegalCharacters(curAlbum.name)))
                    {
                        var existingPhotos = Directory.EnumerateFiles(outputLocation + "\\" + stripIllegalCharacters(curAlbum.name)).Select(Path.GetFileNameWithoutExtension);
                        foreach(string existingPhoto in existingPhotos)
                        {
                            photo duplicate = photoIDs.Find(x => x.id.Equals(existingPhoto));
                            if(duplicate.id != "")
                            {
                                foundExistingPhotos = true;
                                photoIDs.Remove(duplicate);
                            }
                        }
                    }
                }

                curAlbum.photos = photoIDs;
                totalPhotos += photoIDs.Count;
                if (foundExistingPhotos)
                    updateProgressText("Searched (" + (i + 1) + "/" + albumIDs.Count + ") Albums. New photos found so far: " + totalPhotos, worker);
                else
                    updateProgressText("Searched (" + (i + 1) + "/" + albumIDs.Count + ") Albums. Photos found so far: " + totalPhotos, worker);
            }

            Application.UseWaitCursor = false;

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
                    pd.labelText = "(" + (curPhotoNum + 1) + "/" + totalPhotos + ") " + outputPath;

                    downloadPhoto(photoLink, outputPath,curPhoto.name);
                    curPhotoNum++;
                    int progress = (int)(((double)(curPhotoNum + 1) / totalPhotos) * 100);
                    worker.ReportProgress(progress, pd);
                }
            }
        }

        public string getOutputPath(string albumDirectory, photo curPhoto)
        {
            string outputPath;

            outputPath = albumDirectory + "\\" + curPhoto.id;

            //Make sure we don't go over windows path limits
            if (outputPath.Length > 255)
                outputPath = outputPath.Substring(0, 255);

            outputPath += ".jpg";

            return outputPath;
        }
    }
}
