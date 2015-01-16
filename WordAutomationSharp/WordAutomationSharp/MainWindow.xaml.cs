using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WordAutomationSharp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            var file = "test.txt";
            var cfr = new SentenceFileReader(file);
            var graph = new WordGraph();
            var str = "";
            while (cfr.HasMore()){
                var sentence = cfr.GetNextSentence();
                var words = sentence.ExtractWords();
                if (words.Length > 1){
                    for (int i = 0, j = 1; j < words.Length; ++i, ++j){
                        graph.MakeLink(words[i], words[j]);
                    }
                }
                else if (words.Length == 1 && words[0] != ""){
                    graph.GetOrCreate(words[0]);
                }
            }

            var threshold = 0;
            foreach (var kv in graph.WordNodes){
                var wordprinted = false;
                foreach (var n in kv.Value.Lefts){
                    if (n.Value >= threshold){
                        if (!wordprinted){
                            Console.Out.Write(kv.Key + "\n- left: ");
                            wordprinted = true;
                        }
                        Console.Out.Write(n.Key.NormalizedName + ":" + n.Value + " ");
                    }
                }
                if(wordprinted)Console.Out.Write("\n- right: ");
                foreach (var n in kv.Value.Rights){
                    if (n.Value >= threshold){
                        if (!wordprinted){
                            Console.Out.Write(kv.Key + "\n- right: ");
                            wordprinted = true;
                        }
                        Console.Out.Write(n.Key.NormalizedName + ":" + n.Value + " ");
                    }
                }
                if(wordprinted) Console.Out.Write("\n\n");
            }
            Output.UpdateLayout();
            var superthreshold = 100;
            var printer = new SentencePrinter();
            foreach (var kv in graph.WordNodes){
              printer.Print(kv.Value, kv.Value.NormalizedName, superthreshold, 0);  
            }
        }
    }
}
