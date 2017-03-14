using SiliconStudio.Xenko.Engine;
using System.Windows.Controls;

namespace XenkoLiveEditor.DataTypeEditors
{
    public partial class Int32Editor : BaseEditor
    {
        public Int32Editor(EntityComponent component, ComponentPropertyItem property)
            : base(component, property)
        {
            InitializeComponent();
        
            PropertyName.Text = property.Name;
            UpdateValues(false);

            Value.LostFocus += OnValueChanged;
            AddTextBoxEvents(Value);
        }

        private void OnValueChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            int value = 0;
            int.TryParse(Value.Text, out value);

            Value.Text = value.ToString();

            ComponentProperty.SetValue(Component, value);
        }
        
        public override void UpdateValues(bool editorWindowIsActive)
        {
            var value = (int)ComponentProperty.GetValue(Component);
            var stringVal = value.ToString();

            if ((!editorWindowIsActive || !Value.IsFocused) && Value.Text != stringVal)
                Value.Text = value.ToString();
        }
    }
}
