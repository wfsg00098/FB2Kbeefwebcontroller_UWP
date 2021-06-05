using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FB2Kbeefwebcontroller_UWP
{
    public class Listitem
    {
        public string title { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string id { get; set; }
        public string duration { get; set; }

    }

    public class ListitemModel
    {
        private Listitem defaultListitem = new Listitem();
        public Listitem DefaultListitem { get { return this.defaultListitem; } }
        private ObservableCollection<Listitem> listitems = new ObservableCollection<Listitem>();
        public  ObservableCollection<Listitem> Listitems { get { return this.listitems; } }


    }
}
