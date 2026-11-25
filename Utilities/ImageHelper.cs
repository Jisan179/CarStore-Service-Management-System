using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CarStoreServiceManagement.Utilities
{
    /// <summary>
    /// Loads vehicle photos without locking the file, and keeps imported photos inside the Images folder.
    /// </summary>
    public static class ImageHelper
    {
        private static string ImagesFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

        public static void Show(PictureBox box, string imagePath)
        {
            Image previous = box.Image;
            box.Image = Load(imagePath);
            previous?.Dispose();
        }

        private static Image Load(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            try
            {
                string path = Path.IsPathRooted(imagePath) ? imagePath : Path.Combine(ImagesFolder, imagePath);
                if (!File.Exists(path))
                    return null;

                using (Image source = Image.FromFile(path))
                {
                    return new Bitmap(source);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException ||
                                       ex is OutOfMemoryException || ex is UnauthorizedAccessException)
            {
                // An unreadable or invalid picture is shown as "no picture"
                return null;
            }
        }

        /// <summary>
        /// Copies a browsed picture into the Images folder and returns only its file name,
        /// so the database never depends on a folder on one particular computer.
        /// </summary>
        public static string Import(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !Path.IsPathRooted(imagePath) || !File.Exists(imagePath))
                return imagePath;

            Directory.CreateDirectory(ImagesFolder);

            string fileName = Path.GetFileName(imagePath);
            string target = Path.Combine(ImagesFolder, fileName);

            if (string.Equals(Path.GetFullPath(imagePath), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                return fileName;

            if (File.Exists(target))
            {
                fileName = Path.GetFileNameWithoutExtension(imagePath) + "_" +
                           Guid.NewGuid().ToString("N").Substring(0, 8) + Path.GetExtension(imagePath);
                target = Path.Combine(ImagesFolder, fileName);
            }

            File.Copy(imagePath, target);
            return fileName;
        }
    }
}
