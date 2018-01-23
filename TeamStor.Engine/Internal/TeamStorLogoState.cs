using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TeamStor.Engine.Graphics;
using SpriteBatch = TeamStor.Engine.Graphics.SpriteBatch;

namespace TeamStor.Engine.Internal
{
    public class TeamStorLogoState : GameState
    {
        private static readonly Vector2 BIG_BALL_POSITION = new Vector2(60, 85);
        private static readonly Vector2 SMALL_BALL_POSITION = new Vector2(695, 680);

        private GameState _initialState;

        private double _startTime;
        private float _smallBallScale = 0.0f;
        private float _bigBallScale = 0.0f;

        public TeamStorLogoState(GameState initialState)
        {
            _initialState = initialState;
        }

        public override void OnEnter(GameState previousState)
        {
            _startTime = Game.Time;
       }

        public override void OnLeave(GameState nextState)
        {
        }

        public override void Update(double deltaTime, double totalTime, long count)
        {
            _smallBallScale = MathHelper.LerpPrecise(_smallBallScale, 1.0f, (float)(deltaTime * 0.8f));
            _bigBallScale = MathHelper.LerpPrecise(_bigBallScale, 1.0f, (float)(deltaTime * 0.8f));

            if(totalTime - _startTime > 5.5f || Input.KeyPressed(Keys.Enter))
                Game.CurrentState = _initialState;
        }

        public override void FixedUpdate(long count)
        {
        }

        public override void Draw(SpriteBatch batch, Vector2 screenSize)
        {
            batch.Rectangle(new Rectangle(0, 0, (int)screenSize.X, (int)screenSize.Y), Color.Black);
            
            batch.Transform = Matrix.CreateTranslation((screenSize.X / 2) / (screenSize.Y / 1080f) - 1080 / 2f, 0, 0) * Matrix.CreateScale(screenSize.Y / 1080f);

            Texture2D smallBall = Assets.Get<Texture2D>("engine/intro/small_ball.png");
            Texture2D bigBall = Assets.Get<Texture2D>("engine/intro/big_ball.png");
            
            batch.Texture(BIG_BALL_POSITION + new Vector2(bigBall.Width / 2 * (1.0f - _bigBallScale), bigBall.Height / 2 * (1.0f - _bigBallScale)), bigBall, Color.White, new Vector2(_bigBallScale, _bigBallScale));
            batch.Texture(SMALL_BALL_POSITION + new Vector2(smallBall.Width / 2 * (1.0f - _smallBallScale), smallBall.Height / 2 * (1.0f - _smallBallScale)), smallBall, Color.White, new Vector2(_smallBallScale, _smallBallScale));
        }
    }
}
