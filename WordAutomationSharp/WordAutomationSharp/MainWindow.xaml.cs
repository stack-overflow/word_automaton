using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Microsoft.Win32;

namespace WordAutomationSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        void TextBoxWrite(String line)
        {
            this.OutputTextBox.Text += line;
        }
        void TextBoxWriteln(String line)
        {
            this.OutputTextBox.Text += line + "\n";
        }

        WordGraph graph;
        Dictionary<List<string>, Dictionary<string, int>> wordListMap = new Dictionary<List<string>, Dictionary<string, int>>(new ListComparer<string>());
        void CreateTwoPrevsMap(string[] words)
        {
            string pp = null;
            string p = null;
            for (int i = 0; i < words.Length; ++i)
            {
                string current = words[i].ToLower();
                if (i == 0)
                {
                    pp = p = null;
                }
                else if (i == 1)
                {
                    pp = null;
                    p = words[i - 1].ToLower();
                }
                else
                {
                    pp = words[i - 2].ToLower();
                    p = words[i - 1].ToLower();
                }
                var l = new List<string>() { pp, p };
                if (!wordListMap.ContainsKey(l))
                {
                    wordListMap[l] = new Dictionary<string, int>();
                }
                if (!wordListMap[l].ContainsKey(current))
                {
                    wordListMap[l].Add(current, 0);
                }
                ++wordListMap[l][current];
            }
        }
        IEnumerable<string> GetCandidates(string[] words, int numCandidates)
        {
            List<string> lastTwoWords = words.Select(s => s).Where(s => s != "").Skip(Math.Max(0, words.Count() - 2)).Take(2).ToList();
            if (words.Length == 0)
            {
                lastTwoWords.Add(null); lastTwoWords.Add(null);
            }
            else if (words.Length == 1)
            {
                lastTwoWords = new List<string>() { null, lastTwoWords.First() };
            }
            IEnumerable<string> sortedRights;
            if (wordListMap.ContainsKey(lastTwoWords))
            {
                sortedRights = (from entry in wordListMap[lastTwoWords].Keys orderby wordListMap[lastTwoWords][entry] descending select entry).Take(numCandidates);
            }
            else
            {
                sortedRights = null;
            }
            return sortedRights;
        }


        public MainWindow()
        {
            InitializeComponent();

            this.graph = new WordGraph();
            
        }

        private void ReadFile(string file){
            var t = new Thread(() =>{
                var cfr = new SentenceFileReader(file);
                this.graph = new WordGraph();
                var str = "";
                while (cfr.HasMore()){
                    progress.Dispatcher.Invoke(() => progress.Value = cfr.GetProgress());
                    var sentence = cfr.GetNextSentence();
                    var words = sentence.ExtractWords();
                    if (words.Length > 1){
                        for (int i = 0, j = 1; j < words.Length; ++i, ++j){
                            graph.MakeLink(words[i], words[j]);
                        }
                    } else if (words.Length == 1 && words[0] != ""){
                        graph.GetOrCreate(words[0]);
                    }
                    CreateTwoPrevsMap(words);
                }
                Dispatcher.Invoke(() => progress.Value = 100.0);
                Dispatcher.Invoke(ShowTree);
            });
            t.IsBackground = false;
            t.Start();
            
        }

        private void OutputTreeView_Loaded(object sender, RoutedEventArgs e){
            ShowTree();
        }

        private void ShowTree(){
            OutputTreeView.Items.Clear();
            var t = new Thread(() =>{
                var nodes = this.graph.WordNodes.OrderBy(w => w.Key).ToList();
                for(var i=0; i<nodes.Count; ++i){
                    Dispatcher.Invoke(() => progress.Value = (double)i/nodes.Count * 100.0);
                    var kv = nodes[i];
                    Dispatcher.Invoke(() =>{
                        var wordNode = new TreeViewItem{Header = kv.Value.NormalizedName};
                        var lefts = new TreeViewItem{Header = "Left"};
                        lefts.Items.Clear();
                        var rights = new TreeViewItem{Header = "Right"};
                        rights.Items.Clear();
                        wordNode.Items.Add(lefts);
                        wordNode.Items.Add(rights);
                        OutputTreeView.Items.Add(wordNode);
                    });

                    var sortedLefts = from entry in kv.Value.Lefts orderby entry.Value descending select entry;
                    var list = sortedLefts.Select(item => item.Key.NormalizedName + ":" + item.Value).ToList();
                    Dispatcher.Invoke(() => {
                        var node = OutputTreeView.Items[i] as TreeViewItem;
                        var leftitems = node.Items[0] as TreeViewItem;
                        var rightitems = node.Items[1] as TreeViewItem;
                        leftitems.ItemsSource = list;
                    });

                    var sortedRights = from entry in kv.Value.Rights orderby entry.Value descending select entry;
                    list = sortedRights.Select(item => item.Key.NormalizedName + ":" + item.Value).ToList();
                    Dispatcher.Invoke(() => {
                        var node = OutputTreeView.Items[i] as TreeViewItem;
                        var leftitems = node.Items[0] as TreeViewItem;
                        var rightitems = node.Items[1] as TreeViewItem;
                        rightitems.ItemsSource = list;
                    });

                }
                Dispatcher.Invoke(() => progress.Value = 100.0);
            });
            t.IsBackground = false;
            t.Start();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            var words = textBox1.Text.ToLower().ExtractWords();
            if (words.Length <= 0)
            {
                return;
            }
            try
            {
                int numCandidates = 5;
                var sortedRights = GetCandidates(words, numCandidates);
                if (sortedRights == null)
                {
                    var word = words.Last(s => s != "");
                    sortedRights = (from entry in graph.WordNodes[word].Rights orderby entry.Value descending select entry.Key.NormalizedName).Take(numCandidates);
                }

                listBox1.Items.Clear();
                foreach (var zord in sortedRights)
                {
                    listBox1.Items.Add(zord);
                }
            }

            catch { }
            listBox1.UpdateLayout();
        }

        private void Button_Click(object sender, RoutedEventArgs e){
            var op = new OpenFileDialog();
            op.Multiselect = false;
            op.ShowDialog(this);
            ReadFile(op.FileName);
        }
    }
}
