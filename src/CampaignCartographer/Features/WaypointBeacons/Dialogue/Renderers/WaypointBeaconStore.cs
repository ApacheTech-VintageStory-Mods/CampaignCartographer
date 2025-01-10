using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Features.WaypointBeacons.Dialogue.Renderers;

/// <summary>
///     Provides functionality for creating and storing mesh references for pillar and quad models.
/// </summary>
public static class WaypointBeaconStore
{
    /// <summary>
    ///     Gets the mesh reference for the pillar model.
    /// </summary>
    public static MeshRef PillarMeshRef { get; private set; }

    /// <summary>
    ///     Gets the mesh reference for the quad model.
    /// </summary>
    public static MeshRef QuadModel { get; private set; }

    /// <summary>
    ///     Creates the pillar and quad models and uploads them to the render system.
    /// </summary>
    public static void Create()
    {
        var pillar = CubeMeshUtil.GetCube(0.1f, ApiEx.Client.World.BlockAccessor.MapSizeY, new Vec3f());
        PillarMeshRef = ApiEx.Client.Render.UploadMesh(pillar);
        QuadModel = ApiEx.Client.Render.UploadMesh(QuadMeshUtil.GetQuad());
    }
}