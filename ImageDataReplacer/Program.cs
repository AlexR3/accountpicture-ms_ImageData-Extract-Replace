using System;
using System.Drawing;
using System.Drawing.Imaging;
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
				Console.WriteLine("Please provide the .accountpicture-ms file and image file.\n\t<executable name> <file name> <image file name>");

				return -1;
			}

			var fs = ReadFile(args[0]);

			if (fs == null)
			{
				return -1;
			}

			var image = GetImage(args[1]);

			if (image == null)
			{
				Console.WriteLine("Cannot read image file.");

				return -1;
			}

			var image96 = new Bitmap(image, new Size(96, 96));
			var image448 = new Bitmap(image, new Size(448, 448));

			InsertImage(fs, GetImageOffset(fs, 0), image96);
			InsertImage(fs, GetImageOffset(fs, 100), image448);

			fs.Close();
			fs.Dispose();

			Console.WriteLine("Operation completed");

			return 0;
		}

		/// <summary>
		/// Get image from image file
		/// </summary>
		/// <param name="imageFileName">Path to image file</param>
		/// <returns>an image from file</returns>
		public static Image GetImage(string imageFileName)
		{
			var fs = ReadFile(imageFileName);

			if (fs == null)
			{
				return null;
			}

			byte[] buffer = new byte[Convert.ToInt32(fs.Length)];

			fs.Read(buffer, 0, buffer.Length);

			MemoryStream memoryStream;

			try
			{
				memoryStream = new MemoryStream(buffer);
			}
			catch (Exception)
			{
				return null;
			}

			return new Bitmap(memoryStream);
		}

		/// <summary>
		/// Insert an image into filestream at given offset
		/// </summary>
		/// <param name="fs">file stream</param>
		/// <param name="offset">offset in file stream</param>
		/// <param name="image">an image to insert</param>
		public static void InsertImage(FileStream fs, long offset, Image image)
		{
			var fsOut = new FileStream(MakeOutFileName(fs.Name), FileMode.Create);

			if (fsOut == null)
			{
				Console.WriteLine("Cannot create new file.");

				return;
			}
			else
			{
				MemoryStream memoryStream = new MemoryStream();
				byte[] buffer = new byte[Convert.ToInt32(fs.Length)];

				fs.Read(buffer, 0, buffer.Length);
				image.Save(memoryStream, ImageFormat.Png);

				try
				{
					memoryStream.ToArray().CopyTo(buffer, offset);
					fsOut.Write(buffer, 0, buffer.Length);
				}
				catch (Exception e)
				{
					Console.WriteLine($"Cannot insert image into file at offset {offset}.\n{e.Message}");
				}
			}

			fsOut.Close();
			fsOut.Dispose();
		}

		#endregion

		#region Helpers

		/// <summary>
		/// Get image data offset at file stream
		/// </summary>
		/// <param name="fs">file stream</param>
		/// <param name="offset">approximate file stream offset</param>
		/// <returns></returns>
		public static long GetImageOffset(FileStream fs, int offset)
		{

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

			return position;
		}

		/// <summary>
		/// Make filename for a new file
		/// </summary>
		/// <param name="filename">original file name</param>
		/// <returns>new filename</returns>
		public static string MakeOutFileName(string filename)
		{
			var originalFileName = Path.GetFileNameWithoutExtension(filename);

			originalFileName += "_modified" + Path.GetExtension(filename);

			return originalFileName;
		}

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
