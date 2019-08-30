using Marvel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace Marvel.Config
{
    public class PersonagemConfig : EntityTypeConfiguration<Personagem>
    {
        public PersonagemConfig()
        {
            HasKey(p => p.Id);
            Property(p => p.Id_marvel)
                .IsRequired();

            Property(p => p.Nome)
                .HasMaxLength(300)
                .IsRequired();

            Property(p => p.Descricao)
                .HasColumnType("TEXT")
                .HasMaxLength(int.MaxValue).IsOptional();

            Property(p => p.Pic_url)
                .HasMaxLength(300);

            Property(p => p.Wiki_url)
                .HasMaxLength(300);

            ToTable("personagem");
        }
    }
}