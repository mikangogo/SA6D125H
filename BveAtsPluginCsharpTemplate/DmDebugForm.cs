using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AtsPlugin
{
    public partial class DmDebugForm : Form
    {
        public void SetText(Label label, string value)
        {
            if (!this.Visible)
            {
                return;
            }

            label.Text = label.Name + "\n" + value;
        }

        public DmDebugForm()
        {
            InitializeComponent();
        }
    }
}
