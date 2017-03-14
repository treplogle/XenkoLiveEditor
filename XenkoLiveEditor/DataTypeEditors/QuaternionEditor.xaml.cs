using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using System.Windows.Controls;

namespace XenkoLiveEditor.DataTypeEditors
{
    public partial class QuaternionEditor : BaseEditor
    {
        public QuaternionEditor(EntityComponent component, ComponentPropertyItem property)
            : base(component, property)
        {
            InitializeComponent();
            
            PropertyName.Text = property.Name;
            UpdateValues(false);
            
            X.LostFocus += OnValueChanged;
            Y.LostFocus += OnValueChanged;
            Z.LostFocus += OnValueChanged;
            W.LostFocus += OnValueChanged;

            AddTextBoxEvents(X, Y, Z, W);
        }

        private void OnValueChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            float x = 0, y = 0, z = 0, w = 0;
            float.TryParse(X.Text, out x);
            float.TryParse(Y.Text, out y);
            float.TryParse(Z.Text, out z);
            float.TryParse(W.Text, out w);

            X.Text = x.ToString();
            Y.Text = y.ToString();
            Z.Text = z.ToString();
            W.Text = w.ToString();

            var q = new Quaternion(x, y, z, w);
            ComponentProperty.SetValue(Component, q);
        }
        
        public override void UpdateValues(bool editorWindowIsActive)
        {
            var value = (Quaternion)ComponentProperty.GetValue(Component);

            var xStr = value.X.ToString();
            var yStr = value.Y.ToString();
            var zStr = value.Z.ToString();
            var wStr = value.W.ToString();

            if ((!editorWindowIsActive || !X.IsFocused) && X.Text != xStr)
                X.Text = xStr;
            if ((!editorWindowIsActive || !Y.IsFocused) && Y.Text != yStr)
                Y.Text = yStr;
            if ((!editorWindowIsActive || !Z.IsFocused) && Z.Text != zStr)
                Z.Text = zStr;
            if ((!editorWindowIsActive || !W.IsFocused) && W.Text != wStr)
                W.Text = wStr;
        }
    }
}
