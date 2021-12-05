using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PFEditor
{
    public partial class frmMain : Form
    {
        #region FIELDS

        private ToolStripButton _activeToolButton;          //Активен бутон, инструмент за рисуване
        private MyCanvas _myCanvas;                         //Платно
        private Color _foreColor = Color.Black;             //Цвят на контура
        private Color _backColor = Color.White;             //Цвят на фона
        private bool _isDitry;                              //Знак за промени в картината
        private DrawingHelper _drawingHelper;               //Работа с файлове

        #endregion

        #region EVENTS

        //Събитията (делегатите) се създават самостоятелно (за разлика от платното) за промяна ;)

        private event ToolChanged DrawingToolChanged;         //Задейства се при смяна на инструмента за рисуване
        private event ColorChanged DrawingForeColorChanged;   //Задейства се при промяна на цвета на контура
        private event ColorChanged DrawingBackColorChanged;   //Задейства се, когато задният фон на фигурите се промени

        #endregion

        #region CTOR
        public frmMain()
        {
            InitializeComponent();

            //Добавяне и инициализиране на платното

            this._myCanvas = new MyCanvas(Program.DEFAULT_DRAWING_WIDTH, Program.DEFAULT_DRAWING_HEIGHT);
            this.InitCanvas();

            this._drawingHelper = new DrawingHelper();

            //Свързване на бутони със списък с инструменти

            this.tsbtnPen.Tag = DrawingTools.PEN;
            this.tsbtnLine.Tag = DrawingTools.LINE;
            this.tsbtnRectange.Tag = DrawingTools.RECTANGLE;
            this.tsbtnEllipse.Tag = DrawingTools.ELLIPSE;

            //Задайте първоначалните цветове на контролите за избор на цвят

            this.tsbtnForeColor.BackColor = this._foreColor;
            this.tsbtnBackColor.BackColor = this._backColor;

            //Текущ инструмент по подразбиране

            this.tsbtnPen.Checked = true;
            this._activeToolButton = this.tsbtnPen;
        }

        #endregion

        #region OVERRIDED METHODS

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = this.AskUserOfLosingChanges();
            base.OnClosing(e);
        }

        #endregion

        #region EVENTS HANDLERS (CONTROLS)

        /// <summary>
        /// Задава натиснатото състояние на инструмент за рисуване
        /// </summary>
        private void drawingTools_Click(object sender, EventArgs e)
        {
            ToolStripButton currentButton = sender as ToolStripButton;
            if (currentButton != null)
            {
                currentButton.Checked = true;
                this._activeToolButton.Checked = false;
                this._activeToolButton = currentButton;
            }
        }

        /// <summary>
        /// Промяна на инструмента за рисуване, инициализация на събитие
        /// </summary>
        private void drawingTools_CheckedChanged(object sender, EventArgs e)
        {
            ToolStripButton currentButton = sender as ToolStripButton;
            if (currentButton == null)
            {
                return;
            }
            if (currentButton.Checked == true)
            {
                //Задейства интрумент за чертане върху платното

                if (this.DrawingToolChanged != null)
                {
                    ToolChangeEventArgs te = new ToolChangeEventArgs((DrawingTools)currentButton.Tag);
                    this.DrawingToolChanged(currentButton, te);
                }
                
                //задаване името на иструмента в лентата с инструменти

                this.tslblTool.Text = String.Format("Инструмент: {0}", (DrawingTools)currentButton.Tag);
            }
        }

        /// <summary>
        /// Промяна на цветовете на чертане, инициализиране на събития за промяна
        /// </summary>
        private void tsbtnColorButtons_Click(object sender, EventArgs e)
        {
            ToolStripButton currentButton = sender as ToolStripButton;
            if (currentButton == null)
            {
                return;
            }

            //Задаване на звят

            using (ColorDialog colorDialog = new ColorDialog())
            { 
                colorDialog.Color = (currentButton == this.tsbtnForeColor) ? this._foreColor : this._backColor;
                if (colorDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                currentButton.BackColor = colorDialog.Color;
                this._foreColor = (currentButton == this.tsbtnForeColor) ? colorDialog.Color : this._foreColor;
                this._backColor = (currentButton == this.tsbtnBackColor) ? colorDialog.Color : this._backColor;

                //Инициируем событие смены цвета

                ColorChangeEventArgs ce = new ColorChangeEventArgs(colorDialog.Color);

                if (currentButton == this.tsbtnForeColor && this.DrawingForeColorChanged != null)
                {
                    this.DrawingForeColorChanged(currentButton, ce);
                }
                if (currentButton == this.tsbtnBackColor && this.DrawingBackColorChanged != null)
                {
                    this.DrawingBackColorChanged(currentButton, ce);
                }
            }
        }

        private void createControl_Click(object sender, EventArgs e)
        {
            if (this.AskUserOfLosingChanges() == true)
            {
                return;
            }
            try
            { 
                MyCanvas canvas = this._drawingHelper.CreateDrawing();
                if (canvas == null)
                {
                    return;
                }
                this.ReplaceCanvas(canvas);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Грешка при чертане\n{0}", ex.Message);
                MessageBox.Show("Не може да се създаде чертане", Program.APP_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void saveControl_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem currentMenuItem = sender as ToolStripMenuItem;
            try
            { 
                bool isSaved = this._drawingHelper.SaveDrawing(this._myCanvas, currentMenuItem != null && currentMenuItem == this.mnSaveAs);
                this._isDitry = !isSaved;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Грешка при записване на файла\n{0}", ex.Message);
                MessageBox.Show("Грешка при запис на чертане", Program.APP_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void openControl_Click(object sender, EventArgs e)
        {
            if (this.AskUserOfLosingChanges() == true)
            {
                return;
            }
            try
            {
                MyCanvas canvas = this._drawingHelper.OpenDrawing();
                if (canvas == null)
                {
                    return;
                }
                this.ReplaceCanvas(canvas);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Грешка при отваряне на файла\n{0}", ex.Message);
                MessageBox.Show("Файла не може да се отвори", Program.APP_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void mnInverse_Click(object sender, EventArgs e)
        {
            this._myCanvas.InverseDrawing();
        }
        
        private void mnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnAbout_Click(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
        }

        #endregion

        #region EVENTS HANDLERS (MYCANVAS)
        
        /// <summary>
        /// Задаване на флай при изменение на чертежа
        /// </summary>
        private void OnDrawingChanged(object sender, EventArgs e)
        {
            this._isDitry = true;
        }

        /// <summary>
        /// Покзва координатите на мишката в лентата с инструменти
        /// </summary>
        private void OnMousePositionChanged(object sender, EventArgs e)
        {
            if (e is MouseEventArgs)
            { 
                this.tslblMouseCoord.Text = String.Format("Позиция: {0}", ((MouseEventArgs)e).Location);
            }
        }

        /// <summary>
        /// Актуализиране на лентата за напредък при инвертиране на изображението. 
        /// Изпълнява се при инвертиране
        /// </summary>
        private void OnInverseProgressChanged(object sender, EventArgs e)
        {
            InverseProgressChangedEventArgs eArgs = e as InverseProgressChangedEventArgs;
            if (eArgs != null)
            { 
                this.Invoke(new Action(() => this.sbpbProgress.Value = 100 * eArgs.Progress / (eArgs.Max - eArgs.Min)));
            }
        }

        /// <summary>
        /// Деактивирайте контролите за платно
        /// за предотвратяване на едновременен достъп на нишки до ресурси.
        /// Изпълнява се при инвертиране.
        /// </summary>
        private void OnBeginInverse(object sender, EventArgs e)
        {
            this.SetControlState(false);
        }

        /// <summary>
        /// Включва контролите след инвертиране на изображението.
        /// Изпълнява се в нишка за обръщане на изображението
        /// </summary>
        private void OnEndInverse(object sender, EventArgs e)
        {
            this.SetControlState(true);
        }

        #endregion

        #region HELPERS

        /// <summary>
        /// Добавя към формуляра и инициализира обекта в платното
        /// Следи за събития, задава първоначални стойности
        /// </summary>
        private void InitCanvas()
        {
            //Следи за формиране на събития (промяна на настройките за рисуване)

            this.DrawingToolChanged += this._myCanvas.OnToolChanged;
            this.DrawingForeColorChanged += this._myCanvas.OnForeColorChanged;
            this.DrawingBackColorChanged += this._myCanvas.OnBackColorChanged;

            //Следи за промяна върху платното

            this._myCanvas.DrawingChanged += this.OnDrawingChanged;
            this._myCanvas.MousePositionCoordChanged += this.OnMousePositionChanged;
            this._myCanvas.InverseProressChanged += this.OnInverseProgressChanged;
            this._myCanvas.BeginInverse += this.OnBeginInverse;
            this._myCanvas.EndInverse += this.OnEndInverse;

            //Задава инструмент по подразбиране
           
            this.DrawingToolChanged(this, new ToolChangeEventArgs(DrawingTools.PEN));
           
            //Задава цвят на контура

            this.DrawingForeColorChanged(this, new ColorChangeEventArgs(this._foreColor));

            //Установява промяна на задния фон на инструмента
            
            this.DrawingBackColorChanged(this, new ColorChangeEventArgs(this._backColor));

            //добавяне към платното

            this.pnBase.Controls.Add(this._myCanvas);
        }

        /// <summary>
        /// Премахване платното от формуляра на редактора, като се отпишете от събития
        /// </summary>
        private void RemoveCanvas()
        {
            //Отписване от събития

            this.DrawingToolChanged -= this._myCanvas.OnToolChanged;
            this.DrawingForeColorChanged -= this._myCanvas.OnForeColorChanged;
            this.DrawingBackColorChanged -= this._myCanvas.OnBackColorChanged;

            this._myCanvas.DrawingChanged -= this.OnDrawingChanged;
            this._myCanvas.MousePositionCoordChanged -= this.OnMousePositionChanged;
            this._myCanvas.InverseProressChanged -= this.OnInverseProgressChanged;
            this._myCanvas.BeginInverse -= this.OnBeginInverse;
            this._myCanvas.EndInverse -= this.OnEndInverse;

            this.pnBase.Controls.Clear();
            this._myCanvas = null;
        }

        /// <summary>
        /// Пита потребителя дали да запази чертежа преди затваряне (изтриване)
        /// </summary>
        /// <returns>true - при отказ да се запазят промените, false - за да се разреши запазването</returns>
        private bool AskUserOfLosingChanges()
        {
            if (this._isDitry == true)
            {
                return MessageBox.Show("Искате ли да запазите промените?", Program.APP_NAME, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK;
            }
            return false;
        }

        /// <summary>
        /// Заменя текущото платно
        /// </summary>
        /// <param name="canvas">Промяна едно със друго / замяна</param>
        private void ReplaceCanvas(MyCanvas canvas)
        {
            this.RemoveCanvas();
            this._myCanvas = canvas;
            this.InitCanvas();
            this._isDitry = false;
        }

        private void SetControlState(bool value)
        {
            //Туулбар

            this.Invoke(new Action(() => this.tsbtnNew.Enabled = value));
            this.Invoke(new Action(() => this.tsbtnOpen.Enabled = value));
            this.Invoke(new Action(() => this.tsbtnSave.Enabled = value));

            //Меню

            this.Invoke(new Action(() => this.mnNew.Enabled = value));
            this.Invoke(new Action(() => this.mnOpen.Enabled = value));
            this.Invoke(new Action(() => this.mnSave.Enabled = value));
            this.Invoke(new Action(() => this.mnSaveAs.Enabled = value));
            this.Invoke(new Action(() => this.mnInverse.Enabled = value));

            //Прогрес бар

            this.Invoke(new Action(() => this.sbpbProgress.Visible = !value));
        }

        #endregion
    }
}