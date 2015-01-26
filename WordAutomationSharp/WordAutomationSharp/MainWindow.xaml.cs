using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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
        
        void CreateSingleWords(string[] words)
        {
            if (words.Length > 1)
            {
                for (int i = 0, j = 1; j < words.Length; ++i, ++j)
                {
                    graph.MakeSingleLink(words[i], words[j]);
                }
            }
        }
        void CreateDoubleWords(string[] words)
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
        void CreateTripleWords(string[] words)
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
            else if (defaultLevel != 1 && graph.DoubleWords.ContainsKey(tuple))
            {
                sortedRights = (from entry in graph.DoubleWords[tuple].Keys orderby graph.DoubleWords[tuple][entry] descending select entry).Take(numCandidates);
            }
            else
            {
                var word = words.Last(s => s != "");
                sortedRights = (from entry in graph.GetSingleCandidates(tuple.Item2) orderby entry.Value descending select entry.Key).Take(numCandidates);
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
                    for (var i = 0; i < words.Length; ++i) words[i] = words[i].ToLower();
                    CreateSingleWords(words);
                    CreateDoubleWords(words);
                    CreateTripleWords(words);
                }

                Dispatcher.Invoke(() =>{
                    progress.Value = 100.0;
                    ShowTree();
                });
            });
            t.IsBackground = false;
            t.Start();
        }

        private const int Threshold = 10;

        private List<TreeViewItem> treenodes1 = new List<TreeViewItem>();
        private List<TreeViewItem> treenodes2 = new List<TreeViewItem>();
        private List<TreeViewItem> treenodes3 = new List<TreeViewItem>(); 

        private void ShowTree(){
            GetAutocompletions();
            TreeViewOrder1.ItemsSource = null;
            TreeViewOrder1.Items.Clear();
            TreeViewOrder2.ItemsSource = null;
            TreeViewOrder2.Items.Clear();
            TreeViewOrder3.ItemsSource = null;
            TreeViewOrder3.Items.Clear();
            treenodes1.Clear();
            treenodes2.Clear();
            treenodes3.Clear();
            fixprogress(0);
            progress.IsIndeterminate = true;
            ProgressLabel.Text = "Generating views";
            new Thread(PopulateTreeOrder1) { IsBackground = false }.Start();
            new Thread(PopulateTreeOrder2) { IsBackground = false }.Start();
            new Thread(PopulateTreeOrder3) { IsBackground = false }.Start();
        }

        private int fixprogressstaticvalue = 0;
        private void fixprogress(int i){
            if (i == 0) fixprogressstaticvalue = 0;
            else fixprogressstaticvalue += i;
            if(fixprogressstaticvalue == 6) {
                ProgressLabel.Text = "Finished";
                progress.IsIndeterminate = false;
                progress.Value = 100;
            }
        }
  
        private void PopulateTreeOrder1(){
            var g = (from node in graph.SingleWords orderby node.Key descending select node).ToList();
            for (var i = 0; i < g.Count; ++i){
                var n = g[i];

                var items = from node in n.Value where node.Value > Threshold orderby node.Value descending select node;

                Dispatcher.Invoke(() =>{
                    if (items.Any()) treenodes1.Add(new TreeViewItem{Header = n.Key, ItemsSource = items});
                });
            }
            Dispatcher.Invoke(() => {
                TreeViewOrder1.ItemsSource = treenodes1;
                fixprogress(1);
            });
        }

        private void PopulateTreeOrder2() {
            var g = (from node in graph.DoubleWords orderby node.Key descending select node).ToList();
            for(var i = 0; i < g.Count; ++i) {
                var n = g[i];

                var items = from node in n.Value where node.Value > Threshold orderby node.Value descending select node;

                Dispatcher.Invoke(() =>{
                    if(items.Any()) treenodes2.Add(new TreeViewItem { Header = string.Format("{0} {1}",n.Key.Item1??"-", n.Key.Item2??"-"), ItemsSource = items });
                });
            }
            Dispatcher.Invoke(() => {
                TreeViewOrder2.ItemsSource = treenodes2;
                fixprogress(2);
            });
        }

        private void PopulateTreeOrder3() {
            var g = (from node in graph.TripleWords orderby node.Key descending select node).ToList();
            for(var i = 0; i < g.Count; ++i) {
                var n = g[i];

                var items = from node in n.Value where node.Value > Threshold orderby node.Value descending select node;

                Dispatcher.Invoke(() =>{
                    if(items.Any()) treenodes3.Add(new TreeViewItem { Header = string.Format("{0} {1} {2}", n.Key.Item1??"-", n.Key.Item2??"-", n.Key.Item3??"-"), ItemsSource = items });
                });
            }
            Dispatcher.Invoke(() => {
                TreeViewOrder3.ItemsSource = treenodes3;
                fixprogress(3);
            });
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e){
            GetAutocompletions();
        }

        private void GetAutocompletions(){
            var words = textBox1.Text.ToLower().ExtractWords();
            if (words.Length <= 0){
             //   return;
            }
            try{
                int numCandidates = 5;
                var sortedRights = GetCandidates(words, numCandidates);

                listBox1.Items.Clear();
                foreach (var zord in sortedRights){
                    listBox1.Items.Add(zord);
                }
            } catch{}
            listBox1.UpdateLayout();
        }

        private void Button_Click(object sender, RoutedEventArgs e){
            var op = new OpenFileDialog{Multiselect = false, Filter = "All files (*.*)|*.*"};
            op.ShowDialog(this);
            if (op.FileName == "") return;
            progress.Value = 0;
            ProgressLabel.Text = "Reading";
            ReadFile(op.FileName);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LevelComboBox.Text != "")
            {
                defaultLevel = Int32.Parse((string)((ComboBoxItem)LevelComboBox.SelectedItem).Content);
            }
            GetAutocompletions();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseButtonEventArgs e){
            var c = textBox1.Text.Last();
            var space = c != ' ';
            textBox1.Text += space ? " " : "" + listBox1.SelectedItem + " ";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            //load db
            var op = new OpenFileDialog{Multiselect = false, Filter = "AMG databases (*.amg)|*.amg|All files (*.*)|*.*"};
            op.ShowDialog();
            if (op.FileName == "") return;
            progress.IsIndeterminate = true;
            ProgressLabel.Text = "Loading";
            var t = new Thread(() =>{
                var serializer = new DataContractSerializer(typeof (WordGraph));
                using (var sr = new StreamReader(op.FileName)){
                    using (var reader = new XmlTextReader(sr)){
                        graph = (WordGraph) serializer.ReadObject(reader, true);
                    }
                }
                Dispatcher.Invoke(() => {
                    progress.IsIndeterminate = false;
                    ShowTree();
                });
            });
            t.IsBackground = false;
            t.Start();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e){
            //new db
            var res = MessageBox.Show("Are you sure you want to create new database?", "New database", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes){
                graph.Clear();
                listBox1.ItemsSource = null;
                listBox1.Items.Clear();
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
            progress.IsIndeterminate = true;
            ProgressLabel.Text = "Saving";
            var t = new Thread(() =>{
                using (var sw = new StreamWriter(save.FileName)){
                    using (var writer = new XmlTextWriter(sw)){
                        //writer.Formatting = Formatting.Indented;
                        serializer.WriteObject(writer, graph);
                        writer.Flush();
                    }
                }
                Dispatcher.Invoke(() =>{
                    progress.IsIndeterminate = false;
                    progress.Value = 100;
                    ProgressLabel.Text = "Finished";
                });
            });
            t.IsBackground = false;
            t.Start();
        }
    }
}
