using System.Collections.Generic;
using Timber_and_Stone;
using Timber_and_Stone.API.Event;
using Timber_and_Stone.Event;
using Timber_and_Stone.Tasks;
using UnityEngine;
using Timber_and_Stone.Utility;
using Plugin.Bobisback.CombinedMods.Extension_Methods;

namespace Plugin.Bobisback.CombinedMods.Tasks
{
    public class WorkRepairDoor : AWorkTask
    {
        private Coordinate structPos;
        private Coordinate[] standPositions;

        public Transform structure
        {
            get;
            private set;
        }

        public WorkRepairDoor(IWorkPool workPool, Transform structure) : base(workPool)
        {
            this.structure = structure;
            structPos = Coordinate.FromWorld(structure.position);
            name = "Repairing door";
            BuildStructure buildStructure;
            if (structure.OutComponent(out buildStructure)) {
                name = "Repairing " + buildStructure.structureName;
            }
        }

        public override IEnumerable<ATask> task()
        {
            int[] tools = {69, 68, 67};

            EventWorkTools toolEvent = new EventWorkTools(unit, this, tools);
            EventManager.getInstance().InvokePre(toolEvent);
            tools = toolEvent.tools;
            EventManager.getInstance().InvokePost(toolEvent);

            BuildStructure buildStructure = structure.GetComponent<BuildStructure>();
            if (buildStructure.beingBuilt)
            {
                yield break;
            }
            buildStructure.beingBuilt = true;
            
            ItemList requiredItems = ItemList.All();
            requiredItems.Add(ItemList.Any(tools));

            //TODO do soem sort of calculation to determine what resources are needed to repair this structure
            Dictionary<int ,int> toRemove = new Dictionary<int, int>();
			List<int> required = new List<int>();
			for (int index = 0; index < buildStructure.resourceTypes.Length; index++)
            {
                toRemove.Add(buildStructure.resourceTypes[index], buildStructure.resourceAmounts[index]);
                for (int i = 0; i < buildStructure.resourceAmounts[index]; i++)
                   required.Add(buildStructure.resourceTypes[index]);
            }
            requiredItems.Add(required.ToArray());

            TaskGetItems toolCheck = new TaskGetItems(unit, requiredItems);
            yield return toolCheck;
            if (!toolCheck.completedSuccessfully)
            {
                bool hasTool = false;

                for (int i = 0; i < tools.Length; i++)
                {
                    int id = tools[i];
                    Resource r = Resource.FromID(id);
                    if (unit.inventory.Contains(r)) {
                        hasTool = true;
                        break;
                    }
                    if (unit.faction.storage[r] > 0) {
                        hasTool = true;
                        break;
                    }
                }

				if (!hasTool)
				{
					unit.setBubble("I need a hammer to repair the " + buildStructure.structureName.ToLower() + ".");
				}
				else
				{
					unit.setBubble("We don't have the materials to repair the " + buildStructure.structureName.ToLower() + ".");
				}

                yield break;
            }

            for (int i = 0; i < tools.Length; i++)
            {
                if (unit.EquipHandR(Resource.FromID(i)))
				{
					break;
				}
            }

            standPositions = buildStructure.GetAllWorkingPositions(unit);
            if (standPositions.Length <= 0)
            {
                unit.setBubble("I can't get to the " + buildStructure.structureName.ToLower() + ".");
                yield break;
            }

            //Each line below gives a differnet error
            //TaskSmartWalk<TaskGetPathToBlock> path = new TaskSmartWalk<TaskGetPathToBlock>(unit, (TaskSmartWalk<TaskGetPathToBlock>.DTaskPathfinder)PathFinder);

            //TaskSmartWalk<TaskGetPathToBlock> path = new TaskSmartWalk<TaskGetPathToBlock>(unit, (TaskSmartWalk<TaskGetPathToBlock>.DTaskPathfinder)((rules, start) => new TaskGetPathToBlock(rules, start, standPositions, unit.getModel().collider)));
            
            //TaskSmartWalk<TaskGetPathToBlock> path = new TaskSmartWalk<TaskGetPathToBlock>(unit, (rules, start) => new TaskGetPathToBlock(rules, start, standPositions, unit.getModel().collider));
            
            /*yield return path;
            if (!path.completedSuccessfully)
            {
                yield break;
            }*/

            while (!buildStructure.canBuild) {
                unit.setBubble("Cannot repair the door.");
                yield return new TaskWait(0.5f);
            }

            unit.LookAtIgnoreHeight(structure.position);
            unit.PlayAnimation("hammer");
            float workingPace;

            unit.fatigueRate = 1;
            buildStructure.crafter = unit;
            do {
                if (buildStructure.requiresHammer && unit.getModel().animation["hammer"].normalizedTime % 1 > 0.2 && unit.getModel().animation["hammer"].normalizedTime % 1 < 0.25 && !unit.audio.isPlaying && Time.timeScale != 0.0) {
                    unit.audio.clip = AssetManager.getInstance().hammeringSFX.RandomElement();
                    unit.audio.volume = Options.getInstance().soundfxVolume;
                    unit.audio.Play();
                }

                workingPace = (.25f + (unit.getProfession().getLevel() * .05f)) * Time.deltaTime * (unit.fatigue + 0.75f);

                unit.workPercent = buildStructure.Build(workingPace);
                yield return null;
            }
            while (unit.workPercent < 1.0f);

            foreach (KeyValuePair<int, int> kv in toRemove) {
                if (!unit.inventory.Contains(kv.Key, kv.Value)) {
                    if (unit.weaponR != null && unit.weaponR.index == kv.Key) unit.EquipHandR(null);
                    if (unit.weaponL != null && unit.weaponL.index == kv.Key) unit.EquipHandL(null);
                    if (unit.armorHelm != null && unit.armorHelm.index == kv.Key) unit.EquipHelm(null);
                    if (unit.armorChest != null && unit.armorChest.index == kv.Key) unit.EquipChest(null);
                    if (unit.armorBoots != null && unit.armorBoots.index == kv.Key) unit.EquipBoots(null);
                }
                unit.inventory.Remove(kv.Key, kv.Value);
            }

            unit.getProfession().AddExperience(buildStructure.expAwarded/2);
            if (ResourceManager.getInstance().CheckForBreakage(unit.weaponR, unit.preferences["trait.clumsy"])) {
                if (unit.preferences["notification.brokentool"]) GUIManager.getInstance().AddTextLine(unit.unitName + "'s " + unit.weaponR.name + " has broken.", unit.transform, false);
                unit.SetHandR(null);
            }
            completedSuccessfully = true;
            switch (buildStructure.structureName) {
                case "Fence Gate":
                    buildStructure.health = SettingsManager.CurrentFenceHp;
                    break;
                case "Timber Door":
                    buildStructure.health = SettingsManager.CurrentTimberHp;
                    break;
                case "Braced Door":
                    buildStructure.health = SettingsManager.CurrentBracedHp;
                    break;
                case "Studded Door":
                    buildStructure.health = SettingsManager.CurrentStuddedHp;
                    break;
                case "Dungeon Door":
                    buildStructure.health = SettingsManager.CurrentDungeonHp;
                    break;
                case "Castle Arch Gate":
                case "Castle Gate":
                    buildStructure.health = SettingsManager.CurrentCastleHp;
                    break;
            }
        }

        private TaskGetPathToBlock PathFinder(Pathfinder.DWalkableBlocks rules, Coordinate unitStart)
        {
            return new TaskGetPathToBlock(rules, unitStart, standPositions, unit.getModel().collider);
        }

        public override float getPriority(ALivingEntity entity)
        {
            Coordinate coordinate = entity.coordinate;
            return Vector3.Distance(coordinate.absolute, this.structPos.absolute) + base.getPriorityOffset() - 50f;
        }

        public override void cleanup()
        {
            unit.workPercent = null;
            this.structure.GetComponent<BuildStructure>().beingBuilt = false;
            unit.PlayAnimation(ALivingEntity.Animations.Idle);
            base.cleanup();
        }
    }
}
