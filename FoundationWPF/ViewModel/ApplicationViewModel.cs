using FoundationWPF.Navigation;
using FoundationWPF.Navigation.Config;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FoundationWPF.ViewModel {

   /// <summary>
   /// Represents the current state of the application and manages a two levels navigation system.
   /// Sort of an Orchestrator.
   /// </summary>
   public class ApplicationViewModel : ViewModelFoundation {

      #region private fields

      private ICommand changeViewCmd;
      private ICommand changeMainCmd;

      private ViewModelFoundation currentViewModel;
      private NavigConfig currentMainNav;

      private List<NavigConfig> navStructure;

      private ViewModelFoundation nowLoadingViewModel;

      #endregion

      #region Properties

      /// <summary>
      /// Represents the currently displayed ViewModel. 
      /// There is a special setter, however, to handle the async loading of IPreLoadable ViewModels
      /// </summary>
      public ViewModelFoundation CurrentViewModel {
         get { return currentViewModel; }
         set {
            if(currentViewModel != value) {
               currentViewModel = value;

               if(currentViewModel is IPreLoadable) { 
                  var cvm = currentViewModel as IPreLoadable;
                  if(cvm.IsPreLoadNeeded)      // Check if the ViewModel needs PreLoading
                     AsyncLoadViewModel(cvm);  // Launch asynchronous loading
                  else if(cvm.IsCurrentlyLoading) {           // If it's already loading 
                     nowLoadingViewModel = currentViewModel;  // Change the current loading VM
                     currentViewModel = cvm.LoadingViewModel; // And display the loading view
                  }
               }
               RaiseViewModelDisplayed(currentViewModel);
               RaisePropertyChanged("CurrentViewModel");
            }
         }
      }

      /// <summary>
      /// Sets the view as a loading one and execute asynchronously the PreLoad method of IPreLoadable.
      /// Once loaded, it replaces the loading view by the freshly loaded ViewModel
      /// </summary>
      /// <param name="vm"></param>
      private async void AsyncLoadViewModel(IPreLoadable vm) {
         vm.IsPreLoadNeeded = false;             // This ViewModel doesn't need loading anymore
         nowLoadingViewModel = currentViewModel; // Set the currently loading ViewModel
         currentViewModel = vm.LoadingViewModel; // Display the loading view

         vm.IsCurrentlyLoading = true;
         await Task.Factory.StartNew(() => vm.PreLoad()); // Launch the preloading in another task
         vm.IsCurrentlyLoading = false;

         // Check if the view is a ILoadingViewModel and if it's currently in a loading state
         if(currentViewModel is ILoadingViewModel && !(nowLoadingViewModel as IPreLoadable).IsCurrentlyLoading) {
            currentViewModel = nowLoadingViewModel; // Replace the loading by the freshly loaded ViewModel
            nowLoadingViewModel = null;
            RaiseViewModelDisplayed(currentViewModel);
            RaisePropertyChanged("CurrentViewModel");
         }
      }

      // Currently selected main navig
      public NavigConfig CurrentMainNav {
         get { return currentMainNav; } 
         set {
            if(currentMainNav != value) {
               currentMainNav = value;
               CurrentViewModel = SubNavig[0].VM;
               RaisePropertyChanged("CurrentMainNav");
               RaisePropertyChanged("SubNavig");
            }
         }
      }

      // List of main navig properties
      public List<NavigConfig> MainNavig {
         get {
            return (from elem in navStructure
                    where elem.Enabled
                    orderby elem.Position
                    select elem).Distinct().ToList();
         }
      }

      /// List of current sub-viewmodels or if there is no subVMs return the VM of the MainNavig
      public List<NavigConfig> SubNavig {
         get {
            if(CurrentMainNav.SubConfig.Count > 0)
               return (from elem in CurrentMainNav.SubConfig
                       where elem.Enabled
                       orderby elem.Position
                       select elem).ToList();
            else
               return new List<NavigConfig> { CurrentMainNav };
         }
      }

      #endregion

      // ctor: injection of all registred viewmodels (in MainViewModelsModule.cs)
      /// <summary>
      /// Instanciate the ApplicationViewModel. All registred ViewModels are injected into the param mainViewModels
      /// </summary>
      /// <param name="mainViewModels">An array of the application's ViewModels (injected)</param>
      /// <param name="currentUser">The current user of the application (injected)</param>
      public ApplicationViewModel(ViewModelFoundation[] mainViewModels) {

         // Load navigation informations
         NavigConfigLoader.RegisterConfigurations(new RH(), new Emp(), new EmpDetails(),
                                                  new TS(), new Agenda());
         navStructure = new List<NavigConfig>();

         // Browse every ViewModels and search if it has a Navig attribute
         foreach(var vm in mainViewModels) {
            foreach(NavigAttribute na in vm.GetType().GetCustomAttributes(typeof(NavigAttribute), inherit: false)) {
               NavigConfig mainConf;
                  
               // Check if it already exists a NavigConf with the same name
               var existingMainConf = navStructure.SingleOrDefault(n => n.Name == na.MainConfig.Name);

               if(existingMainConf != null) // If it's the case, set mainConf to it
                  mainConf = existingMainConf;
               else {                       // Else we use the MainConfig as a new elem of navStructure
                  if(!IsAuthorized(vm.GetType(), CurrentUser))
                     na.MainConfig.Enabled = false;
                  mainConf = na.MainConfig;
                  navStructure.Add(mainConf);
               }

               // Assign the VM to the SubConfig or the main (if NavigAttribute has a single param)
               if(na.SubConfig != null) {
                  if(!IsAuthorized(vm.GetType(), CurrentUser)) 
                     na.SubConfig.Enabled = false;
                  na.SubConfig.VM = vm;
                  mainConf.SubConfig.Add(na.SubConfig);
               } else
                  mainConf.VM = vm;
            }
         }
         CurrentMainNav = MainNavig[0];
         CurrentViewModel = SubNavig[0].VM;
      }

      #region Commands

      public ICommand ChangeMainCmd {
         get {
            if(changeMainCmd == null)
               changeMainCmd = new RelayCommand<NavigConfig>(nc => CurrentMainNav = nc);
            return changeMainCmd;
         }
      }

      public ICommand ChangeViewCmd {
         get {
            if(changeViewCmd == null)
               changeViewCmd = new RelayCommand<ViewModelFoundation>(vm => CurrentViewModel = vm);
            return changeViewCmd;
         }
      }

      #endregion
   }
}
