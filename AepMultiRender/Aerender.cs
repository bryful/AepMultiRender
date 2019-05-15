using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;

using System.IO;
using System;


namespace AepMultiRender
{
	//-------------------------------------------------
	public class FinishedEventArgs : EventArgs
	{
		public string ProcessName = "";
		public int ProcessId = 0;
		public int Index = 0;

	}
	//-------------------------------------------------

	public class Aerender
	{
		public delegate void FinishedEventHandler(object sender, FinishedEventArgs e);
		public event FinishedEventHandler Finished;
		//--------------------------------------------
		private int m_ProcessId = 0;
		private string m_ProcessName = "";
		private int m_Index = -1;
		private Process m_Proc = null;
		private bool m_IsRunning = false;
		//--------------------------------------------
		protected virtual void OnFinised(FinishedEventArgs e)
		{
			Finished?.Invoke(this, e);
		}
		//--------------------------------------------
		public Aerender(Form form,string exePath, string aepPath)
		{
			m_Proc = new Process
			{
				SynchronizingObject = form
			};
			m_Proc.StartInfo.FileName = exePath;
			m_Proc.StartInfo.Arguments = "-project \"" + aepPath + "\""; 
			m_Proc.EnableRaisingEvents = true;
			m_Proc.Exited += new EventHandler(Proc_Exited);
		}
		//--------------------------------------------
		private void Proc_Exited(object sender, EventArgs e)
		{
			FinishedEventArgs r = new FinishedEventArgs();
			r.ProcessId = m_ProcessId;
			r.ProcessName = m_ProcessName;
			r.Index = m_Index;
			m_IsRunning = false;
			m_Proc.Close();
			OnFinised(r);

		}
		//--------------------------------------------
		public bool Start()
		{
			bool ret = m_Proc.Start();
			if (ret) m_IsRunning = true;
			while(m_Proc.HasExited) { };
			return ret;
		}
		//--------------------------------------------
		public int Index
		{
			get { return m_Index; }
			set { m_Index = value; }
			
		}
		//--------------------------------------------
		public bool IsRunning
		{
			get { return m_IsRunning; }
		}
		//--------------------------------------------
		public void CancelOrder()
		{
			if (m_IsRunning == true)
			{
				if (!m_Proc.HasExited)
				{
					m_Proc.CloseMainWindow();
					m_Proc.WaitForExit(1000);
					m_Proc.Close();
				}
			}
			m_IsRunning = false;

		}
		//--------------------------------------------

	}
}
