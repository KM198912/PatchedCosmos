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
            try
            {

                    Backend = FullScreenCanvas.GetFullScreenCanvas(mode);

                Buffer = new Color[Backend.Mode.Columns * Backend.Mode.Rows];
                Global.mDebugger.Send("DEBUG Rows: " + Backend.Mode.Rows + " Columns: " + Backend.Mode.Columns + " Color: " + Buffer.ToString());
            }
            catch(Exception e)
            {
                Global.mDebugger.Send("CGS Crash: " + e.Message);
                throw new Exception(e.Message);
            }
            }
        public BufferedCanvas()
        {
            try
            {
                Backend = FullScreenCanvas.GetFullScreenCanvas();
                Buffer = new Color[Backend.Mode.Columns * Backend.Mode.Rows];
            }
            catch (Exception e)
            {
                Global.mDebugger.Send("CGS Crash: " + e.Message);
                throw new Exception(e.Message);
            }
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


        public override void DrawRectangle(Pen pen, float x_start, float y_start, float width, float height)
        {
            Backend.DrawRectangle(pen, x_start, y_start, width, height);
        }

        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {


            Backend.DrawArray(colors, x, y, width, height);

        }

        public override void DrawPoint(Pen pen, int x, int y)
        {

            Buffer[(y * Backend.Mode.Rows) + x] = pen.Color;

        }

        public override void DrawPoint(Pen pen, float x, float y)
        {

            DrawPoint(pen, (int)x, (int)y);

        }

        public override Color GetPointColor(int x, int y)
        {

            return Buffer[(y * Backend.Mode.Rows) + x];
       
        }
        public void Clear(Color? color = null)
        {
            try
            {

                Color DefaultColor = color ?? Color.Black;
                Backend.Clear(DefaultColor);
                
            }
            catch(Exception e)
            {
                Global.mDebugger.Send("Crashed while clearing screen: " + e.Message);
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
