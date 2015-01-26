using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
using System.Xml;
using Microsoft.Win32;
using System.Web.Script.Serialization;
using System.IO;

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
        int defaultLevel = 3;
        
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
                var tuple = Tuple.Create(pp, p);
                if (!graph.DoubleWords.ContainsKey(tuple))
                {
                    graph.DoubleWords[tuple] = new Dictionary<string, int>();
                }
                if (!graph.DoubleWords[tuple].ContainsKey(current))
                {
                    graph.DoubleWords[tuple].Add(current, 0);
                }
                ++graph.DoubleWords[tuple][current];
            }
        }
        void CreateTriplePrevsMap(string[] words)
        {
            string ppp = null;
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
                else if (i == 2)
                {
                    ppp = null;
                    pp = words[i - 2].ToLower();
                    p = words[i - 1].ToLower();
                }
                else
                {
                    ppp = words[i - 3].ToLower();
                    pp = words[i - 2].ToLower();
                    p = words[i - 1].ToLower();
                }
                var triple = Tuple.Create(ppp, pp, p);

                if (!graph.TripleWords.ContainsKey(triple))
                {
                    graph.TripleWords[triple] = new Dictionary<string, int>();
                }
                if (!graph.TripleWords[triple].ContainsKey(current))
                {
                    graph.TripleWords[triple].Add(current, 0);
                }
                ++graph.TripleWords[triple][current];
            }
        }
        IEnumerable<string> GetCandidates(string[] words, int numCandidates)
        {
            var lastThreeWords = words.Select(s => s).Where(s => s != "").Skip(Math.Max(0, words.Count() - 3)).ToArray();
            Tuple<string, string> tuple;
            Tuple<string, string, string> triple;

            if (lastThreeWords.Length == 0)
            {
                tuple = Tuple.Create<string, string>(null, null);
                triple = Tuple.Create<string, string, string>(null, null, null);
            }
            else if (lastThreeWords.Length == 1)
            {
                tuple = Tuple.Create<string, string>(null, lastThreeWords.First());
                triple = Tuple.Create<string, string, string>(null, null, lastThreeWords.First());
            }
            else if (lastThreeWords.Length == 2)
            {
                tuple = Tuple.Create<string, string>(lastThreeWords.First(), lastThreeWords.Last());
                triple = Tuple.Create<string, string, string>(null, lastThreeWords.First(), lastThreeWords.Last());
            }
            else
            {
                tuple = Tuple.Create<string, string>(lastThreeWords[1], lastThreeWords[2]);
                triple = Tuple.Create<string, string, string>(lastThreeWords[0], lastThreeWords[1], lastThreeWords[2]);
            }

            IEnumerable<string> sortedRights;
            if (defaultLevel == 3 && graph.TripleWords.ContainsKey(triple))
            {
                sortedRights = (from entry in graph.TripleWords[triple].Keys orderby graph.TripleWords[triple][entry] descending select entry).Take(numCandidates);
            }
            else if (graph.DoubleWords.ContainsKey(tuple))
            {
                sortedRights = (from entry in graph.DoubleWords[tuple].Keys orderby graph.DoubleWords[tuple][entry] descending select entry).Take(numCandidates);
            }
            else
            {
                var word = words.Last(s => s != "");
                sortedRights = (from entry in graph.WordNodes[word].Rights orderby entry.Value descending select entry.Key.NormalizedName).Take(numCandidates);
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
                //this.graph = new WordGraph();
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
                    CreateTriplePrevsMap(words);
                }

                Dispatcher.Invoke(() => progress.Value = 100.0);
                Dispatcher.Invoke(ShowTree);
                });
            t.IsBackground = false;
            t.Start();
        }

        private const int Threshold = 5;

        private List<TreeViewItem> treenodes = new List<TreeViewItem>(); 
        private void ShowTree(){
            TreeViewOrder1.ItemsSource = null;
            TreeViewOrder1.Items.Clear();
            treenodes.Clear();
            return;
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
                        treenodes.Add(wordNode);
                    });

                    var sortedLefts = from entry in kv.Value.Lefts where entry.Value>Threshold orderby entry.Value descending select entry;
                    var litems = sortedLefts.Select(item => item.Key.NormalizedName + ":" + item.Value).ToList();

                    var sortedRights = from entry in kv.Value.Rights where entry.Value>Threshold orderby entry.Value descending select entry;
                    var ritems = sortedRights.Select(item => item.Key.NormalizedName + ":" + item.Value).ToList();
                    Dispatcher.Invoke(() =>{
                        var node = treenodes[i];
                        var rightitems = node.Items[1] as TreeViewItem;
                        var leftitems = node.Items[0] as TreeViewItem;
                        leftitems.ItemsSource = litems;
                        rightitems.ItemsSource = ritems;
                    });

                }
                Dispatcher.Invoke(() =>{
                    TreeViewOrder1.ItemsSource = treenodes;
                    progress.Value = 100.0;
                });
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
            var op = new OpenFileDialog{Multiselect = false, Filter = "All files (*.*)|*.*"};
            op.ShowDialog(this);
            if (op.FileName == "") return;
            ReadFile(op.FileName);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LevelComboBox.Text != "")
            {
                defaultLevel = Int32.Parse((string)((ComboBoxItem)LevelComboBox.SelectedItem).Content);
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            //load db
            var op = new OpenFileDialog{Multiselect = false, Filter = "AMG databases (*.amg)|*.amg|All files (*.*)|*.*"};
            op.ShowDialog();
            if (op.FileName == "") return;
            var xmlString = File.ReadAllText(op.FileName);
            var serializer = new DataContractSerializer(typeof (WordGraph));
            using (var sr = new StringReader(xmlString)){
                using (var reader = new XmlTextReader(sr)){
                    graph = (WordGraph)serializer.ReadObject(reader, true);
                }
            }
            ShowTree();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e){
            //new db
            var res = MessageBox.Show("Are you sure you want to create new database?", "New database", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes){
                graph.Clear();
                ShowTree();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) {
            //save db
            var serializer = new DataContractSerializer(typeof (WordGraph));
            string xmlString;
            var save = new SaveFileDialog { OverwritePrompt = true, Filter = "AMG database (*.amg)|*.amg" };
            save.ShowDialog(this);
            if(save.FileName == "") return;
            progress.IsIndeterminate = false;
            progress.Value = 20;
            var t = new Thread(() =>{
                using (var sw = new StreamWriter(save.FileName)){
                    using (var writer = new XmlTextWriter(sw)){
                        writer.Formatting = Formatting.Indented;
                        serializer.WriteObject(writer, graph);
                        writer.Flush();
                    }
                }
                Dispatcher.Invoke(() => { progress.IsIndeterminate = false; });
            });
            t.IsBackground = false;
            t.Start();


            //File.WriteAllText(save.FileName, xmlString);
        }
    }
}
