using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ShareInvest.Client;
using ShareInvest.Message;
using ShareInvest.Models;

namespace ShareInvest.GoblinBatContext
{
    public class CallUp : CallUpStatisticalAnalysis
    {
        protected async Task MoveStorageSpace(char initial)
        {
            try
            {
                var client = GoblinBatClient.GetInstance(initial.ToString());
                var stack = new Stack<string>();
                using (var db = new GoblinBatDbContext(key))
                    foreach (var temp in db.Codes.OrderBy(o => o.Code).Select(o => new { o.Code, o.Name, o.Info }).AsNoTracking())
                        stack.Push(temp.Code);

                while (stack.Count > 0)
                {
                    Catalog.Retention retention;
                    var code = stack.Pop();
                    var stocks = new Queue<Emergency.Stocks>(1024);
                    var options = new Queue<Emergency.Options>(1024);
                    var futures = new Queue<Emergency.Futures>(1024);

                    if (code.Length == 8 && (code.StartsWith("101") || code.StartsWith("106")))
                    {
                        using (var db = new GoblinBatDbContext(key))
                            foreach (var f in db.Futures.Where(o => o.Code.Equals(code)).AsNoTracking().OrderBy(o => o.Date))
                            {
                                futures.Enqueue(new Emergency.Futures
                                {
                                    Code = f.Code,
                                    Date = f.Date.ToString(),
                                    Price = f.Price.ToString(),
                                    Volume = f.Volume,
                                    Retention = f.Date.ToString().Substring(0, 12)
                                });
                                if (futures.Count > 2000000)
                                {
                                    retention = await client.EmergencyContext(futures);
                                    futures.Clear();
                                }
                            }
                        retention = await client.EmergencyContext(futures);
                    }
                    if (code.Length == 8 && (code.StartsWith("2") || code.StartsWith("3")))
                    {
                        using (var db = new GoblinBatDbContext(key))
                            foreach (var o in db.Options.Where(o => o.Code.Equals(code)).AsNoTracking().OrderBy(o => o.Date))
                            {
                                options.Enqueue(new Emergency.Options
                                {
                                    Code = o.Code,
                                    Date = o.Date.ToString(),
                                    Price = o.Price.ToString(),
                                    Volume = o.Volume,
                                    Retention = o.Date.ToString().Substring(0, 12)
                                });
                                if (options.Count > 2000000)
                                {
                                    retention = await client.EmergencyContext(options);
                                    options.Clear();
                                }
                            }
                        retention = await client.EmergencyContext(options);
                    }
                    if (code.Length == 6)
                    {
                        using (var db = new GoblinBatDbContext(key))
                            foreach (var s in db.Stocks.Where(o => o.Code.Equals(code)).AsNoTracking().OrderBy(o => o.Date))
                            {
                                stocks.Enqueue(new Emergency.Stocks
                                {
                                    Code = s.Code,
                                    Date = s.Date.ToString(),
                                    Price = s.Price.ToString(),
                                    Volume = s.Volume,
                                    Retention = s.Date.ToString().Substring(0, 12)
                                });
                                if (stocks.Count > 2000000)
                                {
                                    retention = await client.EmergencyContext(stocks);
                                    stocks.Clear();
                                }
                            }
                        retention = await client.EmergencyContext(stocks);
                    }
                    SendMessage(stack.Count);
                }
            }
            catch (Exception ex)
            {
                new ExceptionMessage(ex.StackTrace);
            }
        }
        protected void BulkRemove(string code)
        {
            using (var db = new GoblinBatDbContext(key))
                try
                {
                    db.Material.BulkDelete(db.Material.Where(o => o.Date.Equals(code)), o => o.BatchSize = 1000000);
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace);
                }
        }
        protected string OnReceiveRemainingDay(string code)
        {
            using (var db = new GoblinBatDbContext(key))
                try
                {
                    var day = db.Codes.Where(o => o.Code.Equals(code)).First().Info;

                    return day.Substring(2);
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace);
                }
            return string.Empty;
        }
        protected int DeleteUnnecessaryInformation(string code, string info)
        {
            using (var db = new GoblinBatDbContext(key))
                try
                {
                    var entity = db.Codes.Where(o => o.Code.Equals(code) && o.Info.Equals(info)).First();
                    db.Codes.Remove(entity);

                    return db.SaveChanges();
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace);
                }
            return int.MinValue;
        }
        protected async Task<List<string>> RequestCodeList(List<string> list)
        {
            string code = string.Empty;
            using (var db = new GoblinBatDbContext(key))
                foreach (var temp in db.Codes.Select(o => new
                {
                    o.Code
                }))
                {
                    code = temp.Code;

                    try
                    {
                        if (db.Codes.Any(o => o.Code.Length < 6))
                            await db.Codes.BulkDeleteAsync(db.Codes.Where(o => o.Code.Length < 6), o => o.BatchSize = 100);

                        if (db.Days.Any(o => o.Code.Equals(temp.Code) && o.Date < 10000000))
                        {
                            await db.Days.BulkDeleteAsync(db.Days.Where(o => o.Date < 10000000), o => o.BatchSize = 100);

                            if (db.Days.Any(o => o.Code.Length < 6))
                                await db.Days.BulkDeleteAsync(db.Days.Where(o => o.Code.Length < 6), o => o.BatchSize = 100);
                        }
                        if (temp.Code.Length == 6 && (db.Days.Any(o => o.Code.Equals(temp.Code)) == false || db.Stocks.Any(o => o.Code.Equals(temp.Code)) == false || int.Parse(db.Days.Where(o => o.Code.Equals(temp.Code)).Max(o => o.Date).ToString().Substring(2)) < int.Parse(db.Stocks.Where(o => o.Code.Equals(code)).Min(o => o.Date).ToString().Substring(0, 6))))
                        {
                            list.Add(temp.Code);

                            if (db.Stocks.Any(o => o.Code.Length < 6))
                                await db.Stocks.BulkDeleteAsync(db.Stocks.Where(o => o.Code.Length < 6), o => o.BatchSize = 100);
                        }
                        else if (temp.Code.Length == 8 && temp.Code.Substring(5, 3).Equals("000") && db.Futures.Any(o => o.Date < 100000000000000))
                        {
                            await db.Futures.BulkDeleteAsync(db.Futures.Where(o => o.Date < 100000000000000), o => o.BatchSize = 100);

                            if (db.Futures.Any(o => o.Code.Length < 8))
                                await db.Futures.BulkDeleteAsync(db.Futures.Where(o => o.Code.Length < 8), o => o.BatchSize = 100);
                        }
                        else if (temp.Code.Length == 8 && temp.Code.Substring(5, 3).Equals("000") == false && db.Options.Any(o => o.Date < 100000000000000))
                        {
                            await db.Options.BulkDeleteAsync(db.Options.Where(o => o.Date < 100000000000000), o => o.BatchSize = 100);

                            if (db.Options.Any(o => o.Code.Length < 8))
                                await db.Options.BulkDeleteAsync(db.Options.Where(o => o.Code.Length < 8), o => o.BatchSize = 100);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        new ExceptionMessage(ex.StackTrace, code);
                    }
                    catch (Exception ex)
                    {
                        new ExceptionMessage(ex.StackTrace, code);
                        var stocks = db.Stocks.Where(o => o.Code.Equals(code));

                        if (stocks.Any(o => o.Date < 100000000000000))
                            await db.Stocks.BulkDeleteAsync(stocks.Where(o => o.Date < 100000000000000), o => o.BatchSize = 100);
                    }
                }
            return list;
        }
        protected List<string> RequestCodeList(List<string> list, string[] market)
        {
            using (var db = new GoblinBatDbContext(key))
                try
                {
                    foreach (var temp in db.Codes.Select(o => new
                    {
                        o.Code,
                        o.Info
                    }).AsNoTracking())
                        if (temp.Code.Length == 6 && Array.Exists(market, o => o.Equals(temp.Code)) || temp.Code.Length == 8 && DateTime.Compare(DateTime.ParseExact(temp.Info, "yyyyMMdd", null), DateTime.Now) >= 0)
                            list.Add(temp.Code);
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace);
                }
            return list;
        }
        protected string GetRetention(int param, string code)
        {
            long max = 0;
            using (var db = new GoblinBatDbContext(key))
                try
                {
                    switch (param)
                    {
                        case 1:
                            max = db.Futures.Where(o => o.Code.Equals(code)).AsNoTracking().Max(o => o.Date);
                            break;

                        case 2:
                            max = db.Options.Where(o => o.Code.Equals(code)).AsNoTracking().Max(o => o.Date);
                            break;

                        case 3:
                            max = db.Stocks.Where(o => o.Code.Equals(code)).AsNoTracking().Max(o => o.Date);
                            break;

                        case 4:
                            max = db.Days.Where(o => o.Code.Equals(code)).AsNoTracking().Max(o => o.Date);
                            break;
                    };
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.TargetSite.Name.Equals("GetValue") == false)
                        new ExceptionMessage(ex.TargetSite.Name, code);
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace, code);
                }
            return max > 0 ? (max.ToString().Length > 12 ? max.ToString().Substring(0, 12) : max.ToString()) : "DoesNotExist";
        }
        protected void SetInsertCode(string code, string name, string info)
        {
            using (var db = new GoblinBatDbContext(key))
                try
                {
                    if (db.Codes.Where(o => o.Code.Equals(code) && o.Info.Equals(info) && o.Name.Equals(name)).Any() == false)
                    {
                        db.Codes.AddOrUpdate(new Codes
                        {
                            Code = code,
                            Name = name,
                            Info = info
                        });
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace, code);
                }
        }
        protected void SetStorage(string code, string[] param)
        {
            if (param.Length > 3)
            {
                string date = string.Empty;
                int i, count = 0;
                bool days = param[0].Split(',')[0].Length == 8, stocks = code.Length == 6, futures = code.Length > 6 && code.Substring(5, 3).Equals("000"), options = code.Length > 6 && !code.Substring(5, 3).Equals("000");
                IList model;

                if (futures)
                    model = new List<Futures>(32);

                else if (options)
                    model = new List<Options>(32);

                else if (days)
                    model = new List<Days>(32);

                else
                    model = new List<Stocks>(32);

                for (i = param.Length - 2; i > -1; i--)
                {
                    var temp = param[i].Split(',');

                    if (temp[0].Length == 8)
                    {
                        model.Add(new Days
                        {
                            Code = code,
                            Date = int.Parse(temp[0]),
                            Price = double.Parse(temp[1])
                        });
                        continue;
                    }
                    else if (temp[0].Equals(date))
                        count++;

                    else
                    {
                        date = temp[0];
                        count = 0;
                    }
                    if (stocks)
                        model.Add(new Stocks
                        {
                            Code = code,
                            Date = long.Parse(string.Concat(temp[0], count.ToString("D3"))),
                            Price = int.Parse(temp[1]),
                            Volume = int.Parse(temp[2])
                        });
                    else if (options)
                        model.Add(new Options
                        {
                            Code = code,
                            Date = long.Parse(string.Concat(temp[0], count.ToString("D3"))),
                            Price = double.Parse(temp[1]),
                            Volume = int.Parse(temp[2])
                        });
                    else if (futures)
                        model.Add(new Futures
                        {
                            Code = code,
                            Date = long.Parse(string.Concat(temp[0], count.ToString("D3"))),
                            Price = double.Parse(temp[1]),
                            Volume = int.Parse(temp[2])
                        });
                }
                using (var db = new GoblinBatDbContext(key))
                    try
                    {
                        db.Configuration.AutoDetectChangesEnabled = false;

                        if (days)
                            db.BulkInsert((List<Days>)model, o =>
                            {
                                o.InsertIfNotExists = true;
                                o.BatchSize = 15000;
                                o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                                o.AutoMapOutputDirection = false;
                            });
                        else if (stocks)
                            db.BulkInsert((List<Stocks>)model, o =>
                            {
                                o.InsertIfNotExists = true;
                                o.BatchSize = 15000;
                                o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                                o.AutoMapOutputDirection = false;
                            });
                        else if (options)
                            db.BulkInsert((List<Options>)model, o =>
                            {
                                o.InsertIfNotExists = true;
                                o.BatchSize = 15000;
                                o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                                o.AutoMapOutputDirection = false;
                            });
                        else if (futures)
                            db.BulkInsert((List<Futures>)model, o =>
                            {
                                o.InsertIfNotExists = true;
                                o.BatchSize = 15000;
                                o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                                o.AutoMapOutputDirection = false;
                            });

                    }
                    catch (Exception ex)
                    {
                        new ExceptionMessage(ex.StackTrace, code);
                    }
                    finally
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;
                    }
            }
        }
        protected void SetStorage(string code, StringBuilder sb)
        {
            string onTime = string.Empty, date = DateTime.Now.ToString("yyMMdd"), yesterday = DateTime.Now.AddDays(-1).ToString("yyMMdd");
            int count = 0;
            var dic = new Dictionary<string, string>();
            var model = new List<Datum>();

            foreach (var str in sb.ToString().Split('*'))
                if (str.Length > 0)
                {
                    var temp = str.Split(';');

                    if (temp[0].Length == 6 && double.TryParse(temp[1].Split(',')[0], out double price) && price > 105.95)
                    {
                        if (temp[0].Equals(onTime))
                            count++;

                        else
                        {
                            onTime = temp[0];
                            count = 0;
                        }
                        if (uint.TryParse(onTime, out uint today))
                        {
                            var time = string.Concat(today > 175959 ? yesterday : date, onTime, count.ToString("D3"));

                            if (dic.ContainsKey(time) && ulong.TryParse(time, out ulong relate))
                            {
                                foreach (var kv in dic.OrderBy(o => o.Key))
                                    if (time.Substring(0, 12).Equals(kv.Key.Substring(0, 12)))
                                        relate += 1;

                                dic[relate.ToString()] = temp[1];
                                count = 0;

                                continue;
                            }
                            dic[time] = temp[1];
                        }
                    }
                }
            if (dic.Count > 0)
            {
                if (new Secret().GetTrustedConnection(key))
                {
                    foreach (var kv in dic)
                    {
                        var temp = kv.Value.Split(',');

                        if (temp.Length == 2)
                        {
                            model.Add(new Datum
                            {
                                Code = code,
                                Date = kv.Key,
                                Price = temp[0],
                                Volume = temp[1]
                            });
                            continue;
                        }
                        model.Add(new Datum
                        {
                            Code = code,
                            Date = kv.Key,
                            SellPrice = temp[0],
                            SellQuantity = temp[1],
                            TotalSellAmount = temp[2],
                            BuyPrice = temp[3],
                            BuyQuantity = temp[4],
                            TotalBuyAmount = temp[5]
                        });
                    }
                    SetStorage(model);
                }
                var param = dic.OrderBy(o => o.Key);
                SetStorage(param, code);
                SetStorage(code, param, new Queue<Futures>());
            }
        }
        protected void SetStorage(string code, IOrderedEnumerable<KeyValuePair<string, string>> param, Queue<Futures> cme)
        {
            using (var sw = new StreamWriter(string.Concat(System.IO.Path.Combine(Application.StartupPath, charts), @"\", code, res), true))
                foreach (var kv in param)
                {
                    if (kv.Key.Substring(6, 4).Equals(start))
                        return;

                    var temp = kv.Value.Split(',');

                    if (temp.Length == 2)
                        try
                        {
                            if (int.TryParse(temp[1], out int cVolume) && double.TryParse(temp[0], out double cPrice) && long.TryParse(kv.Key, out long cDate))
                                cme.Enqueue(new Futures
                                {
                                    Code = code,
                                    Date = cDate,
                                    Price = cPrice,
                                    Volume = cVolume
                                });
                            sw.WriteLine(string.Concat(kv.Key, ",", temp[0], ",", temp[1]));
                        }
                        catch (Exception ex)
                        {
                            new ExceptionMessage(ex.StackTrace, code);
                        }
                }
            if (new Secret().GetTrustedConnection(key))
                using (var db = new GoblinBatDbContext(key))
                    try
                    {
                        db.Configuration.AutoDetectChangesEnabled = false;
                        db.BulkInsert(cme, o =>
                        {
                            o.InsertIfNotExists = true;
                            o.BatchSize = 15000;
                            o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                            o.AutoMapOutputDirection = false;
                        });
                    }
                    catch (Exception ex)
                    {
                        new ExceptionMessage(ex.StackTrace, code);
                    }
                    finally
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;
                    }
        }
        protected void SetStorage(StringBuilder param, string code)
        {
            var model = new List<Datum>();

            foreach (var str in param.ToString().Split(';'))
            {
                var temp = str.Split(',');

                if (temp.Length == 3)
                    model.Add(new Datum
                    {
                        Code = code,
                        Date = temp[0],
                        Price = temp[1],
                        Volume = temp[2]
                    });
                else if (temp.Length == 7)
                    model.Add(new Datum
                    {
                        Code = code,
                        Date = temp[0],
                        SellPrice = temp[1],
                        SellQuantity = temp[2],
                        TotalSellAmount = temp[3],
                        BuyPrice = temp[4],
                        BuyQuantity = temp[5],
                        TotalBuyAmount = temp[6]
                    });
                else
                    Console.WriteLine(temp);
            }
            SetStorage(model);
        }
        void SetStorage(List<Datum> model)
        {
            using (var db = new GoblinBatDbContext(key))
                try
                {
                    db.Configuration.AutoDetectChangesEnabled = false;
                    db.BulkInsert(model, o =>
                    {
                        o.InsertIfNotExists = true;
                        o.BatchSize = 15000;
                        o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                        o.AutoMapOutputDirection = false;
                    });
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace, ex.TargetSite.Name);
                }
                finally
                {
                    db.Configuration.AutoDetectChangesEnabled = true;
                }
        }
        void SetStorage(IOrderedEnumerable<KeyValuePair<string, string>> param, string code)
        {
            try
            {
                var path = System.IO.Path.Combine(Application.StartupPath, quotes, code);
                var directory = new DirectoryInfo(path);

                if (directory.Exists == false)
                    directory.Create();

                using (var sw = new StreamWriter(string.Concat(path, @"\", DateTime.Now.ToString(storage), res), true))
                    foreach (var kv in param)
                        sw.WriteLine(string.Concat(kv.Key, ",", kv.Value));
            }
            catch (Exception ex)
            {
                new ExceptionMessage(ex.StackTrace, ex.TargetSite.Name);
            }
        }
        [Conditional("DEBUG")]
        void SendMessage(object code)
        {
            if (code is int response)
                Console.WriteLine(response);

            else if (code is string str)
                Console.WriteLine(str);
        }
        protected CallUp(string key) : base(key) => this.key = key;
        readonly string key;
        const string quotes = "Quotes";
        const string storage = "yyMMddHH";
        const string start = "0900";
    }
}