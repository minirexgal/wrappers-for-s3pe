using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using s3pi.Interfaces;
using System.IO;

namespace Kuree.Sims.REFS
{
    public class REFSWrapper : AResource 
    {
        // Properties
        [ElementPriority(0)]
        public Int16 Version { get; set; }
        [ElementPriority(1)]
        public EntryList ReferenceList { get; set; }
        
        private Int16[] WordList;
        private bool Thingy = false;
        public string Value { get { return ValueBuilder; } }

        #region AApiVersionedFields
        const Int32 recommendedApiVersion = 1;
        public override int RecommendedApiVersion { get { return recommendedApiVersion; } }
        #endregion

        public REFSWrapper(int APIversion, Stream s) : base(APIversion, s) { if (stream == null) { stream = UnParse(); OnResourceChanged(this, new EventArgs()); } else { stream.Position = 0; Parse(stream); } }

        private void Parse(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            Version = r.ReadInt16();
            if (Version >= 3) 
            {
                Thingy = r.ReadByte() == 0 ? false : true;
            }
            int count1 = r.ReadInt32();
            ReferenceList = new EntryList(null, Thingy);
            for (int i = 0; i < count1; i++)
            {
                if (Thingy)
                {
                    ReferenceList.Add(new Entry1(recommendedApiVersion, null, s));
                }
                else
                {
                    ReferenceList.Add(new Entry2(recommendedApiVersion, null, s));
                }
            }
            int count2 = r.ReadInt32();
            WordList = new Int16[count2];
            for (int i = 0; i < count2; i++)
            {
                WordList[i] = r.ReadInt16();
            }
            
        }

        protected override Stream UnParse()
        {
            MemoryStream s = new MemoryStream();
            BinaryWriter w = new BinaryWriter(s);
            w.Write(Version);
            if (Version >= 3)
            {
                w.Write(Thingy ? (byte)1 : (byte)0);
            }

            int count = ((IList<Entry>)ReferenceList).Count;
            w.Write(count);
            foreach (Entry tgi in ReferenceList)
            {
                tgi.UnParse(s);
            }

            w.Write(WordList.Length);
            foreach (Int16 word in WordList)
            {
                w.Write(word);
            }
            w.Flush();
            return s;
        }

        #region Sub-Types

        public abstract class Entry : AHandlerElement, IEquatable<Entry>
        {
            public TGIBlock TGI { get; set; }
            protected bool isThingy;
            public Entry(int APIversion, EventHandler handler) : base(APIversion, handler)
            {
            }
            public abstract void UnParse(Stream s);
            public abstract void Parse(Stream s);
            public Entry(int APIversion, EventHandler handler, Stream s) : base(APIversion, handler) { Parse(s); }
            public abstract bool Equals(Entry tgi);

            
            public override int RecommendedApiVersion
            {
                get { return recommendedApiVersion; }
            }

            public abstract override AHandlerElement Clone(EventHandler handler);
            //public abstract List<string> ContentFields;
            //public abstract string Value;
        }

        public class Entry1 : Entry
        {

            private Int32 mIndex;
            public Int32 Index { get { return mIndex; } }
            //public Int32 thingy2;  

            public Entry1(int APIversion, EventHandler handler, Stream s)
                : base(APIversion, handler)
            {
                Parse(s);
            }

            public override void Parse(Stream s)
            {
                TGI = new TGIBlock(0, null, "TGI", s);
                BinaryReader r = new BinaryReader(s);
                this.mIndex = r.ReadInt32();
            }

            public Entry1(int APIversion, EventHandler handler, Entry1 tgi)
                : base(APIversion, handler)
            {
                this.mIndex = tgi.mIndex;
                this.TGI = tgi.TGI;
                this.isThingy = tgi.isThingy;
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                TGI.UnParse(s);

                w.Write(mIndex);

            }

            #region AHandlerElement
            
            public override AHandlerElement Clone(EventHandler handler)
            {
                return new Entry1(recommendedApiVersion, handler, this);
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(requestedApiVersion, this.GetType()); }
            }
            public string Value
            {
                get
                {

                    return ValueBuilder;
                }
            }
            #endregion

            #region IEquatable<Block>
            public bool Equals(Entry1 tgi)
            {
                return tgi.TGI == this.TGI && tgi.isThingy == this.isThingy && this.mIndex == tgi.mIndex ;//&& tgi.thingy2 == this.thingy2;
            }
            public override bool Equals(Entry tgi)
            {
                return tgi.TGI == this.TGI;// && tgi.isThingy == this.isThingy && this.Unknown1 == tgi.Unknown1;//&& tgi.thingy2 == this.thingy2;
            }
            #endregion
            
            
        }

        public class Entry2 : Entry
        {

            //public Int16 Unknown1;
            private Int16 mIndex;
            public Int16 Index { get { return mIndex; } }

            public Entry2(int APIversion, EventHandler handler, Stream s)
                : base(APIversion, handler)
            {
                Parse(s);
            }

            public override void Parse(Stream s)
            {
                TGI = new TGIBlock(0, null, "TGI", s);
                BinaryReader r = new BinaryReader(s);
                this.mIndex = r.ReadInt16();
            }

            public Entry2(int APIversion, EventHandler handler, Entry2 tgi)
                : base(APIversion, handler)
            {
                this.mIndex = tgi.mIndex;
                this.TGI = tgi.TGI;
                this.isThingy = tgi.isThingy;
            }

            public override void UnParse(Stream s)
            {
                BinaryWriter w = new BinaryWriter(s);
                TGI.UnParse(s);

                w.Write(mIndex);

            }

            #region AHandlerElement

            public override AHandlerElement Clone(EventHandler handler)
            {
                return new Entry2(recommendedApiVersion, handler, this);
            }

            public override List<string> ContentFields
            {
                get { return GetContentFields(requestedApiVersion, this.GetType()); }
            }
            public string Value
            {
                get
                {

                    return ValueBuilder;
                }
            }
            #endregion

            #region IEquatable<Block>
            public bool Equals(Entry2 tgi)
            {
                return tgi.TGI == this.TGI && tgi.isThingy == this.isThingy && this.mIndex == tgi.mIndex;//&& tgi.thingy2 == this.thingy2;
            }
            public override bool Equals(Entry tgi)
            {
                return tgi.TGI == this.TGI;// && tgi.isThingy == this.isThingy && this.Unknown1 == tgi.Unknown1;//&& tgi.thingy2 == this.thingy2;
            }
            #endregion


        }
        public class EntryList : DependentList<Entry>
        {
            private bool IsThingy;

            public EntryList(EventHandler handler, bool isthingy) : base(handler) { this.IsThingy = isthingy; }
            public EntryList(EventHandler handler, Stream s, bool isthingy) : base(handler, s) { this.IsThingy = isthingy; }
            public EntryList(EventHandler handler, IEnumerable<Entry> lb, bool isthingy) : base(handler, lb) { this.IsThingy = isthingy; }
            
            public override void Add(Entry newElement)
            {
                base.Add(newElement);

            }

            


            protected override Entry CreateElement(Stream s)
            {
                if (IsThingy)
                {
                    return new Entry1(recommendedApiVersion, elementHandler, s);
                }
                else
                {
                    return new Entry2(recommendedApiVersion, elementHandler, s);
                }
            }

            protected override void WriteElement(Stream s, Entry element)
            {
                element.UnParse(s);
            }
        }
        #endregion


        public class REFSWrapperHandler : AResourceHandler
        {
            public REFSWrapperHandler()
            {
                this.Add(typeof(REFSWrapper), new List<string>(new string[] { "0x05ED1226" }));

            }
        } 
    }
}
