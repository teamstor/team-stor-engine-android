
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;

namespace TeamStor.Engine.TestGame
{
    // https://github.com/MonoGame/MonoGame.Samples/blob/develop/Platformer2D/Platforms/Android/Activity1.cs
    [Activity(
        Label = "TeamStor.Engine.TestGame",
        MainLauncher = true,
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.Portrait,
        ResizeableActivity = true,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class GameActivity : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Game teamStorGame = new Game(null);
            SetContentView((View)teamStorGame.Services.GetService(typeof(View)));

            teamStorGame.Stats |= Game.DebugStats.FPS;
            teamStorGame.Stats |= Game.DebugStats.General;
            teamStorGame.Stats |= Game.DebugStats.SpriteBatch;
            teamStorGame.Stats |= Game.DebugStats.TouchPoints;

            teamStorGame.Run();
        }
    }
}