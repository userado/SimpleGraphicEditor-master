using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Threading;

namespace PFEditor
{
    /// <summary>
    ///  Обработка на рисунката. Съхранения / създаване на нова рисунка.
    ///  Изображението се обработва и съхранява като растерна карта
    /// </summary>
    class MyCanvas : Panel
    {
        #region FIELDS

        private BufferedGraphics _drawingBufferedGraphics;  //Графичен буфер (предотвратяване трептене при чертане)
        private BufferedGraphics _imageBufferedGraphics;    //Графичен буфер за съхранение на изображения
        private bool _isDrawing;                            //Рисуване
        private Point _clickPoint;                          //Начална точка на рисуване (координати на чертане)
        private Pen _currentPen;                            //Химикал,  (очертания на формата)
        private Brush _currentBrush;                        //Четка, очертания
        private IDrawable _currentShape;                    //Текуща фигура
        private DrawingTools _currentTool;                  //Текущ използван инструмент
        private List<Point> _movementPoints;                //Данни за движението на мишката/курсора

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Връща текущото изображение като обект на Bitmap
        /// </summary>
        public Bitmap Drawing
        {
            get
            {
                try
                { 
                    Bitmap currentDrawing = null;
                    using (Graphics canvasGraphics = this.CreateGraphics())
                    {
                        currentDrawing = new Bitmap(this.Width, this.Height, canvasGraphics);
                    }
                    using (Graphics bitmapGraphics = Graphics.FromImage(currentDrawing))
                    {
                        this._imageBufferedGraphics.Render(bitmapGraphics);
                    }
                    return currentDrawing;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Грешка при създаване на Bitmap\n{0}", ex.Message);
                    throw new MyCanvasException("Не може да се създаде Bitmap");
                }
            }
        }

        /// <summary>
        /// Връща подготвена графика от графичен буфер за плъзгащо се чертане
        /// </summary>
        private Graphics DrawingGraphics
        {
            get
            {
                Graphics graphicsFromBufferedGraphics = this._drawingBufferedGraphics.Graphics;
                graphicsFromBufferedGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphicsFromBufferedGraphics.Clear(Color.White);
                this._imageBufferedGraphics.Render(graphicsFromBufferedGraphics);
                return graphicsFromBufferedGraphics;
            }
        }

        #endregion

        #region EVENTS

        //Използване на стандартни делегати, като се има предвид ковариация / контравариантност

        public event EventHandler DrawingChanged;                       //Задейства се, когато моделът се промени
        public event EventHandler MousePositionCoordChanged;            //Задейства се при промяна на координатите на курсора върху платното
        public event EventHandler InverseProressChanged;                //Задейства се по време на инвертна операция
        public event EventHandler BeginInverse;                         //Задейства се в началото на инвертна операция
        public event EventHandler EndInverse;                           //Задейства се в края на инвертна операция

        #endregion

        #region CTORS

        private MyCanvas()
        {
            //Коллекции

            this._movementPoints = new List<Point>();

            //Параметры контрола

            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            this.Location = new Point(5, 5);
            this.Cursor = Cursors.Cross;
        }

        public MyCanvas(int width, int height) : this()
        {
            if (Program.MIN_DRAWING_SIZE > width || width >= Program.MAX_DRAWING_SIZE 
                || Program.MIN_DRAWING_SIZE > height || height >= Program.MAX_DRAWING_SIZE)
            {
                string message = String.Format("Невалиден размер. Позволен обхват: {0} ... {1}",
                    Program.MIN_DRAWING_SIZE, Program.MAX_DRAWING_SIZE);
                throw new MyCanvasException(message);
            }
            this.Width = width;
            this.Height = height;

            this.InitBuffering();
        }

        public MyCanvas(Image image) : this(image.Width, image.Height)
        {
            if (image == null)
            {
                throw new MyCanvasException("Не може да се създаде. Няма снимка.");
            }

            //Кеширано изображение в Buffered Graphics

            this._imageBufferedGraphics.Graphics.DrawImage(image, new Point(0, 0));
        }

        #endregion

        #region OVERRIDES METHOD (DRAWING, DISPOSING)

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                return;
            }

            //Начало рисования

            this._isDrawing = true;
            this._clickPoint = e.Location;

            //Създаване на текущия обект за изтегляне
            //В обектните класове се създават копия на инструментите, с които са нарисувани

            switch (this._currentTool)
            {
                case DrawingTools.PEN:
                    this._movementPoints.Add(e.Location);
                    this._currentShape = new PFPen(this._movementPoints) { Pen = this._currentPen };
                    break;
                case DrawingTools.LINE:
                    this._currentShape = new PFLine(Point.Empty, Point.Empty) { Pen = this._currentPen };
                    break;
                case DrawingTools.RECTANGLE:
                    this._currentShape = new PFRectangle(Point.Empty, Point.Empty) { Pen = this._currentPen, Brush = this._currentBrush };
                    break;
                case DrawingTools.ELLIPSE:
                    this._currentShape = new PFEllipse(Point.Empty, Point.Empty) { Pen = this._currentPen, Brush = this._currentBrush };
                    break;
                default:
                    throw new MyCanvasException("Непозат чертожен инструмент");
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //Инициализиране на събитие за промяна на позицията на курсора върху платното

            if (this.MousePositionCoordChanged != null)
            {
                this.MousePositionCoordChanged(this, e);
            }

            if (this._isDrawing == false)
            {
                return;
            }

            Graphics drawingGraphics = this.DrawingGraphics;

            //Текуща форма (плъзгащ се чертеж)

            switch (this._currentTool)
            {
                case DrawingTools.PEN:
                    this._movementPoints.Add(e.Location);
                    break;
                case DrawingTools.LINE:
                    ((Shape)this._currentShape).FirstPoint = this._clickPoint;
                    ((Shape)this._currentShape).LastPoint = e.Location;
                    break;
                case DrawingTools.RECTANGLE:
                case DrawingTools.ELLIPSE:

                    //Нормализиране на координатите на формата

                    Point startPoint = new Point(this._clickPoint.X < e.X ? this._clickPoint.X : e.X, this._clickPoint.Y < e.Y ? this._clickPoint.Y : e.Y);
                    Point endPoint = new Point(this._clickPoint.X >= e.X ? this._clickPoint.X : e.X, this._clickPoint.Y >= e.Y ? this._clickPoint.Y : e.Y);
                    ((Shape)this._currentShape).FirstPoint = startPoint;
                    ((Shape)this._currentShape).LastPoint = endPoint;
                    break;
                default:
                    throw new MyCanvasException("Непозат чертожен инструмент");
            }
            this._currentShape.Draw(drawingGraphics);

            //Буфер за изобразяване върху платното

            using (Graphics panelGraphics = this.CreateGraphics())
            { 
                this._drawingBufferedGraphics.Render(panelGraphics);
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this._isDrawing == false)
            {
                return;
            }

            //Изобразяване на фигура в клипборда

            this._currentShape.Draw(this._imageBufferedGraphics.Graphics);
            this._currentShape = null;
            this._isDrawing = false;

            if(this._currentTool == DrawingTools.PEN)
            { 
                this._movementPoints.Clear();
            }

            //Инициираме събитие за промяна на картината

            if (this.DrawingChanged != null)
            {
                this.DrawingChanged(this, new EventArgs());
            }

            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //Възстановяване на чертеж при преоразмеряване на платното

            this._imageBufferedGraphics.Render(e.Graphics);
            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            //Освобождаване на ресурсите (за всеки случай ;), GC и OS)

            if (this._drawingBufferedGraphics != null)
            { 
                this._drawingBufferedGraphics.Dispose();
                this._drawingBufferedGraphics = null;
            }
            if (this._imageBufferedGraphics != null)
            {
                this._imageBufferedGraphics.Dispose();
                this._imageBufferedGraphics = null;
            }
            if (this._currentBrush != null)
            { 
                this._currentBrush.Dispose();
                this._currentBrush = null;
            }
            if (this._currentPen != null)
            { 
                this._currentPen.Dispose();
                this._currentPen = null;
            }
            this._currentShape = null;
            this._movementPoints.Clear();
            this._movementPoints = null;

            base.Dispose(disposing);
        }

        #endregion

        #region EVENT HANDLERS (MAINFORM)

        public void OnToolChanged(object sender, ToolChangeEventArgs e)
        {
            this._currentTool = e.Tool;
        }

        public void OnForeColorChanged(object sender, ColorChangeEventArgs e)
        {
            if (this._currentPen != null)
            { 
                this._currentPen.Dispose();
            }
            this._currentPen = new Pen(e.Color, Program.DEFAULT_PEN_WIDTH);
        }

        public void OnBackColorChanged(object sender, ColorChangeEventArgs e)
        {
            if (this._currentBrush != null)
            { 
               this._currentBrush.Dispose();
            }
            this._currentBrush = new SolidBrush(e.Color);
        }

        #endregion

        #region HELPERS

        /// <summary>
        /// Създава и стартира инвертиран поток от изображения
        /// </summary>
        public void InverseDrawing()
        {
            new Thread(this.Inverse) { IsBackground = true }.Start();
        }

        /// <summary>
        /// Инвертира текущото изображение
        /// Изпълнява се на работна нишка
        /// </summary>
        private void Inverse()
        {
            //Инициализираме събитията от началото на операцията инвертиране

            if (this.BeginInverse != null)
            {
                this.BeginInverse(this, new EventArgs());
            }

            try
            {
                //Деактивиране възможността за рисуване по време на работа

                this.Invoke(new Action(() => this.Enabled = false));

                //Получаваме и обръщаме текущото изображение във формата Bitmap
                //(неефективен бавен метод, по време на операцията)

                using (Bitmap bitmap = this.Drawing)
                {
                    this.InverseBitmap(bitmap);

                    //Кешираме полученото изображение в буфера за изображения

                    this._imageBufferedGraphics.Graphics.DrawImage(bitmap, new Point(0, 0));
                }

                //Преначертаваме платното

                this.Invalidate();

                //Инициализирайте събитие за промяна на изображението

                if (this.DrawingChanged != null)
                {
                    this.DrawingChanged(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Грешка при инвертиране на изображение\n{0}", ex.Message);
            }
            finally
            {
                //Активиране възможността за рисуване

                this.Invoke(new Action(() => this.Enabled = true));

                // Инициализираме събитията от края на операцията инвертиране

                if (this.EndInverse != null)
                {
                    this.EndInverse(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Инвертиране на битмап
        /// </summary>
        /// <param name="bitmap">Инвертиране на битмап</param>
        private void InverseBitmap(Bitmap bitmap)
        {
            for (int row = 0; row < bitmap.Height; row++)
            {
                for (int col = 0; col < bitmap.Width; col++)
                {
                    Color currentClr = bitmap.GetPixel(col, row);
                    Color inverseClr = Color.FromArgb(255, 255 - currentClr.R, 255 - currentClr.G, 255 - currentClr.B);
                    bitmap.SetPixel(col, row, inverseClr);

                    //Инициализираме събитието за промяна на хода на инверсията

                    if (this.InverseProressChanged != null)
                    {
                        InverseProgressChangedEventArgs e = new InverseProgressChangedEventArgs()
                        {
                            Min = 0,
                            Max = bitmap.Width * bitmap.Height,
                            Progress = row * bitmap.Width + col
                        };
                        this.InverseProressChanged(this, e);
                    }
                }
            }
        }

        /// <summary>
        /// Създава и инициализира графични буфери
        /// Буфер за изображения и буфер за рисуване на слайдове
        /// </summary>
        private void InitBuffering()
        {
            using (Graphics canvasGraphics = this.CreateGraphics())
            {
                Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
                this._drawingBufferedGraphics = BufferedGraphicsManager.Current.Allocate(canvasGraphics, rect);
                this._imageBufferedGraphics = BufferedGraphicsManager.Current.Allocate(canvasGraphics, rect);
                this._imageBufferedGraphics.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                this._imageBufferedGraphics.Graphics.Clear(Color.White);
            }
        }
      
        #endregion
    }
}