using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeaponsEditor__
{
    public partial class AreYouSure : Form
    {
        public AreYouSure()
        {
            InitializeComponent();
        }

        public bool areYouSureChecked
        {
            get { return areYouSureC.Checked; }
            set { areYouSureC.Checked = value; }
        }
    }
}
