using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WordAutomationSharp{
    [DataContract(IsReference = true)]
    public class WordNode{
        [DataMember]
        public List<string> OriginalNames { get; set; }

        [DataMember]
        public string NormalizedName { get; set; }

        [DataMember]
        public Dictionary<WordNode, int> Lefts { get; set; }

        [DataMember]
        public Dictionary<WordNode, int> Rights { get; set; }

        [DataMember]
        public bool Visited { get; set; }

        public int TotalConnections{
            get{
                var l = Lefts.Count > 0 ? Lefts.Max(n => n.Value) : 0;
                var r = Rights.Count > 0 ? Rights.Max(n => n.Value) : 0;
                return r > l ? r : l;
            }
        }

        public WordNode(string name){
            NormalizedName = name.ToLower();
            OriginalNames = new List<string>();
            Lefts = new Dictionary<WordNode, int>();
            Rights = new Dictionary<WordNode, int>();
        }

        public void AddOriginalName(string org){
            var found = "";
            foreach (var s in OriginalNames){
                if (s == org) return;
            }
            OriginalNames.Add(org);
        }
    }
}