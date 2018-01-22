using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TeamStor.Engine.Graphics
{
    /// <summary>
    /// Builds on top of MonoGame SpriteBatch.
    /// </summary>
    public class SpriteBatch
    {
        private Game _game;
        private Microsoft.Xna.Framework.Graphics.SpriteBatch _monoGameSpriteBatch;
        private Texture2D _emptyTexture;

        /// <summary>
        /// GraphicsDevice used by this SpriteBatch.
        /// </summary>
        public GraphicsDevice Device
        {
            get;
            private set;
        }

        /// <summary>
        /// If there are any items queued.
        /// </summary>
        public bool ItemsQueued
        {
            get;
            private set;
        }

        private SpriteSortMode _sortMode = SpriteSortMode.Deferred;
        
        /// <summary>
        /// MonoGame batch sort mode.
        /// </summary>
        public SpriteSortMode SortMode
        {
            get { return _sortMode; }
            set
            {
                if(value != _sortMode)
                {
                    if(ItemsQueued)
                        ResetSpriteBatch();
                    
                    _sortMode = value;
                }
            }
        }
        
        private BlendState _blendState = BlendState.NonPremultiplied;
        
        /// <summary>
        /// MonoGame batch blend state.
        /// </summary>
        public BlendState BlendState
        {
            get { return _blendState; }
            set
            {
                if(value != _blendState)
                {
                    if(ItemsQueued)
                        ResetSpriteBatch();
                    
                    _blendState = value;
                }
            }
        }
        
        private SamplerState _samplerState = SamplerState.AnisotropicWrap;
        
        /// <summary>
        /// MonoGame batch sampler state.
        /// </summary>
        public SamplerState SamplerState
        {
            get { return _samplerState; }
            set
            {
                if(value != _samplerState)
                {            
                    if(ItemsQueued)
                        ResetSpriteBatch();
                    
                    _samplerState = value;
                }
            }
        }
        
        private Effect _effect;
        
        /// <summary>
        /// Effect to use when drawing.
        /// Set to null to use the default effect.
        /// </summary>
        public Effect Effect
        {
            get { return _effect; }
            set
            {
                if(value != _effect)
                {
                    if(ItemsQueued)
                        ResetSpriteBatch();
                    
                    _effect = value;
                }
            }
        }
        
        private Matrix _transform = Matrix.Identity;
        
        /// <summary>
        /// Transformation to apply when drawing.
        /// </summary>
        public Matrix Transform
        {
            get { return _transform; }
            set
            {
                if(value != _transform)
                {
                    if(ItemsQueued)
                        ResetSpriteBatch();
                    
                    _transform = value;
                }
            }
        }

        private Rectangle? _scissor;
        
        /// <summary>
        /// Screen scissor to apply when drawing.
        /// </summary>
        public Rectangle? Scissor
        {
            get { return _scissor; }
            set
            {
                if(value != _scissor)
                {
                    if(ItemsQueued)
                        ResetSpriteBatch();

                    _rastState.ScissorTestEnable = value.HasValue;
                    _scissor = value;
                }
            }
        }
        
        private RenderTarget2D _renderTarget;
        
        /// <summary>
        /// Render target to draw to when drawing.
        /// </summary>
        public RenderTarget2D RenderTarget
        {
            get { return _renderTarget; }
            set
            {
                if(value != _renderTarget)
                {
                    if(ItemsQueued)
                        ResetSpriteBatch();
                    
                    _renderTarget = value;
                }
            }
        }

        private RasterizerState _rastState = new RasterizerState();

        public SpriteBatch(Game game)
        {
            _game = game;
            Device = game.GraphicsDevice;
            
            _monoGameSpriteBatch = new Microsoft.Xna.Framework.Graphics.SpriteBatch(game.GraphicsDevice);
            _emptyTexture = new Texture2D(game.GraphicsDevice, 1, 1);
            _emptyTexture.SetData(new Color[] { Color.White });

            _rastState.CullMode = CullMode.CullClockwiseFace;
            _rastState.ScissorTestEnable = false;
        }

        private void ResetSpriteBatch()
        {
            if(ItemsQueued)
                End();
            _monoGameSpriteBatch.Begin(SortMode, BlendState, SamplerState, DepthStencilState.Default, _rastState, Effect, Transform);
        }
        
        /// <summary>
        /// Draws a texture at the specified position.
        /// </summary>
        /// <param name="pos">The position to draw the texture at.</param>
        /// <param name="texture">The texture to draw</param>
        /// <param name="scale">Amount to scale the texture by.</param>
        /// <param name="crop">The rectangle inside the texture to crop.</param>
        /// <param name="color">The color to tint the texture with.</param>
        /// <param name="rotation">The rotation to rotate the texture by.</param>
        /// <param name="rotationOrigin">The origin of the rotation inside the texture (0-1)</param>
        /// <param name="effect">The sprite effect to apply.</param>
        public void Texture(Vector2 pos, Texture2D texture, Color color = default(Color), Vector2? scale = null, Rectangle? crop = null, float rotation = 0f, Vector2? rotationOrigin = null, SpriteEffects effect = SpriteEffects.None)
        {
            if(!ItemsQueued)
                ResetSpriteBatch();
            
            _monoGameSpriteBatch.Draw(texture, pos, crop, color, rotation, rotationOrigin.HasValue ? rotationOrigin.Value : Vector2.Zero, scale.HasValue ? scale.Value : Vector2.One, effect, 0f);
        }
        
        /// <summary>
        /// Draws a texture inside the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw the texture in.</param>
        /// <param name="texture">The texture to draw</param>
        /// <param name="crop">The rectangle inside the texture to crop.</param>
        /// <param name="color">The color to tint the texture with.</param>
        /// <param name="rotation">The rotation to rotate the texture by.</param>
        /// <param name="rotationOrigin">The origin of the rotation inside the texture (0-1)</param>
        /// <param name="effect">The sprite effect to apply.</param>
        public void Texture(Rectangle rectangle, Texture2D texture, Color color = default(Color), Rectangle? crop = null, float rotation = 0f, Vector2? rotationOrigin = null, SpriteEffects effect = SpriteEffects.None)
        {
            if(!ItemsQueued)
                ResetSpriteBatch();
            
            _monoGameSpriteBatch.Draw(texture, rectangle, crop, color, rotation, rotationOrigin.HasValue ? rotationOrigin.Value : Vector2.Zero, effect, 0f);
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw.</param>
        /// <param name="color">The color to draw the rectangle with.</param>
        /// <param name="rotation">The rotation to rotate the rectangle by.</param>
        /// <param name="rotationOrigin">The origin of the rotation inside the rectangle (0-1)</param>
        public void Rectangle(Rectangle rectangle, Color color = default(Color), float rotation = 0f, Vector2? rotationOrigin = null)
        {
            Texture(rectangle, _emptyTexture, color, null, rotation, rotationOrigin);
        }
        
        /// <summary>
        /// Draws a point.
        /// </summary>
        /// <param name="pos">The position to draw the point at.</param>
        /// <param name="color">The color to draw the point with.</param>
        /// <param name="size">The size of the point.</param>
        public void Point(Vector2 pos, Color color = default(Color), int size = 1)
        {
            Rectangle(new Rectangle((int)pos.X - (int)Math.Floor(size / 2f), (int)pos.Y - (int)Math.Floor(size / 2f), size, size), color);
        }
        
        /// <summary>
        /// Draws a line.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <param name="color">The color to draw the line with.</param>
        /// <param name="thickness">The thickness of the line.</param>
        public void Line(Vector2 start, Vector2 end, Color color = default(Color), int thickness = 1)
        {
            // https://gamedev.stackexchange.com/questions/44015/how-can-i-draw-a-simple-2d-line-in-xna-without-using-3d-primitives-and-shders
            Vector2 edge = end - start;
            Rectangle(new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness), color, (float)Math.Atan2(edge.Y, edge.X), new Vector2(0, 0.5f));
        }
        
        /// <summary>
        /// Draws an outline around a rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw an outline around.</param>
        /// <param name="color">The color to draw the outline with.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        public void Outline(Rectangle rectangle, Color color = default(Color), int thickness = 1)
        {
            // -----
            // #   #
            // #   #
            // #####
            Line(new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.X + rectangle.Width - thickness, rectangle.Y), color, thickness);
            
            // -----
            // #   #
            // #   #
            // -----
            Line(new Vector2(rectangle.X, rectangle.Y + rectangle.Height), new Vector2(rectangle.X + rectangle.Width - thickness, rectangle.Y + rectangle.Height), color, thickness);
            
            // -----
            // -   #
            // -   #
            // -----
            Line(new Vector2(rectangle.X, rectangle.Y + thickness), new Vector2(rectangle.X, rectangle.Y + rectangle.Height - thickness), color, thickness);
            
            // -----
            // -   -
            // -   -
            // -----
            Line(new Vector2(rectangle.X + rectangle.Width, rectangle.Y + thickness), new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height - thickness), color, thickness);    
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="center">The center position of the circle.</param>
        /// <param name="size">The size of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        /// <param name="thickness">The thickness of the circle.</param>
        public void Circle(Vector2 center, float size, Color color = default(Color), int thickness = 1)
        {
            for(float i = -1f; i <= 0.9f; i += 0.1f)
            {
                Vector2 from = new Vector2(center.X + (float)Math.Sin(i) * size, center.Y + (float)Math.Cos(i) * size);
                Vector2 to = new Vector2(center.X + (float)Math.Sin(i + 0.1f) * size, center.Y + (float)Math.Cos(i + 0.1f) * size);
                
                Line(from, to, color, thickness);
            }
        }
        
        /// <summary>
        /// Draws text with the specified font.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="size">The font size.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="pos">The position to draw the text at.</param>
        /// <param name="color">The color to draw the text with.</param>
        /// <param name="lineMult">The line height multiplier.</param>
        /// <param name="spacing">The spacing multiplier.</param>
        public void Text(Font font, uint size, string text, Vector2 pos, Color color = default(Color), float lineMult = 1f, float spacing = 1f)
        {
            font.Draw(this, size, text, pos, color, lineMult, spacing);
        }
        
        /// <summary>
        /// Mirrored by Game.DefaultFonts
        /// </summary>
        public enum FontStyle
        {
            Normal,
            Bold,
            Italic,
            ItalicBold,
            Mono,
            MonoBold
        }
        
        /// <summary>
        /// Draws text with the specified font style.
        /// </summary>
        /// <param name="font">The font to use.</param>
        /// <param name="size">The font size.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="pos">The position to draw the text at.</param>
        /// <param name="color">The color to draw the text with.</param>
        /// <param name="lineMult">The line height multiplier.</param>
        /// <param name="spacing">The spacing multiplier.</param>
        public void Text(FontStyle fontStyle, uint size, string text, Vector2 pos, Color color = default(Color), float lineMult = 1f, float spacing = 1f)
        {
            Font font = _game.DefaultFonts.Normal;

            switch(fontStyle)
            {
                case FontStyle.Bold:
                    font = _game.DefaultFonts.Bold;
                    break;
                    
                case FontStyle.Italic:
                    font = _game.DefaultFonts.ItalicNormal;
                    break;
                    
                case FontStyle.ItalicBold:
                    font = _game.DefaultFonts.ItalicBold;
                    break;
                    
                case FontStyle.Mono:
                    font = _game.DefaultFonts.Mono;
                    break;
                    
                case FontStyle.MonoBold:
                    font = _game.DefaultFonts.MonoBold;
                    break;
            }
            
            Text(font, size, text, pos, color, lineMult, spacing);
        }

        /// <summary>
        /// Draws all queued items.
        /// </summary>
        public void End()
        {
            if(ItemsQueued)
            {
                if(Scissor.HasValue)
                    _game.GraphicsDevice.ScissorRectangle = Scissor.Value;
                if(RenderTarget != null)
                    _game.GraphicsDevice.SetRenderTarget(RenderTarget);
                _monoGameSpriteBatch.End();
                if(RenderTarget != null)
                    _game.GraphicsDevice.SetRenderTarget(null);
                if(Scissor.HasValue)
                    _game.GraphicsDevice.ScissorRectangle = _game.GraphicsDevice.Viewport.Bounds;
            }

            ItemsQueued = false;
        }

        /// <summary>
        /// Resets the queue to it's default state.
        /// </summary>
        public void Reset()
        {
            _sortMode = SpriteSortMode.Deferred;
            _blendState = BlendState.NonPremultiplied;
            _samplerState = SamplerState.AnisotropicWrap;
            _effect = null;
            _transform = Matrix.Identity;
            _scissor = null;
            _renderTarget = null;
            _rastState.ScissorTestEnable = false;
            
            if(ItemsQueued)
                ResetSpriteBatch();
        }
    }
}