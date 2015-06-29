using Android.Content;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Binding.Droid;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.Droid.Support.AppCompat;
using Cirrious.MvvmCross.ViewModels;

namespace Example.Droid
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new Core.App();
        }
		
        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

        protected override MvxAndroidBindingBuilder CreateBindingBuilder()
        {
            return new MvxAppCompatBindingBuilder();
        }
    }
}