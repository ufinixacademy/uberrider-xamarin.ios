// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace uberrider
{
    [Register ("PlaceCell")]
    partial class PlaceCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel placeAddressText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel placeTitleText { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (placeAddressText != null) {
                placeAddressText.Dispose ();
                placeAddressText = null;
            }

            if (placeTitleText != null) {
                placeTitleText.Dispose ();
                placeTitleText = null;
            }
        }
    }
}