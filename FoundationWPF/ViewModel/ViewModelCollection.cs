﻿using FoundationData.GenericRepo;
using FoundationWPF.DI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Data;

namespace FoundationWPF.ViewModel {

   /// <summary>
   /// Represents a collection of typed ViewModels (all the same type). It exposes a "CollectionView" property containing
   /// all the ViewModels in a ICollectionView.
   /// </summary>
   /// <typeparam name="TEntity">The entity to be used along with the IRepository interface</typeparam>
   /// <typeparam name="TViewModel">The ViewModel of type DynamicViewModel<TEntity></typeparam>
   public abstract class ViewModelCollection<TEntity, TViewModel> : ViewModelFoundation, IPreLoadable 
                                                                    where TEntity : class 
                                                                    where TViewModel : ViewModelProxy<TEntity> {
      private IEnumerable<TEntity> entites;
      protected IRepository<TEntity> Repo { get; set; }
      private Lazy<ICollectionView> collectionView;
      protected ObservableCollection<TViewModel> All { get; set; }

      /// <summary>
      /// Exposes all the entites as a ICollectionView, it is a lazy loaded property
      /// </summary>
      public ICollectionView CollectionView {
         get { return collectionView.Value; }
      }

      /// <summary>
      /// Create a new ViewModelCollection, containing by default every entites. Use the 
      /// Filter(predicate) method to apply a filter.
      /// </summary>
      public ViewModelCollection() : base() {
         All = new ObservableCollection<TViewModel>();
         Repo = Injector.I.Get<IRepository<TEntity>>();
         entites = Repo.GetAllAsEnumerable(); // lazy loaded
         IsPreLoadNeeded = true;

         // Lazy initialization of the collection view (real loading)
         collectionView = new Lazy<ICollectionView>(() => {
            foreach(var ent in entites) // real loading of entites (previously lazily loaded)
               All.Add(Activator.CreateInstance(typeof(TViewModel), ent) as TViewModel);
            return CollectionViewSource.GetDefaultView(All);
         });
      }

      /// <summary>
      /// Filter the entites collection with a given predicate, null for all entites.
      /// </summary>
      /// <param name="predicate">The predicate to filter the entites collection, null to select all entites.</param>
      public void Filter(Expression<Func<TEntity, bool>> predicate) {
         entites = predicate != null ? Repo.Query(predicate) : Repo.GetAllAsEnumerable();
      }

      #region IPreLoadable stuff
      
      /// <summary>
      /// Must be overridden to perform all kind of long loading. It's used to know when to display a loading screen.
      /// </summary>
      public virtual void PreLoad() {
         var c = CollectionView;
      }

      public virtual bool IsPreLoadNeeded { get; set; }

      public virtual bool IsCurrentlyLoading { get; set; }

      #endregion
   }
}
 