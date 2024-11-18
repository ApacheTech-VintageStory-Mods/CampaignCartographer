using Vintagestory.API.MathTools;

namespace ApacheTech.VintageMods.CampaignCartographer.Domain.GuiComponents;

public class CapturedMethods
{
    private readonly ICoreClientAPI _capi = new ClientCoreAPI(null);
    private readonly BlockPos _startPos = new(Dimensions.NormalWorld);

    private BlockPos _blocks;

    public TextCommandResult WaypointTranslocator(TextCommandCallingArgs args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        if (_capi.World.Player.CurrentBlockSelection == null)
            return TextCommandResult.Success("No object selected");
        _blocks = _capi.World.Player.CurrentBlockSelection.Position;
        var blocks = _blocks;
        var path = _capi.World.BlockAccessor.GetBlock(_blocks).Code.Path;
        if (path.Contains("translocator-normal"))
        {
            var blockEntity = _capi.World.BlockAccessor.GetBlockEntity(blocks);
            var obj = blockEntity.GetType().GetField("tpLocation")!.GetValue(blockEntity);
            var separator = new[] { ", " };
            var strArray = obj!.ToString()!.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var blockPos2 = new BlockPos(int.Parse(strArray[0]), int.Parse(strArray[1]), int.Parse(strArray[2]), Dimensions.NormalWorld).AddCopy(-_startPos.X, 0, -_startPos.Z);
            var blockPos3 = blocks.AddCopy(-_startPos.X, 0, -_startPos.Z);
            _capi.SendChatMessage(string.Concat("/waypoint addati spiral ", blockPos3.X.ToString(), " ", blockPos3.Y.ToString(), " ", blockPos3.Z.ToString(), " false fuchsia Active Translocator (", blockPos3.X.ToString(), ",", blockPos3.Y.ToString(), ",", blockPos3.Z.ToString(), ") to (", blockPos2.X.ToString(), ",", blockPos2.Y.ToString(), ",", blockPos2.Z.ToString(), ")"));
            _capi.SendChatMessage(string.Concat("/waypoint addati spiral ", blockPos2.X.ToString(), " ", blockPos2.Y.ToString(), " ", blockPos2.Z.ToString(), " false fuchsia Active Translocator (", blockPos2.X.ToString(), ",", blockPos2.Y.ToString(), ",", blockPos2.Z.ToString(), ") to (", blockPos3.X.ToString(), ",", blockPos3.Y.ToString(), ",", blockPos3.Z.ToString(), ")"));
            return TextCommandResult.Success(string.Concat("Teleports to ", blockPos2.ToString()));
        }

        if (!path.Contains("translocator-broken"))
            return TextCommandResult.Success(path.Contains("translocator-unrepairable")
                ? "Translocator is unrepairable"
                : "Selected object is not a Translocator");

        var blockPos4 = blocks.AddCopy(-_startPos.X, 0, -_startPos.Z);
        _capi.SendChatMessage(string.Concat("/waypoint addati spiral ", blockPos4.X.ToString(), " ", blockPos4.Y.ToString(), " ", blockPos4.Z.ToString(), " false red Inactive Translocator at (", blockPos4.X.ToString(), ",", blockPos4.Y.ToString(), ",", blockPos4.Z.ToString(), ")"));
        return TextCommandResult.Success("Repairable Translocator found");

    }

}