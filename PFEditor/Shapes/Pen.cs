using System;
using System.Collections.Generic;
using System.Drawing;

namespace PFEditor
{
    //Pen - управляются холстом
    class PFPen : IDrawable
    {
        public List<Point> Points { get; set; }

        public Pen Pen { get; set; }

        public PFPen(List<Point> pointsList)
        {
            if (pointsList == null)
            {
                throw new ArgumentNullException("pointsList", "Грешка при чертане. Няма работен лист");
            }
            this.Points = pointsList;
        }

        public void Draw(Graphics graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics", "Грешка при чертане. Няма работен лист");
            }
            if (this.Pen == null)
            {
                throw new ShapeException("Грешка при чертане. Няма избран химикал");
            }
            if (this.Points == null)
            {
                throw new ShapeException("Грешка при чертане.");
            }

            //Свързва свободните чертожни точки с прави линии

            for (int i = 0; i < this.Points.Count - 1; i++)
            {
                Point p1 = this.Points[i];
                Point p2 = this.Points[i + 1];
                graphics.DrawLine(this.Pen, p1, p2);
            }
        }
    }
}