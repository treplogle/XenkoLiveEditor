using SiliconStudio.Xenko.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace XenkoLiveEditor.DataTypeEditors
{
    public abstract class BaseEditor : UserControl
    {
        public EntityComponent Component { get; private set; }
        public ComponentPropertyItem ComponentProperty { get; private set; }

        public BaseEditor() { }

        public BaseEditor(EntityComponent component, ComponentPropertyItem property)
        {
            Component = component;
            ComponentProperty = property;
        }
        
        protected void AddTextBoxEvents(params TextBox[] boxes)
        {
            foreach (var box in boxes)
            {
                box.GotFocus += OnGotFocus;
                box.KeyDown += OnKeyDown;
            }
        }

        protected void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var element = sender as TextBox;
                if (element != null)
                    element.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.Next));
            }
        }

        protected void OnGotFocus(object sender, RoutedEventArgs e)
        {
            var element = sender as TextBox;
            if (element != null)
                element.SelectAll();
        }

        public abstract void UpdateValues(bool editorWindowIsActive);
    }
}
