using System;
using System.Drawing;
using System.IO;

namespace ImageDataExtractor
{
	class Program
	{
		#region Methods

		public static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Please provide the .accountpicture-ms file path and file format.\n\t <executable name> <file name> <format (\"PNG'\" or \"JFIF\")> <output file name (optional)>");

				return 1;
			}

			string outFilename = Path.GetFileNameWithoutExtension(args[0]);

			if (args[2] != null)
			{
				outFilename = args[2];
			}

			FileStream fs = new FileStream(args[0], FileMode.Open);

			Bitmap image96 = GetImage(args[1], 96, fs);
			image96.Save(outFilename + "-96.bmp");
			Bitmap image448 = GetImage(args[1], 448, fs);
			image448.Save(outFilename + "-448.bmp");

			fs.Close();
			fs.Dispose();

			Console.WriteLine("Extracted images successfully");

			return 0;
		}

		public static Bitmap GetImage(string imageFormat, int size, FileStream fs)
		{
			var offset = size switch
			{
				96 => 0,
				448 => 100,
				_ => throw new Exception($"Size {size} is not valid"),
			};

			byte[] buffer = new byte[Convert.ToInt32(fs.Length)];
			long position = GetIndexOfFormatString(fs, imageFormat, offset);

			switch (imageFormat)
			{
				case "JFIF":
					position -= 6;
					break;
				case "PNG":
					position -= 1;
					break;
				default:
					break;
			}

			fs.Seek(position, SeekOrigin.Begin);
			fs.Read(buffer, 0, buffer.Length);

			var ms = new MemoryStream(buffer);
			var bitmapImage = new Bitmap(ms);

			return bitmapImage;
		}

		#endregion

		#region Helpers
		public static long GetIndexOfFormatString(FileStream fs, string imageFormat, int offset)
		{
			char[] search = imageFormat.ToCharArray();
			long result = -1, position = 0, stored = offset;
			int c;

			fs.Seek(0, SeekOrigin.Begin);

			while ((c = fs.ReadByte()) != -1)
			{
				if ((char)c == search[position])
				{
					if (stored == -1 && position > 0 && (char)c == search[0])
					{
						stored = fs.Position;
					}
					if (position + 1 == search.Length)
					{
						result = fs.Position - search.Length;
						fs.Position = result;
						break;
					}

					position++;
				}
				else if (stored > -1)
				{
					fs.Position = stored + 1;
					position = 1;
					stored = -1;
				}
				else
				{
					position = 0;
				}
			}

			fs.Seek(0, SeekOrigin.Begin);

			return result;
		}

		#endregion
	}
}
