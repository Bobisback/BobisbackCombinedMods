using Timber_and_Stone;

namespace Plugin.Bobisback.CombinedMods.Tasks
{
    public class DTaskPathfinderWrapper<T>
    {
        public delegate T DTaskPathfinder(Pathfinder.DWalkableBlocks rules, Coordinate unitStart);
    }
}
