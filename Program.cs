using Raylib_cs;
using System.Drawing;
using System.Numerics;
using System.Security.Authentication.ExtendedProtection;
using Color = Raylib_cs.Color;

namespace splines_and_bezier
{
    struct Point
    {
        public Point(Vector2 p)
        {
            this.p = p;
            dragging = false;
        }
        public bool dragging;
        public Vector2 p;
    }
    internal class Program
    {
        static readonly Random random = new();
        static int w, h;
        static Color GetRandomColor() => new(random.Next(256), random.Next(256), random.Next(256));
        static Vector2 Lerp(Vector2 a, Vector2 b, float t) => a + (b - a) * (t);
        static void DrawBezierLinear(Vector2 p0, Vector2 p1, float step, float thick, bool connect = true)
        {
            float limit = 1.0f / step;
            List<Vector2> pts = [];
            for (float i = 0; i <= limit; i++)
            {
                float t = i * step;
                Vector2 p = Lerp(p0, p1, t);
                pts.Add(p);
                Raylib.DrawCircleV(p, thick / 2, Color.White);
            }
            if (connect)
                for (int i = 0; i < pts.Count - 1; i++)
                    Raylib.DrawLineEx(pts[i], pts[i + 1], thick, Color.White);
        }
        static void DrawBezierQuadratic(Vector2 p0, Vector2 p1, Vector2 p2, float step, float thick, bool connect = true)
        {
            float limit = 1.0f / step;
            List<Vector2> pts = [];
            for (float i = 0; i <= limit; i++)
            {
                float t = i * step;
                Vector2 p = Lerp(Lerp(p0, p1, t), Lerp(p1, p2, t), t);
                pts.Add(p);
                Raylib.DrawCircleV(p, thick / 2, Color.White);
            }
            if (connect)
                for (int i = 0; i < pts.Count - 1; i++)
                    Raylib.DrawLineEx(pts[i], pts[i + 1], thick, Color.White);
        }
        static void DrawBezierCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float step, float thick, bool connect = true)
        {
            float limit = 1.0f / step;
            List<Vector2> pts = [];
            for (float i = 0; i <= limit; i++)
            {
                float t = i * step;
                Vector2 b = Lerp(p1, p2, t);
                Vector2 p = Lerp(Lerp(Lerp(p0, p1, t), b, t), Lerp(b, Lerp(p2, p3, t), t), t);
                pts.Add(p);
                Raylib.DrawCircleV(p, thick / 2, Color.White);
            }
            if (connect)
                for (int i = 0; i < pts.Count - 1; i++)
                    Raylib.DrawLineEx(pts[i], pts[i + 1], thick, Color.White);
        }
        static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            Raylib.InitWindow(800, 600, "curves");

            float r = 10.0F;
            List<Point> pts = [];
            bool RenderLinear = false;
            bool RenderQuadratic = false;
            bool RenderCubic = false;
            while (!Raylib.WindowShouldClose())
            {
                w = Raylib.GetScreenWidth();
                h = Raylib.GetScreenHeight();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                {
                    pts.Add(new(Raylib.GetMousePosition()));
                }
                for (int i = 0; i < pts.Count; i++)
                {
                    Vector2 mousep = Raylib.GetMousePosition();
                    Point p = pts[i];
                    if (Raylib.CheckCollisionPointCircle(mousep, p.p, r) && Raylib.IsMouseButtonPressed(MouseButton.Right))
                        p.dragging = true;
                    if (Raylib.IsMouseButtonReleased(MouseButton.Right))
                        p.dragging = false;

                    if (p.dragging)
                        p.p = mousep;

                    pts[i] = p;
                }
                for (int i = 0; i < pts.Count; i++)
                {
                    Raylib.DrawCircleV(pts[i].p, r, Color.White);
                }

                if (Raylib.IsKeyPressed(KeyboardKey.L)) RenderLinear = !RenderLinear;
                if (Raylib.IsKeyPressed(KeyboardKey.Q)) RenderQuadratic = !RenderQuadratic;
                if (Raylib.IsKeyPressed(KeyboardKey.C)) RenderCubic = !RenderCubic;

                if (RenderLinear && pts.Count >= 2)
                    DrawBezierLinear(pts[0].p, pts[1].p, 0.05f, 5.0f, false);
                if (RenderQuadratic && pts.Count >= 3)
                    DrawBezierQuadratic(pts[0].p, pts[1].p, pts[2].p, 0.05f, 5.0f, false);
                if (RenderCubic && pts.Count >= 4)
                    DrawBezierCubic(pts[0].p, pts[1].p, pts[2].p, pts[3].p, 0.05f, 5.0f, false);

                if (Raylib.IsKeyPressed(KeyboardKey.R)) pts.Clear();
                Raylib.DrawFPS(0, 0);
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
            //- is there a generalization to draw any degree of the curve
            //- extend to 3D?
        }
    }
}
