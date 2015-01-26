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
        public Dictionary<string, WordNode> WordNodes { get; set; }

        //[DataMember]
        public Dictionary<Tuple<string, string>, Dictionary<string, int>> DoubleWords { get; set; }

        //[DataMember]
        public Dictionary<Tuple<string, string, string>, Dictionary<string, int>> TripleWords { get; set; }

        public WordGraph(){
            TripleWords = new Dictionary<Tuple<string, string, string>, Dictionary<string, int>>();
            DoubleWords = new Dictionary<Tuple<string, string>, Dictionary<string, int>>();
            WordNodes = new Dictionary<string, WordNode>();
        }

        public WordNode GetOrCreate(string name){
            var word = name.ToLower();
            WordNode node;
            WordNodes.TryGetValue(word, out node);
            if (node == null){
                node = new WordNode(name);
                WordNodes.Add(word, node);
            }
            node.AddOriginalName(name);
            return node;
        }

        public void MakeLink(string a, string b){
            var aNode = GetOrCreate(a);
            var bNode = GetOrCreate(b);
            try{aNode.Rights[bNode]++;}catch(Exception e){aNode.Rights.Add(bNode, 1);}
            try{bNode.Lefts[aNode]++;}catch(Exception e){bNode.Lefts.Add(aNode, 1);}
        }

        public void Clear(){
            WordNodes.Clear();
            DoubleWords.Clear();
            TripleWords.Clear();

        }
    }
}
