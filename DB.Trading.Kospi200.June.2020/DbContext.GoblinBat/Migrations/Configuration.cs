﻿using System.Data.Entity.Migrations;

namespace ShareInvest.GoblinBatContext.Migrations
{
    sealed class Configuration : DbMigrationsConfiguration<GoblinBatDbContext>
    {
        protected override void Seed(GoblinBatDbContext context)
        {

        }
        public Configuration() => AutomaticMigrationsEnabled = false;
    }
}