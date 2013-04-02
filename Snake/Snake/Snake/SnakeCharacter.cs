using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Snake
{
    public class SnakeCharacter : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public enum CharacterDirection
        {
            Up, Down, Left, Right
        }

        // character parts
        private List<Point> characterBodyPoints;
        private Texture2D characterHead, characterBody, characterTail, characterAngle;
        
        private CharacterDirection currentDir;
        private CharacterDirection nextDir;

        private float characterSpeed;
        private float elapsedTime;
        private bool  getBigger;

        public SnakeCharacter( Game snakeGame )
            : base( snakeGame )
        {
            characterBodyPoints = new List<Point>();
            currentDir = CharacterDirection.Right;
            nextDir = CharacterDirection.Right;
            characterSpeed = 0.1f;

            ResetCharacter();
        }

        public void LoadCharacterContent( ContentManager characterContent )
        {
            characterHead = characterContent.Load<Texture2D>( @"Sprites/Head" );
            characterBody = characterContent.Load<Texture2D>( @"Sprites/Body" );
            characterTail = characterContent.Load<Texture2D>( @"Sprites/Tail" );
            characterAngle = characterContent.Load<Texture2D>( @"Sprites/Angle" );
        }

        public void Draw( SpriteBatch characterSpriteBatch )
        {
            // draw the body points
            for ( int i = 1; i < characterBodyPoints.Count - 1; i++ ) {
                DrawCharacterBody( characterSpriteBatch,
                                   characterBodyPoints[ i ],
                                   characterBodyPoints[ i + 1 ],
                                   characterBodyPoints[ i - 1 ] );

            }

            DrawCharacterHead( characterSpriteBatch );
            DrawCharacterTail( characterSpriteBatch );
        }

        // determine point of rotation and attach character body to grid
        private void DrawCharacterBody( SpriteBatch characterSpriteBatch,
                                        Point currentPt, Point nextPt, Point previousPt )
        {
            if ( ( currentPt.X == previousPt.X &&
                   currentPt.X != nextPt.X &&
                   currentPt.Y != previousPt.Y ) ||
                 ( currentPt.X == nextPt.X &&
                   currentPt.X != previousPt.X && 
                   currentPt.Y != nextPt.Y ) ) {

                SnakeGrid.DrawCharacter( characterSpriteBatch, characterAngle,
                                         currentPt, GetAngleRotation( currentPt, previousPt, nextPt ) );
            }else if ( currentPt.X != previousPt.X ) {
                SnakeGrid.DrawCharacter( characterSpriteBatch, characterBody, currentPt, 0.0f );

            }else if ( currentPt.Y != previousPt.Y ) {
                SnakeGrid.DrawCharacter( characterSpriteBatch, characterBody,
                                         currentPt, MathHelper.PiOver2 );

            }
        }

        // determine point of rotation and attach character head to grid
        private void DrawCharacterHead( SpriteBatch characterSpriteBatch )
        {
            Point characterHeadPt = characterBodyPoints[ 0 ];
            Point nextBodyPt      = characterBodyPoints[ 1 ];

			float rotation;

            if ( characterHeadPt.Y == nextBodyPt.Y - 1 ) {   
				rotation = -MathHelper.PiOver2;

            } else if ( characterHeadPt.Y == nextBodyPt.Y + 1 ) {
				rotation = MathHelper.PiOver2;

            } else if ( characterHeadPt.X == nextBodyPt.X - 1 ) {
                rotation = MathHelper.Pi;

            } else {
                rotation = 0.0f;
            }

            SnakeGrid.DrawCharacter( characterSpriteBatch, characterHead, characterHeadPt, rotation );
		}

        // determine point of rotation and attach character tail to grid
        private void DrawCharacterTail( SpriteBatch characterSpriteBatch )
        {
    	    // get the point of the tail
            Point tailPoint = characterBodyPoints[ characterBodyPoints.Count - 1 ];

			// get the point of the previous body part
            Point lastBody = characterBodyPoints[ characterBodyPoints.Count - 2 ];

			// figure out the rotation for the tail based on the positions
			// of the tail and the next body part
			float rotation;

            if ( tailPoint.X == lastBody.X + 1 ) {
                rotation = MathHelper.Pi;

            } else if ( tailPoint.Y == lastBody.Y + 1 ) {
                rotation = -MathHelper.PiOver2;

            } else if ( tailPoint.Y == lastBody.Y - 1 ) {
                rotation = MathHelper.PiOver2;

            } else {
                rotation = 0.0f;
            }

			// draw the tail using the global DrawSprite method.
            SnakeGrid.DrawCharacter( characterSpriteBatch, characterTail, tailPoint, rotation );
		}

        // check for keypress direction and ensure that a change in direction
        // will not cause snake to double back on itself
        public override void Update( GameTime snakeTime )
        {
            if (( InputManager.OnKeyDown( Keys.Up ) ||
                  InputManager.OnKeyDown( Keys.W ) ) && 
                
                currentDir != CharacterDirection.Down ) {

                nextDir = CharacterDirection.Up;
            }
            if (( InputManager.OnKeyDown( Keys.Down ) ||
                  InputManager.OnKeyDown( Keys.S )) && 
                
                currentDir != CharacterDirection.Up ) {

                nextDir = CharacterDirection.Down;
            }
            if (( InputManager.OnKeyDown( Keys.Left ) ||
                  InputManager.OnKeyDown( Keys.A ) ) && 
                 
                currentDir != CharacterDirection.Right ) {

                nextDir = CharacterDirection.Left;
            }
            if ( ( InputManager.OnKeyDown( Keys.Right ) ||
                   InputManager.OnKeyDown( Keys.D ) ) &&

                currentDir != CharacterDirection.Left ) {

                nextDir = CharacterDirection.Right;
            }

            // increase timer
            elapsedTime += (float)snakeTime.ElapsedGameTime.TotalSeconds;

            // reset timer and move character if speed > timer
            // otherwise the character speed will run too fast
            if ( elapsedTime > characterSpeed ) {
                currentDir = nextDir;
                elapsedTime = 0.0f;

                MoveCharacter();
            }
        }

        // return bool request for head location
        public bool IsHeadAt( Point p )
        {
            if ( characterBodyPoints[ 0 ] == p ) {
                return true;
            }

            return false;
        }

        // return bool request for whether given point exists on character body
        public bool IsBodyAt( Point p ) { return characterBodyPoints.Contains( p ); }

        public void GetBigger() { getBigger = true; }

        // return bool request for whether character has collided with itself or the grid border
        public bool IsCrashed()
        {
            Point p = characterBodyPoints[ 0 ];

            if ( p.Y < 4 || p.Y >= ( SnakeGrid.scaledGridHeight / 2) - 1 ||
                 p.X < 1 || p.X >= ( SnakeGrid.scaledGridWidth / 2) - 1 ) {

                return true;
            }

            // check all points against location of character head
            for ( int i = 1; i < characterBodyPoints.Count; i++ ) {
                if ( characterBodyPoints[ 0 ] == characterBodyPoints[ i ] ) {
                    return true;
                }
            }

            return false;
        }

        private void MoveCharacter()
        {
            Point a = characterBodyPoints[ 0 ];

            if ( currentDir == CharacterDirection.Up ) {
                characterBodyPoints[ 0 ] = new Point( a.X, a.Y - 1 );

            } else if ( currentDir == CharacterDirection.Down ) {
                characterBodyPoints[ 0 ] = new Point( a.X, a.Y + 1 );

            } else if ( currentDir == CharacterDirection.Left ) {
                characterBodyPoints[ 0 ] = new Point( a.X - 1, a.Y );

            } else if ( currentDir == CharacterDirection.Right ) {
                characterBodyPoints[ 0 ] = new Point( a.X + 1, a.Y );
            }

            if ( getBigger ) {
                getBigger = false;
                characterBodyPoints.Insert( 1, a );

                return;
            }

            // move the rest of the character on grid
            for ( int i = 1; i < characterBodyPoints.Count; i++ ) {
                Point b = characterBodyPoints[ i ]; // save current position

                // character body part position is now the same as part in front of it
                characterBodyPoints[ i ] = a;

                a = b; // saves old position for next part
            }

        }

        // *** similar concept was given as example on MSDN TechNet for C# language specification ***
        private float GetAngleRotation( Point currentPt, Point previousPt, Point nextPt )
        {
            Point pi = new Point( nextPt.X - 1, previousPt.Y - 1 );
            Point pi2 = new Point( previousPt.X - 1, nextPt.Y - 1 );
            Point piOver2 = new Point( nextPt.X - 1, previousPt.Y + 1 );
            Point piOver22 = new Point( previousPt.X - 1, nextPt.Y + 1 );
            Point negPiOver2 = new Point( nextPt.X + 1, previousPt.Y - 1 );
            Point negPiOver22 = new Point( previousPt.X + 1, previousPt.Y - 1 );

            if ( currentPt == pi || currentPt == pi2 ) {
                return MathHelper.Pi;

            } else if ( currentPt == piOver2 || currentPt == piOver22 ) {
                return MathHelper.PiOver2;

            } else if ( currentPt == negPiOver2 || currentPt == negPiOver22 ) {
                return -MathHelper.PiOver2;

            } else {
                return 0.0f;
            }
        }

        public void ResetCharacter()
        {
            characterBodyPoints.Clear();

            // head, body, tail
            characterBodyPoints.Add( new Point( 2, 10 ) );
            characterBodyPoints.Add( new Point( 1, 10 ) );
            characterBodyPoints.Add( new Point( 0, 10 ) );

            currentDir = CharacterDirection.Right;
            nextDir    = CharacterDirection.Right;
        }
    }
}
