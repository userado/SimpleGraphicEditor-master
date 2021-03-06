using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PFEditor
{
    /// <summary>
    /// Диалогов прозорец за нов чертеж.
    /// Задава размера на картината
    /// </summary>
    public partial class frmNewDrawing : Form
    {
        #region CONSTS

        private readonly string MIN_SIZE_ERROR_MESSAGE = String.Format("Минимален размер {0} px", Program.MIN_DRAWING_SIZE);

        #endregion

        #region PROPERTIES

        public int DrawingWidth { get; private set; }
        public int DrawingHeight { get; private set; }

        #endregion

        #region CTOR
        
        public frmNewDrawing()
        {
            InitializeComponent();
        }

        #endregion

        #region OVERRIDED METHOD
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
            {
                return;
            }
            try
            {
                this.DrawingWidth = Int32.Parse(this.mtbWidth.Text.Replace(" ", ""));
                this.DrawingHeight = Int32.Parse(this.mtbHeight.Text.Replace(" ", ""));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Грешка! Голям размер на снимката \n{0}", ex.Message);
                MessageBox.Show("Непозволено чертане!. Голям размер на снимката", Program.APP_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
                return;
            }

            //Максимальный размер орграничен полями ввода

            bool isValidValues = DrawingWidth >= Program.MIN_DRAWING_SIZE && this.DrawingHeight >= Program.MIN_DRAWING_SIZE;
            if (!isValidValues)
            {
                if (this.DrawingWidth < Program.MIN_DRAWING_SIZE)
                {
                    this.errSize.SetError(this.mtbWidth, this.MIN_SIZE_ERROR_MESSAGE);
                }
                if (this.DrawingHeight < Program.MIN_DRAWING_SIZE)
                {
                    this.errSize.SetError(this.mtbHeight, this.MIN_SIZE_ERROR_MESSAGE);
                }
                e.Cancel = true;
            }
            else
            {
                this.errSize.SetError(this.mtbWidth, "");
                this.errSize.SetError(this.mtbHeight, "");
            }
        }

        #endregion
    }
}