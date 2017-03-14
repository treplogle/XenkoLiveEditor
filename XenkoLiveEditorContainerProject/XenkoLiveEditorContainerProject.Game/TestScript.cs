using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace XenkoLiveEditorContainerProject
{
    public class TestScript : SyncScript
    {
        public override void Update()
        {
            var elapsed = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            Entity.Transform.Position += new Vector3(1, 1, 1) * elapsed;
        }
    }
}
