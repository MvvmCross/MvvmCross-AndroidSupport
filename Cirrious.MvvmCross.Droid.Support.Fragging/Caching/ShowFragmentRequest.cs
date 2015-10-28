using Android.OS;

namespace Cirrious.MvvmCross.Droid.Support.Fragging.Caching
{
    /// <summary>
    /// 
    /// </summary>
    public class ShowFragmentRequest
    {
        public ShowFragmentRequest()
        {
            ShouldAddToBackStack = false;
            ShouldForceFragmentReplace = false;
            Tag = string.Empty;
        }

        /// <summary>
        /// The tag for the fragment to lookup
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Where you want to show the Fragment
        /// </summary>
        public int ContentId { get; set; }

        /// <summary>
        /// Bundle which usually contains a Serialized MvxViewModelRequest
        /// </summary>
        public Bundle Bundle { get; set; }

        /// <summary>
        /// If you want to add the fragment to the backstack so on backbutton it will go back to it
        /// </summary>
        public bool ShouldAddToBackStack { get; set; }
        /// <summary>
        /// Force replace a fragment with the same viewmodel at the same contentid
        /// </summary>
        public bool ShouldForceFragmentReplace { get; set; }
    }
}