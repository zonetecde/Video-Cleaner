using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp3
{
    public class PourcentAndPreview
    {
        public PourcentAndPreview(double pourcent, Bitmap img)
        {
            this.pourcent = pourcent;
            this.img = img;
        }

        public double pourcent { get; set; }
        public Bitmap img { get; set; }
    }
}
