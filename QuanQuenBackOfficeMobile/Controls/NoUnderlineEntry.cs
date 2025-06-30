
using Microsoft.Maui.Controls;
#if ANDROID
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
#endif
#if IOS
using UIKit;
#endif

namespace QuanQuenBackOfficeMobile.Controls
{
    public class NoUnderlineEntry : Entry
    {
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
#if ANDROID
            if (Handler is IEntryHandler entryHandler)
            {
                entryHandler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            }
#endif
#if IOS
    if (Handler?.PlatformView is UIKit.UITextField textField)
    {
        textField.BorderStyle = UIKit.UITextBorderStyle.None; // Ẩn border
        textField.Layer.BorderWidth = 0; // Ẩn border layer
    }
#endif
        }
    }
}