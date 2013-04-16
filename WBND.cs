using s3pi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kuree.Sims3.WBND
{
    /// <summary>
    /// WBND Resource Wrapper For Sims3 Made by Kuree
    /// </summary>
    public class WBNDWrapper : AResource
    {


        #region AApiVersionedFields
        const Int32 recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        #endregion

        #region Attributes
        private UInt16 m_Version;
        private Int16 m_Type;
        private Int32 m_MapWidth;
        private Int32 m_MapHeight;
        private static Int32 m_BlockWidth;
        private static Int32 m_BlockHeight;
        private Int32 m_LayerCount;
        private Layer m_LayerList;
        private Int32 m_BlockSize;
        private Int32 m_BlockCount;
        private Byte zero;

        [ElementPriority(0)]
        public UInt16 Version { get { return m_Version; } set { m_Version = value; } }
        [ElementPriority(1)]
        public Int16 Type { get { return m_Type; } set { m_Type = value; } }
        [ElementPriority(2)]
        public Int32 MapWidth { get { return m_MapWidth; } }
        [ElementPriority(3)]
        public Int32 MapHeight { get { return m_MapHeight; } }
        [ElementPriority(4)]
        public static Int32 BlockWidth { get { return m_BlockWidth; } }
        [ElementPriority(5)]
        public static Int32 BlockHeight { get { return m_BlockHeight; }  }
        [ElementPriority(6)]
        public Layer LayerList { get { return m_LayerList; } set { m_LayerList = value; } }

        [ElementPriority(7)]
        public Int32 BlockCount { get { return m_BlockCount; } }
        [ElementPriority(8)]
        public Int32 BlockSize { get { return m_BlockSize; } }


        [ElementPriority(9)]
        public Int32 LayerCount { get { return m_LayerCount; } }
        [ElementPriority(10)]
        public string Value 
        { 
            get 
            { 
                StringBuilder s = new StringBuilder();
                s.Append(@"Version: ").Append(m_Version.ToString("X")).AppendLine();
                s.Append(@"Type: ").Append(m_Type.ToString("X")).AppendLine();
                s.Append(@"Map Width: ").Append(m_MapWidth.ToString("X")).AppendLine();
                s.Append(@"Map Height: ").Append(m_MapHeight.ToString("X")).AppendLine();
                s.Append(@"Block Width: ").Append(m_BlockWidth.ToString("X")).AppendLine();
                s.Append(@"Block Height: ").Append(m_BlockHeight.ToString("X")).AppendLine();
                s.AppendLine().AppendLine();
                s.Append(@"Layer Count: ").Append(m_LayerCount.ToString("X")).AppendLine();
                foreach (Block block in m_LayerList)
                {
                    s.AppendLine().Append(@"Block Number ".PadLeft(20, '-')).Append(block.BlockNumber.ToString("X").PadLeft(4, '0')).AppendLine();
                    s.Append(@"Is Compressed: ".PadLeft(22, ' ')).Append(block.Compressed.ToString()).AppendLine();
                }
                return s.ToString(); 
            } 
        }

        #endregion

        #region Data I/O
        private void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            this.m_Version = r.ReadUInt16();
            this.m_Type = r.ReadInt16();
            this.m_MapWidth = r.ReadInt32();
            this.m_MapHeight = r.ReadInt32();
            m_BlockWidth = r.ReadInt32();
            m_BlockHeight = r.ReadInt32();
            this.m_LayerCount = r.ReadInt32();
            this.m_LayerList = new Layer(OnResourceChanged);

            
            this.m_BlockSize = r.ReadInt32();
            this.m_BlockCount = r.ReadInt32();
            this.zero = r.ReadByte();
            for (int i = 0; i < m_BlockCount; i++)
            {
                this.m_LayerList.Add(new Block(recommendedApiVersion, OnResourceChanged, s));
            }

            
        }



        protected override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);

            w.Write(m_Version);
            w.Write(m_Type);
            w.Write(m_MapWidth);
            w.Write(m_MapHeight);
            w.Write(m_BlockWidth);
            w.Write(m_BlockHeight);
            w.Write(m_LayerCount);

            w.Write(m_BlockSize);
            w.Write(m_LayerList.Count);
            w.Write(zero);
            
            foreach (Block block in m_LayerList)
            {
                block.UnParse(s);
            }

            return s;

        }




        #endregion

        public WBNDWrapper(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } else { stream.Position = 0;  Parse(stream); } }


        #region Sub-Type


        public class Block : AHandlerElement, IEquatable<Block>
        {
            #region Attributes
            private Int16 m_BlockNumber;
            private CompressedFlag m_Compressed;
            private DataList m_DataList;
            private Int32 m_Size = BlockHeight * BlockWidth;

            public Int16 BlockNumber { get { return m_BlockNumber; } }
            public CompressedFlag Compressed { get { return m_Compressed; } }
            public DataList dataList { get { return m_DataList; } set { m_DataList = value; } }


            #endregion

            #region Constructors
            public Block(int APIversion, EventHandler handler, Stream s)
                : base(APIversion, handler)
            {

                Parse(s);

            }
            public Block(int APIversion, EventHandler handler, Block block)
                : base(APIversion, handler)
            {
                this.m_BlockNumber = block.m_BlockNumber;
                this.m_Compressed = block.m_Compressed;
                this.m_DataList = block.m_DataList;
            }
            #endregion

            #region Data I/O
            public void Parse(Stream s)
            {

                BinaryReader r = new BinaryReader(s);

                m_BlockNumber = Swap(r.ReadInt16());

                m_Compressed = (CompressedFlag)r.ReadByte();

                if (m_Compressed == CompressedFlag.Yes )
                {
                    this.m_DataList = new DataList(handler);
                    byte value = r.ReadByte();
                    for (int i = 0; i < m_Size; i++)
                    {

                        this.m_DataList.Add(new Data(recommendedApiVersion, handler, value));
                    }
                    
                }
                else
                {
                    this.m_DataList = new DataList(handler);

                    for (int i = 0; i < m_Size; i++)
                    {
                        this.m_DataList.Add(new Data(recommendedApiVersion, handler, s));
                    }
                }


            }
            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);

                w.Write(Swap(m_BlockNumber));

                bool test = false;

                for (int i = 0; i < m_Size - 1; i++)
                {
                    test = (m_DataList.ElementAt(i).DataValue == m_DataList.ElementAt(i + 1).DataValue);
                    if (!test)
                    {
                        break;
                    }
                }

                if (test)
                {
                    w.Write((byte)CompressedFlag.Yes);
                    w.Write((byte)m_DataList.ElementAt(0).DataValue);
                }
                else
                {
                    w.Write((byte)CompressedFlag.No);
                    foreach (Data data in m_DataList)
                    {
                        w.Write((byte)data.DataValue);
                    }
                        
                }
                
                
            }

            #endregion

            #region Sub-Type
            public enum CompressedFlag : byte
            {
                Yes = 0x01,
                No = 0x00
            }
            #endregion

            #region IEquatable<Block>
            public bool Equals(Block block)
            {
                return (this.m_DataList == block.m_DataList) && (this.m_BlockNumber == block.m_BlockNumber) && (this.m_Compressed == block.m_Compressed);
            }
            #endregion

            #region AHandlerElement<Block>
            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }

            public override AHandlerElement Clone(EventHandler handler)
            {
                return new Block(recommendedApiVersion, handler, this);
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(requestedApiVersion, this.GetType()); }
            }
            //public string Value 
            //{ 
            //    get 
            //    {
                    
            //        return ValueBuilder; 
            //    } 
            //}
            #endregion
        }


        public class Data : AHandlerElement, IEquatable<Data>
        {
            #region Attributes
            private ValueFlag m_Value;
            public ValueFlag DataValue { get { return m_Value; } set { m_Value = value; } }
            #endregion

            #region Data I/O
            public void Parse(Stream s)
            {
                BinaryReader r = new BinaryReader(s);

                m_Value = (ValueFlag)r.ReadByte();
            }
            public void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                w.Write((byte)m_Value);
            }
            #endregion

            #region Constructors
            public Data(int APIversion, EventHandler handler, Stream s)
                : base(APIversion, handler)
                
            {
                Parse(s);
            }

            public Data(int APIversion, EventHandler handler, byte value) : base(APIversion, handler)
                
            {
                this.m_Value = (ValueFlag)value;
            }

            public Data(int APIversion, EventHandler handler, Data data)
                : base(APIversion, handler)
               
            {
                this.m_Value = data.m_Value;
            }

            #endregion

            #region Sub-Type
            public enum ValueFlag : byte
            {
                Outside = 0xFF,
                CamOnly = 0x80,
                Both = 0x00
            }
            #endregion

            #region IEquatable<Data>
            public bool Equals(Data data)
            {
                return (this.m_Value == data.m_Value);
            }
            #endregion

            #region AHandlerElement<Data>

            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }

            public override AHandlerElement Clone(EventHandler handler)
            {
                return new Data(recommendedApiVersion, handler, this);
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(requestedApiVersion, this.GetType()); }
            }
            

            //public string Value
            //{ 
                
            //    get
            //    {
            //        return ValueBuilder; 
            //    } 
            //}
            #endregion
        }

        public class DataList : DependentList<Data>
        {
            public DataList(EventHandler handler) : base(handler) { }
            public DataList(EventHandler handler, Stream s) : base(handler, s) { }
            public DataList(EventHandler handler, IEnumerable<Data> lb) : base(handler, lb) { }

            public override void Add(Data newElement)
            {

                base.Add(newElement);

            }

            protected override Data CreateElement(Stream s)
            {
                return new Data(recommendedApiVersion, elementHandler, s);
            }

            protected override void WriteElement(Stream s, Data element)
            {
                element.UnParse(s);
            }
        }

        public class Layer : DependentList<Block>
        {

            public Layer(EventHandler handler) : base(handler) { }
            public Layer(EventHandler handler, Stream s) : base(handler, s) { }
            public Layer(EventHandler handler, IEnumerable<Block> lb) : base(handler, lb) { }





            public override void Add(Block newElement)
            {

                base.Add(newElement);

            }

            protected override Block CreateElement(Stream s)
            {
                return new Block(0, elementHandler, s);
            }

            protected override void WriteElement(Stream s, Block element)
            {
                element.UnParse(s);
            }

        }

        #endregion


        #region Methods
        public static Int16 Swap(Int16 i)
        {

            return (Int16)((i & 0x00FF) << 8 | (i & 0xFF00) >> 8);
        }
        #endregion

        public class WBNDWrapperHandler : AResourceHandler
        {
            public WBNDWrapperHandler()
            {
                this.Add(typeof(WBNDWrapper), new List<string>(new string[] { "0x05DA8AF6" }));
            }
        }
    }
}
