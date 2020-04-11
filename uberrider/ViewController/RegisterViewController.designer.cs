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
    [Register ("RegisterViewController")]
    partial class RegisterViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel clickToLogin { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField emailText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField fulNameText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView overlay { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField passwordText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField phoneText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView progressBar { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel progressStatusText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton registerButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (clickToLogin != null) {
                clickToLogin.Dispose ();
                clickToLogin = null;
            }

            if (emailText != null) {
                emailText.Dispose ();
                emailText = null;
            }

            if (fulNameText != null) {
                fulNameText.Dispose ();
                fulNameText = null;
            }

            if (overlay != null) {
                overlay.Dispose ();
                overlay = null;
            }

            if (passwordText != null) {
                passwordText.Dispose ();
                passwordText = null;
            }

            if (phoneText != null) {
                phoneText.Dispose ();
                phoneText = null;
            }

            if (progressBar != null) {
                progressBar.Dispose ();
                progressBar = null;
            }

            if (progressStatusText != null) {
                progressStatusText.Dispose ();
                progressStatusText = null;
            }

            if (registerButton != null) {
                registerButton.Dispose ();
                registerButton = null;
            }
        }
    }
}