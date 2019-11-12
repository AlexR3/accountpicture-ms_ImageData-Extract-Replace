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
			if (args.Length < 1)
			{
				Console.WriteLine("Please provide the .accountpicture-ms file.\n\t<executable name> <file name> <output file name (optional)>");

				return -1;
			}

			string outFilename;

			if (args.Length >= 2)
				outFilename = args[1];
			else
				outFilename = Path.GetFileNameWithoutExtension(args[0]);

			var fs = ReadFile(args[0]);

			if (fs == null)
			{
				return -1;
			}

			var image96 = GetImage(fs, 96);
			var image448 = GetImage(fs, 448);

			if (image96 == null)
			{
				Console.WriteLine("Cannot extract 96x96 image.");
			}

			if (image448 == null)
			{
				Console.WriteLine("Cannot extract 448x448 image.");
			}

			try
			{
				image96.Save(outFilename + "-96.bmp");
				image448.Save(outFilename + "-448.bmp");
			}
			catch (Exception e)
			{
				Console.WriteLine("Cannot save images");
				Console.WriteLine(e.Message);

				return -1;
			}

			fs.Close();
			fs.Dispose();

			Console.WriteLine("Operation completed.");

			return 0;
		}

		/// <summary>
		/// Extract an image from file stream
		/// </summary>
		/// <param name="size">image size</param>
		/// <param name="fs">file stream</param>
		/// <returns>an image</returns>
		public static Image GetImage(FileStream fs, int size)
		{
			var offset = size switch
			{
				96 => 0,
				448 => 100,
				_ => throw new Exception($"Size {size} is not valid")
			};

			string[] imageFormats = { "PNG", "JFIF" };
			long position = -1;
			byte[] buffer = new byte[Convert.ToInt32(fs.Length)];

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

			var memoryStream = new MemoryStream(buffer);

			Image image;

			try
			{
				image = new Bitmap(memoryStream);
			}
			catch (ArgumentException)
			{
				Console.WriteLine("Cannot extract an image from file.");

				return null;
			}

			return image;
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Open file wrapper
		/// </summary>
		/// <param name="filename">filename or absolute path</param>
		/// <returns>file stream or null</returns>
		public static FileStream ReadFile(string filename)
		{
			FileStream fs;

			try
			{
				fs = new FileStream(filename, FileMode.Open);
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine(e.Message);

				return null;
			}

			return fs;
		}

		/// <summary>
		/// Find image format position in file stream
		/// </summary>
		/// <param name="fs">file stream</param>
		/// <param name="imageFormat">image format to look for</param>
		/// <param name="offset">offset in file stream</param>
		/// <returns>index of image format string</returns>
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
