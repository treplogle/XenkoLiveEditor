using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using System.Windows.Controls;

namespace XenkoLiveEditor.DataTypeEditors
{
    public partial class Vector2Editor : BaseEditor
    {
        public Vector2Editor(EntityComponent component, ComponentPropertyItem property)
            : base(component, property)
        {
            InitializeComponent();
            
            PropertyName.Text = property.Name;
            UpdateValues(false);

            X.LostFocus += OnValueChanged;
            Y.LostFocus += OnValueChanged;

            AddTextBoxEvents(X, Y);
        }

        private void OnValueChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            float x = 0, y = 0;
            float.TryParse(X.Text, out x);
            float.TryParse(Y.Text, out y);

            X.Text = x.ToString();
            Y.Text = y.ToString();

            var v = new Vector2(x, y);
            ComponentProperty.SetValue(Component, v);
        }
        
        public override void UpdateValues(bool editorWindowIsActive)
        {
            var value = (Vector2)ComponentProperty.GetValue(Component);

            var xStr = value.X.ToString();
            var yStr = value.Y.ToString();

            if ((!editorWindowIsActive || !X.IsFocused) && X.Text != xStr)
                X.Text = xStr;
            if ((!editorWindowIsActive || !Y.IsFocused) && Y.Text != yStr)
                Y.Text = yStr;
        }
    }
}
