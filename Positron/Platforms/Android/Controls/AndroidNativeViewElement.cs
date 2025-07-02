using AView = Android.Views.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

namespace NeuroSpeech.Positron.Platforms.Android.Controls;

public class AndroidNativeViewElement: View
{
    public readonly AView nativeView;

    public AndroidNativeViewElement(AView nativeView)
    {
        this.nativeView = nativeView;
    }

}

public class AndroidNativeViewHandler : ViewHandler<AndroidNativeViewElement, AView>
{
    public AndroidNativeViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(mapper, commandMapper)
    {
    }

    protected override AView CreatePlatformView()
    {
        return VirtualView.nativeView;
    }
}
