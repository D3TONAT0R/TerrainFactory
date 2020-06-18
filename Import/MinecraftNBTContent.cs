using System;
using System.Collections.Generic;
using System.Text;
using ASCReader;

public class MinecraftNBTContent {
	
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
		TAG_Long_Array = 12
	}

	public Dictionary<string,object> content;

 	List<Dictionary<string,object>> parentTree;
	Dictionary<string,object> compound;

	public MinecraftNBTContent() {
		content = new Dictionary<string, object>();
		parentTree = new List<Dictionary<string, object>>();
		compound = content;
	}

	public MinecraftNBTContent(byte[] nbt) : this() {
		int i = 0;
		Dictionary<string,object> compound = content;
		while(i < nbt.Length) {
			RegisterTag(nbt, ref i);
		}
		Program.WriteLine("NBT Loaded!");
	}

	void RegisterTag(byte[] data, ref int i) {
		NBTTag tag = (NBTTag)data[i];
		i++;
		if(tag != NBTTag.TAG_End) {
			int nameLength = BitConverter.ToInt32(new byte[]{data[i],data[i+1]});
			i += 2;
			string name = "";
			for(int j = 0; j < nameLength; j++) {
				name += (char)data[i+j];
			}
			i += nameLength;
			object value = null;
			if(tag == NBTTag.TAG_Byte) {
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
				value = Get<object[]>(data, ref i);
			} else if(tag == NBTTag.TAG_Compound) {
				value = Get<Dictionary<string,object>>(data, ref i);
			} else if(tag == NBTTag.TAG_Int_Array) {
				value = Get<int[]>(data, ref i);
			} else if(tag == NBTTag.TAG_Long_Array) {
				value = Get<long[]>(data, ref i);
			}
			compound.Add(name, content);
			if(tag == NBTTag.TAG_Compound) {
				parentTree.Add(compound);
				compound = (Dictionary<string,object>)value;
			}
		} else {
			compound = parentTree[parentTree.Count-1];
			parentTree.RemoveAt(parentTree.Count-1);
		}
	}

	object[] GetList(NBTTag tag, int length, byte[] data, ref int i) {
		object[] arr = new object[length];
		for(int j = 0; j < length; j++) {
			switch(tag) {
				case NBTTag.TAG_End: return null;
				case NBTTag.TAG_Byte: arr[i] = Get<byte>(data, ref i); break;
				case NBTTag.TAG_Short: arr[i] = Get<short>(data, ref i); break;
				case NBTTag.TAG_Int: arr[i] = Get<int>(data, ref i); break;
				case NBTTag.TAG_Long: arr[i] = Get<long>(data, ref i); break;
				case NBTTag.TAG_Float: arr[i] = Get<float>(data, ref i); break;
				case NBTTag.TAG_Double: arr[i] = Get<double>(data, ref i); break;
				case NBTTag.TAG_Byte_Array: arr[i] = Get<byte[]>(data, ref i); break;
				case NBTTag.TAG_String: arr[i] = Get<string>(data, ref i); break;
				case NBTTag.TAG_List: arr[i] = Get<object[]>(data, ref i); break;
				case NBTTag.TAG_Compound: arr[i] = Get<Dictionary<string,object>>(data, ref i); break;
				case NBTTag.TAG_Int_Array: arr[i] = Get<int[]>(data, ref i); break;
				case NBTTag.TAG_Long_Array: arr[i] = Get<long[]>(data, ref i); break;
				default: return null;
			}
		}
		return arr;
	}

	Type GetTypeOfTag(NBTTag tag) {
		switch(tag) {
			case NBTTag.TAG_End: return null;
			case NBTTag.TAG_Byte: return typeof(byte);
			case NBTTag.TAG_Short: return typeof(short);
			case NBTTag.TAG_Int: return typeof(int);
			case NBTTag.TAG_Long: return typeof(long);
			case NBTTag.TAG_Float: return typeof(float);
			case NBTTag.TAG_Double: return typeof(double);
			case NBTTag.TAG_Byte_Array: return typeof(byte[]);
			case NBTTag.TAG_String: return typeof(string);
			case NBTTag.TAG_List: return typeof(object[]);
			case NBTTag.TAG_Compound: return typeof(Dictionary<string,object>);
			case NBTTag.TAG_Int_Array: return typeof(int[]);
			case NBTTag.TAG_Long_Array: return typeof(long[]);
			default: return null;
		}
	}

	T Get<T>(byte[] data, ref int i) {
		object ret = null;
		if(typeof(T) == typeof(byte)) {
			ret = data[i];
			i++;
		} else if(typeof(T) == typeof(byte)) {
			ret = BitConverter.ToInt16(new byte[]{data[i], data[i+1]});
			i += 2;
		} else if(typeof(T) == typeof(int)) {
			ret = BitConverter.ToInt32(new byte[]{data[i], data[i+1], data[i+2], data[i+3]});
			i += 4;
		} else if(typeof(T) == typeof(long)) {
			ret = BitConverter.ToInt64(new byte[]{data[i], data[i+1], data[i+2], data[i+3], data[i+4], data[i+5], data[i+6], data[i+7]});
			i += 8;
		} else if(typeof(T) == typeof(float)) {
			ret = BitConverter.ToSingle(new byte[]{data[i], data[i+1], data[i+2], data[i+3]});
			i += 4;
		} else if(typeof(T) == typeof(double)) {
			ret = BitConverter.ToDouble(new byte[]{data[i], data[i+1], data[i+2], data[i+3], data[i+4], data[i+5], data[i+6], data[i+7]});
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
		} else if(typeof(T) == typeof(object[])) {
			NBTTag type = (NBTTag)Get<byte>(data, ref i);
			int len = Get<int>(data, ref i);
			ret = GetList(type, len, data, ref i);
		}
		return (T)Convert.ChangeType(ret, typeof(T));
	}
}