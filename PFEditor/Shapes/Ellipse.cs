using System;
using System.Drawing;

namespace PFEditor
{
    //Химикал, четка - използвани при чертеж
    class PFEllipse : Shape, IDrawable
    {
        public Pen Pen { get; set; }

        public Brush Brush { get; set; }

        public PFEllipse(Point first, Point last) : base(first, last) { }

        public void Draw(Graphics graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics", "Грешка при чертане. Няма работен лист");
            }
            if (this.Pen == null || this.Brush == null)
            {
                throw new ShapeException("Грешка при чертане. Не са избрани четка или химикал.");
            }

            Rectangle rect = new Rectangle(this.FirstPoint.X, this.FirstPoint.Y, this.LastPoint.X - this.FirstPoint.X, this.LastPoint.Y - this.FirstPoint.Y);
            graphics.FillEllipse(this.Brush, rect);
            graphics.DrawEllipse(this.Pen, rect);
        }
    }
}