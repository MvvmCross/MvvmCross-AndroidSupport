using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Support.V4.App;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.Droid.Support.Fragging.Fragments;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;

namespace Cirrious.MvvmCross.Droid.Support.Fragging.Caching
{
    public class FragmentCache
    {
        private const string SavedFragmentTypesKey = "__mvxSavedFragmentTypes";
        private const string SavedCurrentFragmentsKey = "__mvxSavedCurrentFragments";
        private const string SavedBackStackFragmentsKey = "__mvxSavedBackStackFragments";
        private readonly Func<FragmentManager> _fragmentManager;

        private readonly Dictionary<string, FragmentInfo> _lookup = new Dictionary<string, FragmentInfo>();
        internal IList<KeyValuePair<int, string>> BackStackFragments = new List<KeyValuePair<int, string>>();
        internal Dictionary<int, string> CurrentFragments = new Dictionary<int, string>();


        public FragmentCache(Func<FragmentManager> fragmentManager)
        {
            _fragmentManager = fragmentManager;
        }

        public void RegisterFragment<TFragment, TViewModel>(string tag)
            where TFragment : IMvxFragmentView
            where TViewModel : IMvxViewModel
        {
            var fragInfo = new FragmentInfo(tag, typeof (TFragment), typeof (TViewModel));

            _lookup.Add(tag, fragInfo);
        }


        public void RestoreCachedFragments(Bundle fromSavedInstanceState)
        {
            if (fromSavedInstanceState == null)
                return;

            RestoreLookupFromSleep();

            IMvxJsonConverter serializer;
            if (!Mvx.TryResolve(out serializer))
            {
                Mvx.Trace("Could not resolve IMvxNavigationSerializer, it is going to be hard to create ViewModel cache");
                return;
            }

            RestoreCurrentFragmentsFromBundle(serializer, fromSavedInstanceState);
            RestoreBackStackFragmentsFromBundle(serializer, fromSavedInstanceState);
            RestoreViewModelsFromBundle(serializer, fromSavedInstanceState);
        }

        private static void RestoreViewModelsFromBundle(IMvxJsonConverter serializer, Bundle savedInstanceState)
        {
            IMvxSavedStateConverter savedStateConverter;
            IMvxMultipleViewModelCache viewModelCache;
            IMvxViewModelLoader viewModelLoader;

            if (!Mvx.TryResolve(out savedStateConverter))
            {
                Mvx.Trace("Could not resolve IMvxSavedStateConverter, won't be able to convert saved state");
                return;
            }

            if (!Mvx.TryResolve(out viewModelCache))
            {
                Mvx.Trace("Could not resolve IMvxMultipleViewModelCache, won't be able to convert saved state");
                return;
            }

            if (!Mvx.TryResolve(out viewModelLoader))
            {
                Mvx.Trace("Could not resolve IMvxViewModelLoader, won't be able to load ViewModel for caching");
                return;
            }

            // Harder ressurection, just in case we were killed to death.
            var json = savedInstanceState.GetString(SavedFragmentTypesKey);
            if (string.IsNullOrEmpty(json)) return;

            var savedState = serializer.DeserializeObject<Dictionary<string, Type>>(json);
            foreach (var item in savedState)
            {
                var bundle = savedInstanceState.GetBundle(item.Key);
                if (bundle.IsEmpty) continue;

                var mvxBundle = savedStateConverter.Read(bundle);
                var request = MvxViewModelRequest.GetDefaultRequest(item.Value);

                // repopulate the ViewModel with the SavedState and cache it.
                var vm = viewModelLoader.LoadViewModel(request, mvxBundle);
                viewModelCache.Cache(vm);
            }
        }

        private void RestoreCurrentFragmentsFromBundle(IMvxJsonConverter serializer, Bundle savedInstanceState)
        {
            var json = savedInstanceState.GetString(SavedCurrentFragmentsKey);
            if (string.IsNullOrEmpty(json)) // no fragments instances were saved.
                return;

            var currentFragments = serializer.DeserializeObject<Dictionary<int, string>>(json);
            CurrentFragments = currentFragments;
        }

        private void RestoreBackStackFragmentsFromBundle(IMvxJsonConverter serializer, Bundle savedInstanceState)
        {
            var jsonBackStack = savedInstanceState.GetString(SavedBackStackFragmentsKey);
            if (string.IsNullOrEmpty(jsonBackStack)) // no backstack fragments were saved.
                return;

            var backStackFragments = serializer.DeserializeObject<List<KeyValuePair<int, string>>>(jsonBackStack);
            BackStackFragments = backStackFragments;
        }

        private void RestoreLookupFromSleep()
        {
            var supportFragmentManager = _fragmentManager();

            if (supportFragmentManager == null)
                return;

            // when there are no fragments SupportFragmentmanager.Fragments is null
            var fragments = supportFragmentManager.Fragments ?? Enumerable.Empty<Fragment>();

            // See if Fragments were just sleeping, and repopulate the _lookup
            // with references to them.
            foreach (var fragment in fragments)
            {
                if (fragment != null)
                {
                    var fragmentType = fragment.GetType();
                    var lookup = _lookup.Where(x => x.Value.FragmentType == fragmentType);
                    foreach (var item in lookup.Where(item => item.Value != null))
                    {
                        // reattach fragment to lookup
                        item.Value.CachedFragment = fragment;
                    }
                }
            }
        }

        public void CacheFragments(Bundle toBundle)
        {
            if (_lookup.Any())
            {
                var typesForKeys = CreateFragmentTypesDictionary(toBundle);
                if (typesForKeys == null)
                    return;

                IMvxJsonConverter jsonSerializer;
                if (!Mvx.TryResolve(out jsonSerializer))
                {
                    return;
                }

                var json = jsonSerializer.SerializeObject(typesForKeys);
                toBundle.PutString(SavedFragmentTypesKey, json);

                json = jsonSerializer.SerializeObject(CurrentFragments);
                toBundle.PutString(SavedCurrentFragmentsKey, json);

                json = jsonSerializer.SerializeObject(BackStackFragments);
                toBundle.PutString(SavedBackStackFragmentsKey, json);
            }
        }


        private Dictionary<string, Type> CreateFragmentTypesDictionary(Bundle outState)
        {
            IMvxSavedStateConverter savedStateConverter;
            if (!Mvx.TryResolve(out savedStateConverter))
            {
                return null;
            }

            var typesForKeys = new Dictionary<string, Type>();

            foreach (var item in _lookup)
            {
                if (CurrentFragments.Any(x => x.Value == item.Key))
                {
                    var fragment = item.Value.CachedFragment as IMvxFragmentView;
                    if (fragment == null)
                        continue;

                    var mvxBundle = fragment.CreateSaveStateBundle();
                    var bundle = new Bundle();
                    savedStateConverter.Write(bundle, mvxBundle);
                    outState.PutBundle(item.Key, bundle);

                    typesForKeys.Add(item.Key, item.Value.ViewModelType);
                }
            }

            return typesForKeys;
        }

        internal bool TryGetLookupFragmentInfo(string forTag, out FragmentInfo target)
        {
            return _lookup.TryGetValue(forTag, out target);
        }

 


        public class FragmentInfo
        {
            public FragmentInfo(string tag, Type fragmentType, Type viewModelType)
            {
                Tag = tag;
                FragmentType = fragmentType;
                ViewModelType = viewModelType;
            }

            public string Tag { get; private set; }
            public Type FragmentType { get; }
            public Type ViewModelType { get; private set; }
            public Fragment CachedFragment { get; set; }
            public int ContentId { get; set; }
        }

        internal bool TryGetCurrentFragmentValue(int contentId, out string currentFragment)
        {
            return CurrentFragments.TryGetValue(contentId, out currentFragment);
        }

        internal void UpdateCurrentFragmentValue(int contentId, string newValue)
        {
            if (CurrentFragments.ContainsKey(contentId))
                CurrentFragments[contentId] = newValue;
            else
                CurrentFragments.Add(contentId, newValue);
        }
         
    }
}