using Foundation;
using UIKit;

namespace ShipGame.iOS
{
    [Register("AppDelegate")]
    internal class Program : UIApplicationDelegate
    {
        private static ShipGameGame game;

        internal static void RunGame()
        {
            game = new ShipGameGame();
            ShipGameGame.SetInstance(game);
            game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, typeof(Program));
        }

        public override void FinishedLaunching(UIApplication app)
        {
            RunGame();
        }
    }
}