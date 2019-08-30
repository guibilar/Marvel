using Marvel.Config;
using Marvel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Marvel.Context
{
    public class MarvelContext : DbContext
    {
        public MarvelContext() : base("MarvelProject")
        {
            Database.CreateIfNotExists();
        }
        public DbSet<Personagem> Personagens { get; set; }
        public DbSet<Comic> Comics { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Properties()
               .Where(p => p.Name == "Id")
               .Configure(p => p.IsKey());
            modelBuilder.Properties<string>()
                .Configure(p => p.HasColumnType("varchar"));
            modelBuilder.Properties<string>()
                .Configure(p => p.HasMaxLength(100));

            modelBuilder.Configurations.Add(new PersonagemConfig());
            modelBuilder.Configurations.Add(new ComicConfig());

            base.OnModelCreating(modelBuilder);
        }
    }
}