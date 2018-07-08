using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ConfigCreater.Model {
    class CryptoListJsonModel {

        [JsonProperty ("supportlist")]
        public List<CryptoData> SupportList { get; set; }
    }

    class CryptoData {
        [JsonProperty ("name")]
        public string Nmae { get; set; }

        [JsonProperty ("code")]
        public string Code { get; set; }

        [JsonProperty ("dir")]
        public string Dir { get; set; }

        [JsonProperty ("data")]
        public List<string> Data { get; set; }

        public void view () {
            Console.WriteLine ("Name: " + this.Nmae);
            Console.WriteLine ("Code: " + this.Code);
        }
    }
}