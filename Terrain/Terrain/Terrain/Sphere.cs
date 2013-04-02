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
    public class Sphere : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public Vector3  position; // initial position

        private Game    mainGame; // passing the Game object along

        private Vector3 cameraOffset; // offset from camera so player can find
        private Model   sphere;
        private Matrix  rotation; // rotation matrix to move in facing direction

        private float   direction; // which way the sphere is facing
        private float   radius;    // size of sphere
        private float   turnSpeed; // speed of turn when moved
        private float   velocity;  // speed of movement

        public Sphere( Game game )
            : base( game )
        {
            mainGame = game;
        }

        public override void Initialize()
        {
            cameraOffset = new Vector3( 3000.0f, -60.0f, -800.0f );
            position = mainGame.camera.position + cameraOffset;

            rotation = Matrix.Identity;

            radius    = 20.0f;
            turnSpeed = 0.05f;
            velocity  = 4;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // sphere texture used with permission under MS-PL from:
            // http://xbox.create.msdn.com/en-us/education/catalog/sample/collision_3d_heightmap
            sphere = mainGame.Content.Load<Model>( @"sphere" );

            base.LoadContent();
        }

        public override void Update( GameTime gameTime )
        {
            // some sphere movement code (mostly matrix rotations) based on example 
            // from MSDN under MS-PL from:
            // http://xbox.create.msdn.com/en-us/education/catalog/sample/collision_3d_heightmap

            Vector3 movement = Vector3.Zero;
            float turnAmount = 0.0f; // how much sphere will be turned by clamped from -1 to + 1

            if( InputManager.IsKeyDown( Keys.Up ) ) {
                movement.Z = -1.0f;
            }
            if( InputManager.IsKeyDown( Keys.Down ) ) {
                movement.Z = 1.0f;
            }
            if( InputManager.IsKeyDown( Keys.Left ) ) {
                turnAmount += 1;
            }
            if( InputManager.IsKeyDown( Keys.Right ) ) {
                turnAmount -= 1;
            }

            turnAmount = MathHelper.Clamp( turnAmount, -1, 1 );

            // apply new direction to sphere
            direction += turnAmount * turnSpeed;

            // create new rotation/velocity matrix based on direction sphere is headed
            Matrix directionMatrix = Matrix.CreateRotationY( direction );
            Vector3 newVelocity    = Vector3.Transform( movement, directionMatrix );
            newVelocity *= velocity;

            // set new position for sphere based on transformed vector
            Vector3 newSpherePosition = position + newVelocity;

            // check that relative position is on top of terrain and prevent movement if not
            if( mainGame.terrain.IsOnTerrain( newSpherePosition ) ) {

                newSpherePosition.Y =
                    mainGame.terrain.GetHeight( newSpherePosition ) +
                    mainGame.sphere.radius;

            } else {
                newSpherePosition = position;
            }

            // determine how far the sphere moved
            float distance = Vector3.Distance( position, newSpherePosition );

            // determine angle of rotation based on the sphere's arc length
            // this gives the amount needed for the appropriate rotation
            float rotationAngle = distance / radius;

            int rollDirection;

            // determine direction that sphere should roll in
            if( movement.Z > 0 ) {
                rollDirection = 1;
            } else {
                rollDirection = -1;
            }

            // create a new rotation matrix based on the spheres right vector
            rotation *= Matrix.CreateFromAxisAngle(
                directionMatrix.Right,
                rotationAngle * rollDirection );

            // assign new position
            position = newSpherePosition;
            
            base.Update( gameTime );
        }

        public override void Draw( GameTime gameTime )
        {
            // sphere model matrix to draw
            Matrix[] boneTransforms = new Matrix[sphere.Bones.Count];

            // transform model before applying effects
            sphere.CopyAbsoluteBoneTransformsTo( boneTransforms );

            foreach( ModelMesh mesh in sphere.Meshes ) {
                foreach( BasicEffect sphereEffect in mesh.Effects ) {

                    sphereEffect.View       = mainGame.view;
                    sphereEffect.Projection = mainGame.proj;

                    // sphere needs to have its own world matrix
                    sphereEffect.World = 
                        boneTransforms[mesh.ParentBone.Index] * 
                        rotation *
                        Matrix.CreateTranslation( position );

                    sphereEffect.EnableDefaultLighting();

                    // keep from sphere showing through terrain
                    mainGame.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                }

                mesh.Draw();
            }

            base.Draw( gameTime );
        }
    }
}
