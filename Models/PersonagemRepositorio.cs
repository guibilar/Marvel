using Marvel.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace Marvel.Models
{
    public class PersonagemRepositorio
    {
        public MarvelContext Db { get; set; }
        public DbSet<Personagem> DbSet { get; set; }
        public PersonagemRepositorio(MarvelContext context = null)
        {
            if (context == null)
            {
                context = new MarvelContext();
            }

            Db = context;
            DbSet = Db.Set<Personagem>();
        }

        /// <summary>
        /// Procura o personagem pelo id do BD local
        /// </summary>
        /// <param name="id">Id do personagem no BD local</param>
        /// <returns>Objeto do tipo personagem</returns>
        public virtual Personagem FindById(long id)
        {
            return DbSet.FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Procura o personagem pelo id no BD da API
        /// </summary>
        /// <param name="id_marvel">Id do personagem no BD da API</param>
        /// <returns>Objeto do tipo personagem</returns>
        public virtual Personagem FindByIdMarvel(long id_marvel)
        {
            return DbSet.FirstOrDefault(p => p.Id_marvel == id_marvel);
        }

        /// <summary>
        /// Salva o personagem no BD local
        /// </summary>
        /// <param name="obj">Objeto do tipo personagem</param>
        /// <returns>Objeto do tipo personagem</returns>
        public virtual Personagem Save(Personagem obj)
        {
            if (obj.Id > 0)
            {
                return Update(obj);
            }
            var objAdd = DbSet.Add(obj);
            SaveChanges();
            return objAdd;
        }

        /// <summary>
        /// Salva uma lista de personagens no BD local
        /// </summary>
        /// <param name="objs">Lista de objetos do tipo personagem</param>
        /// <returns>Lista de objetos do tipo personagem</returns>
        public virtual void SaveBach(IEnumerable<Personagem> objs)
        {
            foreach (var obj in objs)
            {
                obj.Id = 0;
                Save(obj);
            }
            SaveChanges();
        }

        /// <summary>
        /// Atualiza um personagem no BD local
        /// </summary>
        /// <param name="obj">Objeto do tipo personagem</param>
        /// <returns>Objeto do tipo personagem</returns>
        public virtual Personagem Update(Personagem obj)
        {
            var entry = Db.Entry(obj);
            var attachedEntity = Db.ChangeTracker.Entries<Personagem>().FirstOrDefault(p => p.Entity.Id == obj.Id);
            if (attachedEntity != null)
            {
                Db.Entry<Personagem>(attachedEntity.Entity).State = EntityState.Modified;
            }
            else
            {
                DbSet.Attach(obj);
                entry.State = EntityState.Modified;
            }
            SaveChanges();
            return obj;
        }

        /// <summary>
        ///     Remove registro no banco de dados que contem o id informado
        /// </summary>
        /// <param name="id">
        ///     Id do objeto que deseja remover
        /// </param>
        public virtual void RemoveById(long id)
        {
            DbSet.Remove(DbSet.Find(id));
            SaveChanges();
        }

        /// <summary>
        /// Remove por Varios elementos pelos seus Ids
        /// </summary>
        /// <param name="ids"></param>
        public virtual void RemoveRange(long[] ids)
        {
            foreach (var id in ids)
            {
                DbSet.Remove(DbSet.Find(id));
            }
            SaveChanges();
        }

        /// <summary>
        /// Finaliza todas as conexões e alterações realizadas sem efetiva-las
        /// </summary>
        public virtual void Dispose()
        {
            Db.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finaliza todas as conexões e alterações realizadas as efetivando
        /// </summary>
        public virtual void SaveChanges()
        {
            try
            {
                Db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {

                // Get the current entity values and the values in the database
                // as instances of the entity type
                var entry = ex.Entries.Single();
                var databaseValues = entry.GetDatabaseValues();
                var databaseValuesAsPersonagem = (Personagem)databaseValues.ToObject();

                // Choose an initial set of resolved values. In this case we
                // make the default be the values currently in the database.
                var resolvedValuesAsPersonagem = (Personagem)databaseValues.ToObject();

                // Update the original values with the database values and
                // the current values with whatever the user choose.
                entry.OriginalValues.SetValues(databaseValues);
                entry.CurrentValues.SetValues(resolvedValuesAsPersonagem);
            }


        }
    }
}
