using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordAutomationSharp {
    public class WordGraph{
        public Dictionary<string, WordNode> WordNodes { get; set; }
        public WordNode Head { get; set; }

        public WordGraph(){
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
            try{aNode.Lefts[bNode]++;}catch(Exception e){aNode.Lefts.Add(bNode, 1);}
            try{bNode.Rights[aNode]++;}catch(Exception e){bNode.Rights.Add(aNode, 1);}
        }
    }
}
