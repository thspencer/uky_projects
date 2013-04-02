using Microsoft.Xna.Framework.Input;

namespace Snake
{
    // generic keyboard input handler; returns state of passed key
    public class InputManager
    {
        public static void Update()
        {
            prevKeyState = newKeyState;
            newKeyState = Keyboard.GetState();
        }

        // returns if key was pressed
        public static bool IsKeyDown( Keys key )
        {
            return newKeyState.IsKeyDown( key );
        }

        // returns if key was released
        public static bool IsKeyUp( Keys key )
        {
            return newKeyState.IsKeyUp( key );
        }

        // returns whether or not same button was up last frame and down this frame
        public static bool OnKeyDown( Keys key )
        {
            return newKeyState.IsKeyDown( key ) && prevKeyState.IsKeyUp( key );
        }

        // returns whether or not same button was down last frame and up this frame
        public static bool OnKeyUp( Keys key )
        {
            return newKeyState.IsKeyUp( key ) && prevKeyState.IsKeyDown( key );
        }

        // save key state
        private static KeyboardState newKeyState;
        private static KeyboardState prevKeyState;
    }
}
