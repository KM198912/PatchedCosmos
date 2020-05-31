using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cosmos.System.Graphics
{
    public class BasicBufferScreen : Canvas
    {

        private Canvas Backend;

        private Color[] Buffer;
        public BasicBufferScreen(Canvas backend,Color color)
        {
            backend.Clear(color);
            Backend = backend;
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
                Clear();
            }
        }




        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            Backend.DrawArray(colors, x, y, width, height);
        }

        public override void DrawPoint(Pen pen, int x, int y)
        {
            Buffer[(y * Backend.Mode.Columns) + x] = pen.Color;
        }

        public override void DrawPoint(Pen pen, float x, float y)
        {
            DrawPoint(pen, (int)x, (int)y);
        }

        public override Color GetPointColor(int x, int y)
        {
            return Buffer[(y * Backend.Mode.Rows) + x];
        }
        public void Clear(Color color,bool buffered = false)
        {
            if (buffered)
            {
                Pen pen = new Pen(color);
                BufferClear(0, 0, Backend.Mode.Rows, Backend.Mode.Columns, pen);
            }
            else
            {
                Clear(color);
            }
        }

        public void BufferClear(uint x0, uint y0, int Width, int Height, Pen pen)
        {
            for (uint i = 0; i < Width; i++)
            {
                for (uint h = 0; h < Height; h++)
                {
                    DrawPoint(pen,(int)(x0 + i), (int)(y0 + h));
                }
            }
        }
        public override void Disable()
        {
            Disable();
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
