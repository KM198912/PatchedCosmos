using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cosmos.System.Graphics
{
    public class BufferedCanvas : Canvas
    {

        private Canvas Backend;

        private Color[] Buffer;

        public BufferedCanvas(Mode mode)
        {

            Backend = FullScreenCanvas.GetFullScreenCanvas(mode);
            Buffer = new Color[Backend.Mode.Columns * Backend.Mode.Rows];
            Global.mDebugger.Send("DEBUG Rows: " + Backend.Mode.Rows + " Columns: " + Backend.Mode.Columns + " Color: "+Buffer.ToString());
        }
        public BufferedCanvas()
        {
            Backend = FullScreenCanvas.GetFullScreenCanvas();
            Buffer = new Color[Backend.Mode.Columns * Backend.Mode.Rows];
        }


        public override List<Mode> AvailableModes => Backend.AvailableModes;
        
        public override Mode DefaultGraphicMode => Backend.DefaultGraphicMode;

        public override Mode Mode
        {
            get => Backend.Mode;
            set
            {
                Backend.Mode = value;
                Buffer = new Color[Backend.Mode.Columns * Backend.Mode.Rows];
                Global.mDebugger.Send("DEBUG Rows: " + Backend.Mode.Rows + " Columns: " + Backend.Mode.Columns);
                Backend.Clear();
            }
        }




        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            Global.mDebugger.Send("4545456456456");

            Backend.DrawArray(colors, x, y, width, height);
            Global.mDebugger.Send("yfgfgfdx");
        }

        public override void DrawPoint(Pen pen, int x, int y)
        {
            Global.mDebugger.Send("gruhjztjzuf");
            Buffer[(y * Backend.Mode.Rows) + x] = pen.Color;
            Global.mDebugger.Send("67567uhgv");
        }

        public override void DrawPoint(Pen pen, float x, float y)
        {
            Global.mDebugger.Send("654e654e");
            DrawPoint(pen, (int)x, (int)y);
            Global.mDebugger.Send("7567567u56");
        }

        public override Color GetPointColor(int x, int y)
        {
            Global.mDebugger.Send("5654654");
            return Buffer[(y * Backend.Mode.Rows) + x];
       
        }
        public void ClearBuf(Color color,bool buffered = false)
        {
            try
            {
                Global.mDebugger.Send("Clearing the Screen with " + color.ToString());
                Backend.Clear(color);
            }
            catch(Exception e)
            {
                Global.mDebugger.Send("Crashed while clearing screen: " + e.Message);
            }
        }

        public void BufferClear(uint x0, uint y0, int Width, int Height, Pen pen)
        {
            Global.mDebugger.Send("546754654654hgjghvxchjztg");
            for (uint i = 0; i < Width; i++)
            {
                Global.mDebugger.Send("546754654654lkfglöhk löklöäk öl");
                for (uint h = 0; h < Height; h++)
                {
                    Global.mDebugger.Send("54675465465454654654");
                    DrawPoint(pen,(int)(x0 + i), (int)(y0 + h));
                    Global.mDebugger.Send("54675465465460ß5496ß079gnfß0h9gfß09jß0g");
                }
            }
        }
        public override void Disable()
        {
            Backend.Disable();
        }
        public void Render()
        {
            for (int y = 0; y < Backend.Mode.Rows; y++)
            {
                for (int x = 0; x < Backend.Mode.Columns; x++)
                {
                    if (GetPointColor(x, y) != Buffer[(y * Backend.Mode.Rows) + x])
                    {
                        Backend.DrawPoint(new Pen(Buffer[(y * Backend.Mode.Rows) + x]), x, y);
                    }
                }
            }
        }
    }
}
