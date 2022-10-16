using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace lab_graphic_2 {
	internal class Program {
		static void Main(string[] args)
		{
            BMPImage image = BMPImage.readFromFile("C:\\Users\\ElizabethNN\\source\\repos\\lab-graphic-2\\lab-graphic-2\\images\\c.bmp");
            //VariantImage variantImage = new VariantImage(image.data);
            Image timage = new Image(image.data);
            Texture texture = new Texture(timage);
            Sprite sprite = new Sprite(texture);
            RenderWindow renderWindow = new RenderWindow(new VideoMode(image.header.width, image.header.height), "Lab1");
            sprite.Scale = new Vector2f(1f, 1f);
            renderWindow.SetVerticalSyncEnabled(true);
            while (renderWindow.IsOpen) {
                renderWindow.DispatchEvents();


                renderWindow.Clear(Color.Black);
                renderWindow.Draw(sprite);

                renderWindow.Display();
            }
        }
	}
}
