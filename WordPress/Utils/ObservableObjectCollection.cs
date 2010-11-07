using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WordPress
{
    public class ObservableObjectCollection:ObservableCollection<object>
    {
        public ObservableObjectCollection() { }

        public ObservableObjectCollection(List<object> collection)
        {
            collection.ForEach(obj => Add(obj));
        }
    }
}
