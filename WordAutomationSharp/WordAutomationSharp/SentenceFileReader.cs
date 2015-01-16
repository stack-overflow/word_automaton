using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordAutomationSharp {
    public class SentenceFileReader : IDisposable{
        private readonly StreamReader ifs;
        private Queue<string> sentenceBuffer;

        public SentenceFileReader(string filename){
            ifs = new StreamReader(filename);
            sentenceBuffer = new Queue<string>();
        }

        public void Dispose(){
            ifs.Close();
        }

        public string GetNextSentence(){
            if (sentenceBuffer.Count == 0){
                var sentences = ifs.ReadLine().ExtractSentences();
                foreach (var s in sentences) sentenceBuffer.Enqueue(s);
            }
            return sentenceBuffer.Dequeue().Trim();
        }

        public bool HasMore(){
            return !ifs.EndOfStream || sentenceBuffer.Count > 0;
        }

    }
}
