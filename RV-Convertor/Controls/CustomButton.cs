using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


namespace RV_Convertor
{
    public class CustomButton : Control
    {

        #region -- Свойства --

        [Description("Text rendered on hover")]
        public string TextHover { get; set; }

        public bool roundingEnable = false;
        [Description("ON/OFF Rounding for object")]
        public bool RoundingEnable
        {
            get => roundingEnable;
            set
            {
                roundingEnable = value;
                Refresh();
            }
        }

        private int roundingPercent = 100;
        [DisplayName("Rounding [%]")]
        [DefaultValue(100)]
        [Description("Specifies the radius of rounding of the object as a percentage")]
        public int Rounding
        {
            get => roundingPercent;
            set
            {
                if (value >= 0 && value <= 100)
                {
                    roundingPercent = value;

                    Refresh();
                }
            }
        }

        #endregion

        #region -- Values --

        private StringFormat SF = new StringFormat();

        private bool MouseEntered = false;
        // private bool MousePressedd = false;

        Animation CurtainButtonAnim = new Animation();
        Animation RippleButtonAnim = new Animation();
        Animation TextSlideAnim = new Animation();

        Point ClickLocation = new Point();

        #endregion

        public CustomButton() {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
            DoubleBuffered = true;

            Size = new Size(100, 30);

            Font = new Font("Vedana", 8.25F, FontStyle.Regular);

            BackColor = Color.Tomato;
            ForeColor = Color.White;

            SF.Alignment = StringAlignment.Center;
            SF.LineAlignment = StringAlignment.Center;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.Clear(Parent.BackColor);

            Rectangle rect = new Rectangle(0, 0, Width-1, Height-1);
            Rectangle rectCurtain = new Rectangle(0, 0, (int)CurtainButtonAnim.Value, Height - 1);
            Rectangle rectRipple = new Rectangle(
                ClickLocation.X - (int)RippleButtonAnim.Value / 2,
                ClickLocation.Y - (int)RippleButtonAnim.Value / 2,
                (int)RippleButtonAnim.Value,
                (int)RippleButtonAnim.Value
                );
            Rectangle rectText = new Rectangle((int)TextSlideAnim.Value, rect.Y, rect.Width, rect.Height);
            Rectangle rectTextHover = new Rectangle((int)TextSlideAnim.Value - rect.Width, rect.Y, rect.Width, rect.Height);

            // Rounding
            float roundingValue = 0.1F; 
            if (RoundingEnable && roundingPercent > 0)
            {
                roundingValue = Height / 100F * roundingPercent;
            }
            GraphicsPath rectPath = Drawer.RoundedRectangle(rect, roundingValue);

            // Main rectangle (Background)
            g.DrawPath(new Pen(BackColor), rectPath);
            g.FillPath(new SolidBrush(BackColor), rectPath);

            g.SetClip(rectPath);

            // Hover from rectangle
            g.DrawRectangle(new Pen(Color.FromArgb(60, Color.White)), rectCurtain);
            g.FillRectangle(new SolidBrush(Color.FromArgb(60, Color.White)), rectCurtain);
            
            // Standart Paint rectangle for click 
            /*if (MousePressedd)
            {
                g.DrawRectangle(new Pen(Color.FromArgb(30, Color.Black)), rect);
                g.FillRectangle(new SolidBrush(Color.FromArgb(30, Color.Black)), rect);
            }*/

            // Ripple Effect - Wave
            if(RippleButtonAnim.Value > 0 && RippleButtonAnim.Value < RippleButtonAnim.TargetValue)
            {
                g.DrawEllipse(new Pen(Color.FromArgb(30, Color.Black)), rectRipple);
                g.FillEllipse(new SolidBrush(Color.FromArgb(30, Color.Black)), rectRipple);
            }
            else if (RippleButtonAnim.Value == RippleButtonAnim.TargetValue)
            {
                RippleButtonAnim.Value = 0;
            }

            // Draw text
            if (string.IsNullOrEmpty(TextHover))
            {
                g.DrawString(Text, Font, new SolidBrush(ForeColor), rect, SF);
            }
            else
            {
                g.DrawString(Text, Font, new SolidBrush(ForeColor), rectText, SF);
                g.DrawString(TextHover, Font, new SolidBrush(ForeColor), rectTextHover, SF);
            }
        }

        private void ButtonCurtainAction()
        {
            if(MouseEntered)
            {
                CurtainButtonAnim = new Animation("ButtonCurtain_" + Handle, Invalidate, CurtainButtonAnim.Value, Width - 1);
            }
            else
            {
                CurtainButtonAnim = new Animation("ButtonCurtain_" + Handle, Invalidate, CurtainButtonAnim.Value, 0);
            }

            CurtainButtonAnim.StepDivider = 8;
            Animator.Request(CurtainButtonAnim, true);
        }

        private void ButtonRippleAction()
        {
            RippleButtonAnim = new Animation("ButtonRipple_" + Handle, Invalidate, 0, Width);

            Animator.Request(RippleButtonAnim, true);
        }

        private void TextSlideAction()
        {
            if (MouseEntered)
            {
                TextSlideAnim = new Animation("TextSlide_" + Handle, Invalidate, TextSlideAnim.Value, Width - 1);
            }
            else
            {
                TextSlideAnim = new Animation("TextSlide_" + Handle, Invalidate, TextSlideAnim.Value, 0);
            }

            TextSlideAnim.StepDivider = 8;
            Animator.Request(TextSlideAnim, true);
        }


        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            MouseEntered = true;

            ButtonCurtainAction();
            TextSlideAction();

            // Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            MouseEntered = false;

            ButtonCurtainAction();
            TextSlideAction();

            // Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // MousePressedd = true;

            CurtainButtonAnim.Value = CurtainButtonAnim.TargetValue;

            ClickLocation = e.Location;
            ButtonRippleAction();

            // Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // MousePressedd = false;

            // Invalidate();
        }
    }
}
