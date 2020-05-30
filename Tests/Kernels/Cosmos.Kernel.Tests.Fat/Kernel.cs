using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using ColorDepth = Cosmos.System.Graphics.ColorDepth;
using Sys = Cosmos.System;
//using VGATest.GUI;
using Point = Cosmos.System.Graphics.Point;

namespace VGATest
{
    public class Kernel : Sys.Kernel
    {
        public static Canvas canvas;
        Pen pen;
        protected override void BeforeRun()
        {
            pen = new Pen(Color.Red);
            canvas = new VGACanvas(new Mode(720, 480, ColorDepth.ColorDepth4));
            canvas.Clear(Color.GhostWhite);
            Sys.MouseManager.ScreenWidth = (uint)canvas.Mode.Columns;
            Sys.MouseManager.ScreenHeight = (uint)canvas.Mode.Rows;
        }

        protected override void Run()
        {
            try
            {

                uint X = Sys.MouseManager.X;
                uint Y = Sys.MouseManager.Y;
                // canvas.DrawFilledRectangle(pen, (int)X, (int)Y,7,7);
                /* pen.Color = Color.GhostWhite;
                 canvas.DrawFilledRectangle(pen,(int)X, (int)Y, 40, 40);
                 pen.Color = Color.Red;
                 Point cur = new Point((int)Sys.MouseManager.X, (int)Sys.MouseManager.Y);
                 canvas.DrawLine(pen, cur, new Point(cur.X, cur.Y + 14));
                 canvas.DrawLine(pen, new Point(cur.X + 1, cur.Y + 1), new Point(cur.X + 10, cur.Y + 10));
                 canvas.DrawLine(pen, new Point(cur.X + 6, cur.Y + 10), new Point(cur.X + 10, cur.Y + 10));
                 canvas.DrawLine(pen, new Point(cur.X + 1, cur.Y + 13), new Point(cur.X + 3, cur.Y + 11));
                 canvas.DrawLine(pen, new Point(cur.X + 4, cur.Y + 12), new Point(cur.X + 6, cur.Y + 17));
                 canvas.DrawPoint(pen, new Point(cur.X + 6, cur.Y + 11));
                 canvas.DrawLine(pen, new Point(cur.X + 7, cur.Y + 12), new Point(cur.X + 9, cur.Y + 17));
                 canvas.DrawLine(pen, new Point(cur.X + 7, cur.Y + 18), new Point(cur.X + 8, cur.Y + 18));*/


                // FontDrawer.WriteText("VGA Canvas", 50, 50, pen);
                //   canvas.DrawFilledRectangle(pen, 150, 50, 320, 6);
                Console.ReadKey();
                canvas.Disable();
                PCScreenFont screenFont = new PCScreenFont();
                VGAScreen.SetFont(screenFont.CreateVGAFont(), screenFont.CharHeight);
                Console.Clear();
                Console.WriteLine("test");
                Console.ReadLine();

            }
            catch
            {

            }
        }
    }
}
