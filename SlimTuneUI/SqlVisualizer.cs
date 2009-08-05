﻿/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlServerCe;

/*
 * Useful queries:
 * 

SELECT M1.Name AS "Caller", M2.Name AS "Constructor", HitCount, CallerId, CalleeId
FROM Callers
JOIN Mappings M1 ON CallerId = M1.Id
JOIN Mappings M2 ON CalleeId = M2.Id
WHERE M2.Name LIKE '%..ctor'
ORDER BY HitCount DESC

SELECT Name, HitCount
FROM Callers
JOIN Mappings on CallerId = Mappings.Id
WHERE CalleeId = 0
ORDER BY HitCount DESC

SELECT Mappings.Name AS "Name", ROUND(100.0 * HitCount / @SampleCount, 2) AS "Percent", Threads.Name AS "Thread"
FROM Callers
JOIN Mappings ON CallerId = Mappings.Id
JOIN Threads ON ThreadId = Threads.Id
WHERE CalleeId = 0
ORDER BY HitCount DESC

*/

namespace SlimTuneUI
{
	[DisplayName("SQL Visualizer (for debugging)")]
	public partial class SqlVisualizer : WeifenLuo.WinFormsUI.Docking.DockContent, IVisualizer
	{
		Connection m_connection;

		public SqlVisualizer()
		{
			InitializeComponent();
		}

		public void Initialize(MainWindow mainWindow, Connection connection)
		{
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_connection = connection;
			m_connection.Closing += new EventHandler(m_connection_Closing);

			this.Text = Utilities.GetStandardCaption(connection);
		}

		void m_connection_Closing(object sender, EventArgs e)
		{
			if(!this.IsDisposed)
				this.Invoke((Action) delegate { this.Close(); });
		}

		private void Results_FormClosing(object sender, FormClosingEventArgs e)
		{
		}

		private void m_queryButton_Click(object sender, EventArgs e)
		{
			try
			{
				DataSet ds = m_connection.StorageEngine.Query(m_queryTextBox.Text);
				if(ds != null)
				{
					m_dataGrid.DataSource = ds;
					m_dataGrid.DataMember = "Query";
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message, "Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void m_clearDataButton_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show(this, "WARNING: This will clear ALL profiling data received so far. This cannot be reversed. Are you sure?",
				"Irreversible Deletion Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			if(result == DialogResult.Yes)
			{
				m_connection.StorageEngine.ClearData();
			}
		}
	}
}
