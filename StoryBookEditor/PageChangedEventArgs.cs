using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StoryBookEditor
{
    public class PageChangedEventArgs : EventArgs
    {
        public StoryBranchModel Branch { get; set; }
    }
}
