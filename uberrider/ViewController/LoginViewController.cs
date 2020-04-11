using CoreGraphics;
using Firebase.Auth;
using Firebase.Database;
using Foundation;
using System;
using UIKit;

namespace uberrider
{
    public partial class LoginViewController : UIViewController
    {
        public LoginViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            emailText.ShouldReturn = delegate {

                emailText.ResignFirstResponder();
                return true;
            };

            passwordText.ShouldReturn = delegate {

                passwordText.ResignFirstResponder();
                return true;

            };

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, keyWillChange);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, keyWillChange);

            loginButton.TouchUpInside += LoginButton_TouchUpInside;

            clickToTRegister.UserInteractionEnabled = true;
            clickToTRegister.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                RegisterViewController registerViewController = this.Storyboard.InstantiateViewController("RegisterViewController") as RegisterViewController;
                registerViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(registerViewController, true, null);
            }));
           

        }

        private void LoginButton_TouchUpInside(object sender, EventArgs e)
        {
            string email, password;
            email = emailText.Text;
            password = passwordText.Text;

            if (!email.Contains("@"))
            {
                var alert = UIAlertController.Create("Alert", "Please provide a valid email", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            else if (password.Length < 8)
            {
                var alert = UIAlertController.Create("Alert", "Please provide a valid password", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }

            ShowProgressBar("Logging you in..");

            Auth.DefaultInstance.SignInWithPassword(email, password, (AuthDataResult authDataResult, NSError error) =>
            {
                if(error == null)
                {
                    if (authDataResult.User.Uid != null)
                    {
                        FetchUserInfo(authDataResult.User.Uid);
                    }
                }
                else
                {
                    HideProgressBar();
                    var alert = UIAlertController.Create("Error", error.LocalizedDescription, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }

            });
            
        }


        void FetchUserInfo(string userid)
        {
            DatabaseReference reference = Database.DefaultInstance.GetRootReference().GetChild("users/" + userid);
            reference.ObserveSingleEvent(DataEventType.Value, (DataSnapshot snapshot) =>
            {

                if(snapshot.GetValue<NSObject>() != NSNull.Null)
                {
                    string email, fullname, phone;

                    if(snapshot.GetChildSnapshot("email").GetValue<NSObject>() != NSNull.Null)
                    {
                        email = snapshot.GetChildSnapshot("email").GetValue<NSObject>().ToString();
                        fullname = snapshot.GetChildSnapshot("fullname").GetValue<NSObject>().ToString();
                        phone = snapshot.GetChildSnapshot("phone").GetValue<NSObject>().ToString();


                        // Save User info Locally
                        var userDefaults = NSUserDefaults.StandardUserDefaults;
                        userDefaults.SetString(phone, "phone");
                        userDefaults.SetString(fullname, "fullname");
                        userDefaults.SetString(email, "email");
                        userDefaults.SetString(userid, "user_id");

                        HideProgressBar();

                        MainViewController mainViewController = this.Storyboard.InstantiateViewController("MainViewController") as MainViewController;
                        mainViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                        PresentViewController(mainViewController, true, null);

                    }
                }

            });
        }


        void keyWillChange(NSNotification notification)
        {
            if(notification.Name == UIKeyboard.WillShowNotification)
            {
                var keyboard = UIKeyboard.FrameBeginFromNotification(notification);

                CGRect frame = View.Frame;
                frame.Y = -keyboard.Height;
                View.Frame = frame;
               
            }

            if(notification.Name == UIKeyboard.WillHideNotification)
            {
                CGRect frame = View.Frame;
                frame.Y = 0;
                View.Frame = frame;
            }
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            this.View.EndEditing(true);
        }

        void ShowProgressBar(string status)
        {
            progressStatusText.Text = status;
            overlay.Hidden = false;
            progressBar.Hidden = false;
        }

        void HideProgressBar()
        {
            overlay.Hidden = true;
            progressBar.Hidden = true;
        }

    }
}