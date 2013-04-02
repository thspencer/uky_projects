using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Snake
{
    public class SnakeGame : Microsoft.Xna.Framework.Game
    {
        // various strings used throughout the game
        private const string gameTitle = "Snake";
        private const string gameAuthor = "Created by Taylor Spencer";
        private const string startInstructions = "Press Enter to Start New Game";
        private const string quitInstructions = "Press Escape to Quit";
        private const string gameOver = "Game Over";
        private const string playInstructions = ("Use the cursor keys to move character or \n") +
                                                ("W = UP, S = DOWN, A = LEFT, R = RIGHT");
        private const string announceScore = "Score:";

        // available game states
        public enum GameStatus
        {
            TitleScreen, InGame, GameOver
        }

        private Viewport defaultViewport;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont  gameFont;

        // default status is the title screen
        private GameStatus gameStatus = GameStatus.TitleScreen;

        // game objects
        public SnakeGrid       snakeGrid;
        public SnakeCharacter  snakeChar;
        public SnakeFood       snakeFood;

        private int gameScore;

        public SnakeGame()
        {
            graphics = new GraphicsDeviceManager( this );
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            defaultViewport = GraphicsDevice.Viewport;

            this.IsMouseVisible = true;
                       
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch( GraphicsDevice );
            gameFont = Content.Load<SpriteFont>( @"Fonts/gameFont" );
        }

        protected override void UnloadContent()
        {
        }
        
        protected override void Update( GameTime snakeTime )
        {
            // update keyboard state
            InputManager.Update();

            // determine game status
            if ( gameStatus == GameStatus.TitleScreen ) {

                TitleScreen();

            } else if ( gameStatus == GameStatus.InGame ) {

                InGame( snakeTime );

            } else if ( gameStatus == GameStatus.GameOver ) {

                EndGame();

            }
            
            base.Update( snakeTime );
        }

        protected override void Draw( GameTime snakeTime )
        {
            GraphicsDevice.Viewport = defaultViewport;
            GraphicsDevice.Clear( Color.MediumAquamarine );

            float centerX = base.Window.ClientBounds.Width  / 2;
            float centerY = base.Window.ClientBounds.Height / 2;

            spriteBatch.Begin();

            // display game status objects
            if ( gameStatus == GameStatus.TitleScreen ) {
                spriteBatch.DrawString( gameFont, gameTitle,
                                        new Vector2( centerX - 40, 0.0f ), Color.White );
                spriteBatch.DrawString( gameFont, gameAuthor,
                                        new Vector2( centerX - 130, 30.0f ), Color.White );
                spriteBatch.DrawString( gameFont, startInstructions,
                                        new Vector2( centerX - 150, 100.0f ), Color.White );
                spriteBatch.DrawString( gameFont, quitInstructions,
                                        new Vector2( centerX - 110, 120.0f ), Color.White );
                spriteBatch.DrawString( gameFont, playInstructions,
                                        new Vector2( centerX - 200, 170.0f ), Color.White );

            } else if ( gameStatus == GameStatus.InGame ) {              
                spriteBatch.DrawString( gameFont, quitInstructions,
                                        new Vector2( 2.0f, 2.0f ), Color.White );
                spriteBatch.DrawString( gameFont, announceScore,
                                        new Vector2( centerX * 2 - 125, 0.0f ), Color.White );
                spriteBatch.DrawString( gameFont, gameScore.ToString(),
                                        new Vector2( centerX * 2 - 25, 0.0f ), Color.White );
                snakeChar.Draw( spriteBatch );

                snakeFood.Draw( spriteBatch );
            } else if ( gameStatus == GameStatus.GameOver ) {
                spriteBatch.DrawString( gameFont, announceScore,
                                        new Vector2( centerX * 2 - 125, 0.0f ), Color.White );
                spriteBatch.DrawString( gameFont, gameScore.ToString(),
                                        new Vector2( centerX * 2 - 25, 0.0f ), Color.White );
                spriteBatch.DrawString( gameFont, gameOver,
                                        new Vector2( centerX - 80, centerY - 50 ), Color.White );
                spriteBatch.DrawString( gameFont, startInstructions,
                                        new Vector2( centerX - 170, centerY ), Color.White );
                spriteBatch.DrawString( gameFont, quitInstructions,
                                        new Vector2( centerX - 120, centerY + 20 ), Color.White );
            }

            spriteBatch.End();

            base.Draw( snakeTime );
        }

        // initialize game components
        private void StartGame()
        {
            snakeGrid = new SnakeGrid( this, 40, 30, 10, 10, 50, 10 );
            snakeChar = new SnakeCharacter( this );
            snakeFood = new SnakeFood( this );

            Components.Add( snakeGrid );
            Components.Add( snakeChar );
            Components.Add( snakeFood );

            snakeFood.LoadFoodContent( Content );
            snakeChar.LoadCharacterContent( Content );
        }

        private void TitleScreen()
        {
            if ( InputManager.OnKeyDown( Keys.Escape ) ) {
                Exit();
            }
            if ( InputManager.OnKeyDown( Keys.Enter )) {
                StartGame();
                gameScore = 0;
                gameStatus = GameStatus.InGame;
            }
        }

        private void InGame(GameTime snakeTime)
        {
            if ( InputManager.OnKeyDown( Keys.Escape )) {
                gameStatus = GameStatus.TitleScreen;
            }
            // determine if character has eaten food
            if ( snakeChar.IsHeadAt( snakeFood.foodPosition ) ) {
                snakeFood.NewPosition();
                snakeChar.GetBigger();
                gameScore++;
            }
            if ( snakeChar.IsCrashed() ) {
                snakeGrid.isVisible = !snakeGrid.isVisible;
                gameStatus = GameStatus.GameOver;
            }
        }
        
        private void EndGame()
        {
            if ( InputManager.OnKeyDown( Keys.Enter ) ) {
                gameStatus = GameStatus.TitleScreen;
            }
            if ( InputManager.OnKeyDown( Keys.Escape )) {
                Exit();
            }
        }
    }
}
