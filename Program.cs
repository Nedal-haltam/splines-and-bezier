using Raylib_cs;
using System.Drawing;
using System.Numerics;
using System.Security.Authentication.ExtendedProtection;
using Color = Raylib_cs.Color;

namespace splines_and_bezier
{
    struct Points
    {
        public Points()
        {
            draggings = [];
            ps = [];
        }
        public List<bool> draggings;
        public List<Vector2> ps;
    }
    internal class Program
    {
        static float r = 10.0F;
        static Points pts = new();
        static bool RenderQuadratic = false;
        static bool RenderCubic = false;
        static bool RenderTangentLines = false;
        static readonly Random random = new();
        static int w, h;
        static Color GetRandomColor() => new(random.Next(256), random.Next(256), random.Next(256));
        static Vector2 Lerp(Vector2 a, Vector2 b, float t) => a + (b - a) * (t);
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
        static Vector2 DeCasteljau(List<Vector2> points, int n, float t)
        {
            List<Vector2> temp = new(points);
            if (points.Count == 0) return new();
            for (int r = 1; r <= n; r++)
            {
                for (int i = 0; i <= n - r; i++)
                    temp[i] = (1 - t) * temp[i] + t * temp[i + 1];
            }
            return temp[0];
        }
        static void DrawBezierCurve_DeCasteljausAlgo(List<Vector2> ps, int n, float step, float thick)
        {
            Shartilities.Assert(ps.Count == n + 1, $"need {n + 1} points to draw a bezier curve of degree {n}");
            for (float t = 0; t <= 1; t += step)
            {
                Vector2 p = DeCasteljau(ps, n, t);
                Raylib.DrawCircleV(p, thick / 2, Color.White);
            }
        }
        static void Render()
        {
            for (int i = 0; i < pts.ps.Count; i++)
            {
                Raylib.DrawCircleV(pts.ps[i], r, Color.White);
            }

            float step = 0.01f;
            float thick = 5.0f;
            DrawBezierCurve_DeCasteljausAlgo(pts.ps, pts.ps.Count - 1, step, thick);
        }
        static void Settings()
        {
            for (int i = 0; i < pts.ps.Count; i++)
            {
                Vector2 mousep = Raylib.GetMousePosition();
                Vector2 p = pts.ps[i];
                if (Raylib.CheckCollisionPointCircle(mousep, p, r) && Raylib.IsMouseButtonPressed(MouseButton.Right))
                    pts.draggings[i] = true;
                if (Raylib.IsMouseButtonReleased(MouseButton.Right))
                    pts.draggings[i] = false;

                if (pts.draggings[i])
                    p = mousep;

                pts.ps[i] = p;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.Q)) RenderQuadratic = !RenderQuadratic;
            if (Raylib.IsKeyPressed(KeyboardKey.C)) RenderCubic = !RenderCubic;
            if (Raylib.IsKeyPressed(KeyboardKey.T)) RenderTangentLines = !RenderTangentLines;
            if (Raylib.IsKeyPressed(KeyboardKey.R)) pts.ps.Clear();
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                pts.ps.Add(Raylib.GetMousePosition());
                pts.draggings.Add(false);
            }
        }
        static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            Raylib.InitWindow(800, 600, "curves");

            while (!Raylib.WindowShouldClose())
            {
                w = Raylib.GetScreenWidth();
                h = Raylib.GetScreenHeight();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                Render();
                Settings();

                Raylib.DrawFPS(0, 0);
                Raylib.EndDrawing();
            }
            Raylib.CloseWindow();
        }
    }
}
