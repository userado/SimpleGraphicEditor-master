using System.Drawing;

namespace PFEditor
{
    /// <summary>
    /// Базов клас за форми, които се описват с две точки
    /// (Правоъгълник, елипса, линия...)
    /// </summary>
    abstract class Shape
    {
        public Point FirstPoint { get; set; }

        public Point LastPoint { get; set; }

        public Shape(Point first, Point last)
        {
            this.FirstPoint = first;
            this.LastPoint = last;
        }
    }
}