using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Snake
{
    public class SnakeFood : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public Point foodPosition;

        private Texture2D foodTexture;
        private Random randPos = new Random();

        public SnakeFood( Game snakeGame )
            : base ( snakeGame )
        {
            // initialize food position
            NewPosition();
        }

        public void LoadFoodContent( ContentManager content )
        {
            foodTexture = content.Load<Texture2D>( @"Sprites/Apple" );
        }

        public void Draw( SpriteBatch foodSpriteBatch )
        {
            SnakeGrid.DrawCharacter( foodSpriteBatch, foodTexture, foodPosition, 0f );
        }

        public void NewPosition()
        {
            foodPosition = new Point( randPos.Next( SnakeGrid.scaledGridWidth / 2 ),
                                      randPos.Next( SnakeGrid.scaledGridHeight / 2 ));

            // prevent food from being positioned in upper text area and along borders
            if (( foodPosition.Y < 5 || foodPosition.Y >= ( SnakeGrid.scaledGridHeight / 2 ) -1 ) ||
                  foodPosition.X < 1 || foodPosition.X >= ( SnakeGrid.scaledGridWidth / 2 ) - 1 ){
                NewPosition();
            }
        }
    }
}
