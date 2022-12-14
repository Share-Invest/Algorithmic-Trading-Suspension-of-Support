using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
			string storage = Path.Combine(path, code, now.Year.ToString("D4"), now.Month.ToString("D2")), file = string.Concat(storage, @"\", now.Day.ToString("D2"), '_', start.ToString("D9"), '_', end.ToString("D9"), '_', string.IsNullOrEmpty(price) ? "Empty" : price, extension);
			CreateTheDirectory(new DirectoryInfo(storage));
			using (var sw = new StreamWriter(file, false))
				sw.Write(compress.Item1);

			if (compress.Item2 == 0)
				Base.SendMessage(typeof(Repository), file);
		}
		public static void KeepOrganizedInStorage(string json, Catalog.Models.Charts ch)
		{
			string storage = Path.Combine(path, ch.Code, ch.GetType().Name), file = Path.Combine(storage, $"{ch.Start}_{ch.End}{extension}");
			var compress = Compress(json);
			CreateTheDirectory(new DirectoryInfo(storage));
			using (var sw = new StreamWriter(file, false))
				sw.Write(compress.Item1);

			if (compress.Item2 == 0)
				Base.SendMessage(typeof(Repository), file);
		}
		public static void KeepOrganizedInStorage(Catalog.Models.Tick tick)
		{
			string storage = string.IsNullOrEmpty(tick.Path) ? Path.Combine(path, tick.Code, tick.Date.Substring(0, 4), tick.Date.Substring(4, 2)) : GetPath(tick.Path), file = string.IsNullOrEmpty(tick.Path) ? Path.Combine(storage, $"{tick.Date[6..]}_{tick.Open}_{tick.Close}_{tick.Price}{extension}") : tick.Path;
			CreateTheDirectory(new DirectoryInfo(storage));
			var compress = Compress(tick.Contents);
			using (var sw = new StreamWriter(file, false))
				sw.Write(compress.Item1);

			if (compress.Item2 == 0)
				Base.SendMessage(typeof(Repository), file);
		}
		public static void KeepOrganizedInStorage(Catalog.Models.Stocks stocks, int count)
		{
			using var sw = new StreamWriter(string.Concat(path, @"\", stocks.Code, '_', stocks.Date, extension));
			sw.WriteLine(string.Concat(stocks.Price, '_', stocks.Retention, '_', count));
		}
		public static object RetrieveSavedMaterial(string code)
		{
			if (code.Length is 6 or 8)
			{
				var stack = new Stack<Catalog.Models.Tick>();
				var dic = new DirectoryInfo(Path.Combine(path, code));

				if (dic.Exists)
					foreach (var file in dic.GetFiles($"*{extension}", SearchOption.AllDirectories))
					{
						var directory = file.DirectoryName.Split('\\')[^1];

						if (directory.Length == 2 && Array.TrueForAll(directory.ToCharArray(), o => char.IsDigit(o)))
						{
							var name = file.FullName.Split('\\');
							var info = name[^1].Replace(extension, string.Empty).Split('_');

							if (name[^3].Length == 4 && Array.TrueForAll(name[^3].ToCharArray(), o => char.IsDigit(o)) && info.Length == 4 && name[^2].Length == 2 && Array.TrueForAll(name[^2].ToCharArray(), o => char.IsDigit(o)) && info[^3].Length == 9 && Array.TrueForAll(info[^3].ToCharArray(), o => char.IsDigit(o)) && info[^2].Length == 9 && Array.TrueForAll(info[^2].ToCharArray(), o => char.IsDigit(o)) && (Array.TrueForAll(info[^1].ToCharArray(), o => char.IsDigit(o)) && name[^4].Length == 6 || name[^4].Length == 8) && info[0].Length == 2 && Array.TrueForAll(info[0].ToCharArray(), o => char.IsDigit(o)))
							{
								var date = string.Concat(name[^3], name[^2], info[0]);

								if (string.IsNullOrEmpty(date) is false && date.Length == 8 && base_date.CompareTo(date) < 0)
									stack.Push(new Catalog.Models.Tick
									{
										Price = info[^1],
										Close = info[^2],
										Open = info[^3],
										Date = date,
										Code = name[^4],
										Contents = string.Empty
									});
							}
						}
					}
				return stack.Count > 0 ? stack.OrderBy(o => o.Date) : Enumerable.Empty<Catalog.Models.Tick>();
			}
			else if (ReadTheFile(code))
			{
				using var sr = new StreamReader(code);

				return Decompress(sr.ReadToEnd());
			}
			return null;
		}
		public static string GetPath(Catalog.Models.Codes cn, DateTime now) => Path.Combine("F:\\Res", cn.Code, now.Year.ToString("D4"), now.Month.ToString("D2"), string.Concat((now.Hour < 9 ? now.AddDays(-1).Day : now.Day).ToString("D2"), '_', cn.Name, ".res"));
		public static object RetrieveSavedMaterial<T>(T param) where T : struct
		{
			var file = string.Empty;

			switch (param)
			{
				case Catalog.Models.Charts ch:
					file = Path.Combine(path, ch.Code, ch.GetType().Name, $"{ch.Start}_{ch.End}{extension}");
					break;

				case Catalog.Models.Tick tick:
					file = Path.Combine(path, tick.Code, tick.Date.Substring(0, 4), tick.Date.Substring(4, 2), $"{tick.Date[6..]}_{tick.Open}_{tick.Close}_{tick.Price}{extension}");
					break;

				case Catalog.Models.Loading loading:
					string storage = Path.Combine(path, loading.Code, loading.Year.ToString("D4"), loading.Month.ToString("D2")), material;
					var info = new DirectoryInfo(storage);
					file = string.Concat(storage, @"\", loading.Day.ToString("D2"), '_', loading.Start.ToString("D9"), '_', loading.End.ToString("D9"), '_', loading.Price, extension);

					if (ReadTheFile(file))
					{
						using (var sr = new StreamReader(file))
							material = Decompress(sr.ReadToEnd());

						return (material, true);
					}
					else if (info.Exists)
						foreach (var pf in info.GetFiles())
							if (loading.Day.ToString("D2").Equals(pf.Name.Substring(0, 2)))
							{
								var split = pf.Name.Split('.')[0].Split('_');

								if (pf.Length > loading.Length && uint.TryParse(split[1], out uint start) && start < loading.Start && start > 0x55D4A80 - 1 && uint.TryParse(split[^2], out uint end) && end > loading.End && (loading.Code.Length == 6 ? end > 0x91E9840 - 1 : end > 0x9357BA0 - 0x1) && ReadTheFile(pf.FullName))
								{
									using (var sr = new StreamReader(pf.FullName))
										material = Decompress(sr.ReadToEnd());

									return (material, false);
								}
							}
					return (string.Empty, false);
			}
			if (ReadTheFile(file))
			{
				using var sr = new StreamReader(file);

				return Decompress(sr.ReadToEnd());
			}
			return null;
		}
		public static void Save(string path, string file, string param)
		{
			var directory = new DirectoryInfo(path);

			if (directory.Exists)
				foreach (var before in directory.GetFiles("*.Res", SearchOption.AllDirectories))
				{
					var find = before.Name.Split('.')[0].Split('-');

					if (find.Length == 2)
					{
						if (DateTime.TryParseExact(find[0], Base.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime start) && DateTime.TryParseExact(find[1], Base.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime end))
						{
							if (start.AddDays(0x2D).CompareTo(end) < 0)
								before.Delete();
						}
						else
							before.Delete();
					}
				}
			else
				directory.Create();

			using var sw = new StreamWriter(file, false);
			sw.Write(Compress(param).Item1);
		}
		public static void Delete(string param)
		{
			var file = new FileInfo(param);

			if (file.Exists)
				file.Delete();
		}
		public static bool Delete(Catalog.Models.Tick tick)
		{
			var file = new FileInfo(Path.Combine(path, tick.Code, tick.Date.Substring(0, 4), tick.Date.Substring(4, 2), $"{tick.Date[6..]}_{tick.Open}_{tick.Close}_{tick.Price}{extension}"));

			if (file.Exists)
				file.Delete();

			return file.Exists;
		}
		public static bool ReadTheFile(string name) => new FileInfo(name).Exists;
		public static void CreateTheDirectory(DirectoryInfo info)
		{
			if (info.Exists is false)
				info.Create();
		}
		public static string Decompress(string param)
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
		public static string R => Path.Combine(path, nameof(R));
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
		static string GetPath(string param)
		{
			var path = param.Split('.')[0];

			return path.Remove(path.Length - 3);
		}
		const string path = @"C:\Algorithmic Trading\Res";
		const string base_date = "20210104";
		const string extension = ".res";
	}
}