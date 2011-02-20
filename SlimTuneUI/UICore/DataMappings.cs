﻿using System;
using System.Collections.Generic;

using FluentNHibernate.Mapping;

namespace UICore.Mappings
{
	public class PropertyMap : ClassMap<Property>
	{
		public PropertyMap()
		{
			Id(x => x.Name)
				.GeneratedBy.Assigned();
			Map(x => x.Value);
			Table("Properties");
		}
	}

	public class SnapshotMap : ClassMap<Snapshot>
	{
		public SnapshotMap()
		{
			Id(x => x.Id);
			Map(x => x.Name);
			Map(x => x.DateTime);
			Table("Snapshots");
		}
	}

	public class ThreadInfoMap : ClassMap<ThreadInfo>
	{
		public ThreadInfoMap()
		{
			Id(x => x.Id)
				.GeneratedBy.Assigned();
			Map(x => x.IsAlive);
			Map(x => x.Name);
			HasMany(x => x.Samples)
				.Inverse()
				.ReadOnly();
			HasMany(x => x.Calls)
				.Inverse()
				.ReadOnly();
			Table("Threads");
		}
	}

	public class FunctionInfoMap : ClassMap<FunctionInfo>
	{
		public FunctionInfoMap()
		{
			Id(x => x.Id)
				.GeneratedBy.Assigned();
			Map(x => x.Name);
			Map(x => x.Signature);
			Map(x => x.IsNative);
			Map(x => x.ClassId);
			References(x => x.Class, "ClassId")
				.ReadOnly();
			HasMany(x => x.CallsAsParent)
				.Inverse()
				.ReadOnly();
			HasMany(x => x.CallsAsChild)
				.Inverse()
				.ReadOnly();
			/*HasMany(x => x.Samples)
				.Inverse()
				.ReadOnly();*/
			Table("Functions");
		}
	}

	public class ClassInfoMap : ClassMap<ClassInfo>
	{
		public ClassInfoMap()
		{
			Id(x => x.Id)
				.GeneratedBy.Assigned();
			Map(x => x.Name);
			Map(x => x.IsValueType);
			HasMany(x => x.Functions)
				.Inverse()
				.ReadOnly();
			Table("Classes");
		}
	}

	public class CallMap : ClassMap<Call>
	{
		public CallMap()
		{
			ReadOnly();
			Id(x => x.Id);

			Map(x => x.ThreadId).Index("Calls_ThreadIndex, Calls_Composite");
			Map(x => x.ParentId).Index("Calls_ParentIndex, Calls_Composite");
			Map(x => x.ChildId).Index("Calls_ChildIndex, Calls_Composite");
			Map(x => x.Time)
				.Not.Nullable();
			Map(x => x.SnapshotId)
				.Not.Nullable()
				.Default("0");

			References(x => x.Thread, "ThreadId");
			References(x => x.Parent, "ParentId");
			References(x => x.Child, "ChildId");

			ApplyFilter<Filters.Snapshot>("SnapshotId = :snapshot");
			ApplyFilter<Filters.Thread>("ThreadId = :threadId");

			Table("Calls");
		}
	}

	public class SampleMap : ClassMap<Sample>
	{
		public SampleMap()
		{
			ReadOnly();
			Id(x => x.Id);
			Map(x => x.ThreadId).Index("Samples_ThreadIndex, Samples_Composite");
			Map(x => x.FunctionId).Index("Samples_FunctionIndex, Samples_Composite");
			Map(x => x.Time)
				.Not.Nullable();
			Map(x => x.SnapshotId, "SnapshotId")
				.Not.Nullable()
				.Default("0");
			References(x => x.Thread, "ThreadId");
			//References(x => x.Function, "FunctionId");

			ApplyFilter("Snapshot", "SnapshotId = :snapshot");
			
			Table("Samples");
		}
	}

	public class CounterMap : ClassMap<Counter>
	{
		public CounterMap()
		{
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Name);
			HasMany(x => x.Values)
				.Inverse()
				.ReadOnly();
			Table("Counters");
		}
	}

	public class CounterValueMap : ClassMap<CounterValue>
	{
		public CounterValueMap()
		{
			Id(x => x.Id);
			Map(x => x.CounterId).Index("CounterValues_IdIndex");
			Map(x => x.Time);
			Map(x => x.Value);
			References(x => x.Counter, "CounterId")
				.ReadOnly();
			Table("CounterValues");
		}
	}

	public class GarbageCollectionMap : ClassMap<GarbageCollection>
	{
		public GarbageCollectionMap()
		{
			Id(x => x.Id);
			Map(x => x.Generation);
			Map(x => x.FunctionId);
			Map(x => x.Time);
			Table("GarbageCollections");
		}
	}

	public class AllocationMap : ClassMap<Allocation>
	{
		public AllocationMap()
		{
			Id(x => x.Id);
			Map(x => x.ClassId);
			Map(x => x.FunctionId);
			Map(x => x.Count);
			Map(x => x.Size);
			Table("Allocations");
		}
	}
}
