using System;
using System.Collections.Generic;
using System.Drawing;

using System.Drawing.Drawing2D;
using System.Windows.Forms;
public class BoidsEnvironment : Form
{
    private Timer timer;
    private Swarm swarm;
    private Image iconRegular;
    private Image iconZombie;


    private static void Main()
    {
        Application.Run(new BoidsEnvironment());
    }
    public BoidsEnvironment()
    {
        //hard code UI
        int boundary = 640;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(boundary, boundary);
        iconRegular = CreateIcon(Brushes.Blue);
        iconZombie = CreateIcon(Brushes.Red);
        swarm = new Swarm(boundary, 20);
        timer = new Timer();
        timer.Tick += new EventHandler(this.timerTick);
        timer.Interval = 75;
        timer.Start();


    }
    protected override void OnPaint(PaintEventArgs e)
    {
        foreach(Boid boid in swarm.Boids)
        {
            float angle;
            if (boid.dx == 0) angle = 90f;
            else angle = (float)(Math.Atan(boid.dy / boid.dx) * 57.3);
            if (boid.dx < 0f) angle += 180f;
            Matrix matrix = new Matrix();
            matrix.RotateAt(angle, boid.position);
            e.Graphics.Transform = matrix;
            if (boid.zombie) e.Graphics.DrawImage(iconZombie, boid.position);
            else e.Graphics.DrawImage(iconRegular, boid.position);

        }
    }
    private static Image CreateIcon(Brush brush)
    {
        Bitmap icon = new Bitmap(16, 16);
        Graphics g =  Graphics.FromImage(icon);
        Point p1 = new Point(0, 16);
        Point p2 = new Point(16, 8);
        Point p3 = new Point(0, 0);
        Point p4 = new Point(5, 8);
        Point[] points = { p1, p2, p3, p4 };
        g.FillPolygon(brush, points);
        return icon;

    }
    private void timerTick(object sender, EventArgs e)
    {
        swarm.MoveBoids();
        Invalidate();
    }
}
public class Swarm
{
    public List<Boid> Boids = new List<Boid>();
    public Swarm(int boundary, int boidCount)
    {
        for( int i = 0; i < boidCount; i++)
        {
            Boids.Add(new Boid((i > boidCount - 4), boundary));
        }
    }
    public void MoveBoids()
    {
        foreach(Boid boid in Boids)
        {
            boid.Move(Boids);
        }
    }

}
/** 
 * Boid contains data pertaining to each boid's position,velocity, 
 * spacing relative to other boids, and type of boid etc. Also 
 * 
    **/
public class Boid
{
    
    private static Random rand = new Random();
    private static float border = 100f;
    private static float sight = 75f;
    private static float space = 30f;
    private static float speed = 12f;
    private float boundary;
    public float dx;
    public float dy;
    public bool zombie;
    public PointF position;

    public Boid(bool zombie, int boundary)
    {
        position = new PointF(rand.Next(boundary), rand.Next(boundary));
        this.boundary = boundary;
        this.zombie = zombie;
    }
    
    public void Move(List<Boid> boids)
    {
        
         if(!zombie)
         {
            Flock(boids);
         } else {
            Hunt(boids);
         }
         CheckBounds();
         CheckSpeed();
         position.X += dx;
         position.Y += dy;

    }
    private void Flock(List<Boid> boids)
    {
        foreach (Boid boid in boids)
        {
            float distance = Distance(position, boid.position);
            if (boid != this && !boid.zombie)
            {
                if (distance < space)
                {
                    //Create space (boids stay seperated)
                    dx += position.X - boid.position.X;
                    dy += position.Y - boid.position.Y;
                }
                else if (distance < sight)
                {
                    //Flock toward center
                    dx += (boid.position.X - position.X) * 0.05f;
                    dy += (boid.position.Y - position.Y) * 0.05f;
                }
                if (distance < sight)
                {
                    //Alight movement
                    dx += boid.dx * 0.5f;
                    dy += boid.dy * 0.5f;
                }
            }
            if ( boid.zombie && distance < sight)
            {
                //dodge zombies
                dx += position.X - boid.position.X;
                dy += position.Y - boid.position.X;
            }
        }
    }
    private void Hunt(List<Boid> boids)
    {
        float range = float.MaxValue;
        Boid prey = null;
         foreach(Boid boid in boids)
        {
            if (!boid.zombie)
            {
                float distance = Distance(position, boid.position);
                if (distance < sight && distance < range)
                {
                    range = distance;
                    prey = boid;
                }
            }
        }
        if (prey != null)
        {
            // Move towards closest prey.
            dx += prey.position.X - position.X;
            dy += prey.position.Y - position.Y;
        }
    }
    private static float Distance(PointF p1, PointF p2)
    {
        double val = Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);
        return (float)Math.Sqrt(val);
    }
    private void CheckBounds()
    {
        float val = boundary - border;
        if (position.X < border) dx += border - position.X;
        if (position.Y < border) dy += border - position.Y;
        if (position.X > val) dx += val - position.X;
        if (position.Y > val) dy += val - position.Y;
    }
    private void CheckSpeed()
    {
        float s;
        //Set speed according to type of Boid
        if (!zombie)
        {

            s = speed;
        }else
        {
            s = speed / 4f;
        }
        //calculate magnitude of velocity vector
        float mag = Distance(new PointF(0f, 0f), new PointF(dx, dy));
        if (mag > s)
        {
            dx = dx * s / mag;
            dy = dy * s / mag;
        }
    }
    


}

