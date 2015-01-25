using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordAutomationSharp {
    public class SentenceFileReader{
        //private readonly StreamReader ifs;
        private Queue<string> sentenceBuffer;
        private string[] Lines;
        private int currentLine;
        public SentenceFileReader(string filename){
            //ifs = new StreamReader(filename);
            Lines = File.ReadAllLines(filename);
            currentLine = 0;
            sentenceBuffer = new Queue<string>();
        }

        public string ReadLine(){
            return Lines[currentLine++];
        }

        public string GetNextSentence(){
            if (sentenceBuffer.Count == 0){
                var sentences = ReadLine().ExtractSentences();
                foreach (var s in sentences) sentenceBuffer.Enqueue(s);
            }
            
            if(sentenceBuffer.Count>0)
                return sentenceBuffer.Dequeue().Trim();
            return "";
        }

        public bool HasMore(){
            return currentLine < Lines.Length || sentenceBuffer.Count > 0;
        }

        public double GetProgress(){
            return (double)currentLine/Lines.Length * 100.0;
        }

    }
}
