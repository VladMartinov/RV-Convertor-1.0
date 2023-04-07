using System.ComponentModel;
using System.Windows.Forms;

namespace RV_Convertor
{
    public partial class RoundingButtonsComponent : Component
    {
        public Form TargetForm { get; set; }

        public bool roundingEnable = false;
        [Description("ON/OFF Rounding for object")]
        public bool RoundingEnable
        {
            get => roundingEnable;
            set
            {
                roundingEnable = value;
                Update();
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

                    Update();
                }
            }
        }

        [DefaultValue(true)]
        [Description("Apply rounding to nested containers")]
        public bool NestedContainers { get; set; } = true;

        public RoundingButtonsComponent()
        {
            InitializeComponent();
        }

        public RoundingButtonsComponent(IContainer container)
        {
            Update();

            container.Add(this);

            InitializeComponent();
        }

        public void Update()
        {
            if(TargetForm != null && TargetForm.Controls.Count > 0)
            {
                DefineRounding(TargetForm.Controls);
            }
        }

        public void DefineRounding(Control.ControlCollection control)
        {
            foreach (Control ctrl in control)
            {
                if(ctrl is CustomButton)
                {
                    CustomButton btn = (CustomButton)ctrl;

                    btn.RoundingEnable = RoundingEnable;
                    btn.Rounding = Rounding;

                    btn.Refresh();
                }

                if(NestedContainers)
                {
                    if (ctrl.Controls.Count > 0)
                    {
                        DefineRounding(ctrl.Controls);
                    }
                }
            }
        }
    }
}
