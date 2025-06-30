
using Microsoft.Maui.Controls;
#if ANDROID && IOS
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
#endif

namespace QuanQuenBackOfficeMobile.Controls
{
    public class NoUnderlineEntry : Entry
    {
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
#if ANDROID && IOS
            if (Handler is IEntryHandler entryHandler)
            {
                entryHandler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            }
#endif
        }
    }
}