using System.Drawing;

namespace PFEditor
{
    /// <summary>
    /// Интерфейс за обекти, които трябва да могат да се изобразяват в графика
    /// </summary>
    interface IDrawable
    {
        void Draw(Graphics graphics);
    }
}