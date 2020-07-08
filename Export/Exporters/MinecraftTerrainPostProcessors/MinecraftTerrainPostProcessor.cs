using ASCReader.Export.Exporters;

public abstract class MinecraftTerrainPostProcessor {
	
	public virtual int RequiredPasses {
		get{ return 1; }
	}

	public virtual void ProcessBlock(MinecraftRegionExporter region, int x, int y, int z) {

	}

	public virtual void ProcessSurface(MinecraftRegionExporter region, int x, int y, int z) {

	}

	public virtual void OnFinish(MinecraftRegionExporter region) {

	}
}