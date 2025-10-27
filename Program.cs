using Raylib_cs;
using System.Drawing;
using System.Numerics;
using System.Security.Authentication.ExtendedProtection;
using Color = Raylib_cs.Color;

namespace splines_and_bezier
{
    struct Points<T>
    {
        public Points()
        {
            draggings = [];
            ps = [];
        }
        public void Add(T p, bool d)
        {
            ps.Add(p);
            draggings.Add(d);
        }
        public List<bool> draggings;
        public List<T> ps;
    }
    public struct Camera
    {
        public Camera3D Camera3D;
        public float CameraSpeed;
        public float ZoomSpeed;
        public float RotationX;
        public float RotationY;
        public float Sensitivity;
        public readonly void UpdateRotations()
        {
            Rlgl.Translatef(0.0f, 0.0f, 0.0f);
            Rlgl.Rotatef(RotationX, 1.0f, 0.0f, 0.0f);
            Rlgl.Rotatef(RotationY, 0.0f, 1.0f, 0.0f);
        }
        public void UpdateSettings()
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                Vector2 md = Raylib.GetMouseDelta();
                RotationX -= md.Y * Sensitivity;
                RotationY += md.X * Sensitivity;
            }
            Camera3D.Position.Z += ZoomSpeed * Raylib.GetMouseWheelMove() * Raylib.GetFrameTime();
        }
    }
    internal class Program
    {
        static float r = 10.0F;
        static Points<Vector2> pts = new();
        static Points<Vector3> pts3d = new();
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
        static void Render2D()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            for (int i = 0; i < pts.ps.Count; i++)
            {
                Raylib.DrawCircleV(pts.ps[i], r, Color.White);
            }
            float step = 0.01f;
            float thick = 5.0f;
            DrawBezierCurve_DeCasteljausAlgo(pts.ps, pts.ps.Count - 1, step, thick);
            Raylib.DrawFPS(0, 0);
            Raylib.EndDrawing();
        }
        static void Settings2D()
        {
            Vector2 mousep = Raylib.GetMousePosition();
            for (int i = 0; i < pts.ps.Count; i++)
            {
                Vector2 p = pts.ps[i];
                if (Raylib.CheckCollisionPointCircle(mousep, p, r) && Raylib.IsMouseButtonPressed(MouseButton.Right))
                    pts.draggings[i] = true;
                if (Raylib.IsMouseButtonReleased(MouseButton.Right))
                    pts.draggings[i] = false;

                if (pts.draggings[i])
                    p = mousep;

                pts.ps[i] = p;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.R)) pts.ps.Clear();
            if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            {
                pts.Add(mousep, false);
            }
        }
        ////////////////////////////////////////////////////////////////////
        static Vector3 DeCasteljau3D(List<Vector3> points, float t)
        {
            if (points.Count == 0)
                return Vector3.Zero;

            // Make a working copy so we can interpolate in place
            var temp = new List<Vector3>(points);
            int n = points.Count - 1;

            for (int r = 1; r <= n; r++)
            {
                for (int i = 0; i <= n - r; i++)
                    temp[i] = Vector3.Lerp(temp[i], temp[i + 1], t);
            }

            return temp[0];
        }

        static void DrawBezierCurve3D(Points<Vector3> controlPoints, float size, float step, Color color)
        {
            if (controlPoints.ps.Count < 2)
                return;

            Vector3 prev = controlPoints.ps[0];
            for (float t = step; t <= 1.0f + step / 2; t += step)
            {
                Vector3 cur = DeCasteljau3D(controlPoints.ps, t);
                Raylib.DrawLine3D(prev, cur, color);
                prev = cur;
            }
        }
        ////////////////////////////////////////////////////////////////////
        static float SPHERE_SIZE = 0.1f;
        static void Render3D(ref Camera camera)
        {
            camera.UpdateSettings();
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            Raylib.BeginMode3D(camera.Camera3D);
            Rlgl.PushMatrix();
            camera.UpdateRotations();


            for (int i = 0; i < pts3d.ps.Count; i++)
            {
                Raylib.DrawSphere(pts3d.ps[i], SPHERE_SIZE, Color.Red);
                //if (i < controlPoints.Count - 1)
                //    Raylib.DrawLine3D(controlPoints[i], controlPoints[i + 1], Color.Gray);
            }

            float step = 0.01f;
            DrawBezierCurve3D(pts3d, SPHERE_SIZE, step, Color.White);

            Vector3 c = new(0, 0, 0);
            float s = SPHERE_SIZE*2;
            Raylib.DrawCube(c, s, s, s, Color.DarkGray);
            Raylib.DrawCubeWires(c, s, s, s, Color.White);

            Rlgl.PopMatrix();
            Raylib.EndMode3D();
            Raylib.DrawFPS(0, 0);
            Raylib.EndDrawing();
        }
        static void Settings3D(ref Camera camera)
        {
            Vector2 mousep = Raylib.GetMousePosition();
            Ray mouseRay = Raylib.GetScreenToWorldRay(mousep, camera.Camera3D);

            for (int i = 0; i < pts3d.ps.Count; i++)
            {
                Vector3 p = pts3d.ps[i];
                float radius = SPHERE_SIZE;
                RayCollision hit = Raylib.GetRayCollisionSphere(mouseRay, p, radius);

                if (hit.Hit && Raylib.IsMouseButtonPressed(MouseButton.Right))
                    pts3d.draggings[i] = true;

                if (Raylib.IsMouseButtonReleased(MouseButton.Right))
                    pts3d.draggings[i] = false;

                if (pts3d.draggings[i])
                {

                    Plane dragPlane = new Plane { Normal = new Vector3(0, 0, 1), D = -p.Z };
                    RayCollision dragHit = Raylib.GetRayCollisionQuad(
                        mouseRay,
                        new Vector3(-100, -100, p.Z),
                        new Vector3(100, -100, p.Z),
                        new Vector3(100, 100, p.Z),
                        new Vector3(-100, 100, p.Z)
                    );

                    if (dragHit.Hit)
                        pts3d.ps[i] = dragHit.Point; // Move sphere along that plane
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.R)) pts3d.ps.Clear();
            //if (Raylib.IsMouseButtonReleased(MouseButton.Left))
            //{
            //    pts3d.ps.Add(new(mousep.X, mousep.Y, 0));
            //    pts3d.draggings.Add(false);
            //}
        }
        static void Main()
        {
            Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
            Raylib.SetTargetFPS(0);
            Raylib.InitWindow(800, 600, "curves");

            Camera camera = new()
            {
                Camera3D = new()
                {
                    Position = new Vector3(0, 0, -10),
                    Target = new Vector3(0.0f, 0.0f, 0.0f),
                    Up = new Vector3(0.0f, 1.0f, 0.0f),
                    FovY = 45.0f,
                    Projection = CameraProjection.Perspective,
                },
                CameraSpeed = 1.0f,
                ZoomSpeed = 300.0f,
                RotationX = 0,
                RotationY = 0,
                Sensitivity = 0.3f,
            };
            pts3d.Add(new(0, 0, 0), false);
            pts3d.Add(new(0, 3, 0), false);
            pts3d.Add(new(-3, 0, 0), false);

            while (!Raylib.WindowShouldClose())
            {
                w = Raylib.GetScreenWidth();
                h = Raylib.GetScreenHeight();

                //Render2D();
                //Settings2D();
                Render3D(ref camera);
                Settings3D(ref camera);
            }
            Raylib.CloseWindow();
        }
    }
}
