using System.Threading;
using ApacheTech.Common.BrighterSlim;
using Gantry.Services.Brighter.Filters;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointGroups.Abstractions;

/// <summary>
///     Pauses the processing of map layers while a command is being handled.
/// </summary>
/// <seealso cref="RequestHandlerAttribute" />
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.WithMembers)]
public class LockMapLayerGenerationAttribute() : RequestHandlerAttribute(1)
{
    /// <inheritdoc />
    public override Type GetHandlerType()
    {
        return typeof(LockMapLayerGenerationHandler<>);
    }

    /// <summary />
    internal class LockMapLayerGenerationHandler<TRequest> : RequestHandler<TRequest> where TRequest : class, IRequest
    {
        private readonly IMapLayerGeneration _mapLayerGeneration;

        public LockMapLayerGenerationHandler(IMapLayerGeneration mapLayerGeneration)
        {
            _mapLayerGeneration = mapLayerGeneration;
        }

        /// <summary />
        [HandledOnClient]
        public override TRequest Handle(TRequest command)
        {
            ApiEx.Logger.VerboseDebug($"Suspending map layer generation thread.");
            _mapLayerGeneration.Suspend();
            var textureAtlasManager = ApiEx.Client.BlockTextureAtlas.To<TextureAtlasManager>();
            textureAtlasManager.PauseRegenMipmaps();
            Thread.Sleep(20);
            var result = base.Handle(command);
            ApiEx.Logger.VerboseDebug($"Resuming map layer generation thread.");
            textureAtlasManager.ResumeRegenMipmaps();
            _mapLayerGeneration.Resume();
            return result;
        }
    }
}