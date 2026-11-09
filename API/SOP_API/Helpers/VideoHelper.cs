using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace SOP_API.Helpers
{
    public static class VideoHelper
    {
        private static string FFmpegPath =>
            ConfigurationManager.AppSettings["FFmpegPath"] ?? @"D:\SOP\Tools\ffmpeg\ffmpeg.exe";

        /// <summary>
        /// Kompres video ke H.264/mp4, max lebar 1280px, CRF 28 (kualitas cukup,
        /// ukuran jauh lebih kecil dari sumber aslinya).
        /// </summary>
        //public static void CompressVideo(string inputPath, string outputPath)
        //{
        //    RunFFmpeg($"-y -i \"{inputPath}\" -vcodec libx264 -crf 28 -preset fast " +
        //              $"-vf \"scale='min(1280,iw)':-2\" -acodec aac -b:a 128k \"{outputPath}\"");
        //}

        public static void CompressVideo(string inputPath, string outputPath)
        {
            RunFFmpeg($"-y -i \"{inputPath}\" -threads 0 " +
                      $"-vcodec libx264 -crf 30 -preset slower " +
                      $"-vf \"scale='min(960,iw)':-2,fps=24\" " +
                      $"-acodec aac -b:a 96k -ac 1 " +
                      $"\"{outputPath}\"");
        }

        /// <summary>
        /// Ambil 1 frame di detik ke-1 sebagai thumbnail JPG.
        /// </summary>
        public static void GenerateThumbnail(string videoPath, string thumbnailOutputPath)
        {
            RunFFmpeg($"-y -i \"{videoPath}\" -ss 00:00:01 -vframes 1 \"{thumbnailOutputPath}\"");
        }

        private static void RunFFmpeg(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = FFmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit(120000); // timeout 2 menit

                if (!process.HasExited)
                {
                    process.Kill();
                    throw new Exception("FFmpeg timeout - video terlalu besar/lama diproses.");
                }

                if (process.ExitCode != 0)
                    throw new Exception("FFmpeg gagal: " + stderr);
            }
        }
    }
}