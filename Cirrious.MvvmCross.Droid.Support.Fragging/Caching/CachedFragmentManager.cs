using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Cirrious.CrossCore.Exceptions;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace Cirrious.MvvmCross.Droid.Support.Fragging.Caching
{
    internal class CachedFragmentManager
    {
        private readonly FragmentCache _fragmentCacheToManage;
        private readonly FragmentActivity _activity;

        public CachedFragmentManager(FragmentActivity activity, FragmentCache fragmentCacheToManage)
        {
            _activity = activity;
            _fragmentCacheToManage = fragmentCacheToManage;
        }

        public void ShowFragment(ShowFragmentRequest showFragmentRequest)
        {
            FragmentCache.FragmentInfo fragInfo;
            _fragmentCacheToManage.TryGetLookupFragmentInfo(showFragmentRequest.Tag, out fragInfo);

            if (fragInfo == null)
                throw new MvxException("Could not find tag: {0} in cache, you need to register it first.", showFragmentRequest.Tag);

            string currentFragment;
            _fragmentCacheToManage.CurrentFragments.TryGetValue(showFragmentRequest.ContentId, out currentFragment);

            var shouldReplaceCurrentFragment = showFragmentRequest.ShouldForceFragmentReplace ||
                                               ShouldReplaceCurrentFragment(showFragmentRequest.ContentId,
                                                   showFragmentRequest.Tag);
            if (!shouldReplaceCurrentFragment)
                return;

            var supportFragmentManager = _activity.SupportFragmentManager;
            var fragmentTransaction = supportFragmentManager.BeginTransaction();

            OnBeforeFragmentChanging(new FragmentChangingEvenArgs(showFragmentRequest.Tag, fragmentTransaction));
            // if there is a Fragment showing on the contentId we want to present at
            // remove it first.   
            RemoveFragmentIfShowing(fragmentTransaction, showFragmentRequest.ContentId);

            fragInfo.ContentId = showFragmentRequest.ContentId;
            // if we haven't already created a Fragment, do it now
            if (fragInfo.CachedFragment == null || shouldReplaceCurrentFragment)
            {
                fragInfo.CachedFragment = Fragment.Instantiate(_activity, FragmentJavaName(fragInfo.FragmentType), showFragmentRequest.Bundle);
                fragmentTransaction.Add(fragInfo.ContentId, fragInfo.CachedFragment, fragInfo.Tag);
            }
            else
                fragmentTransaction.Attach(fragInfo.CachedFragment);

            _fragmentCacheToManage.UpdateCurrentFragmentValue(showFragmentRequest.ContentId, fragInfo.Tag);

            if (showFragmentRequest.ShouldAddToBackStack)
                fragmentTransaction.AddToBackStack(fragInfo.Tag);

            OnFragmentChanging(new FragmentChangingEvenArgs(showFragmentRequest.Tag, fragmentTransaction));
            fragmentTransaction.Commit();

            supportFragmentManager.ExecutePendingTransactions();
        }

        private bool ShouldReplaceCurrentFragment(int contentId, string tag)
        {
            string currentFragment;
            _fragmentCacheToManage.TryGetCurrentFragmentValue(contentId, out currentFragment);

            return ShouldReplaceFragment(contentId, currentFragment, tag);
        }

        protected virtual bool ShouldReplaceFragment(int contentId, string currentTag, string replacementTag)
        {
            return currentTag != replacementTag;
        }

        private void RemoveFragmentIfShowing(FragmentTransaction ft, int contentId)
        {
            var frag = _activity.SupportFragmentManager.FindFragmentById(contentId);
            if (frag == null)
                return;

            ft.Detach(frag);

            var fragmentToRemove = _fragmentCacheToManage.CurrentFragments?.FirstOrDefault(x => x.Key == contentId);

            if (fragmentToRemove == null)
                return;

            _fragmentCacheToManage.BackStackFragments?.Add(fragmentToRemove.Value);
            _fragmentCacheToManage.CurrentFragments?.Remove(contentId);
        }


        public void CloseFragment(CloseFragmentRequest closeFragmentRequest)
        {
            var supportFragmentManager = _activity.SupportFragmentManager;
            var frag = supportFragmentManager.FindFragmentById(closeFragmentRequest.ContentId);
            if (frag == null) return;

            supportFragmentManager.PopBackStackImmediate(closeFragmentRequest.Tag, 1);

            _fragmentCacheToManage.CurrentFragments.Remove(closeFragmentRequest.ContentId);

            if (_fragmentCacheToManage.BackStackFragments.Count > 0 && _fragmentCacheToManage.BackStackFragments.Any(x => x.Key == closeFragmentRequest.ContentId))
            {
                var currentFragment = _fragmentCacheToManage.BackStackFragments.Last(x => x.Key == closeFragmentRequest.ContentId);

                _fragmentCacheToManage.CurrentFragments.Add(currentFragment.Key, currentFragment.Value);
                _fragmentCacheToManage.BackStackFragments.Remove(currentFragment);
            }
        }

        public void HandleOnBackPressed()
        {
            var supportFragmentManager = _activity.SupportFragmentManager;
            if (supportFragmentManager.BackStackEntryCount > 1)
            {
                var backStackFrag = supportFragmentManager.GetBackStackEntryAt(supportFragmentManager.BackStackEntryCount - 1);

                var currentFragmentToRemove = _fragmentCacheToManage.CurrentFragments.Last(x => x.Value == backStackFrag.Name).Key;
                _fragmentCacheToManage.CurrentFragments.Remove(currentFragmentToRemove);

                var newFragment = supportFragmentManager.GetBackStackEntryAt(supportFragmentManager.BackStackEntryCount - 2);
                var currentFragment = _fragmentCacheToManage.BackStackFragments.Last(x => x.Value == newFragment.Name);

                _fragmentCacheToManage.CurrentFragments.Add(currentFragment.Key, currentFragment.Value);
                _fragmentCacheToManage.BackStackFragments.Remove(currentFragment);

                supportFragmentManager.PopBackStackImmediate();
                return;
            }

            supportFragmentManager.PopBackStack();
            _activity.OnBackPressed();
        }

        protected virtual string FragmentJavaName(Type fragmentType)
        {
            var namespaceText = fragmentType.Namespace ?? "";
            if (namespaceText.Length > 0)
                namespaceText = namespaceText.ToLowerInvariant() + ".";
            return namespaceText + fragmentType.Name;
        }

        public event Action<FragmentChangingEvenArgs> BeforeFragmentChanging;
        public event Action<FragmentChangingEvenArgs> FragmentChanging;

        protected virtual void OnBeforeFragmentChanging(FragmentChangingEvenArgs obj)
        {
            BeforeFragmentChanging?.Invoke(obj);
        }

        protected virtual void OnFragmentChanging(FragmentChangingEvenArgs obj)
        {
            FragmentChanging?.Invoke(obj);
        }
    }
}