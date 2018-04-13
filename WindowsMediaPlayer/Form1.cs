using System;
using System.ComponentModel;
using System.Windows.Forms;
using WMPLib;

namespace Player
{
    public partial class Form1 : Form
    {
        WindowsMediaPlayer x = new WindowsMediaPlayer();
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;
            x.URL = textBox1.Text;
            textBox1.ReadOnly = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            x.controls.play();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            x.controls.pause();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            x.controls.stop();
        }
    }
}
