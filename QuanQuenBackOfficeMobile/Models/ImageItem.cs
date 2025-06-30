using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanQuenBackOfficeMobile.Models
{
    public partial class ImageItem : ObservableObject
    {
        [ObservableProperty]
        private ImageSource source;

        public bool HasImage => Source != null;
    }
}
