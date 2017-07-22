using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using PersonaTextLib.Extensions;
using System.Windows.Media;
using System.ComponentModel;
using System.Xml.Linq;
using System.Globalization;
using System.Windows;

namespace PersonaTextLib.Types
{
    public delegate void ValueChangedEventHandler();

    public class FnMpImg
    {
        public int Index { get; set; } = 0;
        public string Char { get; set; } = "";
        public BitmapSource Image { get; set; }
    }

    public class FnMpData
    {
        public event ValueChangedEventHandler ValueChange;

        string _Char = "";

        public int Index { get; set; } = 0;
        public string Char
        {
            get { return _Char; }
            set
            {
                _Char = value;
                ValueChange?.Invoke();
            }
        }
        public byte[] Image_data { get; set; } = new byte[0];
        public MyByte Cut { get; set; }
    }

    public class CharList
    {
        public CharList(string Path, string Name)
        {
            List.ReadFNMP(Path + Name + ".TXT");
            this.ReadFONT(Path + Name + ".FNT");
        }

        public event ValueChangedEventHandler CharListChanged;

        public void Update()
        {
            CharListChanged?.Invoke();
        }

        public bool Save(string filename)
        {
            return List.WriteFNMP(filename);
        }

        public List<FnMpData> List { get; set; } = new List<FnMpData>();
        public BitmapPalette Palette { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public PixelFormat PixelFormat { get; set; }
    }

    public class MyByte
    {
        public byte Left { get; set; } = 0;
        public byte Right { get; set; } = 0;
    }

    public class BinaryReaderBE : BinaryReader
    {
        public BinaryReaderBE(Stream stream) : base(stream) { }

        public override short ReadInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public override ushort ReadUInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public override int ReadInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public override uint ReadUInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public override long ReadInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public override ulong ReadUInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }
    }

    public class BinaryWriterBE : BinaryWriter
    {
        public BinaryWriterBE(Stream stream) : base(stream) { }

        public override void Write(short value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            short newvalue = BitConverter.ToInt16(data, 0);
            base.Write(newvalue);
        }

        public override void Write(ushort value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            ushort newvalue = BitConverter.ToUInt16(data, 0);
            base.Write(newvalue);
        }

        public override void Write(int value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            int newvalue = BitConverter.ToInt32(data, 0);
            base.Write(newvalue);
        }

        public override void Write(uint value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            uint newvalue = BitConverter.ToUInt32(data, 0);
            base.Write(newvalue);
        }

        public override void Write(long value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            long newvalue = BitConverter.ToInt64(data, 0);
            base.Write(newvalue);
        }

        public override void Write(ulong value)
        {
            var data = BitConverter.GetBytes(value);
            Array.Reverse(data);
            ulong newvalue = BitConverter.ToUInt64(data, 0);
            base.Write(newvalue);
        }
    }

    public class Binary
    {
        public Binary(Stream stream, bool IsLittleEndian)
        {
            BaseStream = stream;

        }

        public Stream BaseStream { get; private set; }

        public BinaryReader Reader { get; private set; }

        public BinaryWriter Writer { get; private set; }
    }

    public class glyphYshift
    {
        public int Index { get; set; } = 0;
        public byte Shift { get; set; } = 0;
    }
}