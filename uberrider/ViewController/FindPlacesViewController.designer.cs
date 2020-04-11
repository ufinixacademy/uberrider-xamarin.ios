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
    [Register ("FindPlacesViewController")]
    partial class FindPlacesViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView placesTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISearchBar searchbox { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView searchLoader { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (placesTableView != null) {
                placesTableView.Dispose ();
                placesTableView = null;
            }

            if (searchbox != null) {
                searchbox.Dispose ();
                searchbox = null;
            }

            if (searchLoader != null) {
                searchLoader.Dispose ();
                searchLoader = null;
            }
        }
    }
}