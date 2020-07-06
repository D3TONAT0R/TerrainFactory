using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Encodings.Web;
using ASCReader;

public class MinecraftNBTContent {

	public abstract class Container {

		public Container() {

		}

		public abstract NBTTag containerType {
			get;
		}

		public abstract void Add(string key, object value);
	}

	public class CompoundContainer : Container {

		public override NBTTag containerType {
			get { return NBTTag.TAG_Compound; }
		}

		public Dictionary<string, object> cont;

		public CompoundContainer() : base() {
			cont = new Dictionary<string, object>();
		}

		public override void Add(string key, object value) {
			/*if(key == null || key == "") {
				Program.WriteError("WTF?");
				key = "WTF-" + new Random().Next(1000, 9999);
			}*/
			if(!cont.ContainsKey(key)) {
				cont.Add(key, value);
			} else {
				cont[key] = value;
			}
		}

		public object Get(string key) {
			if(cont.ContainsKey(key)) {
				return cont[key];
			} else {
				return null;
			}
		}

		public bool Contains(string key) {
			return cont.ContainsKey(key);
		}

		public CompoundContainer GetAsCompound(string key) {
			return (CompoundContainer)Get(key);
		}

		public ListContainer GetAsList(string key) {
			return (ListContainer)Get(key);
		}

		public bool HasSameContent(CompoundContainer other) {
			if(other.cont.Keys.Count != cont.Keys.Count) return false;
			foreach(string k in cont.Keys) {
				if(!other.Contains(k)) return false;
				if(!cont[k].Equals(other.cont[k])) return false;
			}
			return true;
		}
	}

	public class ListContainer : Container {

		public override NBTTag containerType {
			get { return NBTTag.TAG_List; }
		}

		public NBTTag contentsType;
		public int Length {
			get {
				return cont.Count;
			}
		}
		public List<object> cont;

		public ListContainer(NBTTag baseType) : base() {
			contentsType = baseType;
			cont = new List<object>();
		}

		public override void Add(string key, object value) {
			//Program.writeLineSpecial("Adding to #" + no);
			cont.Add(value);
		}

		public object this[int i] {
			get { return cont[i]; }
			set { cont[i] = value; }
		}
	}

	public enum NBTTag {
		TAG_End = 0,
		TAG_Byte = 1,
		TAG_Short = 2,
		TAG_Int = 3,
		TAG_Long = 4,
		TAG_Float = 5,
		TAG_Double = 6,
		TAG_Byte_Array = 7,
		TAG_String = 8,
		TAG_List = 9,
		TAG_Compound = 10,
		TAG_Int_Array = 11,
		TAG_Long_Array = 12,
		UNSPECIFIED = 99
	}

	public static Dictionary<Type, NBTTag> NBTTagDictionary = new Dictionary<Type, NBTTag> {
			{ typeof(byte), NBTTag.TAG_Byte },
			{ typeof(short), NBTTag.TAG_Short },
			{ typeof(int), NBTTag.TAG_Int },
			{ typeof(long), NBTTag.TAG_Long },
			{ typeof(float), NBTTag.TAG_Float },
			{ typeof(double), NBTTag.TAG_Double },
			{ typeof(byte[]), NBTTag.TAG_Byte_Array },
			{ typeof(string), NBTTag.TAG_String },
			{ typeof(ListContainer), NBTTag.TAG_List },
			{ typeof(CompoundContainer), NBTTag.TAG_Compound },
			{ typeof(int[]), NBTTag.TAG_Int_Array },
			{ typeof(long[]), NBTTag.TAG_Long_Array }
		};

	public CompoundContainer contents;
	public int dataVersion;

	List<Container> parentTree;
	public Container currentCompound {
		get {
			return actualCompound;
		}
		set {
			
			if(actualCompound != null && value != null && actualCompound.GetType() == typeof(ListContainer) && value.GetType() == typeof(CompoundContainer)) {
				//Program.writeWarning("CHANGE");
			}
			actualCompound = value;
		}
	}
	Container actualCompound;

	public MinecraftNBTContent() {
		contents = new CompoundContainer();
		parentTree = new List<Container>();
		currentCompound = contents;
	}

	public MinecraftNBTContent(byte[] nbt) : this() {
		int i = 0;
		while(i < nbt.Length) {
			RegisterTag(nbt, currentCompound, ref i);
		}
		var root = contents.GetAsCompound("");
		if(root != null) {
			//Remove the unnessecary root compound and unpack the level compound
			foreach(string k in root.cont.Keys) {
				contents.Add(k, root.cont[k]);
			}
			contents.cont.Remove("");
		}
		if(contents.Contains("DataVersion")) dataVersion = (int)contents.Get("DataVersion");
		if(contents.Contains("Level")) {
			var level = contents.GetAsCompound("Level");
			foreach(string k in level.cont.Keys) {
				contents.Add(k, level.cont[k]);
			}
			contents.cont.Remove("Level");
		}
		//Program.writeLine("NBT Loaded!");
		var chunk = new MinecraftChunkData(this);
	}

	NBTTag RegisterTag(byte[] data, Container c, ref int i) {
		return RegisterTag(NBTTag.UNSPECIFIED, data, c, ref i);
	}

	NBTTag RegisterTag(NBTTag predef, byte[] data, Container c, ref int i) {
		NBTTag tag;
		/*if(compound.GetType() == typeof(ListContainer)) {
			tag = ((ListContainer)compound).containerType;
			i++;
		} else {*/
		if(predef == NBTTag.UNSPECIFIED) {
			tag = (NBTTag)data[i];
			i++;
		} else {
			tag = predef;
		}       
		//}
		object value = null;
		if(tag != NBTTag.TAG_End) {
			string name = "";
			if(predef == NBTTag.UNSPECIFIED) {
				short nameLength = BitConverter.ToInt16(Reverse(new byte[] { data[i], data[i + 1] }));
				if(nameLength > 64) {
					Program.WriteWarning("NL=" + nameLength + "! Something is going wrong");
				}
				i += 2;
				for(int j = 0; j < nameLength; j++) {
					name += (char)data[i + j];
				}
				i += nameLength;
			}
			/*if(name == "MOTION_BLOCKING" || name == "MOTION_BLOCKING_NO_LEAVES" || name == "OCEAN_FLOOR" || name == "WORLD_SURFACE") {
				Get<int>(data, ref i); //Throw away the length int, it's always 36
				value = GetHeightmap(data, c, ref i);
			} else */if(tag == NBTTag.TAG_Byte) {
				value = Get<byte>(data, ref i);
			} else if(tag == NBTTag.TAG_Short) {
				value = Get<short>(data, ref i);
			} else if(tag == NBTTag.TAG_Int) {
				value = Get<int>(data, ref i);
			} else if(tag == NBTTag.TAG_Long) {
				value = Get<long>(data, ref i);
			} else if(tag == NBTTag.TAG_Float) {
				value = Get<float>(data, ref i);
			} else if(tag == NBTTag.TAG_Double) {
				value = Get<double>(data, ref i);
			} else if(tag == NBTTag.TAG_Byte_Array) {
				value = Get<byte[]>(data, ref i);
			} else if(tag == NBTTag.TAG_String) {
				value = Get<string>(data, ref i);
			} else if(tag == NBTTag.TAG_List) {
				value = Get<ListContainer>(data, ref i);
			} else if(tag == NBTTag.TAG_Compound) {
				value = Get<CompoundContainer>(data, ref i);
			} else if(tag == NBTTag.TAG_Int_Array) {
				value = Get<int[]>(data, ref i);
			} else if(tag == NBTTag.TAG_Long_Array) {
				value = Get<long[]>(data, ref i);
			}
			c.Add(name, value);
			LogTree(tag, name, value);
		} else {
			
			//ExitContainer();
		}
		return tag;
	}

	ListContainer GetList(NBTTag tag, int length, byte[] data, ref int i) {
		ListContainer arr = new ListContainer(tag);
		//compound = EnterContainer(compound, arr);
		for(int j = 0; j < length; j++) {
			RegisterTag(tag, data, arr, ref i);
		}
		//compound = ExitContainer();
		return arr;
	}

	void LogTree(NBTTag tag, string name, object value) {
		string tree = "";
		for(int t = 0; t < parentTree.Count; t++) {
			tree += " > ";
		}
		if(name == "") name = "[LIST_ENTRY]";
		string vs = value != null ? value.ToString() : "-";
		if(vs.Length > 64) vs = vs.Substring(0, 60) + "[...]";
		tree += tag != NBTTag.TAG_End ? name + ": " + tag.ToString() + " = " + vs : "END";
		//Program.writeLine(tree);
	}

	T Get<T>(byte[] data, ref int i) {
		object ret = null;
		if(typeof(T) == typeof(byte)) {
			ret = data[i];
			i++;
		} else if(typeof(T) == typeof(short)) {
			ret = BitConverter.ToInt16(Reverse(new byte[] { data[i], data[i + 1] }));
			i += 2;
		} else if(typeof(T) == typeof(int)) {
			ret = BitConverter.ToInt32(Reverse(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3] }));
			i += 4;
		} else if(typeof(T) == typeof(long)) {
			ret = BitConverter.ToInt64(Reverse(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3], data[i + 4], data[i + 5], data[i + 6], data[i + 7] }));
			i += 8;
		} else if(typeof(T) == typeof(float)) {
			ret = BitConverter.ToSingle(Reverse(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3] }));
			i += 4;
		} else if(typeof(T) == typeof(double)) {
			ret = BitConverter.ToDouble(Reverse(new byte[] { data[i], data[i + 1], data[i + 2], data[i + 3], data[i + 4], data[i + 5], data[i + 6], data[i + 7] }));
			i += 8;
		} else if(typeof(T) == typeof(byte[])) {
			int len = Get<int>(data, ref i);
			byte[] arr = new byte[len];
			for(int j = 0; j < len; j++) {
				arr[j] = Get<byte>(data, ref i);
			}
			ret = arr;
		} else if(typeof(T) == typeof(string)) {
			int len = Get<short>(data, ref i);
			byte[] arr = new byte[len];
			for(int j = 0; j < len; j++) {
				arr[j] = Get<byte>(data, ref i);
			}
			ret = Encoding.UTF8.GetString(arr);
		} else if(typeof(T) == typeof(ListContainer)) {
			NBTTag type = (NBTTag)Get<byte>(data, ref i);
			int len = Get<int>(data, ref i);
			ret = GetList(type, len, data, ref i);
		} else if(typeof(T) == typeof(CompoundContainer)) {
			var newCompound = new CompoundContainer();
			//compound = EnterContainer(compound, (Container)ret);
			while(RegisterTag(data, newCompound, ref i) != NBTTag.TAG_End) {

			}
			ret = newCompound;
			//compound = ExitContainer();
		} else if(typeof(T) == typeof(int[])) {
			int len = Get<int>(data, ref i);
			int[] arr = new int[len];
			for(int j = 0; j < len; j++) {
				arr[j] = Get<int>(data, ref i);
			}
			ret = arr;
		} else if(typeof(T) == typeof(long[])) {
			int len = Get<int>(data, ref i);
			long[] arr = new long[len];
			for(int j = 0; j < len; j++) {
				arr[j] = Get<long>(data, ref i);
			}
			ret = arr;
		}
		return (T)Convert.ChangeType(ret, typeof(T));
	}

	public void WriteToBytes(List<byte> bytes) {
		//Repackage into the original structure
		
		CompoundContainer root = new CompoundContainer();
		CompoundContainer level = new CompoundContainer();
		foreach(string k in contents.cont.Keys) {
			level.Add(k, contents.Get(k));
		}
		root.Add("Level", level);
		root.Add("DataVersion", 2504);
		Write(bytes, "", root);
	}

	void Write(List<byte> bytes, string name, object o) {
		var tag = NBTTagDictionary[o.GetType()];
		bytes.Add((byte)tag);
		byte[] nameBytes = Encoding.UTF8.GetBytes(name);
		byte[] lengthBytes = Reverse(BitConverter.GetBytes((short)nameBytes.Length));
		bytes.AddRange(lengthBytes);
		bytes.AddRange(nameBytes);
		WriteValue(bytes, tag, o);
	}

	void WriteValue(List<byte> bytes, NBTTag tag, object o) {
		if(tag == NBTTag.TAG_Byte) {
			bytes.Add((byte)o);
		} else if(tag == NBTTag.TAG_Short) {
			bytes.AddRange(Reverse(BitConverter.GetBytes((short)o)));
		} else if(tag == NBTTag.TAG_Int) {
			bytes.AddRange(Reverse(BitConverter.GetBytes((int)o)));
		} else if(tag == NBTTag.TAG_Long) {
			bytes.AddRange(Reverse(BitConverter.GetBytes((long)o)));
		} else if(tag == NBTTag.TAG_Float) {
			bytes.AddRange(Reverse(BitConverter.GetBytes((float)o)));
		} else if(tag == NBTTag.TAG_Double) {
			bytes.AddRange(Reverse(BitConverter.GetBytes((double)o)));
		} else if(tag == NBTTag.TAG_Byte_Array) {
			WriteValue(bytes, NBTTag.TAG_Int, ((byte[])o).Length);
			foreach(byte b in (byte[])o) {
				WriteValue(bytes, NBTTag.TAG_Byte, b);
			}
		} else if(tag == NBTTag.TAG_String) {
			byte[] utf8 = Encoding.UTF8.GetBytes((string)o);
			WriteValue(bytes, NBTTag.TAG_Short, (short)utf8.Length);
			bytes.AddRange(utf8);
		} else if(tag == NBTTag.TAG_List) {
			ListContainer list = (ListContainer)o;
			bytes.Add((byte)list.contentsType);
			WriteValue(bytes, NBTTag.TAG_Int, list.cont.Count);
			foreach(object item in list.cont) {
				WriteValue(bytes, list.contentsType, item);
			}
		} else if(tag == NBTTag.TAG_Compound) {
			CompoundContainer compound = (CompoundContainer)o;
			foreach(string k in compound.cont.Keys) {
				Write(bytes, k, compound.cont[k]);
			}
			bytes.Add((byte)NBTTag.TAG_End);
		} else if(tag == NBTTag.TAG_Int_Array) {
			WriteValue(bytes, NBTTag.TAG_Int, ((int[])o).Length);
			foreach(var item in (int[])o) {
				WriteValue(bytes, NBTTag.TAG_Int, item);
			}
		} else if(tag == NBTTag.TAG_Long_Array) {
			WriteValue(bytes, NBTTag.TAG_Int, ((long[])o).Length);
			foreach(var item in (long[])o) {
				WriteValue(bytes, NBTTag.TAG_Long, item);
			}
		}
	}

	byte[] Reverse(byte[] input) {
		if(BitConverter.IsLittleEndian) Array.Reverse(input);
		return input;
	}
}