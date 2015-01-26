using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace WordAutomationSharp {
    [DataContract]
    public class WordGraph{

        [DataMember]
        public Dictionary<string, Dictionary<string, int>> SingleWords { get; set; }

        [DataMember]
        public Dictionary<Tuple<string, string>, Dictionary<string, int>> DoubleWords { get; set; }

        [DataMember]
        public Dictionary<Tuple<string, string, string>, Dictionary<string, int>> TripleWords { get; set; }

        public WordGraph(){
            SingleWords = new Dictionary<string, Dictionary<string, int>>();
            TripleWords = new Dictionary<Tuple<string, string, string>, Dictionary<string, int>>();
            DoubleWords = new Dictionary<Tuple<string, string>, Dictionary<string, int>>();
        }

        public void MakeSingleLink(string a, string b){
            if (!SingleWords.ContainsKey(a))
            {
                SingleWords.Add(a, new Dictionary<string, int>());
            }
            if (!SingleWords[a].ContainsKey(b))
            {
                SingleWords[a].Add(b, 0);
            }
            ++SingleWords[a][b];
        }

        public Dictionary<string, int> GetSingleCandidates(string str)
        {
            if (str != null && SingleWords.ContainsKey(str))
            {
                return SingleWords[str];
            }
            else
            {
                return new Dictionary<string, int>();
            }
        }

        public void Clear(){
            SingleWords.Clear();
            DoubleWords.Clear();
            TripleWords.Clear();
            GC.Collect();
        }
    }
}
