using CoreGraphics;
using Firebase.Auth;
using Firebase.Database;
using Foundation;
using System;
using UIKit;

namespace uberrider
{
    public partial class RegisterViewController : UIViewController
    {
        public RegisterViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            emailText.ShouldReturn = delegate {

                emailText.ResignFirstResponder();
                return true;
            };

            phoneText.ShouldReturn = delegate {

                phoneText.ResignFirstResponder();
                return true;
            };

            fulNameText.ShouldReturn = delegate {

                fulNameText.ResignFirstResponder();
                return true;
            };

            passwordText.ShouldReturn = delegate {

                passwordText.ResignFirstResponder();
                return true;
            };

            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, keyWillChange);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, keyWillChange);

            registerButton.TouchUpInside += RegisterButton_TouchUpInside;

            clickToLogin.UserInteractionEnabled = true;
            clickToLogin.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                LoginViewController loginViewController = this.Storyboard.InstantiateViewController("LoginViewController") as LoginViewController;
                loginViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                PresentViewController(loginViewController, true, null);
            }));

        }

        private void RegisterButton_TouchUpInside(object sender, EventArgs e)
        {
            string fullname, phone, email, password;

            fullname = fulNameText.Text;
            phone = phoneText.Text;
            email = emailText.Text;
            password = passwordText.Text;

            if(fullname.Length < 5)
            {
                var alert = UIAlertController.Create("Alert", "Please enter a valid name", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                return;               
            }
            else if(phone.Length < 8)
            {
                var alert = UIAlertController.Create("Alert", "Please enter a valid Phone Number", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                return;
            }
            else if (!email.Contains("@"))
            {
                var alert = UIAlertController.Create("Alert", "Please enter a valid email address", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                return;
            }
            else if (password.Length < 8)
            {
                var alert = UIAlertController.Create("Alert", "Please enter a password upto 8 characters", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                return;
            }


            ShowProgressBar("Regsitering you...");

            Auth.DefaultInstance.CreateUser(email, password, (AuthDataResult authresult, NSError error) => {


                if(error == null)
                {
                    var user = authresult.User;

                    if(user != null)
                    {
                        var userDictionary = new NSDictionary
                        (
                            "fullname", fullname,
                            "email", email,
                            "phone", phone
                        );

                        //Save user details to firebase database
                        DatabaseReference userRef = Database.DefaultInstance.GetRootReference().GetChild("users/" + authresult.User.Uid);
                        userRef.SetValue<NSDictionary>(userDictionary);

                        // Save User info Locally
                        var userDefaults = NSUserDefaults.StandardUserDefaults;
                        userDefaults.SetString(phone, "phone");
                        userDefaults.SetString(fullname, "fullname");
                        userDefaults.SetString(email, "email");
                        userDefaults.SetString(authresult.User.Uid, "user_id");

                        HideProgressBar();

                        MainViewController mainViewController = this.Storyboard.InstantiateViewController("MainViewController") as MainViewController;
                        mainViewController.ModalPresentationStyle = UIModalPresentationStyle.FullScreen;
                        PresentViewController(mainViewController, true, null);


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

        void keyWillChange(NSNotification notification)
        {
            if (notification.Name == UIKeyboard.WillShowNotification)
            {
                var keyboard = UIKeyboard.FrameBeginFromNotification(notification);

                CGRect frame = View.Frame;
                frame.Y = -keyboard.Height;
                View.Frame = frame;

            }

            if (notification.Name == UIKeyboard.WillHideNotification)
            {
                CGRect frame = View.Frame;
                frame.Y = 0;
                View.Frame = frame;
            }
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