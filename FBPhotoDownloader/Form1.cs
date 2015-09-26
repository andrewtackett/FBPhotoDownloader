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
        public FBPhotoDownloader()
        {
            InitializeComponent();
            outputText.Text = "C:\\fb";
        }

        private string outputLocation;

        public struct data
        {
            public string name;
            public string id;
        }

        public static List<data> getAlbumData(string url)
        {
            JObject response = JObject.Parse(performWebRequest(url));
            List<data> albums = new List<data>();

            Console.WriteLine("json:" + response);
            foreach (JToken token in response["data"])
            {
                data curAlbum;
                curAlbum.id = token["id"].ToString();
                JEnumerable<JToken> children = token.Children();
                curAlbum.name = token.Value<string>("name") ?? "";
                albums.Add(curAlbum);
            }

            string next = response["paging"].Value<string>("next");
            Console.WriteLine(next);

            if (response["paging"].Value<string>("next") != null)
                albums.AddRange(getAlbumData(response["paging"]["next"].ToString()));

            return albums;
        }

        public static List<data> getPhotoData(string url)
        {
            return getAlbumData(url);
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

        private void downloadPhotos_Click(object sender, EventArgs e)
        {
            //TODO:
            //Handle access token error
            //Handle rate limit error?
            /*
            make successive runs skip existing photos (how to deal with same name?)
            write readme
             * */

            if (outputLocation == null)
                outputLocation = "C:\\fb";

            string[] input = File.ReadAllLines("user.dat");
            string accessToken = input[0];

            Console.WriteLine("access token:" + accessToken);

            //string url = "https://scontent.xx.fbcdn.net/hphotos-xta1/v/t1.0-9/12039563_10153371789554079_7323702840868121349_n.jpg?oh=941b90875190eebc690fde045942bd5e&oe=5691BE5C";

            //downloadPhoto(url, "C:\\fb\\test.jpg");

            //"https://graph.facebook.com/v2.4/10153371789554079?fields=images&access_token=" + accessToken

            // /v2.4/me/albums -> get album ids,names (for folder names)

            string albumURL = "https://graph.facebook.com/v2.4/me/albums?access_token=" + accessToken;
            List<data> albumIDs = getAlbumData(albumURL);

            foreach (data curAlbum in albumIDs)
            {
                string photoIDsURL = "https://graph.facebook.com/v2.4/" + curAlbum.id + "/photos?access_token=" + accessToken;

                List<data> photoIDs = getPhotoData(photoIDsURL);

                string albumDirectory = outputLocation + "\\" + stripIllegalCharacters(curAlbum.name);

                Directory.CreateDirectory(albumDirectory);

                foreach (data curPhoto in photoIDs)
                {
                    string photoURL = "https://graph.facebook.com/v2.4/" + curPhoto.id + "?fields=images&access_token=" + accessToken;

                    string photoLink = getPhotoLink(photoURL);

                    string outputPath;

                    if (curPhoto.name == "")
                        outputPath = albumDirectory + "\\" + curPhoto.id;
                    else
                        outputPath = albumDirectory + "\\" + stripIllegalCharacters(curPhoto.name);

                    while (File.Exists(outputPath + ".jpg"))
                        outputPath += "_";

                    outputPath += ".jpg";

                    downloadPhoto(photoLink, outputPath);
                }
            }

            //loop on albums
            // /v2.4/<album-id>/photos -> get photo ids, names (for photo names -> transform to no spaces?)
            // e.g. /v2.4/10150145047609079/photos
            //loop on photos
            // /v2.4/<photo-id>?fields=images -> get image link
            // e.g. /v2.4/10153371789554079?fields=images
            // download image -> store in folder with name of album and name name of photo

            Console.WriteLine("Done! Please hit enter to exit...");

            Console.Read();
        }
    }
}
