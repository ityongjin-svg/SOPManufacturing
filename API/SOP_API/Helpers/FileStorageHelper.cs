using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace SOP_API.Helpers
{
    /// <summary>
    /// Helper terpusat untuk menyimpan & mengambil file fisik di luar folder aplikasi
    /// (D:\SOP\Document\{Menu}\{TipeFile}\...), supaya file TIDAK ikut hilang/tertimpa
    /// setiap kali project di-publish ulang, dan supaya semua modul (Document, Machine,
    /// SOP, dll) pakai konvensi yang sama dan terorganisir per menu.
    ///
    /// Path relatif yang disimpan ke kolom FilePath di DB selalu berbentuk:
    ///     "{Menu}/{TipeFile}/namafile.ext"   contoh: "Machine/PDF/manual-mesin.pdf"
    /// bukan path fisik lengkap, supaya kalau root folder pindah suatu saat,
    /// cukup ganti 1 baris config tanpa perlu update data lama.
    /// </summary>
    public static class FileStorageHelper
    {
        private static string RootPath =>
            ConfigurationManager.AppSettings["FileStorageRoot"] ?? @"D:\SOP\Document\";

        private static readonly Dictionary<string, string> TypeFolders =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".pdf", "PDF" },
            { ".xls", "Excel" },
            { ".xlsx", "Excel" },
            { ".doc", "Word" },
            { ".docx", "Word" },
            { ".ppt", "PowerPoint" },
            { ".pptx", "PowerPoint" },
            { ".jpg", "Image" },
            { ".jpeg", "Image" },
            { ".png", "Image" },
        };

        /// <summary>Menentukan sub-folder berdasarkan extension file. Default "Others" kalau tidak dikenal.</summary>
        public static string ResolveTypeFolder(string extension)
        {
            string folder;
            bool found = TypeFolders.TryGetValue(extension ?? "", out folder);
            return found ? folder : "Others";
        }

        /// <summary>Membersihkan nama menu supaya aman dipakai sebagai nama folder.</summary>
        private static string SanitizeMenuFolder(string menuFolder)
        {
            if (string.IsNullOrWhiteSpace(menuFolder))
                return "Others";

            foreach (char c in Path.GetInvalidFileNameChars())
                menuFolder = menuFolder.Replace(c, '_');

            return menuFolder.Trim();
        }

        /// <summary>
        /// Menyiapkan lokasi simpan untuk file baru, dikelompokkan per menu lalu per tipe file
        /// (folder otomatis dibuat kalau belum ada), dan menghindari overwrite kalau ada file
        /// dengan nama sama.
        /// </summary>
        /// <param name="menuFolder">Nama menu/modul, mis. "Document", "Machine", "SOP".</param>
        /// <param name="originalFileName">Nama file asli yang diupload.</param>
        public static FileSaveInfo BuildSavePath(string menuFolder, string originalFileName)
        {
            string menu = SanitizeMenuFolder(menuFolder);
            string ext = Path.GetExtension(originalFileName);
            string typeFolder = ResolveTypeFolder(ext);

            string folderPhysical = Path.Combine(RootPath, menu, typeFolder);

            if (!Directory.Exists(folderPhysical))
                Directory.CreateDirectory(folderPhysical);

            string fileName = Path.GetFileName(originalFileName);
            string nameOnly = Path.GetFileNameWithoutExtension(fileName);
            string physicalPath = Path.Combine(folderPhysical, fileName);

            int counter = 1;
            while (File.Exists(physicalPath))
            {
                fileName = $"{nameOnly}_{counter}{ext}";
                physicalPath = Path.Combine(folderPhysical, fileName);
                counter++;
            }

            string relativePath = $"{menu}/{typeFolder}/{fileName}";

            return new FileSaveInfo
            {
                PhysicalPath = physicalPath,
                RelativePath = relativePath
            };
        }

        /// <summary>
        /// Overload lama TANPA menu (dipertahankan untuk backward-compat, DEPRECATED).
        /// Otomatis dikelompokkan ke folder "Others" supaya file lama yang belum
        /// dimigrasi tetap bisa dibedakan dan mudah ditemukan untuk dipindahkan manual.
        /// Jangan dipakai lagi untuk kode baru — selalu sertakan menuFolder.
        /// </summary>
        [Obsolete("Gunakan BuildSavePath(menuFolder, originalFileName) supaya file terorganisir per menu.")]
        public static FileSaveInfo BuildSavePath(string originalFileName)
        {
            return BuildSavePath("Others", originalFileName);
        }

        /// <summary>Path fisik lengkap dari relative path yang tersimpan di DB, mis. "Machine/PDF/nama.pdf".</summary>
        public static string GetPhysicalPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            return Path.Combine(RootPath, relativePath.Replace("/", "\\"));
        }
    }

    /// <summary>
    /// Hasil BuildSavePath. Dipakai sebagai class biasa (bukan tuple) karena
    /// project ini di-pin ke langversion 6 di Web.config (tuple = fitur C# 7+).
    /// </summary>
    public class FileSaveInfo
    {
        public string PhysicalPath { get; set; }
        public string RelativePath { get; set; }
    }
}