using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WordAutomationSharp{
    public static class StringExtensions{
        public static string[] SplitWhitespace(this string str){
            return str.Split(null);
        }

        public static string[] ExtractSentences(this string str) {
            return str.Split(new []{'.','?','!','\"'}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] ExtractWords(this string str){
            return str.Split(new[]{' ', '.', ',', ';', '\"', '?', '!', '\n', '\r', '(', ')', '[', ']','_'},
                StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
