using System;
using Firebase.Auth;
using Foundation;
using uberrider;
using UIKit;

namespace NewSingleViewTemplate
{
    [Register("SceneDelegate")]
    public class SceneDelegate : UIResponder, IUIWindowSceneDelegate
    {

        [Export("Window")]
        public UIWindow Window { get; set; }

        [Export("scene:willConnectToSession:options:")]
        public void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
        {
            // Use this method to optionally configure and attach the UIWindow `Window` to the provided UIWindowScene `scene`.
            // If using a storyboard, the `Window` property will automatically be initialized and attached to the scene.
            // This delegate does not imply the connecting scene or session are new (see UIApplicationDelegate `GetConfiguration` instead).


            //  UIApplication.shared.connectedScenes.first
            UIWindowScene WindowScene = new UIWindowScene(session, connectionOptions);
            Window = new UIWindow(WindowScene);
            var storyboard = UIStoryboard.FromName("Main", null);


            if (Auth.DefaultInstance.CurrentUser != null)
            {
                var mainViewController = storyboard.InstantiateViewController("MainViewController") as MainViewController;
                Window.RootViewController = mainViewController;
                Window?.MakeKeyAndVisible();
                this.SetWindow(Window);

            }
            else
            {
                var loginViewController = storyboard.InstantiateViewController("LoginViewController") as LoginViewController;
                Window.RootViewController = loginViewController;
                this.SetWindow(Window);
                Window?.MakeKeyAndVisible();

            }


        }

        [Export("sceneDidDisconnect:")]
        public void DidDisconnect(UIScene scene)
        {
            // Called as the scene is being released by the system.
            // This occurs shortly after the scene enters the background, or when its session is discarded.
            // Release any resources associated with this scene that can be re-created the next time the scene connects.
            // The scene may re-connect later, as its session was not neccessarily discarded (see UIApplicationDelegate `DidDiscardSceneSessions` instead).
        }

        [Export("sceneDidBecomeActive:")]
        public void DidBecomeActive(UIScene scene)
        {
            // Called when the scene has moved from an inactive state to an active state.
            // Use this method to restart any tasks that were paused (or not yet started) when the scene was inactive.
        }

        [Export("sceneWillResignActive:")]
        public void WillResignActive(UIScene scene)
        {
            // Called when the scene will move from an active state to an inactive state.
            // This may occur due to temporary interruptions (ex. an incoming phone call).
        }

        [Export("sceneWillEnterForeground:")]
        public void WillEnterForeground(UIScene scene)
        {
            // Called as the scene transitions from the background to the foreground.
            // Use this method to undo the changes made on entering the background.
        }

        [Export("sceneDidEnterBackground:")]
        public void DidEnterBackground(UIScene scene)
        {
            // Called as the scene transitions from the foreground to the background.
            // Use this method to save data, release shared resources, and store enough scene-specific state information
            // to restore the scene back to its current state.
        }
    }
}
