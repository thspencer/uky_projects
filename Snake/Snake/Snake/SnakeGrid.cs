using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

// GRID CODE REFERENCED WITH PERMISSION FROM UKY CS485G CLASS EXAMPLE BY PAUL MIHAIL

namespace Snake
{
    // this is the grid overlay component for the game, not visible to the user
    public class SnakeGrid : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public static int gridScale, scaledGridWidth, scaledGridHeight;
        public static int gridWidth, gridHeight;

        int xB, yB, margLeft, margRight, margTop, margBottom;
        Rectangle gridRect;

        // line primitives to define grid border
        VertexPositionColor[] leftVertical, rightVertical, topHoriz, bottomHoriz;

        BasicEffect effectGrid;

        public bool isVisible { get; set; }

        public SnakeGrid( Game snakeGame, int xBlock, int yBlock,
                          int leftMargin, int rightMargin,
                          int topMargin, int bottomMargin )
           : base( snakeGame )
        {
            gridScale = 16;

            gridWidth = Game.Window.ClientBounds.Width;
            gridHeight = Game.Window.ClientBounds.Height;

            scaledGridWidth = gridWidth / ( gridScale / 2 );
            scaledGridHeight = gridHeight / ( gridScale / 2 );

            xB = xBlock;
            yB = yBlock;

            isVisible = true; // visible by default

            margLeft = leftMargin;
            margRight = rightMargin;
            margTop = topMargin;
            margBottom = bottomMargin;

            CreateBorder();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        // define grid borders
        private void CreateBorder()
        {
            gridRect = new Rectangle( margLeft, margTop,
                                      Game.Window.ClientBounds.Width - margRight - margLeft,
                                      Game.Window.ClientBounds.Height - margBottom - margTop );

            topHoriz = new VertexPositionColor[ 2 ];
            topHoriz[ 0 ].Position = new Vector3( 0.0f, 0.0f, -1.0f );
            topHoriz[ 1 ].Position = new Vector3( Game.Window.ClientBounds.Width, 0.0f, -1.0f );
            topHoriz[ 0 ].Color = Color.White;
            topHoriz[ 1 ].Color = Color.White;

            bottomHoriz = new VertexPositionColor[ 2 ];
            bottomHoriz[ 0 ].Position = new Vector3( 0.0f, Game.Window.ClientBounds.Height - 1, -1.0f );
            bottomHoriz[ 1 ].Position = new Vector3( Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height - 1, -1.0f );
            bottomHoriz[ 0 ].Color = Color.White;
            bottomHoriz[ 1 ].Color = Color.White;

            rightVertical = new VertexPositionColor[ 2 ];
            rightVertical[ 0 ].Position = new Vector3( Game.Window.ClientBounds.Width - 1, 0.0f, -1.0f );
            rightVertical[ 1 ].Position = new Vector3( Game.Window.ClientBounds.Width - 1, Game.Window.ClientBounds.Height - 1, -1.0f );
            rightVertical[ 0 ].Color = Color.White;
            rightVertical[ 1 ].Color = Color.White;

            leftVertical = new VertexPositionColor[ 2 ];
            leftVertical[ 0 ].Position = new Vector3( 0.0f, 0.0f, -1.0f );
            leftVertical[ 1 ].Position = new Vector3( 0.0f, Game.Window.ClientBounds.Height, -1.0f );
            leftVertical[ 0 ].Color = Color.White;
            leftVertical[ 1 ].Color = Color.White;
        }

        public override void Update( GameTime snakeTime )
        {
            if ( InputManager.OnKeyDown( Keys.Space ) ) {
                isVisible = !isVisible;
            }

            base.Update( snakeTime );
        }

        public override void Draw( GameTime snakeTime )
        {
            if ( isVisible ) {
                // create effect object
                effectGrid = new BasicEffect( GraphicsDevice );

                // create orthographic projection matrix (basically discards Z value of a vertex)
                Matrix projection = Matrix.CreateOrthographicOffCenter( 0.0f, Game.Window.ClientBounds.Width,
                                                                        Game.Window.ClientBounds.Height,
                                                                        0.0f, 0.1f, 10.0f );

                // set effect properties
                effectGrid.Projection = projection;
                effectGrid.View = Matrix.Identity;
                effectGrid.World = Matrix.Identity;
                effectGrid.VertexColorEnabled = true;

                // change viewport to fit desired grid 
                GraphicsDevice.Viewport = new Viewport( gridRect );

                // set vertex/pixel shaders from the effect
                effectGrid.Techniques[ 0 ].Passes[ 0 ].Apply();

                // draw the lines
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>( PrimitiveType.LineList,
                                                                        topHoriz, 0, 1 );
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>( PrimitiveType.LineList,
                                                                        bottomHoriz, 0, 1 );
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>( PrimitiveType.LineList,
                                                                        rightVertical, 0, 1 );
                GraphicsDevice.DrawUserPrimitives<VertexPositionColor>( PrimitiveType.LineList,
                                                                        leftVertical, 0, 1 );
            }
            base.Draw( snakeTime );
        }

        // draw the character sprite on a particular cell in the grid
        public static void DrawCharacter( SpriteBatch spriteBatch, Texture2D characterTexture, Point point, float characterRotation )
        {
            float spriteSize = Math.Max( characterTexture.Width / 2.0f, characterTexture.Height / 2.0f );

            spriteBatch.Draw( characterTexture,
                              PointToVector2( point ),
                              null,
                              Color.White,
                              characterRotation,
                              new Vector2( characterTexture.Width / 2.0f, characterTexture.Height / 2.0f ),
                              gridScale / spriteSize,
                              SpriteEffects.None,
                              0 );
        }

        // converts point on grid to a scaled Vector2 position
		public static Vector2 PointToVector2(Point p)
		{
            return new Vector2( p.X * gridScale + gridScale / 2,
                                p.Y * gridScale + gridScale / 2 );
		}
    }
}
