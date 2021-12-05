using System;
using System.Drawing;

/*
* Този файл декларира типове и класове за работа със събитията в прозореца на редактора
* и предавa параметри към платното (MyCanvas) и обратно към формата
*/

namespace PFEditor
{
    //декларация за събития в прозореца на редактора

    public delegate void ToolChanged(object sender, ToolChangeEventArgs e);
    public delegate void ColorChanged(object sender, ColorChangeEventArgs e);

    /// <summary>
    /// декларация за събития при промяна на инструмента за чертане
    /// </summary>
    public class ToolChangeEventArgs : EventArgs
    {
        public DrawingTools Tool { get; private set; }
        public ToolChangeEventArgs(DrawingTools tool)
        {
            this.Tool = tool;
        }
    }

    /// <summary>
    /// Декларация за събития при смяна на използвания цвят
    /// </summary>
    public class ColorChangeEventArgs : EventArgs
    {
        public Color Color { get; private set; }
        public ColorChangeEventArgs(Color foreColor)
        {
            this.Color = foreColor;
        }
    }

    /// <summary>
    ///Декларация за процеса на инвертиране
    /// </summary>
    public class InverseProgressChangedEventArgs : EventArgs
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public int Progress { get; set; }
    }
}