using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;

namespace Veml
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void SaveCoordToFile(string value)
        {
            string filePath = "coordinates.txt";
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(value);
            }
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string coordinates = longitude.Text + " " + latitude.Text;
            SaveCoordToFile(coordinates);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
