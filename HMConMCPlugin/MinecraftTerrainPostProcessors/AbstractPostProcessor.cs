using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{

	public enum PostProcessType
	{
		RegionOnly,
		Block,
		Surface,
		Both
	}

	public enum Priority
	{
		First = 0,
		AfterFirst = 1,
		BeforeDefault = 2,
		Default = 3,
		AfterDefault = 4,
		BeforeLast = 5,
		Last = 6
	}

	public abstract class AbstractPostProcessor
	{

		protected static Random random = new Random();

		public virtual Priority OrderPriority => Priority.Default;

		public abstract PostProcessType PostProcessorType { get; }

		public virtual int BlockProcessYMin => 0;
		public virtual int BlockProcessYMax => 255;

		public virtual int NumberOfPasses => 1;

		protected int worldOriginOffsetX;
		protected int worldOriginOffsetZ;

		public Weightmap<float> mask = null;

		public AbstractPostProcessor(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ)
		{
			worldOriginOffsetX = offsetX;
			worldOriginOffsetZ = offsetZ;
			var maskElem = xml.Element("mask");
			if (maskElem != null)
			{
				string maskPath = Path.Combine(rootPath, maskElem.Value);
				var channelAttr = maskElem.Attribute("channel");
				ColorChannel channel;
				string attr = channelAttr?.Value.ToLower();
				switch (attr)
				{
					case "r":
					case "red": channel = ColorChannel.Red; break;
					case "g":
					case "green": channel = ColorChannel.Green; break;
					case "b":
					case "blue": channel = ColorChannel.Blue; break;
					case "a":
					case "alpha": channel = ColorChannel.Alpha; break;
					default: channel = ColorChannel.Red; break;
				}
				mask = Weightmap<float>.CreateSingleChannelMap(maskPath, channel, 0, 0, sizeX, sizeZ);
			}
		}

		protected Weightmap<float> LoadWeightmapAndLayers(string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ, Dictionary<int, Layer> layers, Func<XElement, Layer> createLayerAction)
		{
			if (layers == null) layers = new Dictionary<int, Layer>();
			var map = xml.Element("weightmap");
			if (map != null)
			{
				string mapFileName = Path.Combine(rootPath, xml.Attribute("file").Value);
				var weightmap = Weightmap<float>.CreateRGBAMap(mapFileName, offsetX, offsetZ, sizeX, sizeZ);
				foreach (var elem in map.Elements())
				{
					string name = elem.Name.LocalName.ToLower();
					if (name == "r" || name == "red")
					{
						RegisterLayer(0, layers, createLayerAction, elem);
					}
					else if (name == "g" || name == "green")
					{
						RegisterLayer(1, layers, createLayerAction, elem);
					}
					else if (name == "b" || name == "blue")
					{
						RegisterLayer(2, layers, createLayerAction, elem);
					}
					else if (name == "a" || name == "alpha")
					{
						RegisterLayer(3, layers, createLayerAction, elem);
					}
					else if (name == "n" || name == "none")
					{
						RegisterLayer(-1, layers, createLayerAction, elem);
					}
					else
					{
						throw new ArgumentException("Unknown channel name: " + name);
					}
				}
				return weightmap;
			}
			else
			{
				return null;
			}
		}

		private void RegisterLayer(int maskChannelIndex, Dictionary<int, Layer> layers, Func<XElement, Layer> createLayerAction, XElement elem)
		{
			layers.Add(maskChannelIndex, createLayerAction(elem));
		}

		public void ProcessBlock(World world, int x, int y, int z, int pass)
		{
			float maskValue = mask != null ? mask.GetValue(x - worldOriginOffsetX, z - worldOriginOffsetZ) : 1;
			if (maskValue > 0)
			{
				OnProcessBlock(world, x, y, z, pass, maskValue);
			}
		}

		public void ProcessSurface(World world, int x, int y, int z, int pass)
		{
			float maskValue = mask != null ? mask.GetValue(x - worldOriginOffsetX, z - worldOriginOffsetZ) : 1;
			if (maskValue > 0)
			{
				OnProcessSurface(world, x, y, z, pass, maskValue);
			}
		}

		protected void ProcessSplatmapLayersSurface(Dictionary<int, Layer> layers, Weightmap<float> weightmap, World world, int x, int y, int z, int pass, float mask)
		{
			foreach (var l in layers)
			{
				if (l.Key > -1)
				{
					mask *= weightmap.GetValue(x, z, l.Key);
				}
				if (mask > 0.001f)
				{
					l.Value.ProcessBlockColumn(world, random, x, y, z, mask);
				}
			}
		}

		protected virtual void OnProcessBlock(World world, int x, int y, int z, int pass, float mask)
		{

		}

		protected virtual void OnProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{

		}

		public virtual void ProcessRegion(World world, Region reg, int rx, int rz, int pass)
		{

		}

		public virtual void OnFinish(World world)
		{

		}
	}
}