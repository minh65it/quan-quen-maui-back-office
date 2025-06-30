using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
#if IOS
using UIKit;
using QuanQuenBackOfficeMobile.Controls;

namespace QuanQuenBackOfficeMobile.Controls
{
   public class CustomDropdownPickerHandler : PickerHandler
    {
        protected override void ConnectHandler(Microsoft.Maui.Platform.MauiPicker platformView)
        {
            base.ConnectHandler(platformView);

            // Lấy native UIPickerView từ platformView.InputView
            if (platformView.InputView is UIPickerView uiPicker)
            {
                uiPicker.BackgroundColor = UIColor.FromRGB(255, 255, 255); // Màu nền trắng sáng
            }
        }
    }
}
#endif