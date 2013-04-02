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
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        public Vector3 position { get; private set; } // camera position on screen
        public Vector3 up       { get; private set; } // camera up position (doesnt really change right now since not rolling)
        public Vector3 forward  { get; private set; } // camera forward/target movement
        public Vector3 angle; // camera angle (only for display purposes)
        public Matrix  view;  // camera view matrix
        public float   cameraOffset; // camera offset from the "ground" so the view looks more natural

        private Vector3 prevPos;
        private float moveScale;
        private float turnSpeed;
        private MouseState prevMouseState; // store previous mouse state

        Game mainGame;

        public Camera( Game game )
            : base( game )
        {
            mainGame = game;
        }

        public override void Initialize()
        {
            cameraOffset = 60.0f; // height offset for camera
            turnSpeed    = 25.0f; // speed of mouse movement
            moveScale    = 0.5f;  // camera movement scale

            // place initial position of camera on terrain plus a height offset to see the ground better
            position = new Vector3(
                 mainGame.terrain.heightmapOrigin.X,
                 mainGame.terrain.heightmapOrigin.Y + cameraOffset,
                 mainGame.terrain.heightmapOrigin.Z );
            
            forward  = new Vector3( 1.0f, 0.0f, -25.0f ); // increase movement speed
            up       = Vector3.Up;
            angle    = Vector3.Zero;

            prevPos = position;

            prevMouseState = Mouse.GetState();

            Mouse.SetPosition( (int)mainGame.windowCenter.X, (int)mainGame.windowCenter.Y );

            base.Initialize();
        }

        public override void Update( GameTime gameTime )
        {
            MouseState mouse = Mouse.GetState();

            prevPos = position;

            if( InputManager.IsKeyDown( Keys.W ) ) {
                // move camera forward
                Thrust( moveScale );
            }
            if( InputManager.IsKeyDown( Keys.S ) ) {
                // move camera backward
                Thrust( -moveScale );
            }

            if( InputManager.IsKeyDown( Keys.A ) ) {
                // move camera left
                StrafeHoriz( moveScale * 20.0f );
            }
            if( InputManager.IsKeyDown( Keys.D ) ) {
                // move camera right
                StrafeHoriz( -moveScale * 20.0f );
            }

            // prevent pitching if moving at same time to keep from tilting
            // prevent pitching on exceeding max/min angle
            if(( angle.Y <= 22.5f || angle.Y >= -22.5f ) &&
                InputManager.IsKeyDown( Keys.E )    &&
                InputManager.IsKeyUp( Keys.A )      &&
                InputManager.IsKeyUp( Keys.D ) ){
                // move camera view up
                    Pitch( -moveScale );
            }

            if ( ( angle.Y < 22.5f || angle.Y >= -22.5f ) &&
                 InputManager.IsKeyDown( Keys.C )     &&
                 InputManager.IsKeyUp( Keys.A )       &&
                 InputManager.IsKeyUp( Keys.D )){
                 // move camera view down
                    Pitch( moveScale );
            }

            // check for mouse movement to adjust camera view

            // left/right rotation
            if ( !mouse.X.Equals( prevMouseState.X )) {
                Yaw( MathHelper.ToRadians( ( (int)mainGame.windowCenter.X - mouse.X ) * turnSpeed * moveScale ) );
            } 
            
            // up/down rotation
            if ( !mouse.Y.Equals( prevMouseState.Y )) {
                Pitch( MathHelper.ToRadians( ( mouse.Y - (int)mainGame.windowCenter.Y ) * turnSpeed * moveScale ) );
            }

            // forward movement (not documented)
            if( mouse.RightButton == ButtonState.Pressed ) {
                Thrust( moveScale );
            }

            // clamp camera angle at 0 and 360
            angle = new Vector3(
                MathHelper.WrapAngle( MathHelper.ToRadians(angle.X )),
                MathHelper.WrapAngle( MathHelper.ToRadians( angle.Y )),
                MathHelper.WrapAngle( MathHelper.ToRadians( angle.Z )));
            angle = new Vector3(
                MathHelper.ToDegrees( angle.X ),
                MathHelper.ToDegrees( angle.Y ),
                MathHelper.ToDegrees( angle.Z ));

            CheckPosition(); // adjust placement of camera on terrain;

            // store old mouse position
            prevMouseState = mouse;
            // reset position of mouse to center of window
            Mouse.SetPosition( (int)mainGame.windowCenter.X, (int)mainGame.windowCenter.Y );

            base.Update( gameTime );
        }

        public Matrix View()
        {
            return Matrix.CreateLookAt( position, position + forward, up );
        }

        // move forward
        public void Thrust( float scale )
        {
            forward.Normalize();

            // only move if next position is on terrain
            if( CheckPosition() ) {
                position += forward * scale;
            } else {
                // trivial attempt to keep camera on terrain
                position += ( new Vector3(
                    forward.X,
                    forward.Y,
                   -forward.Z ) * scale );
            }
        }

        // strafe left/right
        public void StrafeHoriz( float scale )
        {
            var left = Vector3.Cross( up, forward );
            left.Normalize();

            // only move if next position is on terrain
            if( CheckPosition() ) {
                position += left * scale;
            } else {
                // trivial attempt to keep camera on terrain
                position += ( new Vector3(
                    left.X,
                    left.Y,
                   -left.Z ) * scale );
            }
        }

        // strafe forward/back
        public void StrafeVert( float scale )
        {
            up.Normalize();
            position += up * scale;
        }

        // yaw camera left/right
        public void Yaw( float scale )
        {
            forward.Normalize();
            forward = Vector3.Transform(
                forward,
                Matrix.CreateFromAxisAngle( up, MathHelper.ToRadians( scale ) ) );

            angle.X += scale;
        }

        // pitch camera left/right
        public void Pitch( float scale )
        {
            forward.Normalize();
            var left = Vector3.Cross( up, forward );
            left.Normalize();

            forward = Vector3.Transform(
                forward,
                Matrix.CreateFromAxisAngle( left, MathHelper.ToRadians( scale ) ) );

            // disabled for unwanted roll effect
            // up = Vector3.Transform(
            //    up,
            //    Matrix.CreateFromAxisAngle( left, MathHelper.ToRadians( scale ) ) );

            angle.Y += scale;
        }
        
        // checks if the next camera position is still on terrain
        public bool CheckPosition()
        {
            if( mainGame.terrain.IsOnTerrain( position ) ) {

                // check if camera is moving below the terrain
                if( position.Y < mainGame.terrain.GetHeight( position ) + cameraOffset ) {

                    position = new Vector3(
                        position.X,
                        mainGame.terrain.GetHeight( position ) + cameraOffset,
                        position.Z );
                }

                // check if camera is moving to terrain point that is lower than camera
                if( position.Y > mainGame.terrain.GetHeight( position ) + cameraOffset ) {

                    position = new Vector3(
                        position.X,
                        mainGame.terrain.GetHeight( position ) + cameraOffset,
                        position.Z );
                }

                return true;
            } else {
                return false;
            }
        }
    }
}
