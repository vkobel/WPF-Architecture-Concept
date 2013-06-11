﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FoundationData.GenericRepo {

   /// <summary>
   /// Build a repository for a specified POCO class (using a DbContext)
   /// </summary>
   /// <typeparam name="T">The type of entity that the repository will hold, must be present in the DbContext</typeparam>
   public class Repository<T> : IRepository<T> where T : class {

      /// <summary>
      /// The DbContext that holds the database
      /// </summary>
      private DbContext ctx;

      /// <summary>
      /// The set that holds the current entities
      /// </summary>
      protected DbSet<T> entities {
         get;
         set;
      }

      public Repository(DbContext ctx) {
         this.ctx = ctx;

         /// Get the matching DbSet by searching the context's properties
         entities = ctx.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                 .Single(p => p.PropertyType == typeof(DbSet<T>))
                                 .GetValue(ctx) as DbSet<T>;
      }

      #region repo methods

      public IEnumerable<T> GetAllAsEnumerable() {
         return entities.AsEnumerable();
      }

      public T[] GetAll() {
         return entities.ToArray();
      }

      public IQueryable<T> Query(Expression<Func<T, bool>> predicate) {
         return entities.Where(predicate);
      }

      /// <summary>
      /// Reloads the entity from the database and returns the specified property
      /// </summary>
      /// <param name="entity">The entity that need to be reloaded</param>
      /// <param name="property">The property to be returned</param>
      /// <returns>Returns the updated property on the specified entity</returns>
      public object GetReloadedProperty(T entity, string property) {
         var dbEntry = ctx.Entry<T>(entity);
         if(dbEntry.State != System.Data.EntityState.Detached)
            dbEntry.Reload();
         return dbEntry.Entity.GetType().GetProperty(property).GetValue(dbEntry.Entity);
      }

      public T GetSingle(Expression<Func<T, bool>> predicate) {
         return entities.SingleOrDefault(predicate);
      }

      public void Create(T entity) {
         Add(entity);
         Persist();
      }

      public void Add(T entity) {
         entities.Add(entity);
      }

      public void Delete(T entity) {
         entities.Remove(entity);
      }

      public int Persist() {
         return ctx.SaveChanges();
      }

      #endregion

      protected virtual void Dispose(bool all) {
         if(all && ctx != null) {
            ctx.Dispose();
            ctx = null;
         }
         entities = null;
      }

      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

   }
}
