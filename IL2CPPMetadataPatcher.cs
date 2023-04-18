using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Il2CppInterop;
using Il2CppInterop.Runtime;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using LibCpp2IL;
using UnityEngine;
using LibCpp2IL.Metadata;
using BepInEx; 

static class MetadataProcessing
    {
        public static int stringliteralcount = 0;
        public static Dictionary<string, string> processbuffer = new Dictionary<string, string>();
        static List<string> untranslated = new List<string>();
        public static string path = Path.Combine(Application.dataPath, "il2cpp_data", "Metadata", "global-metadata.dat");
        static public void ProcessMetadata()
        {
            var unityversion = new AssetRipper.VersionUtilities.UnityVersion(2020, 3, 12);

            var pepath = Path.Combine(BepInEx.Paths.GameRootPath, "GameAssembly.dll");
            var bytearray = File.ReadAllBytes(path);
            //Il2CppMetadata metadata = LibCpp2IL.Metadata.Il2CppMetadata.ReadFrom(bytearray, unityversion);


            LibCpp2IlMain.LoadFromFile(pepath, path, unityversion);
            var metadata = LibCpp2IlMain.TheMetadata;
            var header = metadata.metadataHeader;
            Plugin.log.LogInfo("Header String Count : " + header.stringLiteralCount);

            for (int i = 0; i < header.stringLiteralCount; i++)
            {
                try
                {
                    var str = LibCpp2IlMain.TheMetadata.GetStringLiteralFromIndex((uint)i);




                    if (Helpers.IsChinese(str))
                    {
                        var finalstring = str.Replace("\n", "<lf>");

                        if (HasNoInvalidChars(finalstring))
                        {
                            Plugin.log.LogInfo("Metadata string ... : " + finalstring);

                            MetaDataReplace(finalstring, LibCpp2IlMain.TheMetadata.GetStringLiteralFromIndex((uint)i));
                           


                        }

                    }
                }
                catch
                {

                }
               
            }
            if (processbuffer.Count > 0)
            {
                Plugin.log.LogInfo("ProcessBuffer Count : " + processbuffer.Count());
                WriteToMetadata();
                var content = File.ReadAllBytes(path + ".temp");

                LibCpp2IlMain.LoadFromFile(pepath, path + ".temp", unityversion);
                metadata = LibCpp2IlMain.TheMetadata;
                
                using(BinaryWriter bw = new BinaryWriter(File.Open(path, FileMode.Open)) )
                {
                    bw.BaseStream.Write(content, 0, content.Length);
                }
                File.Delete(path + ".temp");

                

            }

            var pathMUN = Path.Combine(BepInEx.Paths.PluginPath, "Dump", "MetadataUN.txt");
            if (File.Exists(pathMUN))
            {
                File.Delete(pathMUN);
            }
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(pathMUN, append: true))
            {
                foreach (var str in untranslated)
                {
                    sw.Write(str + System.Environment.NewLine);
                }
            }
        }
        static public bool HasNoInvalidChars(string str)
        {
            var b = true;
            foreach (var ch in str)
            {
                var pattern = @"\p{IsCJKUnifiedIdeographs}";
                var pattern2 = @"\p{IsBasicLatin}";
                var pattern3 = @"\p{P}";
                //Plugin.log.LogInfo("Check character : " + ch + "// CJKCheck = " + Regex.IsMatch(ch.ToString(), pattern) + " // Latin Check " + Regex.IsMatch(ch.ToString(), pattern2) + " // Punct Check : " + Regex.IsMatch(ch.ToString(), pattern3));
                if (str.Contains("UI/") || !Helpers.IsChinese(str) || !Regex.IsMatch(ch.ToString(), pattern) && !Regex.IsMatch(ch.ToString(), pattern2) && !Regex.IsMatch(ch.ToString(), pattern3))
                {
                    b = false;
                }
            }
            return b;
        }

        static string MetaDataReplace(string str, string original)
        {
            if (!Plugin.forbiddenMetadataList.Contains(str))
            {
                if (Plugin.MetaDataDict.ContainsKey(str))
                {
                    var dest = Plugin.MetaDataDict[str].Replace("<lf>", "\n");
                    processbuffer.Add(original, dest);

                }
                else
                {
                    untranslated.Add(str);
                }
            }
            else
            {
                Plugin.log.LogInfo("Not touching this with a 15ft pole : " + str);
            }
            return str;
        }

        /*public static void Processtxtbin()
        {
            var filme = File.ReadAllText(Path.Combine(BepInEx.Paths.PluginPath, "textbintxt", "text.bin.txt"));
            
            
        }*/
        public static int order = 0;
        public static void Test(MetadataFile metadata)
        {
            bool isEdit = false;
            for(int j=0;j< metadata.strBytes.Count();j++)
            {
                var item = metadata.strBytes[j];
                if (processbuffer.ContainsKey(Encoding.UTF8.GetString(metadata.strBytes[j])))
                { 
                isEdit = true;

                }
                else
                {
                    isEdit = false;
                }
            
            if (isEdit)
            {
            var NewStrBytes = Encoding.UTF8.GetBytes(processbuffer[Encoding.UTF8.GetString(item)]);

            var o = item as object;
            metadata.strBytes[j] = NewStrBytes;
            }
            else
            {
               
            }
            }
        }

        public static void hmmm()
        {
            var x = new MetadataFile(path);
            foreach(var k in x.strBytes)
            {

            }
        }
        public static void WriteToMetadata()
        {
            var path = Path.Combine(Application.dataPath, "il2cpp_data", "Metadata", "global-metadata.dat");
            var metadata = new MetadataFile(path);
            Test(metadata);
            metadata.WriteToNewFile(path);

        }
    }

    
     public class MetadataFile : IDisposable
    {
        public BinaryReader reader;

        private uint stringLiteralOffset;
        private uint stringLiteralCount;
        private long DataInfoPosition;
        private uint stringLiteralDataOffset;
        private uint stringLiteralDataCount;
        private List<StringLiteral> stringLiterals = new List<StringLiteral>();
        public List<byte[]> strBytes = new List<byte[]>();
        

        public MetadataFile(string fullName)
        {
            reader = new BinaryReader(File.OpenRead(fullName));

             ReadHeader();

             ReadLiteral();
             ReadStrByte();

        }

        private void ReadHeader()
        {
            uint vansity = reader.ReadUInt32();
            if (vansity != 0xFAB11BAF)
            {
                throw new Exception("Header Check Failed");
            }
            int version = reader.ReadInt32();
            stringLiteralOffset = reader.ReadUInt32();     
            stringLiteralCount = reader.ReadUInt32();      
            DataInfoPosition = reader.BaseStream.Position; 
            stringLiteralDataOffset = reader.ReadUInt32(); 
            stringLiteralDataCount = reader.ReadUInt32();   
        }

        private void ReadLiteral()
        {

            reader.BaseStream.Position = stringLiteralOffset;
            for (int i = 0; i < stringLiteralCount / 8; i++)
            {
                stringLiterals.Add(new StringLiteral
                {
                    Length = reader.ReadUInt32(),
                    Offset = reader.ReadUInt32()
                });
            }
        }

        private void ReadStrByte()
        {

            for (int i = 0; i < stringLiterals.Count; i++)
            {
                reader.BaseStream.Position = stringLiteralDataOffset + stringLiterals[i].Offset;
                strBytes.Add(reader.ReadBytes((int)stringLiterals[i].Length));
            }
        }

        public void WriteToNewFile(string fileName)
        {
            BinaryWriter writer = new BinaryWriter(File.Create(fileName + ".temp"));

            reader.BaseStream.Position = 0;
            reader.BaseStream.CopyTo(writer.BaseStream);

            writer.BaseStream.Position = stringLiteralOffset;
            uint count = 0;
            for (int i = 0; i < stringLiterals.Count; i++)
            {

                stringLiterals[i].Offset = count;
                stringLiterals[i].Length = (uint)strBytes[i].Length;

                writer.Write(stringLiterals[i].Length);
                writer.Write(stringLiterals[i].Offset);
                count += stringLiterals[i].Length;

            }

            var tmp = (stringLiteralDataOffset + count) % 4;
            if (tmp != 0) count += 4 - tmp;

            if (count > stringLiteralDataCount)
            {
                if (stringLiteralDataOffset + stringLiteralDataCount < writer.BaseStream.Length)
                {
                    stringLiteralDataOffset = (uint)writer.BaseStream.Length;
                }
            }
            stringLiteralDataCount = count;

            writer.BaseStream.Position = stringLiteralDataOffset;
            for (int i = 0; i < strBytes.Count; i++)
            {
                writer.Write(strBytes[i]);
            }

            writer.BaseStream.Position = DataInfoPosition;
            writer.Write(stringLiteralDataOffset);
            writer.Write(stringLiteralDataCount);

            writer.Close();
        }

        public void Dispose()
        {
            reader?.Dispose();
        }

        public class StringLiteral
        {
            public uint Length;
            public uint Offset;
        }
    }
