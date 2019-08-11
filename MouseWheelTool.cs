using System;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace CommonLibSL
{
    public class MouseWheelTool
    {
        public readonly bool Enable = false;
        public event Action<int> MouseWheel;
        private static MouseWheelTool _instance;

        public static MouseWheelTool Instance
        {
            get
            {
                if(_instance==null)
                    _instance=new MouseWheelTool();
                return _instance;
            }
        }
        private MouseWheelTool()
        {
            if (HtmlPage.IsEnabled)
            {
                if (System.ComponentModel.DesignerProperties.IsInDesignTool) return;
                if (HtmlPage.BrowserInformation.ProductName == "Chrome")
                {
                    Enable = true;
                    HtmlPage.RegisterScriptableObject("mwt", this);
                    HtmlPage.Window.Eval(@"var passiveSupported = false;
                    try
                    {
                        var options = Object.defineProperty({ }, 'passive', {
                        get: function() {
                                passiveSupported = true;
                            }
                        });
                        window.addEventListener('test', null, options);
                    }
                    catch (err) { }
                    function fn(event) {
                        slHost.Content.mwt.OnMouseWheel(event.wheelDelta);
                    }
                    document.addEventListener('mousewheel', fn, passiveSupported ? { passive: true } : false)");
                }
                else
                    Enable = false;

            }
        }

        [ScriptableMember]
        public void OnMouseWheel(int wheelDelta)
        {
            MouseWheel?.Invoke(wheelDelta);
        }
    }
}
