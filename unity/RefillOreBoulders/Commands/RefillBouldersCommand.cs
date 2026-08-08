using CoreLib.Submodule.Command.Data;
using CoreLib.Submodule.Command.Interface;
using Unity.Collections;
using Unity.Entities;

namespace RefillOreBoulders.Commands
{
    /// <summary>
    /// `/refillboulders` — restores every currently loaded ore boulder to full
    /// health. An ore boulder has no separate yield counter: its remaining ore
    /// IS its HealthCD.health, so topping that up refills the boulder.
    /// </summary>
    public class RefillBouldersCommand : IServerCommandHandler
    {
        public CommandOutput Execute(string[] parameters, Entity sender)
        {
            // sender is deliberately unused: the command's scope is "everything
            // currently loaded", so the player's position never enters into it.
            if (!RefillOreBouldersMod.IsHostOrSinglePlayer)
            {
                return new CommandOutput($"[{RefillOreBouldersMod.Name}] Only the host can refill ore boulders.", CommandStatus.Error);
            }

            World world = Manager.ecs.ServerWorld;
            if (world == null || !world.IsCreated)
            {
                return new CommandOutput($"[{RefillOreBouldersMod.Name}] Server world unavailable.", CommandStatus.Error);
            }

            EntityManager entityManager = world.EntityManager;

            // RequiresDrillCD selects exactly the ore boulders: of the 177 prefabs
            // carrying DestructibleObjectAuthoring, only the ten ore boulder types
            // set requiresDrill, which is the sole source of this component.
            // IncludeDisabledEntities is mandatory — CK disables entities beyond
            // 40 tiles while keeping them loaded out to 300, so without it the
            // command would reach only a fraction of the load bubble.
            EntityQuery query = entityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new ComponentType[] { ComponentType.ReadOnly<RequiresDrillCD>(), ComponentType.ReadWrite<HealthCD>() },
                    Options = EntityQueryOptions.IncludeDisabledEntities,
                }
            );

            NativeArray<Entity> boulders = query.ToEntityArray(Allocator.TempJob);
            int refilled = 0;

            for (int i = 0; i < boulders.Length; i++)
            {
                HealthCD health = entityManager.GetComponentData<HealthCD>(boulders[i]);

                // health <= 0 means the boulder is already being destroyed — do not
                // revive it. health >= maxHealth means there is nothing to do.
                if (health.health <= 0 || health.health >= health.maxHealth)
                {
                    continue;
                }

                health.health = health.maxHealth;
                entityManager.SetComponentData(boulders[i], health);
                refilled++;
            }

            boulders.Dispose();
            query.Dispose();

            if (refilled == 0)
            {
                return new CommandOutput($"[{RefillOreBouldersMod.Name}] No damaged ore boulders loaded.", CommandStatus.Info);
            }

            return new CommandOutput($"[{RefillOreBouldersMod.Name}] Refilled {refilled} ore boulder(s).", CommandStatus.Info);
        }

        public string GetDescription() => "Use /refillboulders to restore every loaded ore boulder to full health.";

        public string[] GetTriggerNames() => new[] { "refillboulders" };
    }
}
