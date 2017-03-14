using SiliconStudio.Xenko.Engine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XenkoLiveEditor
{
    public class EntityTreeItem
    { 
        public string Name { get { return Entity.Name; } }

        public Entity Entity { get; set; }

        public ObservableCollection<EntityTreeItem> Children { get; set; }

        public EntityTreeItem() { }

        public EntityTreeItem(Entity entity)
        {
            Entity = entity;
            
            if (entity == null || entity.Transform == null || entity.Transform.Children == null)
                Children = new ObservableCollection<EntityTreeItem>();
            else
                Children = new ObservableCollection<EntityTreeItem>(entity.Transform.Children.Select(e => new EntityTreeItem(e.Entity)));
        }
    }
}
