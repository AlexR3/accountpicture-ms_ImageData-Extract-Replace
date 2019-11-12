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

			FileStream fs;

			try
			{
				fs = new FileStream(args[0], FileMode.Open);
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine(e.Message);

				return -1;
			}

			Bitmap bitmap = GetImage(args[1]);

			if (bitmap == null)
			{
				Console.WriteLine("Cannot open image file.");

				return -1;
			}

			Image image = bitmap;

			Bitmap image96 = new Bitmap(image, new Size(96, 96));
			Bitmap image448 = new Bitmap(image, new Size(448, 448));

			long position = GetImageOffset(fs, 0);
			InsertImage(fs, position, image96);
			position = GetImageOffset(fs, 100);
			InsertImage(fs, position, image448);

			fs.Close();
			fs.Dispose();

			Console.WriteLine("Operation completed");

			return 0;
		}

		/// <summary>
		/// Read image file
		/// </summary>
		/// <param name="imageFileName">Path to image file</param>
		/// <returns></returns>
		public static Bitmap GetImage(string imageFileName)
		{
			FileStream fs;
			try
			{
				fs = new FileStream(imageFileName, FileMode.Open);
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine(e.Message);

				return null;
			}

			byte[] buffer = new byte[Convert.ToInt32(fs.Length)];
			fs.Read(buffer, 0, buffer.Length);

			MemoryStream memoryStream = new MemoryStream(buffer);

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
			byte[] buffer = new byte[Convert.ToInt32(fs.Length)];

			fs.Read(buffer, 0, buffer.Length);

			MemoryStream memoryStream = new MemoryStream();
			image.Save(memoryStream, ImageFormat.Png);
			var imageByeArray = memoryStream.ToArray();
			imageByeArray.CopyTo(buffer, offset);

			FileStream fsOut = new FileStream(fs.Name + "_Modified", FileMode.Create);
			fsOut.Write(buffer, 0, buffer.Length);

			fsOut.Close();
			fsOut.Dispose();
		}

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

		#endregion

		#region Helpers

		/// <summary>
		/// Find image format position in file stream
		/// </summary>
		/// <param name="fs">file stream</param>
		/// <param name="imageFormat">image format to look for</param>
		/// <param name="offset">offset in file stream</param>
		/// <returns></returns>
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
