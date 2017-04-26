/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:StressLoadDemo"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using StressLoadDemo.Model.DataProvider;

namespace StressLoadDemo.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<TabRequirementViewModel>();
            SimpleIoc.Default.Register<TabResourceViewModel>();
            SimpleIoc.Default.Register<TabMonitorViewModel>();
            SimpleIoc.Default.Register<IStressDataProvider,StressLoadDataProvider>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public TabRequirementViewModel RequireTab
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TabRequirementViewModel>();
            }
        }

        public TabResourceViewModel ResourceTab
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TabResourceViewModel>();
            }
        }

        public TabMonitorViewModel MonitorTab
        {
            get { return ServiceLocator.Current.GetInstance<TabMonitorViewModel>(); }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}