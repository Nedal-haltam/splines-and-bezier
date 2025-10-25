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
        static void DrawBezierLinearLerp(Vector2 p0, Vector2 p1, float step, float thick)
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
        }
        static void DrawBezierQuadraticLerp(Vector2 p0, Vector2 p1, Vector2 p2, float step, float thick)
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
        }
        static void DrawBezierCubicLerp(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float step, float thick)
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
        }
        static void DrawBezierLinearExpr(Vector2 p0, Vector2 p1, float step, float thick, bool DrawTangent)
        {
            // P = p0 +
            // t * (p1 - p0)
            Vector2 a = p1 - p0;
            for (float t = 0; t <= 1; t += step)
            {
                Vector2 p = p0 + t * a;
                Raylib.DrawCircleV(p, thick / 2, Color.White);
            }
        }
        static void DrawBezierQuadraticExpr(Vector2 p0, Vector2 p1, Vector2 p2, float step, float thick, bool DrawTangent)
        {
            // P = p0 +
            // t   * (2p1 - 2p0) +
            // t^2 * (p2 - 2p1 + p0) +
            Vector2 a = 2 * (p1 - p0);
            Vector2 b = p2 - 2 * p1 + p0;
            for (float t = 0; t <= 1; t += step)
            {
                Vector2 p = p0 + t * a + MathF.Pow(t, 2) * b;
                Raylib.DrawCircleV(p, thick / 2, Color.White);
            }
            if (DrawTangent)
            {
                Raylib.DrawLineV(p0, p1, Color.White);
                Raylib.DrawLineV(p1, p2, Color.White);
            }
        }
        static void DrawBezierCubicExpr(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float step, float thick, bool DrawTangent)
        {
            // P = p0 +
            // t   * (-3p0 + 3p1) +
            // t^2 * (3p0 - 6p1 + 3p2) +
            // t^3 * (-p0 + 3p1 - 3p2 + p3)
            Vector2 a = 3 * (p1 - p0);
            Vector2 b = 3 * (p0 - 2 * p1 + p2);
            Vector2 c = p3 - p0 + 3 * p1 - 3 * p2;
            for (float t = 0; t <= 1; t += step)
            {
                Vector2 p = p0 + t * a + MathF.Pow(t, 2) * b + MathF.Pow(t, 3) * c;
                Raylib.DrawCircleV(p, thick / 2, Color.White);
            }
            if (DrawTangent)
            {
                Raylib.DrawLineV(p0, p1, Color.White);
                Raylib.DrawLineV(p2, p3, Color.White);
            }
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
            bool RenderTangentLines = false;
            
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
                if (Raylib.IsKeyPressed(KeyboardKey.T)) RenderTangentLines = !RenderTangentLines;


                float step = 0.01f;
                if (RenderLinear && pts.Count >= 2)
                    DrawBezierLinearExpr(pts[0].p, pts[1].p, step, 5.0f, RenderTangentLines);
                if (RenderQuadratic && pts.Count >= 3)
                    DrawBezierQuadraticExpr(pts[0].p, pts[1].p, pts[2].p, step, 5.0f, RenderTangentLines);
                if (RenderCubic && pts.Count >= 4)
                    DrawBezierCubicExpr(pts[0].p, pts[1].p, pts[2].p, pts[3].p, step, 5.0f, RenderTangentLines);

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
