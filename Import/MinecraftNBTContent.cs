using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using ASCReader;

public class MinecraftNBTContent {

	static int instanceNo = 0;
	
	public abstract class Container {

		protected int no;
		
		public Container() {
			instanceNo++;
			no = instanceNo;
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
				//Program.writeError("WTF?");
				key = "WTF-" + new Random().Next(1000, 9999);
			}*/
			if(!cont.ContainsKey(key)) {
				//Program.writeLineSpecial("Adding to #" + no);

				cont.Add(key, value);
			} else {
				//Program.writeError("Failed to add key '" + key + "'! It already exists.");
			}
		}

		public object Get(string key) {
			if(cont.ContainsKey(key)) {
				return cont[key];
			} else {
				return null;
			}
		}

		public CompoundContainer GetAsCompound(string key) {
			return (CompoundContainer)Get(key);
		}

		public ListContainer GetAsList(string key) {
			return (ListContainer)Get(key);
		}
	}

	public class ListContainer : Container {

		public override NBTTag containerType {
			get { return NBTTag.TAG_List; }
		}

		public NBTTag contentsType;

		public List<object> cont;

		public ListContainer(NBTTag baseType, int cap) : base() {
			contentsType = baseType;
			cont = new List<object>();
			/*for(int i = 0; i < cap; i++) {
				cont.Add(null);
			}*/
		}

		public ListContainer(NBTTag baseType) : this(baseType, 0) {
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

	public CompoundContainer contents;

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

	public MinecraftNBTContent(byte[] nbt) {
		contents = new CompoundContainer();
		parentTree = new List<Container>();
		currentCompound = contents;
		int i = 0;
		while(i < nbt.Length) {
			RegisterTag(nbt, currentCompound, ref i);
		}
		//Program.writeLine("NBT Loaded!");
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
					//Program.writeWarning("NL=" + nameLength + "! Something is going wrong");
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

	short[] GetHeightmap(byte[] data, Container c, ref int i) {
		string bits = "";
		string bin = "";
		for(int j = 0; j < 288; j++) {
			byte b = Get<byte>(data, ref i);
			string binStr = ByteToBigEndianBinary(b);
			//binStr = ReverseString(binStr);
			bits += binStr;
			/*for(int k = 0; k < 8; k++) {
				bits += binStr[k];
			}*/
			bin += (char)b;
		}

		short[] hms = new short[256];
		for(int j = 0; j < 256; j++) {
			string binStr = bits.Substring(j*9,9);
			hms[j] = Convert.ToInt16(binStr, 2);
		}
		return hms;
	}

	ListContainer GetList(NBTTag tag, int length, byte[] data, ref int i) {
		ListContainer arr = new ListContainer(tag, length);
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

	byte[] Reverse(byte[] input) {
		if(BitConverter.IsLittleEndian) Array.Reverse(input);
		return input;
	}

	/*Container EnterContainer(Container current, Container value) {
		parentTree.Add(current);
		return value;
	}

	Container ExitContainer() {
		var tree = parentTree[parentTree.Count - 2];
		parentTree.RemoveAt(parentTree.Count - 1);
		return tree;
	}*/

	string ByteToBigEndianBinary(byte b) {
		switch(b) {
			case 0: return		"00000000";
			case 1: return 		"10000000";
			case 2: return 		"01000000";
			case 3: return 		"11000000";
			case 4: return 		"00100000";
			case 5: return 		"10100000";
			case 6: return 		"01100000";
			case 7: return		"11100000";
			case 8: return 		"00010000";
			case 9: return		"10010000";
			case 10: return 	"01010000";
			case 11: return 	"11010000";
			case 12: return 	"00110000";
			case 13: return 	"10110000";
			case 14: return 	"01110000";
			case 15: return 	"11110000";
			case 16: return 	"00001000";
			case 17: return 	"10001000";
			case 18: return 	"01001000";
			case 19: return 	"11001000";
			case 20: return 	"00101000";
			case 21: return 	"10101000";
			case 22: return 	"01101000";
			case 23: return 	"11101000";
			case 24: return 	"00011000";
			case 25: return 	"10011000";
			case 26: return 	"01011000";
			case 27: return 	"11011000";
			case 28: return 	"00111000";
			case 29: return 	"10111000";
			case 30: return 	"01111000";
			case 31: return 	"11111000";
			case 32: return 	"00000100";
			case 33: return 	"10000100";
			case 34: return 	"01000100";
			case 35: return 	"11000100";
			case 36: return 	"00100100";
			case 37: return 	"10100100";
			case 38: return 	"01100100";
			case 39: return 	"11100100";
			case 40: return 	"00010100";
			case 41: return 	"10010100";
			case 42: return 	"01010100";
			case 43: return 	"11010100";
			case 44: return 	"00110100";
			case 45: return 	"10110100";
			case 46: return 	"01110100";
			case 47: return 	"11110100";
			case 48: return 	"00001100";
			case 49: return 	"10001100";
			case 50: return 	"01001100";
			case 51: return 	"11001100";
			case 52: return 	"00101100";
			case 53: return 	"10101100";
			case 54: return 	"01101100";
			case 55: return 	"11101100";
			case 56: return 	"00011100";
			case 57: return 	"10011100";
			case 58: return 	"01011100";
			case 59: return 	"11011100";
			case 60: return 	"00111100";
			case 61: return 	"10111100";
			case 62: return 	"01111100";
			case 63: return 	"11111100";
			case 64: return 	"00000010";
			case 65: return 	"10000010";
			case 66: return 	"01000010";
			case 67: return 	"11000010";
			case 68: return 	"00100010";
			case 69: return 	"10100010";
			case 70: return 	"01100010";
			case 71: return 	"11100010";
			case 72: return 	"00010010";
			case 73: return 	"10010010";
			case 74: return 	"01010010";
			case 75: return 	"11010010";
			case 76: return 	"00110010";
			case 77: return 	"10110010";
			case 78: return 	"01110010";
			case 79: return 	"11110010";
			case 80: return 	"00001010";
			case 81: return 	"10001010";
			case 82: return 	"01001010";
			case 83: return 	"11001010";
			case 84: return 	"00101010";
			case 85: return 	"10101010";
			case 86: return 	"01101010";
			case 87: return 	"11101010";
			case 88: return 	"00011010";
			case 89: return 	"10011010";
			case 90: return 	"01011010";
			case 91: return 	"11011010";
			case 92: return 	"00111010";
			case 93: return 	"10111010";
			case 94: return 	"01111010";
			case 95: return 	"11111010";
			case 96: return 	"00000110";
			case 97: return 	"10000110";
			case 98: return 	"01000110";
			case 99: return 	"11000110";
			case 100: return	"00100110";
			case 101: return	"10100110";
			case 102: return	"01100110";
			case 103: return	"11100110";
			case 104: return	"00010110";
			case 105: return	"10010110";
			case 106: return	"01010110";
			case 107: return	"11010110";
			case 108: return	"00110110";
			case 109: return	"10110110";
			case 110: return	"01110110";
			case 111: return	"11110110";
			case 112: return	"00001110";
			case 113: return	"10001110";
			case 114: return	"01001110";
			case 115: return	"11001110";
			case 116: return	"00101110";
			case 117: return	"10101110";
			case 118: return	"01101110";
			case 119: return	"11101110";
			case 120: return	"00011110";
			case 121: return	"10011110";
			case 122: return	"01011110";
			case 123: return	"11011110";
			case 124: return	"00111110";
			case 125: return	"10111110";
			case 126: return	"01111110";
			case 127: return	"11111110";
			case 128: return	"00000001";
			case 129: return	"10000001";
			case 130: return	"01000001";
			case 131: return	"11000001";
			case 132: return	"00100001";
			case 133: return	"10100001";
			case 134: return	"01100001";
			case 135: return	"11100001";
			case 136: return	"00010001";
			case 137: return	"10010001";
			case 138: return	"01010001";
			case 139: return	"11010001";
			case 140: return	"00110001";
			case 141: return	"10110001";
			case 142: return	"01110001";
			case 143: return	"11110001";
			case 144: return	"00001001";
			case 145: return	"10001001";
			case 146: return	"01001001";
			case 147: return	"11001001";
			case 148: return	"00101001";
			case 149: return	"10101001";
			case 150: return	"01101001";
			case 151: return	"11101001";
			case 152: return	"00011001";
			case 153: return	"10011001";
			case 154: return	"01011001";
			case 155: return	"11011001";
			case 156: return	"00111001";
			case 157: return	"10111001";
			case 158: return	"01111001";
			case 159: return	"11111001";
			case 160: return	"00000101";
			case 161: return	"10000101";
			case 162: return	"01000101";
			case 163: return	"11000101";
			case 164: return	"00100101";
			case 165: return	"10100101";
			case 166: return	"01100101";
			case 167: return	"11100101";
			case 168: return	"00010101";
			case 169: return	"10010101";
			case 170: return	"01010101";
			case 171: return	"11010101";
			case 172: return	"00110101";
			case 173: return	"10110101";
			case 174: return	"01110101";
			case 175: return	"11110101";
			case 176: return	"00001101";
			case 177: return	"10001101";
			case 178: return	"01001101";
			case 179: return	"11001101";
			case 180: return	"00101101";
			case 181: return	"10101101";
			case 182: return	"01101101";
			case 183: return	"11101101";
			case 184: return	"00011101";
			case 185: return	"10011101";
			case 186: return	"01011101";
			case 187: return	"11011101";
			case 188: return	"00111101";
			case 189: return	"10111101";
			case 190: return	"01111101";
			case 191: return	"11111101";
			case 192: return	"00000011";
			case 193: return	"10000011";
			case 194: return	"01000011";
			case 195: return	"11000011";
			case 196: return	"00100011";
			case 197: return	"10100011";
			case 198: return	"01100011";
			case 199: return	"11100011";
			case 200: return	"00010011";
			case 201: return	"10010011";
			case 202: return	"01010011";
			case 203: return	"11010011";
			case 204: return	"00110011";
			case 205: return	"10110011";
			case 206: return	"01110011";
			case 207: return	"11110011";
			case 208: return	"00001011";
			case 209: return	"10001011";
			case 210: return	"01001011";
			case 211: return	"11001011";
			case 212: return	"00101011";
			case 213: return	"10101011";
			case 214: return	"01101011";
			case 215: return	"11101011";
			case 216: return	"00011011";
			case 217: return	"10011011";
			case 218: return	"01011011";
			case 219: return	"11011011";
			case 220: return	"00111011";
			case 221: return	"10111011";
			case 222: return	"01111011";
			case 223: return	"11111011";
			case 224: return	"00000111";
			case 225: return	"10000111";
			case 226: return	"01000111";
			case 227: return	"11000111";
			case 228: return	"00100111";
			case 229: return	"10100111";
			case 230: return	"01100111";
			case 231: return	"11100111";
			case 232: return	"00010111";
			case 233: return	"10010111";
			case 234: return	"01010111";
			case 235: return	"11010111";
			case 236: return	"00110111";
			case 237: return	"10110111";
			case 238: return	"01110111";
			case 239: return	"11110111";
			case 240: return	"00001111";
			case 241: return	"10001111";
			case 242: return	"01001111";
			case 243: return	"11001111";
			case 244: return	"00101111";
			case 245: return	"10101111";
			case 246: return	"01101111";
			case 247: return	"11101111";
			case 248: return	"00011111";
			case 249: return	"10011111";
			case 250: return	"01011111";
			case 251: return	"11011111";
			case 252: return	"00111111";
			case 253: return	"10111111";
			case 254: return	"01111111";
			case 255: return	"11111111";
			default: return "";
		}
	}
}