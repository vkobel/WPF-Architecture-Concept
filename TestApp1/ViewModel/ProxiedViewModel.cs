﻿using GalaSoft.MvvmLight;
using KobiDataFramework.GenericRepo;
using KobiWPFFramework.Ninject;
using System.ComponentModel;

namespace KobiWPFFramework.ViewModel {

   /// <summary>
   /// Represents a generic ViewModel for a specific model entity. Often, this is the object exposed to the view
   /// directly, or via the ViewModelCollection.
   /// </summary>
   /// <typeparam name="TEntity">The type of the model entity (to create the repository)</typeparam>
   public abstract class ProxiedViewModel<TEntity> : ViewModelBase where TEntity : class {

      protected TEntity entity;
      private IRepository<TEntity> repo;

      /// <summary>
      /// Contains all the properties of the model as well as the extended ones form the ViewModel
      /// </summary>
      public dynamic BindingData { get; set; }

      public ProxiedViewModel(TEntity entity) {
         this.entity = entity;
         this.repo = Nj.I.Get<IRepository<TEntity>>();
         BindingData = new DynamicProxy(entity);
         (BindingData as INotifyPropertyChanged).PropertyChanged += DynamicViewModel_PropertyChanged;
      }

      // Alert the ViewModelBase if a property has changed and persist the data
      private void DynamicViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
         RaisePropertyChanged(e.PropertyName);
         
         // ???????????????????????????????????????
         // ???? Should add a condition ???????????
         repo.Persist(); // AsyncPersist isn't that good huh ?
      }
   }
}
