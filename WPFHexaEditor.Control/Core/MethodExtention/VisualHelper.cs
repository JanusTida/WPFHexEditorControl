using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WPFHexaEditor.Control.Core.MethodExtention {
    public static class VisualHelper {
        public static M GetVisualParent<M>(this DependencyObject source) where M : DependencyObject {
            while (source != null && !(source is M)) {
                if (source is Visual || source is Visual3D)
                    source = VisualTreeHelper.GetParent(source);
                else
                    source = LogicalTreeHelper.GetParent(source);
            }
            return source as M;
        }


    }
}
