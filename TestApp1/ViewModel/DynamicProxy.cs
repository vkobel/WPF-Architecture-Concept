﻿using KobiWPFFramework.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace KobiWPFFramework.ViewModel {

   using PropDep = List<Tuple<string, string>>;

   /// <summary>
   /// Dynamic proxy class used to access multiple objects properties by a single proxy.
   /// i.e. extend a model object with custom properties of a viewmodel. It implements the interface INotifyPropertyChanged.
   /// </summary>
   public class DynamicProxy : DynamicObject, INotifyPropertyChanged {

      private List<object> proxiedObjs;
      private PropDep propDependencies;

      /// <summary>
      /// Subscribe to this to know when a property has changed into the proxy.
      /// </summary>
      public event PropertyChangedEventHandler PropertyChanged;

      /// <summary>
      /// Create a new instance of the proxy
      /// </summary>
      /// <param name="proxiedObjects">objects you want to be proxied</param>
      public DynamicProxy(params object[] proxiedObjects) {
         proxiedObjs = new List<object>(proxiedObjects);
         propDependencies = new PropDep();
      }

      /// <summary>
      /// Register a new object into the proxy after the ctor has been called
      /// </summary>
      /// <param name="obj">The new object to be proxied</param>
      public void Register(object obj) {
         proxiedObjs.Add(obj);
      }

      /// <summary>
      /// Register a new dependency property. When PropertyChanged is raised, the object looks for a registered dependency
      /// and fires a PropertyChanged event for it. i.e. RegisterPropertyDependency("Fullname", "Firstname", "Lastname");
      /// tells that the "Fullname" property must be update when "Firstname" or "Lastname" is modified.
      /// </summary>
      /// <param name="propName">The name of the new property (non-existing in the model btw)</param>
      /// <param name="dependsOn">Any number of string the paramerter depends on</param>
      public void RegisterPropertyDependency(string propName, params string[] dependsOn) {
         foreach(var p in dependsOn)
            propDependencies.Add(new Tuple<string,string>(p, propName));
      }

      public override bool TryGetMember(GetMemberBinder binder, out object result) {
         result = proxiedObjs.GetFirstMatchingPropertyValue(binder.Name);
         return result != null;
      }

      public override bool TrySetMember(SetMemberBinder binder, object value) {
         proxiedObjs.SetFirstMatchingPropertyValue(binder.Name, value);
         PropertyChanged(this, new PropertyChangedEventArgs(binder.Name));
         // Raise dependency properties notifications
         foreach(var prop in propDependencies.Where(p => p.Item1 == binder.Name))
            PropertyChanged(this, new PropertyChangedEventArgs(prop.Item2));
         return true;
      }
   }
}
