using ASCReader.Export.Exporters;

public class BedrockPostProcessor : MinecraftTerrainPostProcessor {

    public override void ProcessBlock(MinecraftRegionExporter region, int x, int y, int z) {
        if(y == 0) region.SetBlock(x,0,z,"bedrock");
    }

}