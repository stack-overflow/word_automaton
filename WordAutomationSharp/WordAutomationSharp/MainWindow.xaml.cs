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
        public MainWindow()
        {
            InitializeComponent();

            var file = "test.txt";
            var cfr = new SentenceFileReader(file);
            this.graph = new WordGraph();
            var str = "";
            while (cfr.HasMore())
            {
                var sentence = cfr.GetNextSentence();
                var words = sentence.ExtractWords();
                if (words.Length > 1)
                {
                    for (int i = 0, j = 1; j < words.Length; ++i, ++j)
                    {
                        graph.MakeLink(words[i], words[j]);
                    }
                }
                else if (words.Length == 1 && words[0] != "")
                {
                    graph.GetOrCreate(words[0]);
                }
            }

            var threshold = 0;
            foreach (var kv in graph.WordNodes)
            {
                var wordprinted = false;
                foreach (var n in kv.Value.Lefts)
                {
                    if (n.Value >= threshold)
                    {
                        if (!wordprinted)
                        {
                            TextBoxWrite(kv.Key + "\n- left: ");
                            wordprinted = true;
                        }
                        TextBoxWrite(n.Key.NormalizedName + ":" + n.Value + " ");
                    }
                }
                if (wordprinted) TextBoxWrite("\n- right: ");
                foreach (var n in kv.Value.Rights)
                {
                    if (n.Value >= threshold)
                    {
                        if (!wordprinted)
                        {
                            TextBoxWrite(kv.Key + "\n- right: ");
                            wordprinted = true;
                        }
                        TextBoxWrite(n.Key.NormalizedName + ":" + n.Value + " ");
                    }
                }
                if (wordprinted) TextBoxWrite("\n\n");
            }
            OutputTextBox.UpdateLayout();
            var superthreshold = 100;
            var printer = new SentencePrinter();
            foreach (var kv in graph.WordNodes)
            {
                printer.Print(kv.Value, kv.Value.NormalizedName, superthreshold, 0, TextBoxWrite);
            }
        }

        private void OutputTreeView_Loaded(object sender, RoutedEventArgs e)
        {

            foreach (var kv in this.graph.WordNodes)
            {
                TreeViewItem wordNode = new TreeViewItem();
                wordNode.Header = kv.Value.NormalizedName;

                TreeViewItem lefts = new TreeViewItem();
                lefts.Header = "Left";
                var sortedLefts = from entry in kv.Value.Lefts orderby entry.Value ascending select entry;
                foreach (var item in sortedLefts)
                {
                    lefts.Items.Add(item.Key.NormalizedName + ":" + item.Value);

                }
                TreeViewItem rights = new TreeViewItem();
                rights.Header = "Right";
                var sortedRights = from entry in kv.Value.Rights orderby entry.Value ascending select entry;
                foreach (var item in sortedRights)
                {
                    rights.Items.Add(item.Key.NormalizedName + ":" + item.Value);

                }
                if (lefts.Items.Count > 0)
                {
                    wordNode.Items.Add(lefts);
                }
                if (rights.Items.Count > 0)
                {
                    wordNode.Items.Add(rights);
                }
                var tree = sender as TreeView;
                tree.Items.Add(wordNode);
            }
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            var words = textBox1.Text.ExtractWords();
            if (words.Length <= 0)
            {
                return;
            }

            try
            {
                var word = words.Last();
                if (word == "") word = words[words.Length - 2];
                if (word == "") word = words[words.Length - 3];
                var sortedRights = (from entry in graph.WordNodes[word].Rights orderby entry.Value ascending select entry.Key.NormalizedName).Take(5);

                listBox1.Items.Clear();
                foreach (var zord in sortedRights)
                {
                    listBox1.Items.Add(zord);
                }
            }
            catch { }
            listBox1.UpdateLayout();
        }
    }
}
