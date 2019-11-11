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
			string outFilename = string.Empty;

			if (args.Length < 1)
			{
				Console.WriteLine("Please provide the .accountpicture-ms file path and file format.\n\t<executable name> <file name> <output file name (optional)>");

				return 1;
			}

			if (args.Length >= 2)
			{
				outFilename = args[1];
			}
			else
			{
				outFilename = Path.GetFileNameWithoutExtension(args[0]);
			}

			FileStream fs = new FileStream(args[0], FileMode.Open);

			Bitmap image96 = GetImage(96, fs);
			image96.Save(outFilename + "-96.bmp");
			Bitmap image448 = GetImage(448, fs);
			image448.Save(outFilename + "-448.bmp");

			fs.Close();
			fs.Dispose();

			Console.WriteLine("Extracted images successfully");

			return 0;
		}

		public static Bitmap GetImage(int size, FileStream fs)
		{
			var offset = size switch
			{
				96 => 0,
				448 => 100,
				_ => throw new Exception($"Size {size} is not valid"),
			};

			byte[] buffer = new byte[Convert.ToInt32(fs.Length)];

			string[] imageFormats = { "PNG", "JFIF" };
			long position = -1;

			foreach (var imageFormat in imageFormats)
			{
				position = GetIndexOfFormatString(fs, imageFormat, offset);

				if (position != -1)
				{
					switch (imageFormat)
					{
						case "PNG":
							position -= 1;
							break;
						case "JFIF":
							position -= 6;
							break;
						default:
							break;
					}

					break;
				}
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
