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
    public class Terrain : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public Vector3 heightmapOrigin; // initial center point of terrain map
        public float heightScale;  // scale factor to control bumpieness

        Game        mainGame;         // passing the Game object along
        SpriteBatch terrainSprites;   // spritebatch for drawing terrain
        BasicEffect terrainEffect;    // terrain basic effect

        VertexPositionNormalTexture[] terrainSurface; // terrain vertices
        int[] terrainIndices; // terrain indices

        Texture2D heightFromTexture; // texture used to get height points from
        Texture2D groundTexture;     // texture applied to terrain for surface

        float[,] heightData; // data points from height image
        Random rnd;

        float terrainWidth;  // dimensions of texture used for height map
        float terrainHeight; // 
        float terrainScale;  // scale factor to manipulate size of generated map

        public Terrain( Game game )
            : base( game )
        {
            mainGame = game;
        }

        public override void Initialize()
        {
            rnd = new Random();

            terrainScale = 50.0f; // set terrain size scaling value
            heightScale  = rnd.Next( 100, 900 ); // add a small measure of randomness

            base.Initialize();
        }

        protected override void LoadContent()
        {
            terrainSprites = new SpriteBatch(mainGame.GraphicsDevice);
            terrainEffect = new BasicEffect(mainGame.GraphicsDevice);

            groundTexture = mainGame.Content.Load<Texture2D>(@"Ground");
            heightFromTexture = mainGame.Content.Load<Texture2D>(@"HeightMap");

            terrainWidth  = heightFromTexture.Width;
            terrainHeight = heightFromTexture.Height;

            // these are the initial origin points that the camera should be based on
            // this should approximate the center of the map
            heightmapOrigin.X = -( terrainWidth - 1) / 2 * terrainScale;
            heightmapOrigin.Y = 0.0f;
            heightmapOrigin.Z = -( terrainHeight - 1) / 2 * terrainScale;

            InitHeightData( heightFromTexture ); // set height map data
            InitIndices();   // intialize the indices
            CreateTerrain(); // initialize the vertices
            CreateNormals(); // normalize the terrain

            base.LoadContent();
        }

        public override void Update( GameTime gameTime )
        {
            base.Update( gameTime );
        }

        public override void Draw( GameTime gameTime )
        {
            // apply the effect parameters for the drawable objects
            terrainEffect.Projection = mainGame.proj;
            terrainEffect.View       = mainGame.view;
            terrainEffect.World      = mainGame.world;

            // apply texture to the surface
            terrainEffect.TextureEnabled = true;
            terrainEffect.Texture = groundTexture;

            terrainEffect.EnableDefaultLighting();

            // apply reflectivity scale
            terrainEffect.SpecularColor = new Vector3( 0.6f, 0.4f, 0.2f );
            terrainEffect.SpecularPower = 10.0f;

            // apply fog effect to cover boundaries
            terrainEffect.FogEnabled = true;
            terrainEffect.FogColor   = Color.White.ToVector3();
            terrainEffect.FogStart   = 100;
            terrainEffect.FogEnd     = 5000;

            terrainEffect.Techniques[0].Passes[0].Apply();

            // set rasterizer state to disable culling
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            //rs.FillMode = FillMode.WireFrame; // terrain model will appear in wireframe mode
            mainGame.GraphicsDevice.RasterizerState = rs;
            
            // prevent objects from showing through when behind another
            mainGame.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            terrainSprites.Begin();

            // draw terrain mesh
            mainGame.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                PrimitiveType.TriangleList,
                terrainSurface,
                0,
                terrainSurface.Length,
                terrainIndices,
                0,
                terrainIndices.Length / 3 );

            terrainSprites.End();

            base.Draw( gameTime );
        }

        // intialize terrain indexes used to create surface
        // based on lecture examples given in class
        protected void InitIndices()
        {
            var accum = 0; // used for vectorizing the grid

            // size of terrain based on width and height of heightmap image scaled by the number of vertices
            terrainIndices = new int[(int)( ( terrainWidth - 1 ) * ( terrainHeight - 1 ) * 6 )];

            for( var z = 0; z < terrainHeight - 1; z++ ) {
                for( var x = 0; x < terrainWidth - 1; x++ ) {

                    int lowerLeft = (int)( x + z * terrainWidth );
                    int lowerRight = (int)( ( x + 1 ) + z * terrainWidth );
                    int topLeft = (int)( x + ( z + 1 ) * terrainWidth );
                    int topRight = (int)( ( x + 1 ) + ( z + 1 ) * terrainWidth );

                    terrainIndices[accum++] = lowerLeft;   // shared
                    terrainIndices[accum++] = topRight; // upper right
                    terrainIndices[accum++] = lowerRight;  // right

                    terrainIndices[accum++] = lowerLeft;   // shared
                    terrainIndices[accum++] = topLeft; // upper left
                    terrainIndices[accum++] = topRight; // upper right
                }
            }
        }

        // create terrain positions along with height and texture coordinates
        protected void CreateTerrain()
        {
            terrainSurface = new VertexPositionNormalTexture[(int)( ( terrainWidth - 1 ) * ( terrainHeight - 1 ) * 6 )];

            for( int z = 0; z < terrainHeight; z++ ) {
                for( int x = 0; x < terrainWidth; x++ ) {

                    int index = (int)( x + z * terrainWidth );

                    // compute next vertex positions modified by scale factor to control height smoothness
                    terrainSurface[index].Position.X = terrainScale * ( x - ( ( terrainWidth  - 1 ) / 2.0f ) );
                    terrainSurface[index].Position.Z = terrainScale * ( z - ( ( terrainHeight - 1 ) / 2.0f ) );

                    // compute next vertex height
                    terrainSurface[index].Position.Y = heightData[x, z];

                    // apply texture coordinates...stretch texture across surface and interpolate from 0 to 1
                    terrainSurface[index].TextureCoordinate.X = x / ( terrainWidth - 1.0f );
                    terrainSurface[index].TextureCoordinate.Y = z / ( terrainHeight - 1.0f );
                }
            }
        }

        // set vector normals for surface and lighting effects
        protected void CreateNormals()
        {
            // initialize normals for terrain
            for( int i = 0; i < terrainSurface.Length; i++ ) {
                terrainSurface[i].Normal = Vector3.Zero;
            }
            
            // set surface normals for each triangle
            for( int i = 0; i < terrainIndices.Length / 3; i++ ) {

                // determine which triangle we are working on
                int indice1 = terrainIndices[i * 3];
                int indice2 = terrainIndices[i * 3 + 1];
                int indice3 = terrainIndices[i * 3 + 2];

                Vector3 side1 = terrainSurface[indice1].Position - terrainSurface[indice3].Position;
                Vector3 side2 = terrainSurface[indice1].Position - terrainSurface[indice2].Position;

                // create normal
                Vector3 normal = Vector3.Cross( 
                    terrainSurface[indice1].Position - terrainSurface[indice3].Position,
                    terrainSurface[indice1].Position - terrainSurface[indice2].Position );

                normal = Vector3.Normalize( normal );

                // apply normals
                terrainSurface[indice1].Normal += normal;
                terrainSurface[indice2].Normal += normal;
                terrainSurface[indice3].Normal += normal;
            }
        }

        /* determine height points from a texture to apply as a heightmap
         * partly based on a example from:
         * http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series1/Terrain_from_file.php
         * with some modifications to ensure 
         * 
         * 
         * the way this works is by using the texture resolution to store a width 
         * and height for other functions.  
         * the color values of the texture are read to determine a height value for each pixel.  
         * the red channel is used to determine these points 
         * the minimum and maximum heights are updated each iteration to ensure
         * that the height points around a give area are similar preventing drastic changes
         * each point is then scaled  to create a measure of smoothness
         */
        protected void InitHeightData( Texture2D heightMap )
        {
            Vector2 previousHeight = Vector2.Zero;

            Color[] heightMapColors = new Color[(int)( terrainWidth * terrainHeight )];
            heightMap.GetData( heightMapColors );

            heightData = new float[(int)terrainWidth, (int)terrainHeight];

            for( int x = 0; x < terrainWidth; x++ ) {
                for( int y = 0; y < terrainHeight; y++ ) {

                    heightData[x, y] = heightMapColors[(int)( x + y * terrainWidth )].R ;

                    if( heightData[x, y] < previousHeight.X ) {
                        previousHeight.X = heightData[x, y];
                    }

                    if( heightData[x, y] > previousHeight.Y ) {
                        previousHeight.Y = heightData[x, y];
                    }
                }
            }

            // take the difference in each height point from the last and 
            // scale to create a larger uniform terrain
            for( int x = 0; x < terrainWidth; x++ ) {
                for( int y = 0; y < terrainHeight; y++ ) {

                    heightData[x, y] =
                        ( heightData[x, y] - previousHeight.Y ) /
                        ( previousHeight.Y - previousHeight.X ) *
                          heightScale;
                }
            }
        }

        // takes given position and determines if relative height point exists on the terrain
        public bool IsOnTerrain( Vector3 givenPosition )
        {
            // determine heightmap position in relation to given terrain point
            Vector3 relPos = givenPosition - heightmapOrigin;
            
            // determines if position is inside the bounds of the terrain
            return ( 
                relPos.X > 0.0f &&
                relPos.X < terrainWidth * terrainScale &&
                relPos.Z > 0.0f &&
                relPos.Z < terrainHeight * terrainScale );
        }

        /* uses bilinear interpolation to determine the height of a given position
         * relative to the height map data which provides a more realistic movement
         * for object/camera between triangles.
         * 
         * uses the Vector.3 Lerp() method that performs a linear interpolation
         * between two vectors
         * 
         * this function is based on example code on MSDN released under the MS-PL
         * http://xbox.create.msdn.com/en-us/education/catalog/sample/collision_3d_heightmap
         * 
         *  BUG - sometimes throws an out of range exception when given a height point outside
         *  of heightdata array.  this is minimalized by keeping camera/object on terrain but still
         *  happens occasionally along borders
         */
        public float GetHeight( Vector3 position )
        {
            int left, top;
            float heightBot;
            float heightTop;
            Vector3 normal = new Vector3();

            // determine heightmap position in relation to given terrain point
            Vector3 positionOnHeightmap = position - heightmapOrigin;

            /* determine where the relative position is in heightmap data
             * using division with an integer result to get the indices of
             * the upper left 4 corners
             */
            left = (int)( positionOnHeightmap.X / terrainScale );
            top  = (int)( positionOnHeightmap.Z / terrainScale );

            // determine distance away from the upper left corner from 0 to terrainScale
            // then normalized from 0 to 1
            normal.X = ( positionOnHeightmap.X % terrainScale ) / terrainScale;
            normal.Z = ( positionOnHeightmap.Z % terrainScale ) / terrainScale;

            /* use bilinear interpolation to find height using heights from bottom
             * and top of cell and interpolating from the left and right sides
             * 
             * uses the Lerp() method which uses the formula value1 + (value2 - value1) * amount
             * and returns a linear interpolation float value
             * 
             * try to make sure this is ONLY ever passed a point that actually exists
             * within the heightdata array otherwise an exception will be thrown
             */
            heightTop = MathHelper.Lerp(
                heightData[left, top],
                heightData[left + 1, top],
                normal.X);

            heightBot = MathHelper.Lerp(
                heightData[left, top + 1],
                heightData[left + 1, top + 1],
                normal.X );

            // return the linear interpolation between top and bottom at position
            return MathHelper.Lerp( heightTop, heightBot, normal.Z );
        }
    }
}

