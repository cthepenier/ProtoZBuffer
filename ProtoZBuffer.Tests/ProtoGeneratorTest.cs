﻿using System.IO;
using NUnit.Framework;
using ProtoZBuffer.Core;

namespace ProtoZBuffer.Tests
{
    internal static class StringExtensions
    {
        public static string RemoveCarriageReturn(this string str)
        {
            return str.Replace("\r", "");
        }
    }
    [TestFixture]
    class ProtoGeneratorTest
    {
        [Test]
        public void MessageWith2Fields()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" modifier=""required"" name=""name"" type=""string""
           description=""Folder Name."" />
    <field id=""42"" modifier=""optional"" name=""size"" type=""int64""
           description=""Folder size."" />
  </message>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
    required string name= 1;
    optional int64 size= 42;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString().RemoveCarriageReturn(), Is.EqualTo(result.RemoveCarriageReturn()));
        }

        [Test]
        public void NoModifierMeansRequired()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" name=""size"" type=""double""
           description=""Folder size."" />
  </message>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
    required double size= 1;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString().RemoveCarriageReturn(), Is.EqualTo(result.RemoveCarriageReturn()));
        }

        [Test]
        public void EnumTypeForField()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" modifier=""required"" name=""name"" type=""enum"" enumType=""my_enum""
           description=""Folder Name."" />
  </message>
  <enum name=""my_enum"" description=""description of my enum"">
     <enumItem name=""item1"" description=""first item"" />
     <enumItem name=""item2"" description=""second item"" value=""42""/>
     <enumItem name=""item3"" description=""third item""/>
  </enum>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
    required my_enum name= 1;
}

enum my_enum
{
    item1;
    item2=42;
    item3;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString().RemoveCarriageReturn(), Is.EqualTo(result.RemoveCarriageReturn()));
        }

        [Test]
        public void Index()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" name=""name"" type=""referenceMessage"" messageType=""File""
           description=""Folder Name."" modifier=""repeated"" />
    <index id=""2"" name=""my_index"" forField=""1"" sortBy=""filename"" />
  </message>
  <message name=""File"" description=""File desc"">
     <field id=""3"" name=""filename"" type=""string"" modifier=""required"" />
  </message>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
  //repeated FileHeader name= 1;
    repeated LocalMessageDescriptor name= 1;
  //repeated FileHeader my_index= 2;
    repeated LocalMessageDescriptor my_index= 2;
}

message FileHeader
{
    required string filename= 3;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            Assert.That(foo, Is.Not.Null);
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString().RemoveCarriageReturn(), Is.EqualTo(result.RemoveCarriageReturn()));
        }

        [Test]
        public void IndexWithDefault()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" name=""name"" type=""referenceMessage"" messageType=""File""
           description=""Folder Name."" modifier=""repeated"" default=""foo"" />
    <index id=""2"" name=""my_index"" forField=""1"" sortBy=""filename""/>
  </message>
  <message name=""File"" description=""File desc"">
     <field id=""3"" name=""filename"" type=""string"" modifier=""required""/>
  </message>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
  //repeated FileHeader name= 1 [default=foo];
    repeated LocalMessageDescriptor name= 1 [default=foo];
  //repeated FileHeader my_index= 2 [default=foo];
    repeated LocalMessageDescriptor my_index= 2 [default=foo];
}

message FileHeader
{
    required string filename= 3;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            Assert.That(foo, Is.Not.Null);
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString().RemoveCarriageReturn(), Is.EqualTo(result.RemoveCarriageReturn()));
        }

        [Test]
        public void NestedMessage()
        {
            var foo = ProtozbuffLoader.Load(new StringReader(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<protozbuff xmlns=""http://tempuri.org/protoZ.xsd"">
  <message name=""Folder"" description=""Document definition"">
    <field id=""1"" name=""name"" type=""nestedMessage"" messageType=""File""
           description=""Folder Name."" modifier=""required"" />
  </message>
  <message name=""File"" description=""File desc"">
     <field id=""2"" name=""filename"" type=""string"" modifier=""required"" />
  </message>
</protozbuff>"));

            const string result = @"package bar;

message FolderHeader
{
  //required FileHeader name= 1;
    required uint32 name= 1;
}

message FileHeader
{
    required string filename= 2;
}

message LocalMessageDescriptor
{
    repeated int32 coordinate = 1 [packed=true];
}
";
            Assert.That(foo, Is.Not.Null);
            var writer = new StringWriter();
            ProtoGenerator.Generate(foo, writer, "bar");
            Assert.That(writer.ToString().RemoveCarriageReturn(), Is.EqualTo(result.RemoveCarriageReturn()));
        }
    }
}
