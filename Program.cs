using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ConfigCreater.Model;
using Newtonsoft.Json;

namespace ConfigCreater {
    class Program {
        public const string _URL = "";

        public static List<string> _DIRs = new List<string> {
            Path.DirectorySeparatorChar + "AppData" + Path.DirectorySeparatorChar + "Roaming" + Path.DirectorySeparatorChar, //Windows %appdata%
            Path.DirectorySeparatorChar + "Libray" + Path.DirectorySeparatorChar, //MacOS ~/Libray/
            Path.DirectorySeparatorChar + ".", //Unix
        };

        static void Main (string[] args) {
            MainAsync (args).Wait ();
        }

        static async Task MainAsync (string[] args) {
            string dirpass = string.Empty;
            string configFile = string.Empty;
            string dirs, confgiPath;

            CryptoData cdata = await CryptoSelecter ();
            Console.WriteLine ("###############################");
            cdata.view ();
            dirs = CryptoDirChecker (cdata);
            Console.WriteLine ("Dir: " + dirs);
            Console.WriteLine ("###############################");
            confgiPath = ConfigChecker (cdata, dirs);
            Console.WriteLine ("###############################");
            ConfigWriter (cdata, confgiPath);
            Console.WriteLine ("###############################");
            Console.WriteLine ("Done.");
        }

        static public void ConfigWriter (CryptoData cdata, string confgiPath) {
            string filedata = string.Empty;
            string tmpFile = cdata.Nmae.ToLower () + ".conf";
            cdata.Data.ForEach (delegate (string item) {
                filedata += item + Environment.NewLine;
            });
            if (File.Exists (tmpFile))
                File.Delete (tmpFile);
            Encoding enc = new UTF8Encoding (false);
            using (FileStream fs = new FileStream (tmpFile, FileMode.Create)) {
                using (StreamWriter sw = new StreamWriter (fs, enc)) {
                    sw.Write (filedata);
                }
            }
            Console.WriteLine ("Created temporarily.");
            Console.WriteLine ("Copy?[y/n]");
            string yn = Console.ReadLine ();
            Console.WriteLine (yn.ToLower ());
            if (yn.ToLower () == "y") {
                File.Copy (cdata.Nmae.ToLower () + ".conf", confgiPath);
            }
        }

        static public string ConfigChecker (CryptoData cdata, string dir) {
            string confpath = dir + cdata.Nmae.ToLower () + ".conf";
            if (File.Exists (confpath)) {
                if (File.Exists (confpath + ".old")) {
                    Console.WriteLine ("Deleted: " + confpath + ".old");
                    File.Delete (confpath + ".old");
                }
                Console.WriteLine ("Backupd: " + confpath);
                File.Move (confpath, confpath + ".old");
            }
            return confpath;
        }

        static public string CryptoDirChecker (CryptoData cdata) {
            string _dir = System.Environment.GetFolderPath (System.Environment.SpecialFolder.UserProfile);
            string dir = string.Empty;
            List<string> cdirs = new List<string> { cdata.Dir, cdata.Dir.ToLower () };
            bool flg = false;
            _DIRs.ForEach (delegate (string DIR) {
                cdirs.ForEach (delegate (string cdir) {
                    string dir_ = _dir + DIR + cdir;
                    //Console.WriteLine (dir_);
                    if (!flg && Directory.Exists (dir_)) {
                        dir = dir_;
                        flg = true;
                    }
                });
            });
            if (dir == string.Empty) {
                Console.WriteLine ("Please Run Wallet.");
                Environment.Exit (100);
            }
            return dir + Path.DirectorySeparatorChar;
        }

        static public async Task<CryptoData> CryptoSelecter () {
            CryptoListJsonModel cljm = await CryptoListGetter ();
            CryptoData cdata = null;
            string readStr;

            Console.WriteLine ("Plse Type Crypto Code(BTC, LTC, ZNY ..etc):");
            readStr = Console.ReadLine ();
            cljm.SupportList.ForEach (delegate (CryptoData item) {
                if (item.Code.ToUpper () == readStr.ToUpper ())
                    cdata = item;
            });
            if (cdata == null) {
                Console.WriteLine ("Sorry. Not Support.");
                Environment.Exit (100);
            }
            return cdata;
        }

        static public async Task<CryptoListJsonModel> CryptoListGetter () {
            string strdata = string.Empty;
            CryptoListJsonModel JsonData;
            //Debug
            if (File.Exists ("crypto.json")) {
                using (StreamReader sr = new StreamReader (new FileStream ("crypto.json", FileMode.Open))) {
                    while (sr.Peek () > 0) {
                        string line = sr.ReadLine ();
                        strdata += line;
                    }
                }
                JsonData = JsonConvert.DeserializeObject<CryptoListJsonModel> (strdata);
                return JsonData;
            } else
                return await GetWebJson ();
        }

        static public async Task<CryptoListJsonModel> GetWebJson () {
            string json = string.Empty;
            CryptoListJsonModel cljm = JsonConvert.DeserializeObject<CryptoListJsonModel> ("{}");

            json = await WebGet (_URL);
            cljm = JsonConvert.DeserializeObject<CryptoListJsonModel> (json);
            return cljm;
        }

        static public async Task<string> WebGet (string url) {
            var client = new HttpClient ();
            client.DefaultRequestHeaders.Accept.Clear ();
            client.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add ("User-Agent", ".Net Core Config_Creator");

            var stringTask = client.GetStringAsync (url);

            var str = await stringTask;
            return str;
        }
    }
}