using System;
using System.Drawing;

namespace PFEditor
{
    //Химикал - използвани при чертеж
    class PFLine : Shape, IDrawable
    {
        public Pen Pen { get; set; }

        public PFLine(Point first, Point last) : base(first, last) { }

        public void Draw(Graphics graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics", "Грешка при чертане. Няма работен лист");
            }
            if (this.Pen == null)
            {
                throw new ShapeException("Грешка при чертане. Не е избран химикал");
            }

            graphics.DrawLine(this.Pen, this.FirstPoint, this.LastPoint);
        }
    }
}