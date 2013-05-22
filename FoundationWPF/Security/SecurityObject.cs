﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoundationWPF.Security {

   /// <summary>
   /// Represent a security object by the Roles it holds.
   /// </summary>
   public abstract class SecurityObject {

      /// <summary>
      /// List of string that contains the roles associated with this SecurityObject
      /// </summary>
      public List<string> Roles { get; set; }

      /// <summary>
      /// Login initialization stuff, should populate the Roles property
      /// </summary>
      public abstract void Login();

      public virtual async void AsyncLogin() {
         await Task.Factory.StartNew(Login);
      }
   
      public SecurityObject(){
         Roles = new List<string>();
      }

   }
}
