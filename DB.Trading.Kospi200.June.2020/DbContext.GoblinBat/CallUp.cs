﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ShareInvest.Message;
using ShareInvest.Models;

namespace ShareInvest.GoblinBatContext
{
    public class CallUp
    {
        protected string GetStrategy()
        {
            using (var db = new GoblinBatDbContext())
            {
                try
                {
                    return db.Codes.First(c => c.Info.Equals(db.Codes.Where(o => o.Code.Length == 8 && o.Code.Substring(0, 3).Equals("101") && o.Code.Substring(5, 3).Equals("000")).Max(o => o.Info))).Code;
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace);
                }
            }
            return GetStrategy();
        }
        protected List<string> RequestCodeList(List<string> list)
        {
            string code = string.Empty;

            try
            {
                using (var db = new GoblinBatDbContext())
                {
                    foreach (var temp in db.Codes.Select(o => new
                    {
                        o.Code
                    }))
                    {
                        code = temp.Code;

                        if (db.Codes.Any(o => o.Code.Length < 6))
                            db.Codes.BulkDelete(db.Codes.Where(o => o.Code.Length < 6), o => o.BatchSize = 100);

                        if (db.Days.Any(o => o.Code.Equals(temp.Code) && o.Date < 10000000))
                        {
                            db.Days.BulkDelete(db.Days.Where(o => o.Date < 10000000), o => o.BatchSize = 100);

                            if (db.Days.Any(o => o.Code.Length < 6))
                                db.Days.BulkDelete(db.Days.Where(o => o.Code.Length < 6), o => o.BatchSize = 100);
                        }
                        if (temp.Code.Equals(q3) && db.Quotes.Any(o => o.Code.Equals(q3)))
                            db.Quotes.BulkDelete(db.Quotes.Where(o => o.Code.Equals(q3)), o => o.BatchSize = 100);

                        if (temp.Code.Length == 6 && (db.Days.Any(o => o.Code.Equals(temp.Code)) == false || db.Stocks.Any(o => o.Code.Equals(temp.Code)) == false || int.Parse(db.Days.Where(o => o.Code.Equals(temp.Code)).Max(o => o.Date).ToString().Substring(2)) < int.Parse(db.Stocks.Where(o => o.Code.Equals(code)).Min(o => o.Date).ToString().Substring(0, 6))))
                        {
                            list.Add(temp.Code);

                            if (db.Stocks.Any(o => o.Code.Length < 6))
                                db.Stocks.BulkDelete(db.Stocks.Where(o => o.Code.Length < 6), o => o.BatchSize = 100);
                        }
                        else if (temp.Code.Length == 8 && temp.Code.Substring(5, 3).Equals("000") && db.Futures.Any(o => o.Date < 100000000000000))
                        {
                            db.Futures.BulkDelete(db.Futures.Where(o => o.Date < 100000000000000), o => o.BatchSize = 100);

                            if (db.Futures.Any(o => o.Code.Length < 8))
                                db.Futures.BulkDelete(db.Futures.Where(o => o.Code.Length < 8), o => o.BatchSize = 100);
                        }
                        else if (temp.Code.Length == 8 && temp.Code.Substring(5, 3).Equals("000") == false && db.Options.Any(o => o.Date < 100000000000000))
                        {
                            db.Options.BulkDelete(db.Options.Where(o => o.Date < 100000000000000), o => o.BatchSize = 100);

                            if (db.Options.Any(o => o.Code.Length < 8))
                                db.Options.BulkDelete(db.Options.Where(o => o.Code.Length < 8), o => o.BatchSize = 100);
                        }
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                new ExceptionMessage(ex.StackTrace, code);
            }
            catch (Exception ex)
            {
                using (var db = new GoblinBatDbContext())
                {
                    var stocks = db.Stocks.Where(o => o.Code.Equals(code));

                    if (stocks.Any(o => o.Date < 100000000000000))
                        db.Stocks.BulkDelete(stocks.Where(o => o.Date < 100000000000000), o => o.BatchSize = 100);
                }
                new ExceptionMessage(ex.StackTrace, code);
                RequestCodeList(new List<string>());
            }
            return list;
        }
        protected List<string> RequestCodeList(List<string> list, string[] market)
        {
            using (var db = new GoblinBatDbContext())
            {
                try
                {
                    foreach (var temp in db.Codes.Select(o => new
                    {
                        o.Code,
                        o.Info
                    }))
                        if (temp.Code.Length == 6 && Array.Exists(market, o => o.Equals(temp.Code)) || temp.Code.Length == 8 && DateTime.Compare(DateTime.ParseExact(temp.Info, "yyyyMMdd", null), DateTime.Now) >= 0)
                            list.Add(temp.Code);

                    return list;
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace);
                }
            }
            return RequestCodeList(list, market);
        }
        protected string GetRetention(int param, string code)
        {
            long max = 0;
            using (var db = new GoblinBatDbContext())
            {
                try
                {
                    switch (param)
                    {
                        case 1:
                            max = db.Futures.Where(o => o.Code.Equals(code)).Max(o => o.Date);
                            break;

                        case 2:
                            max = db.Options.Where(o => o.Code.Equals(code)).Max(o => o.Date);
                            break;

                        case 3:
                            max = db.Stocks.Where(o => o.Code.Equals(code)).Max(o => o.Date);
                            break;

                        case 4:
                            max = db.Days.Where(o => o.Code.Equals(code)).Max(o => o.Date);
                            break;
                    };
                    return max > 0 ? (max.ToString().Length > 12 ? max.ToString().Substring(0, 12) : max.ToString()) : "DoesNotExist";
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
            }
            return "DoesNotExist";
        }
        protected void SetInsertCode(string code, string name, string info)
        {
            new Task(() =>
            {
                using (var db = new GoblinBatDbContext())
                {
                    try
                    {
                        if (db.Codes.Where(o => o.Code.Equals(code) && o.Info.Equals(info) && o.Name.Equals(name)).Any())
                            return;

                        db.Codes.AddOrUpdate(new Codes
                        {
                            Code = code,
                            Name = name,
                            Info = info
                        });
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        new ExceptionMessage(ex.StackTrace, code);
                    }
                }
            }).Start();
        }
        protected void SetStorage(string code, string[] param)
        {
            if (param.Length < 3)
                return;

            new Task(() =>
            {
                try
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
                    using (var db = new GoblinBatDbContext())
                    {
                        db.Configuration.AutoDetectChangesEnabled = true;

                        if (days)
                            db.BulkInsert((List<Days>)model, o =>
                            {
                                o.InsertIfNotExists = true;
                                o.BatchSize = 10000;
                                o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                                o.AutoMapOutputDirection = false;
                            });
                        else if (stocks)
                            db.BulkInsert((List<Stocks>)model, o =>
                            {
                                o.InsertIfNotExists = true;
                                o.BatchSize = 10000;
                                o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                                o.AutoMapOutputDirection = false;
                            });
                        else if (options)
                            db.BulkInsert((List<Options>)model, o =>
                            {
                                o.InsertIfNotExists = true;
                                o.BatchSize = 10000;
                                o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                                o.AutoMapOutputDirection = false;
                            });
                        else if (futures)
                            db.BulkInsert((List<Futures>)model, o =>
                            {
                                o.InsertIfNotExists = true;
                                o.BatchSize = 10000;
                                o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                                o.AutoMapOutputDirection = false;
                            });
                        db.Configuration.AutoDetectChangesEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    new ExceptionMessage(ex.StackTrace, code);
                }
            }).Start();
        }
        protected void SetStorage(string code, Queue<string> quotes)
        {
            new Task(() =>
            {
                string onTime = string.Empty, date = DateTime.Now.ToString("yyMMdd");
                int count = 0;
                List<Quotes> model = new List<Quotes>(1024);

                while (quotes.Count > 0)
                {
                    var temp = quotes.Dequeue().Split(';');

                    if (double.TryParse(temp[1].Split('^')[0], out double price) && price > 105.95)
                    {
                        if (temp[0].Equals(onTime))
                            count++;

                        else
                        {
                            onTime = temp[0];
                            count = 0;
                        }
                        model.Add(new Quotes
                        {
                            Code = code,
                            Date = string.Concat(date, onTime, count.ToString("D3")),
                            Contents = temp[1]
                        });
                    }
                }
                SetStorage(model);
            }).Start();
        }
        private void SetStorage(List<Quotes> model)
        {
            using (var db = new GoblinBatDbContext())
            {
                try
                {
                    foreach (var list in model)
                        if (db.Quotes.Where(o => o.Code.Equals(list.Code) && o.Date.Equals(list.Date) && o.Contents.Equals(list.Contents)).Any())
                            return;

                    db.Configuration.AutoDetectChangesEnabled = true;
                    db.BulkInsert(model, o =>
                    {
                        o.InsertIfNotExists = true;
                        o.BatchSize = 10000;
                        o.SqlBulkCopyOptions = (int)SqlBulkCopyOptions.Default | (int)SqlBulkCopyOptions.TableLock;
                        o.AutoMapOutputDirection = false;
                    });
                    db.Configuration.AutoDetectChangesEnabled = false;
                }
                catch (Exception ex)
                {
                    for (var i = 0; i < GC.GetTotalMemory(true); i++)
                        Console.WriteLine(ex.TargetSite.Name);

                    new ExceptionMessage(ex.StackTrace, ex.TargetSite.Name);
                    new Task(() => SetStorage(model)).Start();
                }
            }
        }
        private const string q3 = "101Q3000";
    }
}