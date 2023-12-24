using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace MangaReader
{
    public partial class MangaReader : Form
    {
        private bool _secondaryControlMode = false;
        private int _index = 0;
        private List<string> _files = new List<string>();
        private Size _resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
        private Point _location = new Point(0, 0);
        private string[] _args = Environment.GetCommandLineArgs();

        public MangaReader()
        {
            InitializeComponent();
        }

        private void MangaReader_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    ChangePage(-1);
                    break;
                case Keys.Right:
                    ChangePage(1);
                    break;
                case Keys.Up:
                    ScrollPage(1);
                    break;
                case Keys.Down:
                    ScrollPage(-1);
                    break;
                case Keys.Add:
                    ChangePageSize(1);
                    break;
                case Keys.Subtract:
                    ChangePageSize(-1); 
                    break;
                case Keys.ControlKey:
                    _secondaryControlMode = true;
                    break;
            }
        }

        private void MangaReader_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Escape:
                    Environment.Exit(0);
                    break;
                case (char)Keys.Space:
                    if (_secondaryControlMode) OpenFolder();
                    else OpenFile();
                    break;
            }
        }

        private void MangaReader_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.ControlKey:
                    _secondaryControlMode = false;
                    break;
            }
        }

        private void MangaReader_MouseWheel(object sender, MouseEventArgs e)
        {
            int scroll = e.Delta / 120;
            if (_secondaryControlMode) ChangePageSize(scroll, 3);
            else ScrollPage(scroll, 3);
        }
        
        private void MangaReader_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if(e.Location.X > _resolution.Width / 2) ChangePage(1);
                else ChangePage(-1);
            }
            else if (e.Button == MouseButtons.Middle)
            {
                if (_secondaryControlMode) OpenFolder();
                else OpenFile();
            }
        }

        private void MangaReader_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_secondaryControlMode)
                {
                    Environment.Exit(0);
                }
            }
        }

        private void MangaReader_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                _secondaryControlMode = true;
            }
        }

        private void MangaReader_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                _secondaryControlMode = false;
            }
        }

        private void MangaReader_Shown(object sender, EventArgs e)
        {
            if (_args.Length > 1)
            {
                Open(_args[1]);
            }
            else OpenFile();
            pictureBox1.Size = _resolution;
            pictureBox1.Location = _location;
        }

        private void ChangePage(int direction)
        {
            int idx = _index + direction;
            if (idx < 0) idx = _files.Count - 1;
            if (idx > _files.Count - 1) idx = 0;
            _index = idx;
            ReloadPage();
        }
        
        private void ChangePageSize(int direction, int mouseFactor = 1)
        {
            pictureBox1.Size = new Size(pictureBox1.Width += mouseFactor * direction * 48, 
                pictureBox1.Height += mouseFactor * direction * 27);
            pictureBox1.Location = new Point(pictureBox1.Location.X - mouseFactor * direction * 24, 
                CheckPageLocation(pictureBox1.Location.Y));
        }

        private void ScrollPage(int direction, int mouseFactor = 1)
        {
            pictureBox1.Location = new Point(pictureBox1.Location.X, 
                CheckPageLocation(pictureBox1.Location.Y + (mouseFactor * direction * 30)));
        }

        private void OpenFolder()
        {
            using(FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if(result == DialogResult.OK)
                {
                    Open(dialog.SelectedPath);
                }
            }
            _secondaryControlMode = false;
        }

        private void OpenFile()
        {
            using(OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory) + "Users\\" + 
                    Environment.UserName + "\\Desktop\\";
                DialogResult result = dialog.ShowDialog();
                if( result == DialogResult.OK )
                {
                    Open(dialog.FileName);
                }
            }
        }

        private void Open(string path)
        {
            bool isFile = Path.HasExtension(path);
            string folderPath = isFile ? Path.GetDirectoryName(path) : path;
            _files = Directory.GetFiles(folderPath, "*.??g", SearchOption.AllDirectories).ToList();
            try
            {
                _files = _files.OrderBy(x => int.Parse(Regex.Replace(x, "[^0-9]+", "0"))).ToList<string>();
            }
            catch
            {
                ;
            }
            _index = isFile ? _files.IndexOf(path) : 0;
            pictureBox1.Size = _resolution;
            pictureBox1.Location = new Point(0, 0);
            ReloadPage();
        }


        private void ReloadPage()
        {
            if (_files.Count > 0)
            {
                pictureBox1.ImageLocation = _files[_index];
                pictureBox1.Refresh();
                pictureBox1.Location = new Point(pictureBox1.Location.X, CheckPageLocation(0));
            }
        }

        private int CheckPageLocation(int Y)
        {
            if(pictureBox1.Size.Height < _resolution.Height - Y) Y = _resolution.Height - pictureBox1.Size.Height;
            if (Y > 0) Y = 0;
            if(pictureBox1.Size.Height < _resolution.Height) Y = (_resolution.Height - pictureBox1.Size.Height) / 2;
            return Y;
        }
    }
}
