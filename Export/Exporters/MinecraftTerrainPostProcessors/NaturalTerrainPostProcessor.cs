using ASCReader.Export.Exporters;

public class NaturalTerrainPostProcessor : IMinecraftTerrainPostProcessor
{

	public int waterLevel = -1;

	public NaturalTerrainPostProcessor(bool fillWithWater) {
		waterLevel = fillWithWater ? 62 : -1;
	}

	public void ProcessBlock(MinecraftRegionExporter region, int x, int y, int z) {
		//Make flat bedrock
		if(y == 0) {
			if(region.IsDefaultBlock(x,0,z)) region.SetBlock(x,0,z,"minecraft:bedrock");
		}
		//Fill the terrain with water up to the waterLevel
		if(y <= waterLevel) {
			if(region.IsAir(x,y,z)) region.SetBlock(x,y,z,"minecraft:water");
		}
	}

	public void ProcessSurface(MinecraftRegionExporter region, int x, int y, int z) {
		//Place grass on top & 3 layers of dirt below
		if(y > waterLevel+1) {
			region.SetBlock(x,y,z,"minecraft:grass_block");
			for(int i = 1; i < 4; i++) {
				region.SetBlock(x,y-i,z,"minecraft:dirt");
			}
		} else {
			for(int i = 0; i < 4; i++) {
				region.SetBlock(x,y-i,z,"minecraft:gravel");
			}
		}
	}

	public void OnFinish(MinecraftRegionExporter region) {

	}
}