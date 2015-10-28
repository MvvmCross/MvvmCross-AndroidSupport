namespace Cirrious.MvvmCross.Droid.Support.Fragging.Caching
{
    public class CloseFragmentRequest
    {
        /// <summary>
        /// The tag for the fragment to lookup
        /// </summary>
        public string Tag { get; private set; }
        /// <summary>
        /// Where you want to close the Fragment
        /// </summary>
        public int ContentId { get; private set; }

        public CloseFragmentRequest(string tag, int contentId)
        {
            Tag = tag;
            ContentId = contentId;
        }
    }
}