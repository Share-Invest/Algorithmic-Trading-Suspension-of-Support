﻿using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Versioning;
using System.Text;

namespace ShareInvest
{
    [SupportedOSPlatform("windows")]
    public static class Repository
    {
        public static void KeepOrganizedInStorage(string json, string code, uint start, uint end, string price)
        {
            var now = DateTime.Now;
            var compress = Compress(json);
            string storage = Path.Combine(path, code, now.Year.ToString("D4"), now.Month.ToString("D2")),
                file = string.Concat(storage, @"\", now.Day.ToString("D2"), '_', start.ToString("D9"), '_', end.ToString("D9"), '_', string.IsNullOrEmpty(price) ? "Empty" : price, extension);
            CreateTheDirectory(new DirectoryInfo(storage));
            using (var sw = new StreamWriter(file, false))
                sw.WriteLine(compress.Item1);

            if (compress.Item2 == 0)
                Base.SendMessage(code, typeof(Repository));

            RecodeToEncryption(file);
        }
        public static (string, bool) RetrieveSavedMaterial(Catalog.Models.Loading loading)
        {
            string storage = Path.Combine(path, loading.Code, loading.Year.ToString("D4"), loading.Month.ToString("D2")), material,
                file = string.Concat(storage, @"\", loading.Day.ToString("D2"), '_', loading.Start.ToString("D9"), '_', loading.End.ToString("D9"), '_', loading.Price, extension);
            var info = new DirectoryInfo(storage);

            if (ReadTheFile(file))
            {
                using (var sr = new StreamReader(file))
                    material = Decompress(sr.ReadToEnd());

                RecodeToEncryption(file);

                return (material, true);
            }
            else if (info.Exists)
                foreach (var pf in info.GetFiles())
                    if (loading.Day.ToString("D2").Equals(pf.Name.Substring(0, 2)))
                    {
                        var split = pf.Name.Split('.')[0].Split('_');

                        if (pf.Length > loading.Length && uint.TryParse(split[1], out uint start) && start < loading.Start && start > 0x55D4A80 - 1 &&
                            uint.TryParse(split[^2], out uint end) && end > loading.End && (loading.Code.Length == 6 ? end > 0x91E9840 - 1 : end > 0x9357BA0 - 0x1) &&
                            ReadTheFile(pf.FullName))
                        {
                            using (var sr = new StreamReader(pf.FullName))
                                material = Decompress(sr.ReadToEnd());

                            RecodeToEncryption(pf.FullName);

                            return (material, false);
                        }
                    }
            return (string.Empty, false);
        }
        static void RecodeToEncryption(string name) => new FileInfo(name).Encrypt();
        static bool ReadTheFile(string name)
        {
            var info = new FileInfo(name);

            if (info.Exists)
                info.Decrypt();

            return info.Exists;
        }
        static void CreateTheDirectory(DirectoryInfo info)
        {
            if (info.Exists == false)
                info.Create();
        }
        static string Decompress(string param)
        {
            byte[] sourceArray = Convert.FromBase64String(param), targetArray = new byte[BitConverter.ToInt32(sourceArray, 0)];
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(sourceArray, 4, sourceArray.Length - 4);
                memoryStream.Position = 0;
                using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                gZipStream.Read(targetArray, 0, targetArray.Length);
            }
            return Encoding.UTF8.GetString(targetArray);
        }
        static (string, int) Compress(string param)
        {
            byte[] sourceArray = Encoding.UTF8.GetBytes(param);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                gZipStream.Write(sourceArray, 0, sourceArray.Length);

            byte[] temporaryArray = new byte[memoryStream.Length], targetArray = new byte[temporaryArray.Length + 4];
            memoryStream.Position = 0;
            var length = memoryStream.Read(temporaryArray, 0, temporaryArray.Length);
            Buffer.BlockCopy(temporaryArray, 0, targetArray, 4, temporaryArray.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(sourceArray.Length), 0, targetArray, 0, 4);

            return (Convert.ToBase64String(targetArray), length);
        }
        const string path = @"C:\Algorithmic Trading\Res";
        const string extension = ".res";
    }
}