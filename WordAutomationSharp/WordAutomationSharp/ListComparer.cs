using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordAutomationSharp
{
    class ListComparer<T> : IEqualityComparer<List<T>>
    {
        public bool Equals(List<T> x, List<T> y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<T> obj)
        {
            int hashcode = 0;
            foreach (T t in obj)
            {
                if (t != null)
                    hashcode ^= t.GetHashCode();
                else
                    hashcode ^= 0;
            }
            return hashcode;
        }
    }
}
