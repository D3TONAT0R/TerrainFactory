using ASCReader.Export.Exporters;

public interface IMinecraftTerrainPostProcessor {
	
	public void ProcessBlock(MinecraftRegionExporter region, int x, int y, int z);

	public void ProcessSurface(MinecraftRegionExporter region, int x, int y, int z);

	public void OnFinish(MinecraftRegionExporter region);
}