using MahApps.Metro.Controls;
using SiliconStudio.Xenko.Engine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace XenkoLiveEditor
{
    public partial class LiveEditorMainWindow : MetroWindow
    {
        private Game game;
        private SceneInstance sceneInstance;
        
        public ObservableCollection<EntityTreeItem> Entities { get; set; } = new ObservableCollection<EntityTreeItem>();
        
        private EntityTreeItem selectedEntity;
        private Dictionary<Type, EntityComponentInfo> componentInfos = new Dictionary<Type, EntityComponentInfo>();
        
        private bool componentsInitialized = false;

        public LiveEditorMainWindow(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("game");

            InitializeComponent();

            RootGrid.DataContext = this;
            
            this.game = game;

            Task.Factory.StartNew(GetSceneInstance);
            Task.Factory.StartNew(UpdateComponentValuesTicker);

            Closing += MainWindow_Closing;
        }
        
        #region Setup Xenko Bindings

        private async void GetSceneInstance()
        {
            await WaitForSceneSystem();
            await WaitForSceneInstance();

            sceneInstance = game.SceneSystem.SceneInstance;

            if (sceneInstance == null)
                Log(LogLevel.Error, "No scene instance found.");
            else
                Dispatcher.Invoke(OnSceneInstanceReady);
        }

        private async Task WaitForSceneSystem()
        {
            Log("Waiting for scene system...");

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (game.SceneSystem != null)
                        return;

                    await Task.Delay(100);
                }
            });
        }

        private async Task WaitForSceneInstance()
        {
            Log("Waiting for scene instance...");

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (game.SceneSystem != null && game.SceneSystem.SceneInstance != null)
                        return;

                    await Task.Delay(100);
                }
            });
        }

        #endregion Setup Xenko Bindings

        #region Xenko Event Handlers

        private void SceneInstance_EntityAdded(object sender, Entity e)
        {
            OnEntityAdded(e);
        }

        private void SceneInstance_EntityRemoved(object sender, Entity e)
        {
            OnEntityRemoved(e);
        }
        
        private void SceneInstance_SceneChanged(object sender, EventArgs e)
        {
            Log("Scene changed");
        }

        private void SceneInstance_ComponentChanged(object sender, SiliconStudio.Xenko.Engine.Design.EntityComponentEventArgs e)
        {
            if (selectedEntity == null || e.Entity != selectedEntity.Entity)
                return;

            if (e.NewComponent != null)
                OnComponentAdded(e.NewComponent);
            if (e.PreviousComponent != null)
                OnComponentRemoved(e.NewComponent);
        }

        private void AddSceneInstanceEvents()
        {
            sceneInstance.SceneChanged += SceneInstance_SceneChanged;
            sceneInstance.EntityAdded += SceneInstance_EntityAdded;
            sceneInstance.EntityRemoved += SceneInstance_EntityRemoved;
            sceneInstance.ComponentChanged += SceneInstance_ComponentChanged;
        }

        private void RemoveSceneInstanceEvents()
        {
            if (sceneInstance == null)
                return;

            sceneInstance.SceneChanged -= SceneInstance_SceneChanged;
            sceneInstance.EntityAdded -= SceneInstance_EntityAdded;
            sceneInstance.EntityRemoved -= SceneInstance_EntityRemoved;
            sceneInstance.ComponentChanged -= SceneInstance_ComponentChanged;
        }
        
        #endregion Xenko Event Handlers

        #region UI Events

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (game != null)
                game.Exit();
        }

        private void entityTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedEntity = null;
            ClearComponentView();

            if (e == null || e.NewValue == null)
                return;

            var entity = e.NewValue as EntityTreeItem;
            if (entity == null)
                return;

            selectedEntity = entity;

            // Build component view and add components changed event handling.
            // Spin up watcher thread to watch for component value changes.

            foreach (var component in entity.Entity.Components)
            {
                OnComponentAdded(component);
            }

            componentsInitialized = true;
        }
        
        #endregion UI Events

        #region UI Updates

        private void OnSceneInstanceReady()
        {
            AddSceneInstanceEvents();
            
            foreach (var entity in sceneInstance.Scene.Entities)
            {
                OnEntityAdded(entity);
            }
        }
        
        private void OnEntityAdded(Entity entity)
        {
            Log($"Entity {entity.Name} added.");

            var treeItem = new EntityTreeItem(entity);

            if (entity.Transform.Parent != null)
            {
                var result = FindEntityInTree(Entities, entity.Transform.Parent.Entity);
                if (result == null)
                    Entities.Add(treeItem);
                else
                    result.Children.Add(treeItem);
            }
            else
            {
                Entities.Add(treeItem);
            }
        }

        private void OnEntityRemoved(Entity entity)
        {
            Log($"Entity {entity.Name} removed.");

            var result = FindEntityInTree(Entities, entity);

            if (result != null)
                Entities.Remove(result);
        }

        private void OnComponentAdded(EntityComponent component)
        {
            var expander = new EntityComponentExpander(component);
            expander.Root.IsExpanded = true;

            var componentInfo = GetEntityComponentInfo(component.GetType());
            
            expander.Root.Header = componentInfo.Name;

            var propertyEditors = new List<UserControl>();
            
            foreach (var prop in componentInfo.Properties)
            {
                if (prop.Name == "Id")
                    continue;
                
                var elem = GetEditorForProperty(component, prop);
                expander.ComponentList.Children.Add(elem);
            }
            
            componentGridList.Children.Add(expander);
        }

        private void OnComponentRemoved(EntityComponent component)
        {
            var listItem = componentGridList.Children.Cast<EntityComponentExpander>().FirstOrDefault(c => c.Component == component);
            if (listItem != null)
                componentGridList.Children.Remove(listItem);
        }
        
        private void ClearComponentView()
        {
            componentGridList.Children.Clear();
            componentsInitialized = false;
        }
        
        private void Log(string message)
        {
            Log(LogLevel.Info, message);
        }

        private void Log(LogLevel logLevel, string message)
        {
            var color = Colors.Black;
            var fontWeight = FontWeights.Normal;
            var fontStyle = FontStyles.Normal;

            switch (logLevel)
            {
                case LogLevel.Debug:
                    fontStyle = FontStyles.Italic;
                    break;
                case LogLevel.Info:
                    break;
                case LogLevel.Warn:
                    color = Colors.Navy;
                    fontWeight = FontWeights.Bold;
                    break;
                case LogLevel.Error:
                    color = Colors.Red;
                    fontWeight = FontWeights.Bold;
                    break;
                default:
                    break;
            }

            Dispatcher.Invoke(() =>
            {
                txtLog.Inlines.Add(new Run(message + "\n") { FontWeight = fontWeight, FontStyle = fontStyle, Foreground = new SolidColorBrush(color) });
                logScrollViewer.ScrollToBottom();
            });
        }

        #endregion UI Updates

        #region Methods

        private async void UpdateComponentValuesTicker()
        {
            while (true)
            {
                await Task.Delay(100);

                if (selectedEntity == null || !componentsInitialized)
                    continue;

                Dispatcher.Invoke(UpdateComponentValues);
            }
        }

        private void UpdateComponentValues()
        {
            foreach (var item in componentGridList.Children.Cast<EntityComponentExpander>())
            {
                foreach (var element in item.ComponentList.Children)
                {
                    if (element is DataTypeEditors.BaseEditor)
                        ((DataTypeEditors.BaseEditor)element).UpdateValues(IsActive);
                }
            }
        }

        private UserControl GetEditorForProperty(EntityComponent component, ComponentPropertyItem property)
        {
            var type = property.PropertyType;

            if (property.PropertyType.IsEnum)
                return new DataTypeEditors.EnumEditor(component, property);
            else if (type == typeof(int))
                return new DataTypeEditors.Int32Editor(component, property);
            else if (type == typeof(float))
                return new DataTypeEditors.SingleEditor(component, property);
            else if (type == typeof(bool))
                return new DataTypeEditors.BooleanEditor(component, property);
            else if (type == typeof(SiliconStudio.Core.Mathematics.Vector3))
                return new DataTypeEditors.Vector3Editor(component, property);
            else if (type == typeof(SiliconStudio.Core.Mathematics.Vector2))
                return new DataTypeEditors.Vector2Editor(component, property);
            else if (type == typeof(SiliconStudio.Core.Mathematics.Quaternion))
            {
                if (component.GetType().Name == "TransformComponent" && property.Name == "Rotation")
                    return new DataTypeEditors.RotationEditor(component, property);
                else
                    return new DataTypeEditors.QuaternionEditor(component, property);
            }
            else
                return new DataTypeEditors.UnsupportedEditor(component, property);
        }

        private EntityTreeItem FindEntityInTree(IEnumerable<EntityTreeItem> collection, Entity entity)
        {
            foreach (var e in collection)
            {
                if (e.Entity == entity)
                    return e;

                var result = FindEntityInTree(e.Children, entity);

                if (result != null)
                    return result;
            }

            return null;
        }

        private EntityComponentInfo GetEntityComponentInfo(Type type)
        {
            if (componentInfos.ContainsKey(type))
                return componentInfos[type];

            var info = new EntityComponentInfo(type);
            componentInfos.Add(type, info);

            return info;
        }

        #endregion Methods
    }
}
