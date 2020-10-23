using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace EternalLandPlugin.Net
{
	public class RawDataWriter
	{
		public RawDataWriter()
		{
			this.memoryStream = new MemoryStream();
			this.writer = new BinaryWriter(this.memoryStream);
			this.writer.BaseStream.Position = 3L;
		}

		public RawDataWriter SetType(PacketTypes type)
		{
			long position = this.writer.BaseStream.Position;
			this.writer.BaseStream.Position = 2L;
			this.writer.Write((short)type);
			this.writer.BaseStream.Position = position;
			return this;
		}

		public RawDataWriter PackSByte(sbyte num)
		{
			this.writer.Write(num);
			return this;
		}

		public RawDataWriter PackByte(byte num)
		{
			this.writer.Write(num);
			return this;
		}

		public RawDataWriter PackInt16(short num)
		{
			this.writer.Write(num);
			return this;
		}

		public RawDataWriter PackUInt16(ushort num)
		{
			this.writer.Write(num);
			return this;
		}

		public RawDataWriter PackInt32(int num)
		{
			this.writer.Write(num);
			return this;
		}

		public RawDataWriter PackUInt32(uint num)
		{
			this.writer.Write(num);
			return this;
		}

		public RawDataWriter PackUInt64(ulong num)
		{
			this.writer.Write(num);
			return this;
		}

		public RawDataWriter PackSingle(float num)
		{
			this.writer.Write(num);
			return this;
		}

		public RawDataWriter PackString(string str)
		{
			this.writer.Write(str);
			return this;
		}

		public RawDataWriter PackRGB(Color? color)
		{
			this.writer.WriteRGB((Color)color);
			return this;
		}

		private void UpdateLength()
		{
			long position = this.writer.BaseStream.Position;
			this.writer.BaseStream.Position = 0L;
			this.writer.Write((short)position);
			this.writer.BaseStream.Position = position;
		}

		public static string ByteArrayToString(byte[] ba)
		{
			StringBuilder stringBuilder = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
			{
				stringBuilder.AppendFormat("{0:x2}", b);
			}
			return stringBuilder.ToString();
		}

		public byte[] GetByteData()
		{
			this.UpdateLength();
			return this.memoryStream.ToArray();
		}

		private MemoryStream memoryStream;

		private BinaryWriter writer;
	}
}
