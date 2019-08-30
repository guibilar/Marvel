using Marvel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace Marvel.Config
{
    public class ComicConfig : EntityTypeConfiguration<Comic>
    {
        public ComicConfig()
        {
            HasKey(c => c.Id);
            Property(c => c.Id_marvel)
                .IsRequired();

            Property(c => c.Titulo)
                .HasMaxLength(300)
                .IsRequired();

            Property(c => c.Preco)
                .IsOptional();

            Property(c => c.Descricao)
                .HasColumnType("TEXT")
                .HasMaxLength(int.MaxValue).IsOptional();

            Property(c => c.Pic_url)
                .HasMaxLength(300);

            Property(c => c.Wiki_url)
                .HasMaxLength(300);

            ToTable("comics");
        }
    }
}