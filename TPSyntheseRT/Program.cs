using SFML.Graphics;
using System;


namespace TPSyntheseRT
{
    class Program
    {
        static void Main(string[] args)
        {
            uint width = 100;
            uint height = 100;

            // Create a 10x10 image filled with black color
            Image image = new Image(width, height, Color.Red);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                }
            }
            image.SaveToFile("result.png");


        }
    }
}
