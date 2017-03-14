using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using System.Windows.Controls;

namespace XenkoLiveEditor.DataTypeEditors
{
    public partial class Vector3Editor : BaseEditor
    {
        public Vector3Editor(EntityComponent component, ComponentPropertyItem property)
            : base(component, property)
        {
            InitializeComponent();
            
            PropertyName.Text = property.Name;
            UpdateValues(false);

            X.LostFocus += OnValueChanged;
            Y.LostFocus += OnValueChanged;
            Z.LostFocus += OnValueChanged;

            AddTextBoxEvents(X, Y, Z);
        }

        private void OnValueChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            float x = 0, y = 0, z = 0;
            float.TryParse(X.Text, out x);
            float.TryParse(Y.Text, out y);
            float.TryParse(Z.Text, out z);

            X.Text = x.ToString();
            Y.Text = y.ToString();
            Z.Text = z.ToString();

            var v = new Vector3(x, y, z);
            ComponentProperty.SetValue(Component, v);
        }
        
        public override void UpdateValues(bool editorWindowIsActive)
        {
            var value = (Vector3)ComponentProperty.GetValue(Component);

            var xStr = value.X.ToString();
            var yStr = value.Y.ToString();
            var zStr = value.Z.ToString();

            if ((!editorWindowIsActive || !X.IsFocused) && X.Text != xStr)
                X.Text = xStr;
            if ((!editorWindowIsActive || !Y.IsFocused) && Y.Text != yStr)
                Y.Text = yStr;
            if ((!editorWindowIsActive || !Z.IsFocused) && Z.Text != zStr)
                Z.Text = zStr;
        }
    }
}
