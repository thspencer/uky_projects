using System;

namespace Snake
{
#if WINDOWS || XBOX
    static class Program
    {
        static void Main( string[] args )
        {
            using ( SnakeGame snake = new SnakeGame() )
            {
                snake.Run();
            }
        }
    }
#endif
}

