using System.Collections.Generic;
using Plugin.Bobisback.CombinedMods.Tasks;
using Timber_and_Stone;
using Timber_and_Stone.Tasks;

namespace Plugin.Bobisback.CombinedMods.WorkPools
{
    public class RepairDoorWorkPool : IWorkPool
    {
        public Dictionary<BuildStructure, WorkRepairDoor> pool = new Dictionary<BuildStructure, WorkRepairDoor>();

        public AWorkTask getWork(ALivingEntity entity, ref float metric)
        {
            Coordinate coordinate = entity.coordinate;
            WorkRepairDoor result = null;
            foreach (KeyValuePair<BuildStructure, WorkRepairDoor> current in this.pool) {
                //if (current.Key.beingRepaired) {
                //    if (current.Value.unit.taskStackContains(current.Value)) {
                //        continue;
                //    }
                //    current.Key.beingRepaired = false;
                //}
                float priority = current.Value.getPriority(entity);
                if (priority < metric) {
                    if (entity.canPerformWork(current.Value)) {
                        metric = priority;
                        result = current.Value;
                    }
                }
            }
            return result;
        }

        public void onComplete(AWorkTask task)
        {
            if (task.completedSuccessfully) {
                WorkRepairDoor workBuildStructure = task as WorkRepairDoor;
                if (workBuildStructure != null) {
                    BuildStructure component = workBuildStructure.structure.GetComponent<BuildStructure>();
                    pool.Remove(component);
                }
            }
        }
    }
}
