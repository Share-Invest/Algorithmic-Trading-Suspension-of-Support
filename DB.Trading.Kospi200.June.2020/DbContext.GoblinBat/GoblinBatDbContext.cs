using System.Data;
using System.Data.Entity;
using System.Threading;

using ShareInvest.Message;
using ShareInvest.Models;

namespace ShareInvest.GoblinBatContext
{
    public class GoblinBatDbContext : DbContext
    {
        public DbSet<Statistics> Statistics
        {
            get; set;
        }
        public DbSet<Identify> Identifies
        {
            get; set;
        }
        public DbSet<Charts> Charts
        {
            get; set;
        }
        public DbSet<Strategics> Material
        {
            get; set;
        }
        public DbSet<Codes> Codes
        {
            get; set;
        }
        public DbSet<Futures> Futures
        {
            get; set;
        }
        public DbSet<Options> Options
        {
            get; set;
        }
        public DbSet<Stocks> Stocks
        {
            get; set;
        }
        public DbSet<Days> Days
        {
            get; set;
        }
        public DbSet<Datum> Datums
        {
            get; set;
        }
        public override int SaveChanges() => this.BatchSaveChanges();
        public GoblinBatDbContext(string key) : base(new Secret().GetPort(key))
        {
            while (Database.Connection.State.Equals(ConnectionState.Closed) == false)
            {
                Thread.Sleep(39735);
                new ExceptionMessage(Database.Connection.State.ToString());
                Database.Connection.Close();
            }
        }
        public GoblinBatDbContext(char emergency, string key) : base(new Secret().GetEmergencyPort(key))
        {
            while (emergency.Equals((char)69) == false && Database.Connection.State.Equals(ConnectionState.Closed) == false)
            {
                Thread.Sleep(39735);
                new ExceptionMessage(Database.Connection.State.ToString());
                Database.Connection.Close();
            }
        }
    }
}