﻿using System;
using System.Collections.Generic;

namespace FoundationWPF.Security {

   [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
   class AuthAttribute : Attribute {

      public List<string> Roles { get; private set; }

      public AuthAttribute(params string[] roles) {
         Roles = new List<string>(roles);
      }
   }
}