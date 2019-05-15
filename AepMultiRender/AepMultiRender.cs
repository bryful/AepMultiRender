using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;

using System.IO;

namespace AepMultiRender
{
    public class AepMultiRender
    {
		public event Aerender.FinishedEventHandler Finished;
		private Form m_form = null;

		private bool m_IsRunning = false;
		private int m_JobCount = 8;
		private string m_AepPath = "";
		private int m_index = 0;

		private List<Aerender> m_ProcList = new List<Aerender>();

		private string[] m_aerenderListOrg =
		{
			@"C:\Program Files\Adobe\Adobe After Effects CS4\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CS6\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2011\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2012\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2013\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2014\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2015\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2016\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2017\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2018\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2019\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2020\Support Files\aerender.exe",
			@"C:\Program Files\Adobe\Adobe After Effects CC 2021\Support Files\aerender.exe"

		};
		private List<string> m_aerenderList = new List<string>();
		private bool m_CanExec = false;
		//------------------------------------------------
		public AepMultiRender(Form p)
		{
			m_form = p;
			ChkAerender();
		}
		//------------------------------------------------
		protected virtual void OnFinished(FinishedEventArgs e)
		{
			Finished?.Invoke(this, e);
		}
		//------------------------------------------------
		private void ChkAerender()
		{
			m_aerenderList.Clear();
			foreach(string p in m_aerenderListOrg)
			{
				if (File.Exists(p)==true)
				{
					m_aerenderList.Add(p);
				}
			}
			if(m_aerenderList.Count>0)
			{
				m_index = 0;
			}
			else
			{
				m_index = -1;
			}
			ChkCanExec();
		}
		//------------------------------------------------
		private void ChkCanExec()
		{
			m_CanExec = ((m_aerenderList.Count > 0) && (m_index < m_aerenderList.Count) && (m_AepPath != ""));
		}
		//------------------------------------------------
		public bool IsAerender
		{
			get { return ((m_aerenderList.Count > 0) && (m_index < m_aerenderList.Count)); }
		}
		//------------------------------------------------
		public bool Enabled
		{
			get
			{
				ChkCanExec();
				return m_CanExec;
			}
		}

		//------------------------------------------------
		public bool SetAepPath(string p)
		{
			bool ret = false;
			if (m_IsRunning == true) return ret;
			m_AepPath = "";
			if (p == "") return ret;
			if (File.Exists(p) == true)
			{
				string e = Path.GetExtension(p).ToLower();
				if (Path.GetExtension(p).ToLower()==".aep")
				{
					m_AepPath = p;
					ret = true;
				}
			}
			ChkCanExec();
			return ret;

		}
		//------------------------------------------------
		public string AepPath
		{
			get { return m_AepPath; }
			set { SetAepPath(value); }
		}
		//------------------------------------------------
		public int JobCount
		{
			get { return m_JobCount; }
			set
			{
				m_JobCount = value;
				if (m_JobCount < 1) m_JobCount = 1;
				else if (m_JobCount > 12) m_JobCount = 12;
			}
		}
		//------------------------------------------------
		public string [] AerenderList
		{
			get { return m_aerenderList.ToArray(); }
			set
			{
				m_aerenderList.Clear();
				m_aerenderList = value.ToList();
			}
		}
		//------------------------------------------------
		public bool IsRunning
		{
			get { return m_IsRunning; }
		}
		//------------------------------------------------
		public bool CanExec
		{
			get
			{
				ChkCanExec();
				return m_CanExec;
			}
		}
		//------------------------------------------------
		public void SetPref(string[] lst,int sidx, int jc)
		{
			m_aerenderList.Clear();
			m_aerenderList = lst.ToList();
			m_index = sidx;
			m_JobCount = jc;

		}
		//------------------------------------------------
		public int SelectedAerenderIndex
		{
			get { return m_index; }

			set
			{
				if (m_aerenderList.Count > 0)
				{

					if (value < 0) value = 0;
					else if (value > m_aerenderList.Count - 1) value = m_aerenderList.Count - 1;

					m_index = value;
				}
				else
				{
					m_index = -1;
				}
				ChkCanExec();
			}

		}
		//------------------------------------------------
		public string AerenderPath
		{
			get
			{
				if (m_aerenderList.Count <= 0)
				{
					return "";
				}
				else
				{
					return m_aerenderList[m_index];
				}
			}
			set
			{
				string p = "";
				if (Path.GetFileName(value) =="aerender.exe")
				{
					if (File.Exists(value)==true)
					{
						p = value;
					}
				}

				if (p == "") return;
				int idx = -1;
				for(int i=0; i<m_aerenderList.Count;i++)
				{
					if (m_aerenderList[i] == p)
					{
						idx = i;
						break;
					}
				}
				if(idx>=0)
				{
					m_index = idx;
				}
				else
				{
					m_aerenderList.Insert(0, p);
					m_index = 0;
				}
				ChkCanExec();
			}
		}
		//------------------------------------------------
		public string AepFileName
		{
			get
			{
				string s = Path.GetFileName(Path.GetDirectoryName(m_AepPath));
				if (s != "") s += "/";
				s += Path.GetFileName(m_AepPath);
				return s;
			}
		}
		//------------------------------------------------
		public bool Exec()
		{
			bool ret = false;
			if (m_IsRunning == true) return ret;
			if (m_AepPath == "") return ret;
			if(m_aerenderList.Count <=0) return ret;
			if (m_index < 0) return ret;

			m_ProcList.Clear();
			if (m_JobCount > 0)
			{
				string ap = m_aerenderList[m_index];

				int cnt = 0;
				for (int i = 0; i < m_JobCount; i++)
				{
					Aerender ar = new Aerender(m_form, ap, m_AepPath);
					ar.Finished += Ar_Finished;
					if (ar.Start()==true){
						ar.Index = cnt;
						cnt++;
						m_ProcList.Add(ar);
						System.Threading.Thread.Sleep(200);
					}
				}
				if (m_ProcList.Count > 0)
				{
					m_IsRunning = true;
					ret = true;
				}

			}
			return ret;
		}
		//------------------------------------------------
		private void Ar_Finished(object sender, FinishedEventArgs e)
		{
			bool ck = false;
			if (m_ProcList.Count>0)
			{
				foreach( Aerender ar in m_ProcList)
				{
					if(ar.IsRunning==true)
					{
						ck = true;
						break;
					}
				}
			}
			m_IsRunning = ck;
			OnFinished(e);
		}
		//------------------------------------------------
		public void CancelOrder()
		{
			if (m_ProcList.Count > 0)
			{
				foreach (Aerender ar in m_ProcList)
				{
					if (ar.IsRunning == true)
					{
						ar.CancelOrder();
					}
				}
			}
			m_IsRunning = false;
		}
	}
}
