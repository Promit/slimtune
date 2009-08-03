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
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SlimTuneUI
{
	public static class Utilities
	{
		public static int Read7BitEncodedInt(BinaryReader reader)
		{
			int value = 0;
			int byteval;
			int shift = 0;
			while(((byteval = reader.ReadByte()) & 0x80) != 0)
			{
				value |= ((byteval & 0x7F) << shift);
				shift += 7;
			}
			return (value | (byteval << shift));
		}

		public static long Read7BitEncodedInt64(BinaryReader reader)
		{
			long value = 0;
			long byteval;
			int shift = 0;
			while(((byteval = reader.ReadByte()) & 0x80) != 0)
			{
				value |= ((byteval & 0x7F) << shift);
				shift += 7;
			}
			return (value | (byteval << shift));
		}

		public static void Write7BitEncodedInt(BinaryWriter writer, int value)
		{
			while(value >= 128)
			{
				writer.Write((byte) value | 0x80);
				value >>= 7;
			}
			writer.Write((byte) value);
		}

		public static string GetStandardCaption(Connection connection)
		{
			if(!string.IsNullOrEmpty(connection.Executable))
			{
				return string.Format("{0} - {1}", System.IO.Path.GetFileNameWithoutExtension(connection.Executable),
					System.IO.Path.GetFileNameWithoutExtension(connection.StorageEngine.Name));
			}
			else if(!string.IsNullOrEmpty(connection.HostName))
			{
				return string.Format("{0}:{1} - {2}", connection.HostName, connection.Port,
					System.IO.Path.GetFileNameWithoutExtension(connection.StorageEngine.Name));
			}
			else
			{
				return System.IO.Path.GetFileName(connection.StorageEngine.Name);
			}
		}
	}
}
