
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace NureTimetable.Droid.BugFixes
{
    public class AndroidBug5497WorkaroundForXamarinAndroid
    {
        private readonly View mChildOfContent;
        private int usableHeightPrevious;
        private FrameLayout.LayoutParams frameLayoutParams;

        public static void assistActivity(Activity activity, IWindowManager windowManager)
        {
            new AndroidBug5497WorkaroundForXamarinAndroid(activity, windowManager);
        }

        private AndroidBug5497WorkaroundForXamarinAndroid(Activity activity, IWindowManager windowManager)
        {

            var softButtonsHeight = getSoftbuttonsbarHeight(windowManager);

            var content = (FrameLayout)activity.FindViewById(Android.Resource.Id.Content);
            mChildOfContent = content.GetChildAt(0);
            var vto = mChildOfContent.ViewTreeObserver;
            vto.GlobalLayout += (sender, e) => possiblyResizeChildOfContent(softButtonsHeight);
            frameLayoutParams = (FrameLayout.LayoutParams)mChildOfContent.LayoutParameters;
        }

        private void possiblyResizeChildOfContent(int softButtonsHeight)
        {
            var usableHeightNow = computeUsableHeight();
            if (usableHeightNow != usableHeightPrevious)
            {
                var usableHeightSansKeyboard = mChildOfContent.RootView.Height - softButtonsHeight;
                var heightDifference = usableHeightSansKeyboard - usableHeightNow;
                if (heightDifference > (usableHeightSansKeyboard / 4))
                {
                    // keyboard probably just became visible
                    frameLayoutParams.Height = usableHeightSansKeyboard - heightDifference + (softButtonsHeight / 2);
                }
                else
                {
                    // keyboard probably just became hidden
                    frameLayoutParams.Height = usableHeightSansKeyboard;
                }
                mChildOfContent.RequestLayout();
                usableHeightPrevious = usableHeightNow;
            }
        }

        private int computeUsableHeight()
        {
            var r = new Rect();
            mChildOfContent.GetWindowVisibleDisplayFrame(r);
            return (r.Bottom - r.Top);
        }

        private int getSoftbuttonsbarHeight(IWindowManager windowManager)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var metrics = new DisplayMetrics();
                windowManager.DefaultDisplay.GetMetrics(metrics);
                int usableHeight = metrics.HeightPixels;
                windowManager.DefaultDisplay.GetRealMetrics(metrics);
                int realHeight = metrics.HeightPixels;
                if (realHeight > usableHeight)
                    return realHeight - usableHeight;
                else
                    return 0;
            }
            return 0;
        }
    }
}