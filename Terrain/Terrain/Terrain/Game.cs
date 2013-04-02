/*
 * Taylor Spencer
 * CS485: XNA Terrain Project
 * Submitted 03/22/2013
 *
 */


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

namespace Terrain
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        public BasicEffect gameEffect;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D   titleBackground;
        SpriteFont  gameFont;
        SpriteFont  debugFont;

        public enum GAME_STATE { TITLE, GAME, RESTART, OVER }; // game states


        // game objects used
        public Camera     camera;
        public Terrain    terrain;
        public Sphere     sphere;
        GAME_STATE state;

        public Vector2 windowCenter;
        public Matrix proj;
        public Matrix view;
        public Matrix world;

        bool showCamInsructions;
        public bool Show_Debug = true; // default debug status

        Color normalTextColor;
        Color debugTextColor;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth  = 1280;
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferMultiSampling = true;  // enable antialiasing

            normalTextColor = Color.White;
            debugTextColor  = Color.Red;

            showCamInsructions = true;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Window.AllowUserResizing = true; // user can resize window
            Window.Title = "XNA: Terrain Project";

            windowCenter.X = GraphicsDevice.Viewport.Width / 2;
            windowCenter.Y = GraphicsDevice.Viewport.Height / 2;

            Window.ClientSizeChanged += new EventHandler<EventArgs>( WindowSizeChanged );

            state = new GAME_STATE();
            state = GAME_STATE.TITLE;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch     = new SpriteBatch( GraphicsDevice );
            gameEffect      = new BasicEffect( GraphicsDevice );
            gameFont        = Content.Load<SpriteFont>( @"GameFont" );
            debugFont       = Content.Load<SpriteFont>( @"DebugFont" );
            titleBackground = Content.Load<Texture2D>( @"TitleBackground" );
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            if ( InputManager.OnKeyDown( Keys.Escape ) ){

                this.Exit();
            }
            if( InputManager.OnKeyDown( Keys.L ) ) {

                Show_Debug = !Show_Debug;
            }
            if( InputManager.OnKeyDown( Keys.R ) ) {

                GameReset();
            }
            if( InputManager.OnKeyDown( Keys.O ) ) {

                state = GAME_STATE.OVER;
                Components.Clear();
            }
            if( InputManager.OnKeyDown( Keys.Enter ) &&
                state == GAME_STATE.TITLE ) {

                    StartGame();
            }
            if( InputManager.OnKeyDown( Keys.F1 ) &&
                state == GAME_STATE.GAME ) {

                    showCamInsructions = !showCamInsructions;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear( Color.SkyBlue );

            // setup game projection/view/world parameters
            if( camera != null ) {
                proj = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4,
                    GraphicsDevice.Viewport.AspectRatio,
                    1.0f,
                    10000.0f );
                view  = camera.View();
                world = Matrix.Identity;

                gameEffect.Projection = proj;
                gameEffect.View = view;
                gameEffect.World = world;
            }

            if( state == GAME_STATE.TITLE ) {

                TitleScreen();

            } else if( state == GAME_STATE.OVER ) {

                GameOver();

            } else if( state == GAME_STATE.GAME ) {

                InGame();
            }

            base.Draw(gameTime);
        }

        // prints provided text object to the screen
        public void PrintToScreen( Vector2 pos, string line, Color color )
        {
            spriteBatch.Begin();

            spriteBatch.DrawString(
                debugFont,
                line,
                pos,
                color );

            spriteBatch.End();
       }

        protected void InGame()
        {
            var textPosY = 0.0f;
            var textPosX = GraphicsDevice.Viewport.Width - 400;

            var _strMove = "Press W,A,S,D keys to move"; // move camera
            var _strPitU = "Use mouse to change rotate"; // pitch camera
            var _strSph  = "Use cursor keys to move sphere"; // move sphere object
            var _strDbg  = "Press L to show/hide debug info"; // print debug
            var _strIns  = "Press F1 to hide instructions";   // print camera instructions
            var _gameOver = "Press O to access Game Over screen";
            var _strReset = "Press 'R' to reset game and terrain"; // reset

            // only print if user requested
            if( showCamInsructions ) {

                // start draw sorting sprites back to front
                spriteBatch.Begin();

                spriteBatch.DrawString(
                    gameFont,
                    _strMove,
                    new Vector2( textPosX, textPosY ),
                    normalTextColor );
                spriteBatch.DrawString(
                    gameFont,
                    _strPitU,
                    new Vector2( textPosX, textPosY += 25 ),
                    normalTextColor );
                spriteBatch.DrawString(
                    gameFont,
                    _strSph,
                    new Vector2( textPosX, textPosY += 25 ),
                    normalTextColor );
                spriteBatch.DrawString(
                    gameFont,
                    _strIns,
                    new Vector2( textPosX, textPosY += 25 ),
                    normalTextColor );
                spriteBatch.DrawString(
                    gameFont,
                    _strDbg,
                    new Vector2( textPosX, textPosY += 25 ),
                    normalTextColor );
                spriteBatch.DrawString(
                    gameFont,
                    _gameOver,
                    new Vector2( textPosX, textPosY += 25 ),
                    normalTextColor );
                spriteBatch.DrawString(
                    gameFont,
                    _strReset,
                    new Vector2( textPosX, textPosY += 25 ),
                    normalTextColor );

                spriteBatch.End();
            }

            // only draw if debug enabled
            if( Show_Debug ) {
                DebugInfo();
            }
        }

        // initialize required game components and game state
        protected void StartGame()
        {
            state   = GAME_STATE.GAME;
            camera  = new Camera( this );
            terrain = new Terrain( this );
            sphere  = new Sphere( this );

            Components.Add( camera );
            Components.Add( terrain );
            Components.Add( sphere );
        }

        protected void TitleScreen()
        {
            var _strStart = "Press 'Enter' to start";   // start
            var _strTitle = "Sphere Finder";          // title
            var _strAuth  = "Taylor Spencer";           // author
            var _strQuit  = "Press 'Esc' to quit";      // quit
            var _strReset = "Press 'R' to reset (will create new terrain)"; // reset
            var _strDbg   = "Press 'L' for debug info"; // debug
            var _strGoal  = "Move camera around terrain to find the sphere and then interact with it"; // game goal

            spriteBatch.Begin();

            // title screen background
            spriteBatch.Draw(
                titleBackground,
                new Rectangle( 0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height ),
                normalTextColor );

            spriteBatch.DrawString(
                gameFont,
                _strGoal,
                new Vector2( ( windowCenter.X - 300 ), windowCenter.Y - 25 ),
                normalTextColor );
            spriteBatch.DrawString(
                gameFont,
                _strStart,
                new Vector2( ( windowCenter.X - 100 ), windowCenter.Y ),
                normalTextColor );
            spriteBatch.DrawString(
                gameFont,
                _strTitle,
                 new Vector2( windowCenter.X - 100, 0.0f),
                 normalTextColor );
            spriteBatch.DrawString(
                gameFont,
                _strAuth,
                 new Vector2( windowCenter.X - 105.0f, 30.0f),
                 normalTextColor );
            spriteBatch.DrawString(
                gameFont,
                _strQuit,
                new Vector2( 0.0f, GraphicsDevice.Viewport.Height - 75 ),
                normalTextColor );
            spriteBatch.DrawString(
                gameFont,
                _strReset,
                new Vector2( 0.0f, GraphicsDevice.Viewport.Height - 50 ),
                normalTextColor );
            spriteBatch.DrawString(
                gameFont,
                _strDbg,
                new Vector2( 0.0f, GraphicsDevice.Viewport.Height - 25 ),
                normalTextColor );

            spriteBatch.End();
        }

        protected void GameOver()
        {
            var _strOver  = "Game Over";             // game over
            var _strQuit  = "Press 'Esc' to quit";   // quit
            var _strReset = "Press 'R' to reset";    // reset

            spriteBatch.Begin();

            // game over screen background
            spriteBatch.Draw(
                titleBackground,
                new Rectangle( 0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height ),
                normalTextColor );

            spriteBatch.DrawString(
                gameFont,
                _strOver,
                new Vector2( windowCenter.X - 50, windowCenter.Y ),
                normalTextColor );
            spriteBatch.DrawString(
                gameFont,
                _strQuit,
                new Vector2( windowCenter.X - 100, windowCenter.Y + 75 ),
                normalTextColor );
            spriteBatch.DrawString(
                gameFont,
                _strReset,
                new Vector2( windowCenter.X - 100, windowCenter.Y + 100 ),
                normalTextColor );

            spriteBatch.End();
        }

        // remove all related game components for round and reset gamestate
        protected void GameReset()
        {
            state        = GAME_STATE.TITLE;

            Components.Remove( sphere );
            Components.Remove( terrain );
            Components.Remove( camera );
        }

        protected void DebugInfo()
        {
            var textPos = new Vector2( 0.0f, 0.0f );

            // current camera vector3 position
            var _strCamPos = (
                    "CAM POS (x,y,z): " +
                    (int)camera.position.X + ":" +
                    (int)camera.position.Y + ":" +
                    (int)camera.position.Z );

            // current camera vector3 angle
            var _strCamAng = (
                    "CAM ANG (x,y,z): " +
                    (int)camera.angle.X + ":" +
                    (int)camera.angle.Y + ":" +
                    (int)camera.angle.Z );

            // current camera vector3 forward
            var _strCamForward = (
                    "CAM FORWARD (x,y,z): " +
                    (int)camera.forward.X + ":" +
                    (int)camera.forward.Y + ":" +
                    (int)camera.forward.Z );

            // terrain height at current position
            var _strTerrainHeight = (
                    "TERRAIN HEIGHT: " +
                    (int)terrain.GetHeight( camera.position ));

            // bumpieness scale used for current round
            var _strScale = (
                "HEIGHT SCALE: " +
                (int)terrain.heightScale );

            // current sphere position on terrain
            var _strSphere = (
                "SPHERE POS (x,y,z): " +
                (int)sphere.position.X + ":" +
                (int)sphere.position.Y + ":" +
                (int)sphere.position.Z + ":" );

            PrintToScreen( new Vector2( textPos.X, textPos.Y ), _strCamPos, debugTextColor );
            PrintToScreen( new Vector2( textPos.X, textPos.Y += 20.0f ), _strCamAng, debugTextColor );
            PrintToScreen( new Vector2( textPos.X, textPos.Y += 20.0f ), _strCamForward, debugTextColor );
            PrintToScreen( new Vector2( textPos.X, textPos.Y += 20.0f ), _strTerrainHeight, debugTextColor );
            PrintToScreen( new Vector2( textPos.X, textPos.Y += 20.0f ), _strScale, debugTextColor );
            PrintToScreen( new Vector2( textPos.X, textPos.Y += 20.0f ), _strSphere, debugTextColor );
        }

        protected void WindowSizeChanged( object sender, EventArgs e )
        {
            windowCenter.X = GraphicsDevice.Viewport.Width / 2;
            windowCenter.Y = GraphicsDevice.Viewport.Height / 2;
        }

    }
}
