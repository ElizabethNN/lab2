using SFML.Graphics;

using System;
using System.IO;
using System.Linq;

using static lab_graphic_2.BMPImage;

namespace lab_graphic_2 {
	class VariantImage {

		struct VARIANTHEADER {
			public int type;
			public uint size;
			public uint width;
			public uint height;
			public ushort bitCount;
			public ushort palleteSize;
		}

		VARIANTHEADER header;
		SFML.Graphics.Color[] pallete;
		ushort[,] data;

		private VariantImage(VARIANTHEADER header, SFML.Graphics.Color[] pallete, ushort[,] data)
		{
			this.header = header;
			this.pallete = pallete;
			this.data = data;
		}
		
		public VariantImage(Color[,] colors) {
			header = new VARIANTHEADER();
			pallete = colors.Cast<SFML.Graphics.Color>().Distinct().ToArray();
			header.type = 0x53503031; // bin signature
			header.width = (uint)colors.GetLongLength(0);
			header.height = (uint)colors.GetLongLength(1);
			if (pallete.Length <= 16)
			{
				header.palleteSize = (ushort)pallete.Length;
				header.bitCount = 4;
			}
			else if (pallete.Length <= 256)
			{
				header.palleteSize = (ushort)pallete.Length;
				header.bitCount = 8;
			}
			else if (pallete.Length <= 65536)
			{
				header.palleteSize = (ushort)pallete.Length;
				header.bitCount = 16;
			}
			else {
				throw new Exception("Too many colors");
			}
			int width_bytes = (int)(header.width * header.bitCount);
			if (width_bytes % 16 != 0)
				width_bytes += 16 - width_bytes % 16;
			header.size = (uint)(20 + (header.palleteSize * 4) + width_bytes * header.height);

			data = new ushort[width_bytes / 16, header.height];
			for (int i = 0; i < colors.GetLength(1); i++) {
				for(int j = 0; j < colors.GetLength(0); j += 16 / header.bitCount)
				{
					if (header.bitCount == 16)
					{
						data[j, i] = (ushort)Array.IndexOf(pallete, colors[j, i]);
					}
					else if (header.bitCount == 8)
					{
						ushort t = (ushort)Array.IndexOf(pallete, colors[j, i]);
						t <<= 8;
						if(j + 1 < colors.GetLength(0)) 
							t += (ushort)Array.IndexOf(pallete, colors[j + 1, i]);
						data[j / 2, i] = t;
					}
					else {
						ushort t = (ushort)Array.IndexOf(pallete, colors[j, i]);
						t <<= 4;
						if (j + 1 < colors.GetLength(0))
							t += (ushort)Array.IndexOf(pallete, colors[j + 1, i]);
						t <<= 4;
						if (j + 2 < colors.GetLength(0))
							t += (ushort)Array.IndexOf(pallete, colors[j + 2, i]);
						t <<= 4;
						if (j + 3 < colors.GetLength(0))
							t += (ushort)Array.IndexOf(pallete, colors[j + 3, i]);
						data[j / 4, i] = t;
					}
				}
			}
		}

		public VariantImage readFromFile(String file) {
			VARIANTHEADER header = new VARIANTHEADER();
			byte[] bytes = File.ReadAllBytes(file);
			header.type = BitConverter.ToInt32(bytes, 0);
			header.size = (uint)BitConverter.ToInt32(bytes, 4);
			header.width = (uint)BitConverter.ToInt32(bytes, 8);
			header.height = (uint)BitConverter.ToInt32(bytes, 12);
			header.bitCount = (ushort)BitConverter.ToInt16(bytes, 14);
			header.palleteSize = (ushort)BitConverter.ToInt16(bytes, 16);
			SFML.Graphics.Color[] pallete = new Color[header.palleteSize];
			for (int i = 0; i < pallete.Length; i++) {
				byte b = bytes[18 + i * 4];
				byte g = bytes[19 + i * 4];
				byte r = bytes[20 + i * 4];
				byte a = bytes[21 + i * 4];
				pallete[i] = new Color(r, g, b, a);	
			}
			int width_bytes = (int)(header.width * header.bitCount);
			if (width_bytes % 16 != 0)
				width_bytes += 16 - width_bytes % 16;
			ushort[,] data = new ushort[width_bytes / 16, header.height];
			for (int i = 0; i < header.height; i++) {
				for (int j = 0; j < width_bytes / 16; j++)
				{
					data[j, i] = (ushort)BitConverter.ToInt16(bytes, 22 + pallete.Length * 4 + (i * width_bytes / 16) + j);
				}
			}
			return new VariantImage(header, pallete, data);
		}

		public void writeToFile(String filename) {
			FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
			stream.Write(BitConverter.GetBytes(header.type));
			stream.Write(BitConverter.GetBytes(header.size));
			stream.Write(BitConverter.GetBytes(header.width));
			stream.Write(BitConverter.GetBytes(header.height));
			stream.Write(BitConverter.GetBytes(header.bitCount));
			stream.Write(BitConverter.GetBytes(header.palleteSize));
			foreach (var i in pallete) {
				stream.Write(BitConverter.GetBytes(i.B));
				stream.Write(BitConverter.GetBytes(i.G));
				stream.Write(BitConverter.GetBytes(i.R));
				stream.Write(BitConverter.GetBytes(i.A));
			}
			for (int i = 0; i < data.GetLength(0); i++) {
				for (int j = 0; j < data.GetLength(1); j++) {
					stream.Write(BitConverter.GetBytes(data[j,i]));
				}
			}
			stream.Close();
		}

		public Color[,] Image() {
			Color[,] result = new Color[header.width, header.height];
			for (int i = 0; i < data.GetLength(1); i++) {
				for (int j = 0; j < data.GetLength(0); j++) {
					if (header.bitCount == 16)
					{
						result[j, i] = pallete[data[j, i]];
					}
					else if (header.bitCount == 8) {
						ushort pixel = data[j, i];
						result[j, i] = pallete[pixel & 0x00FF];
						if (j + 1 < result.GetLength(0))
							result[j + 1, i] = pallete[(pixel & 0xFF00) >> 8];
					}
					else if (header.bitCount == 4)
					{
						ushort pixel = data[j, i];
						try
						{
							result[j, i] = pallete[pixel & 0x000F];
						}
						catch (Exception e) {
							result[j, i] = SFML.Graphics.Color.Black;
						}
						try
						{
							if (j + 1 < result.GetLength(0))
								result[j + 1, i] = pallete[(pixel & 0x00F0) >> 4];
						}
						catch (Exception e) {
							result[j + 1, i] = SFML.Graphics.Color.Black;
						}
						if (j + 2 < result.GetLength(0))
							result[j + 2, i] = pallete[(pixel & 0x0F00) >> 8];
						if (j + 3 < result.GetLength(0))
							result[j + 3, i] = pallete[(pixel & 0xF000) >> 12];
					}
				}
			}
			return result;
		}

	}
}