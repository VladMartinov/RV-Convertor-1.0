using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RV_Convertor
{
    public partial class EgoldsFormStyle : Component
    {
        #region -- Proprties --

        public Form Form { set; get; }

        private Size IconSize = new Size(14, 14);

        private fStyle formStyle = fStyle.None;
        public fStyle FormStyle { get => formStyle;
            set
            {
                formStyle = value;
                Sign();
            }
        }
        public enum fStyle {
            None,
            UserStyle,
            SimpleDark,
            TelegramStyle
        }

        bool MousePressed = false; // Mouse pressed;
        Point clickPosition; // Start pos cursor in click
        Point moveStartPosition; // Start pos cursor in move

        Rectangle rectBtnClose = new Rectangle();

        bool btnCloseHovered = false;

        #endregion

        #region -- Values --

        private Color HeaderColor = Color.DimGray;
        private int HeaderHeight = 28;

        private StringFormat SF = new StringFormat();
        private Font Font = new Font("Montserrat SemiBold", 8.55F, FontStyle.Regular);

        Pen WhitePen = new Pen(Color.White) { Width = 1.55F };

        #endregion

        public EgoldsFormStyle()
        {
            InitializeComponent();
        }

        public EgoldsFormStyle(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private void Sign()
        {
            if (Form != null)
            {
                Form.Load += Form_Load;
            }
        }

        private void Apply()
        {
            SF.Alignment = StringAlignment.Near;
            SF.LineAlignment = StringAlignment.Center; 

            Form.FormBorderStyle = FormBorderStyle.None;

            SetDoubleBuffered(Form);

            OffsetControls();

            Form.Paint += Form_Paint;
            Form.MouseDown += Form_MouseDown; 
            Form.MouseUp += Form_MouseUp;
            Form.MouseMove += Form_MouseMove;
            Form.MouseLeave += Form_MouseLeave;
        }

        private void OffsetControls()
        {
            Form.Height = Form.Height + HeaderHeight;
        
            foreach(Control ctrl in Form.Controls)
            {
                ctrl.Location = new Point(ctrl.Location.X, ctrl.Location.Y + HeaderHeight);
                ctrl.Refresh();
            }
        }

        #region -- Form Events --

        private void Form_Load(object sender, EventArgs e)
        {
            Apply();
        }

        private void Form_Paint(object sender, PaintEventArgs e)
        {
            DrawStyle(e.Graphics);
        }

        private void Form_MouseLeave(object sender, EventArgs e)
        {
            btnCloseHovered = false;
            Form.Invalidate();
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(MousePressed)
            {
                Size frmOffset = new Size(Point.Subtract(Cursor.Position, new Size(clickPosition)));
                Form.Location = Point.Add(moveStartPosition, frmOffset);
            }
            else
            {
                if (rectBtnClose.Contains(e.Location))
                {
                    if(btnCloseHovered == false)
                    {
                        btnCloseHovered = true;
                        Form.Invalidate();
                    }
                }
                else
                {
                    if (btnCloseHovered == true)
                    {
                        btnCloseHovered = false;
                        Form.Invalidate();
                    }
                }
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            MousePressed = false;
        
            if(e.Button == MouseButtons.Left)
            {
                if (rectBtnClose.Contains(e.Location))
                    Form.Close();
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Location.Y <= HeaderHeight)
            {
                MousePressed = true;
                clickPosition = Cursor.Position;
                moveStartPosition = Form.Location;
            }
        }

        #endregion

        private void DrawStyle(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;

            Rectangle rectHeader = new Rectangle(0, 0, Form.Width - 1, HeaderHeight);
            Rectangle rectBorder = new Rectangle(0, 0, Form.Width - 1, Form.Height - 1);

            Rectangle rectTitleText = new Rectangle(rectHeader.X + 25, rectHeader.Y, rectHeader.Width, rectHeader.Height);
            Rectangle rectIcon = new Rectangle(
                rectHeader.Height/ 2 - IconSize.Width / 2,
                rectHeader.Height/ 2 - IconSize.Height / 2,
                IconSize.Width, IconSize.Height
                );

            rectBtnClose = new Rectangle(rectHeader.Width - rectHeader.Height, rectHeader.Y, rectHeader.Height, rectHeader.Height);
            Rectangle rectCrossHair = new Rectangle(
                rectBtnClose.X + rectBtnClose.Width / 2 - 5,
                rectBtnClose.Height / 2 - 5,
                10, 10);

            // Header
            g.DrawRectangle(new Pen(HeaderColor), rectHeader);
            g.FillRectangle(new SolidBrush(HeaderColor), rectHeader);

            // Text Header
            g.DrawString(Form.Text, Font, new SolidBrush(Color.Turquoise), rectTitleText, SF);

            // Icon
            g.DrawImage(Form.Icon.ToBitmap(), rectIcon);

            // Button X
            g.DrawRectangle(new Pen(btnCloseHovered ? FlatColors.Red : HeaderColor), rectBtnClose);
            g.FillRectangle(new SolidBrush(btnCloseHovered ? FlatColors.Red : HeaderColor), rectBtnClose);
            DrawCrosshair(g, rectCrossHair, WhitePen);

            // Border
            g.DrawRectangle(new Pen(HeaderColor), rectBorder);

        }

        private void DrawCrosshair(Graphics g, Rectangle rect, Pen pen)
        {
            g.DrawLine(pen, rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
            g.DrawLine(pen, rect.X + rect.Width, rect.Y, rect.X, rect.Y + rect.Height);
        }

        public static void SetDoubleBuffered(Control c)
        {
            if (SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo pDoubleBuffered =
                typeof(Control).GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
            pDoubleBuffered.SetValue(c, true, null);
        }

    }
}
