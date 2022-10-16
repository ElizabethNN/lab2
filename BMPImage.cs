using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace lab_graphic_2 {

	[Serializable]
	public class BMPImage {

		public struct BITMAPFILEHEADER {
			public short type;
			public int size;
			public int reserved;
			public int offset;
			public int headerSize;
			public uint width;
			public uint height;
			public ushort planes;
			public ushort bitCount;
			public uint biCompression;
			public uint biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;

		}

		public BITMAPFILEHEADER header { get; private set; }
		public SFML.Graphics.Color[,] data { get; private set; }

		private BMPImage(BITMAPFILEHEADER header, SFML.Graphics.Color[,] data)
		{
			this.header = header;
			this.data = data;
		}

		public BMPImage(SFML.Graphics.Color[,] data) {
			BITMAPFILEHEADER header = new BITMAPFILEHEADER();
			header.type = 0x4D42;
			header.size = 54 + data.GetLength(1) * data.GetLength(0) * 4;
			header.offset = 54;
			header.headerSize = 40;
			header.width = (uint)data.GetLongLength(0);
			header.height = (uint)data.GetLongLength(1);
			header.planes = 1;
			header.bitCount = 32;
			header.biCompression = 0;
			header.biSizeImage = 0;
			header.biXPelsPerMeter = 0;
			header.biYPelsPerMeter = 0;
			header.biClrUsed = 0;
			header.biClrImportant = 0;
			this.data = data;
			this.header = header;
		}

		public static BMPImage readFromFile(String file) {
			BITMAPFILEHEADER header = new BITMAPFILEHEADER();
			byte[] bytes = File.ReadAllBytes(file);
			header.type = BitConverter.ToInt16(bytes, 0x0);
			header.size = BitConverter.ToInt32(bytes, 0x2);
			header.offset = BitConverter.ToInt32(bytes, 0x0A);
			header.headerSize = BitConverter.ToInt32(bytes, 0x0E);
			header.width = (uint)BitConverter.ToInt32(bytes, 0x12);
			header.height = (uint)BitConverter.ToInt32(bytes, 0x16);
			header.planes = (ushort)BitConverter.ToInt16(bytes, 0x1A);
			header.bitCount = (ushort)BitConverter.ToInt16(bytes, 0x1C);
			header.biCompression = (uint)BitConverter.ToInt32(bytes, 0x1E);
			header.biSizeImage = (uint)BitConverter.ToInt32(bytes, 0x22);
			header.biXPelsPerMeter = BitConverter.ToInt32(bytes, 0x26);
			header.biYPelsPerMeter = BitConverter.ToInt32(bytes, 0x2A);
			header.biClrUsed = (uint)BitConverter.ToInt32(bytes, 0x2E);
			header.biClrImportant = (uint)BitConverter.ToInt32(bytes, 0x32);
		
			SFML.Graphics.Color[,] colors = new SFML.Graphics.Color[header.width, header.height];

			for (int i = 0; i < header.height; i++) {
				for (int j = 0; j < header.width; j++) {
					byte b = bytes[header.offset + ((i * header.width + j) * 4)];
					byte g = bytes[header.offset + 1 + ((i * header.width + j) * 4)];
					byte r = bytes[header.offset + 2 + ((i * header.width + j) * 4)];
					byte a = bytes[header.offset + 3 + ((i * header.width + j) * 4)];
					colors[j, header.height - 1 - i] = new SFML.Graphics.Color( r, g, b, a);
				}
			}
			return new BMPImage(header, colors);
		}

		public void writeBMP(string file) {
			FileStream stream = new FileStream(file, FileMode.Create, FileAccess.Write);
			stream.Write(BitConverter.GetBytes(header.type));
			stream.Write(BitConverter.GetBytes(header.size));
			stream.Write(BitConverter.GetBytes(header.reserved));
			stream.Write(BitConverter.GetBytes(header.offset));
			stream.Write(BitConverter.GetBytes(header.headerSize));
			stream.Write(BitConverter.GetBytes(header.width));
			stream.Write(BitConverter.GetBytes(header.height));
			stream.Write(BitConverter.GetBytes(header.planes));
			stream.Write(BitConverter.GetBytes(header.bitCount));
			stream.Write(BitConverter.GetBytes(header.biCompression));
			stream.Write(BitConverter.GetBytes(header.biSizeImage));
			stream.Write(BitConverter.GetBytes(header.biXPelsPerMeter));
			stream.Write(BitConverter.GetBytes(header.biYPelsPerMeter));
			stream.Write(BitConverter.GetBytes(header.biClrUsed));
			stream.Write(BitConverter.GetBytes(header.biClrImportant));
			for (long i = this.header.height - 1; i >= 0; i--) {
				for (int j = 0; j < this.header.width; j++) {
					stream.WriteByte(this.data[j, i].B);
					stream.WriteByte(this.data[j, i].G);
					stream.WriteByte(this.data[j, i].R);
					stream.WriteByte(this.data[j, i].A);
				}
			}
			stream.Close();
		}

	}

}