using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using Codeplex.Data; // DynamicJson はこれ。

namespace AepMultiRender
{
    public partial class Form1 : Form
    {

		private AepMultiRender amr;
        public Form1()
        {
			amr = new AepMultiRender(this);
			InitializeComponent();

        }
		//------------------------------------------------------------------------
		private void Form1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.All;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}
		//------------------------------------------------------------------------
		private void Form1_DragDrop(object sender, DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

			GetCmd(files);
		}
		//------------------------------------------------------------------------
		private bool SetAepPath(string p)
		{
			bool ret = false;
			if (amr.IsRunning == true) return ret;
			if (amr.SetAepPath(p) == true)
			{
				lbAep.Text = amr.AepFileName;
				btnExec.Enabled = true;
				ret = true;

			}
			else
			{
				lbAep.Text = "Aep File Drag&&Drop,here!";
				btnExec.Enabled = false;
				ret = false;
			}
			return ret;
		}
		//------------------------------------------------------------------------
		private void ChkIsRunning()
		{
			if (amr.IsRunning)
			{
				quitToolStripMenuItem.Enabled =
				btnExec.Enabled = false;
				btnStop.Enabled = true;
				numJobCount.Enabled = false;
				cmbAerender.Enabled = false;
			}
			else
			{
				btnStop.Enabled = false;
				quitToolStripMenuItem.Enabled = true;
				btnExec.Enabled = amr.Enabled;
				numJobCount.Enabled = true;
				cmbAerender.Enabled = true;
			}
		}
		//------------------------------------------------------------------------
		private void Form1_Load(object sender, EventArgs e)
		{
			PrefLoad();

			if (amr.IsAerender==false)
			{
				MessageBox.Show("aerender.exeが見つかりません!","AepMultiRender");
				Application.Exit();
			}

			string[] lst = amr.AerenderList;
			cmbAerender.Items.Clear();
			foreach(string p in lst)
			{
				cmbAerender.Items.Add(p);
			}
			cmbAerender.SelectedIndex = amr.SelectedIndex;

			numJobCount.Value = amr.JobCount;

			GetCmd(System.Environment.GetCommandLineArgs());
			ChkIsRunning();
			amr.Finished += Amr_Finished1; ;

		}
		//------------------------------------------------------------------------
		private void Amr_Finished1(object sender, FinishedEventArgs e)
		{
			ChkIsRunning();
		}

		//------------------------------------------------------------------------
		public void GetCmd(string[] arg)
		{
			/*
				-JobCount 8
				-aerender "aaa"
				-start
			*/
			bool ret = false;
			if (amr.IsRunning == true) return;
			if (arg.Length > 0)
			{
				foreach (string p in arg)
				{
					if (SetAepPath(p) == true)
					{
						ret = true;
						break;
					}
				}

			}
			if (ret==false)
			{
				SetAepPath("");
			}
		}
		//------------------------------------------------------------------------
		private void btnExec_Click(object sender, EventArgs e)
		{
			Exec();

		}
		//------------------------------------------------------------------------
		public void Exec()
		{
			if (amr.IsRunning == true) return;
			if (amr.Exec() == true)
			{
				ChkIsRunning();
				this.BringToFront();
				this.Activate();
				this.TopMost = true;
				this.TopMost = false;
			}

		}
		//------------------------------------------------------------------------
		private void numJobCount_ValueChanged(object sender, EventArgs e)
		{
			amr.JobCount = (int)numJobCount.Value;
		}
		//------------------------------------------------------------------------
		private void cmbAerender_SelectedIndexChanged(object sender, EventArgs e)
		{
			amr.AerenderPath = cmbAerender.SelectedItem.ToString();
		}
		//------------------------------------------------------------------------
		public void PrefSave()
		{
			dynamic root = new DynamicJson(); // ルートのコンテナ

			root.AerenderPath = amr.AerenderPath;
			root.JobCount = amr.JobCount;

			root.LX = this.Location.X;
			root.LY = this.Location.Y;
			root.SW = this.Size.Width;
			root.SH = this.Size.Height;

			string str = root.ToString();

			string p = Path.Combine(Application.UserAppDataPath, Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".pref");
			File.WriteAllText(p, str, Encoding.GetEncoding("utf-8"));
		}
		//------------------------------------------------------------------------
		public void PrefLoad()
		{
			string p = Path.Combine(Application.UserAppDataPath, Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".pref");

			if (File.Exists(p) == false)
			{
				return;
			}

			string str = File.ReadAllText(p, Encoding.GetEncoding("utf-8"));

			var json = DynamicJson.Parse(str);
			int jc;
			int x=0, y=0, w=0, h=0;
			if (json.IsDefined("AerenderPath") == true)
			{
				string ap = json.AerenderPath;
				amr.AerenderPath = ap;
			}
			if (json.IsDefined("JobCount") == true)
			{
				jc = (int)json.JobCount;
				amr.JobCount = jc;
			}
			if (json.IsDefined("LX")==true)
			{
				x = (int)json.LX;
			}
			if (json.IsDefined("LY") == true)
			{
				y = (int)json.LY;
			}
			if (json.IsDefined("SW") == true)
			{
				w = (int)json.SW;
			}
			if (json.IsDefined("SH") == true)
			{
				h = (int)json.SH;
			}
			if((x>0)&&(y>0) && (w > 0) && (h > 0))
			{
				this.Location = new Point(x, y);
				this.Size = new Size(w, h);
			}
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			PrefSave();
		}

		private void quitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (amr.IsRunning == true)
			{
				DialogResult result = MessageBox.Show("実行中です。強制的に終了しますか？", "警告", MessageBoxButtons.YesNo);
				if(result == DialogResult.No)
				{
					e.Cancel = true;
				}
			}
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			this.TopMost = true;
			if (amr.IsRunning==true)
			{
				amr.CancelOrder();
			}
			ChkIsRunning();
			this.Activate();
			this.TopMost = false;
		}
	}
}
