using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryBookEditor
{
    public class TEventArg<T> : EventArgs
    {
        public T Item { get; set; }
        public TEventArg()
        {

        }
        public TEventArg(T item)
        {
            Item = item;
        }
    }
}
