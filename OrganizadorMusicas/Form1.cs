using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OrganizadorMusicas
{
    public partial class Form1 : Form
    {
        private string[] _extensoes = new[] { "*.mp3", "*.wma", "*.wav", "*.mp4", "*.m4a" };
        private WMPLib.IWMPPlayer4 _player;

        public Form1()
        {
            InitializeComponent();
            _player = new WMPLib.WindowsMediaPlayer();
        }

        private void Load(IEnumerable<FileInfo> arquivos)
        {
            foreach (var arquivo in arquivos)
            {
                var item = new ListViewItem(arquivo.Name);
                item.Tag = arquivo;
                listView1.Items.Add(item);
            }
        }

        private void abrirToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var arquivos = new List<FileInfo>();
                foreach (var arquivo in openFileDialog1.FileNames)
                {
                    arquivos.Add(new FileInfo(arquivo));
                }
                Load(arquivos);
            }
        }

        private void abrirPastaToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var pasta = new DirectoryInfo(folderBrowserDialog1.SelectedPath);
                var arquivos = _extensoes
                    .SelectMany(ext => pasta.GetFiles(ext, SearchOption.TopDirectoryOnly))
                    .ToArray();
                Load(arquivos);
            }
        }

        private void abrirPastarecursivamenteToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                var pasta = new DirectoryInfo(folderBrowserDialog1.SelectedPath);
                var arquivos = _extensoes
                    .SelectMany(ext => pasta.GetFiles(ext, SearchOption.AllDirectories))
                    .ToArray();
                Load(arquivos);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var arquivo = listView1.SelectedItems[0].Tag as FileInfo;
                _player.controls.stop();
                _player.URL = arquivo.FullName;
                _player.controls.play();
            }
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            var tempo = TimeSpan.FromSeconds(_player.controls.currentPosition);
            var total = TimeSpan.FromSeconds(_player.currentMedia?.duration ?? 0);

            trackBar1.Maximum = (int)total.TotalSeconds > 0 ? (int)total.TotalSeconds : 1;
            trackBar1.Value = (int)tempo.TotalSeconds > 0 ? (int)tempo.TotalSeconds : 1;

            if (total.TotalSeconds > 0)
            {
                label1.Text = $"{tempo:mm\\:ss} / {total:mm\\:ss}";
            }
            else
            {
                label1.Text = "00:00 / 00:00";
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var position = trackBar1.Value;
            _player.controls.currentPosition = position;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _player.settings.mute = !_player.settings.mute;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            _player.settings.volume = trackBar2.Value;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _player.controls.stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _player.controls.pause();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _player.controls.play();
        }

        private void btnManter_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                return;
            }

            var indice = listView1.SelectedIndices[0];
            var itenToRemove = listView1.SelectedItems[0];
            listView1.Items.Remove(itenToRemove);
            listView1.SelectedIndices.Add(indice < listView1.Items.Count ? indice : 0);
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0)
            {
                return;
            }

            if (checkBox1.Checked)
            {
                var resposta = MessageBox.Show("Deseja realmente excluir o item selecionado?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resposta != DialogResult.Yes)
                {
                    return;
                }
            }

            var indice = listView1.SelectedIndices[0];
            var itenToRemove = listView1.SelectedItems[0];
            var arquivo = itenToRemove.Tag as FileInfo;
            if (arquivo != null && arquivo.Exists)
            {
                try
                {
                    arquivo.Delete();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir o arquivo: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            listView1.Items.Remove(itenToRemove);
            listView1.SelectedIndices.Add(indice < listView1.Items.Count ? indice : 0);
        }
    }
}
