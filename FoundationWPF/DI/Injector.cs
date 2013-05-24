﻿using FoundationWPF.DI.Modules;
using Ninject;
using Ninject.Modules;
using System;

namespace FoundationWPF.DI {

   /// <summary>
   /// Singleton object to access the Ninject's kernel
   /// </summary>
   public class Injector {

      private static readonly Lazy<Injector> instance = new Lazy<Injector>(() => new Injector());
      protected IKernel Kernel { get; private set; }

      private Injector() {
         Kernel = new StandardKernel(new SecurityModule(),
                                     new RepositoriesModule(),
                                     new ViewModelsModule(),
                                     new NavigationModule());
      }

      public void LoadModules(params NinjectModule[] modules) {
         Kernel.Load(modules);
      }

      public static Injector I {
         get { return instance.Value; }
      }

      public T Get<T>(){
         return Kernel.Get<T>();
      }
   
   }
}
