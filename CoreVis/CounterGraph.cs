﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using UICore;
using ZedGraph;

using NHibernate.Criterion;

namespace SlimTuneUI.CoreVis
{
	[DisplayName("Performance Counters")]
	public partial class CounterGraph : UserControl, IVisualizer
	{
		ProfilerWindowBase m_mainWindow;
		Connection m_connection;
		Snapshot m_snapshot;

		ColorRotator m_colors = new ColorRotator();
		bool m_redrawGraph = true;

		public event EventHandler Refreshed;

		public string DisplayName
		{
			get
			{
				return Utilities.GetDisplayName(typeof(CounterGraph));
			}
		}

		public UserControl Control
		{
			get { return this; }
		}

		public Snapshot Snapshot
		{
			get { return m_snapshot; }
		}

		public bool SupportsRefresh
		{
			get { return false; }
		}

		public CounterGraph()
		{
			InitializeComponent();
		}

		public bool Initialize(ProfilerWindowBase mainWindow, Connection connection, Snapshot snapshot)
		{
			if(mainWindow == null)
				throw new ArgumentNullException("mainWindow");
			if(connection == null)
				throw new ArgumentNullException("connection");

			m_mainWindow = mainWindow;
			m_connection = connection;
			m_snapshot = snapshot;

			Graph.GraphPane.Title.Text = "Performance Counters";
			Graph.GraphPane.XAxis.Title.Text = "Time";
			Graph.GraphPane.YAxis.Title.Text = "Value";

			try
			{
				UpdateCounters();
				m_refreshTimer.Enabled = true;
				return true;
			}
			catch
			{
				MessageBox.Show("This connection does not have any performance counter data.", "Performance Counter Visualizer",
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}

		public void OnClose()
		{
			m_refreshTimer.Enabled = false;
		}

		public void RefreshView()
		{
			UpdateCounters();
			UpdateGraph();

			Utilities.FireEvent(this, Refreshed);
		}

		private void UpdateCounters()
		{
			for(int i = 0; i < m_counterListBox.Items.Count; ++i)
			{
				var entry = m_counterListBox.Items[i] as CounterEntry;
				entry.Tagged = false;
			}

			using(var session = m_connection.DataEngine.OpenSession(m_snapshot.Id))
			using(var tx = session.BeginTransaction())
			{
				var counters = session.CreateCriteria<Counter>()
					.AddOrder(Order.Asc("Id"))
					.List<Counter>();
				foreach(Counter c in counters)
				{
					string name = c.Name;
					if(string.IsNullOrEmpty(name))
						name = string.Format("Counter #{0}", c.Id);

					var newEntry = new CounterEntry(c.Id, name);
					int existingEntryIndex = m_counterListBox.Items.IndexOf(newEntry);
					if(existingEntryIndex >= 0)
					{
						var existingEntry = m_counterListBox.Items[existingEntryIndex] as CounterEntry;
						existingEntry.Name = name;
						existingEntry.Tagged = true;
					}
					else
					{
						m_counterListBox.Items.Add(new CounterEntry(c.Id, name));
					}
				}

				tx.Commit();
			}

			int index = 0;
			while(index < m_counterListBox.Items.Count)
			{
				var entry = m_counterListBox.Items[index] as CounterEntry;
				if(!entry.Tagged)
				{
					//this is a stale entry, so remove it
					m_counterListBox.Items.RemoveAt(index);
				}
				else
				{
					//it's fine, keep going
					++index;
				}
			}
		}

		private void UpdateGraph()
		{
			m_redrawGraph = false;
			for(int i = 0; i < m_counterListBox.Items.Count; ++i)
			{
				if(m_counterListBox.GetItemChecked(i))
				{
					//cheat and simply toggle the check
					m_counterListBox.SetItemChecked(i, false);
					m_counterListBox.SetItemChecked(i, true);
				}
			}
			m_redrawGraph = true;
			Graph.AxisChange();
			Graph.Refresh();
		}

		private void m_counterListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			var entry = m_counterListBox.Items[e.Index] as CounterEntry;
			if(e.NewValue == CheckState.Checked)
			{
				var points = new PointPairList();
				using(var session = m_connection.DataEngine.OpenSession(m_snapshot.Id))
				using(var tx = session.BeginTransaction())
				{
					var counter = session.Load<Counter>(entry.Id);
					foreach(var value in counter.Values)
					{
						points.Add(value.Time / 1000.0, value.Value);
					}

					tx.Commit();
				}

				Graph.GraphPane.AddCurve(entry.Name, points, m_colors.ColorForIndex(entry.Id), SymbolType.None);
			}
			else if(e.NewValue == CheckState.Unchecked)
			{
				Graph.GraphPane.CurveList.Remove(Graph.GraphPane.CurveList[entry.Name]);
			}

			if(m_redrawGraph)
			{
				Graph.AxisChange();
				Graph.Refresh();
			}
		}

		private void m_refreshTimer_Tick(object sender, EventArgs e)
		{
			Refresh();
		}
	}

	class CounterEntry
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool Tagged { get; set; }

		public CounterEntry(int id, string name)
		{
			Id = id;
			Name = name;
			Tagged = true;
		}

		public override string ToString()
		{
			return Name;
		}

		public override bool Equals(object obj)
		{
			var other = obj as CounterEntry;
			if(other == null)
				return false;

			return Id == other.Id;
		}

		public override int GetHashCode()
		{
			return Id;
		}
	}
}
