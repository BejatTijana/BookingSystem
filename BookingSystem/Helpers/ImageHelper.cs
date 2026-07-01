using System;
using System.IO;
using System.Linq;
using System.Web;

namespace BookingSystem.Helpers
{
    public static class ImageHelper
    {
        public static readonly string[] AllowedExt = { ".jpg", ".jpeg", ".png" };
        public const int MaxBytes = 2 * 1024 * 1024;

        public static bool Validate(HttpPostedFileBase file, bool required, out string error)
        {
            error = null;
            if (file == null || file.ContentLength == 0)
            {
                if (required)
                {
                    error = "Image is required.";
                    return false;
                }
                return true;
            }
            if (string.IsNullOrEmpty(file.FileName))
            {
                error = "Image file name is missing.";
                return false;
            }
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
            {
                error = "Allowed image formats: .jpg, .jpeg, .png.";
                return false;
            }
            if (file.ContentLength > MaxBytes)
            {
                error = "Image must be at most 2 MB.";
                return false;
            }
            return true;
        }

        public static bool Save(HttpPostedFileBase file, string absoluteFolderPath, out string savedName, out string error)
        {
            savedName = null;
            error = null;
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            string name = Guid.NewGuid().ToString("N") + ext;
            try
            {
                if (!Directory.Exists(absoluteFolderPath))
                {
                    Directory.CreateDirectory(absoluteFolderPath);
                }
                file.SaveAs(Path.Combine(absoluteFolderPath, name));
                savedName = name;
                return true;
            }
            catch (IOException)
            {
                error = "Failed to save image. Please try again.";
                return false;
            }
        }
    }
}
