// Copyright 2025 Heath Stewart.
// Licensed under the MIT License. See LICENSE.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace sdb2xml
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var program = new Program();
            int result;
            try
            {
                if (program.ParseArguments(args))
                {
                    program.Run();
                }
                result = 0;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Error: " + ex.Message);

                if (ex is Win32Exception ex2)
                {
                    result = ex2.ErrorCode;
                }
                else
                {
                    result = 1;
                }
            }
            finally
            {
                Console.ResetColor();
            }
            return result;
        }

        private Program()
        {
            base64 = false;
            dir = Environment.CurrentDirectory;
            extract = false;
            report = null;
            sdb = null;
            xml = null;
        }

        private bool ParseArguments(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var text = args[i];
                if (text.StartsWith("-") || text.StartsWith("/"))
                {
                    switch (_ = text.Substring(1).ToLower(CultureInfo.InvariantCulture))
                    {
                        case "base64":
                            base64 = true;
                            goto IL_13C;
                        case "extract":
                            extract = true;
                            goto IL_13C;
                        case "out":
                            report = args[++i];
                            goto IL_13C;
                        case "?":
                        case "h":
                        case "help":
                            Program.ShowUsage(Console.Out);
                            return false;
                    }
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The argument \"{0}\" is not supported.", new object[]
                    {
                        text
                    }));
                }
                if (sdb != null)
                {
                    throw new ArgumentException("The shim database has already been specified.");
                }
                sdb = text;
            IL_13C:;
            }
            if (base64 && extract)
            {
                throw new ArgumentException("Only one of -base64 or -extract can be specified.");
            }
            return true;
        }

        private void Run()
        {
            if (sdb == null)
            {
                throw new ArgumentException("Missing path to shim database file.");
            }
            if (report != null)
            {
                report = Path.Combine(Environment.CurrentDirectory, report);
                dir = Path.GetDirectoryName(report);
            }
            using var shimDatabase = new ShimDatabase(sdb);
            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Document
            };
            var textWriter = Console.Out;
            if (report != null)
            {
                Directory.CreateDirectory(dir);
                xmlWriterSettings.CloseOutput = true;
                textWriter = new StreamWriter(report, false, Encoding.UTF8);
            }
            xmlWriterSettings.Encoding = textWriter.Encoding;
            using (xml = XmlWriter.Create(textWriter, xmlWriterSettings))
            {
                xml.WriteStartDocument(true);
                xml.WriteStartElement("SDB");
                xml.WriteAttributeString("xmlns", "xs", null, "http://www.w3.org/2001/XMLSchema");
                xml.WriteAttributeString("path", shimDatabase.Path);
                foreach (var tag in shimDatabase.Root.Tags)
                {
                    WriteTag(tag);
                }
                xml.WriteEndDocument();
            }
        }

        private string GetFilename(Tag tag)
        {
            if (tag.IsFile)
            {
                var parent = tag.Parent;
                var type = parent.Type;
                if (parent != null && (28684 == type || 28677 == type || 28687 == type))
                {
                    var tag2 = parent.Find(24577);
                    if (tag2 != null)
                    {
                        return tag2.ToString();
                    }
                }
            }
            return Path.GetRandomFileName() + ".bin";
        }

        private string GetXmlType(Tag tag, out string baseType)
        {
            baseType = null;
            var typeCode = tag.GetTypeCode();
            if (typeCode != TypeCode.Object)
            {
                switch (typeCode)
                {
                    case TypeCode.Byte:
                        return "xs:byte";
                    case TypeCode.Int16:
                        return "xs:short";
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                        break;
                    case TypeCode.Int32:
                        return "xs:int";
                    case TypeCode.Int64:
                        return "xs:long";
                    default:
                        switch (typeCode)
                        {
                            case TypeCode.DateTime:
                                baseType = "xs:long";
                                return "xs:dateTime";
                            case TypeCode.String:
                                return "xs:string";
                        }
                        break;
                }
                return null;
            }
            if (tag.IsGuid)
            {
                baseType = "xs:base64Binary";
                return "xs:string";
            }
            if (extract)
            {
                return "xs:anyURI";
            }
            return "xs:base64Binary";
        }

        private void WriteTag(Tag tag)
        {
            WriteName(tag);
            WriteTypeAttributes(tag);
            WriteValue(tag);
            foreach (var tag2 in tag.Tags)
            {
                WriteTag(tag2);
            }
            xml.WriteEndElement();
        }

        private void WriteName(Tag tag)
        {
            var text = tag.Name;
            text = XmlConvert.EncodeLocalName(text);
            xml.WriteStartElement(text);
        }

        private void WriteTypeAttributes(Tag tag)
        {
            var xmlType = GetXmlType(tag, out var text);
            if (xmlType != null)
            {
                xml.WriteAttributeString("type", xmlType);
                if (text != null && !extract)
                {
                    xml.WriteAttributeString("baseType", text);
                }
            }
        }

        private void WriteValue(Tag tag)
        {
            var text = tag.ToString();
            if (text != null)
            {
                xml.WriteString(text);
                return;
            }
            if (base64 && TypeCode.Object == tag.GetTypeCode())
            {
                xml.WriteBase64(tag.GetData(), 0, tag.Size);
                return;
            }
            if (extract && TypeCode.Object == tag.GetTypeCode())
            {
                var filename = GetFilename(tag);
                var path = Path.Combine(dir, filename);

                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                fileStream.Write(tag.GetData(), 0, tag.Size);
                xml.WriteString("file://" + filename);
            }
        }

        private static void ShowUsage(TextWriter writer)
        {
            writer.WriteLine("Extracts data as XML from a shim database used for application compatibility.");
            writer.WriteLine();
            writer.WriteLine("Usage: {0} sdb [-out report] [-base64 | -extract] [-?]", Process.GetCurrentProcess().ProcessName);
            writer.WriteLine();
            writer.WriteLine("  sdb          Path to the shim database to process.");
            writer.WriteLine("  -base64      Base-64 encode data in the XML report.");
            writer.WriteLine("  -extract     Extract binary data to current or report directory.");
            writer.WriteLine("  -out report  Path to the XML file to generate; otherwise, output to console.");
        }

        private bool base64;

        private string dir;

        private bool extract;

        private string report;

        private string sdb;

        private XmlWriter xml;
    }
}
