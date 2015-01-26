using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordAutomationSharp {
    public class SentencePrinter {
        //private Dictionary<WordNode, bool> nodeVisited;

        //public SentencePrinter(){
        //    nodeVisited = new Dictionary<WordNode, bool>();
        //}

        //public void Print(WordNode node, string name, int superthreshold, int depth, Action<String> print_func){
        //    bool wasVisited = true;
        //    try{wasVisited = nodeVisited[node];} catch (Exception){nodeVisited.Add(node, true);}
        //    nodeVisited[node] = true;
        //    foreach (var p in node.Rights){
        //        if (p.Value >= superthreshold && !wasVisited){
        //            Print(p.Key, name + " " + p.Key.NormalizedName, superthreshold, depth + 1, print_func);
        //        }
        //        else if (depth > 1){
        //            print_func(name + " " + p.Key.NormalizedName);
        //        }
        //    }
        //}
    }
}
