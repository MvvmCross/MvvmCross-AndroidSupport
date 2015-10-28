using Android.Support.V4.App;

namespace Cirrious.MvvmCross.Droid.Support.Fragging.Caching
{
    public class FragmentChangingEvenArgs
    {
        private readonly string _tag;

        public FragmentChangingEvenArgs(string tag, FragmentTransaction fragmentTransaction)
        {
            _tag = tag;
            FragmentTransaction = fragmentTransaction;
        }

        public string Tag { get; private set; }
        public FragmentTransaction FragmentTransaction { get; private set; }
    }
}