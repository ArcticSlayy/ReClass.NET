using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using ReClassNET.Extensions;
using ReClassNET.Logger;
using ReClassNET.Nodes;
using ReClassNET.Project;

namespace ReClassNET.CodeGenerator
{
	// TODO: modularize components of this file.
	// TODO: maybe add the option to add boilerplate to the bottom for implementations of some typedefs?
	public class CSharpCodeGenerator : ICodeGenerator
	{
		class TypeDefinition
		{
			public readonly string TypeDef;
			// we need this so we can insert includes when needed.
			// when there's includes and we haven't already included something, we'll.
			public readonly string[] Includes = [];
			public static implicit operator string(TypeDefinition self) => self.TypeDef;
			public static implicit operator TypeDefinition(string self) => new TypeDefinition(self);
			public TypeDefinition(string s)
			{
				TypeDef = s;
				Includes = [];
			}
			public TypeDefinition(string s, string[] includes)
			{
				TypeDef = s;
				Includes = includes;
			}
			public override string ToString()
			{
				return TypeDef;
			}
		}
		private static readonly Dictionary<Type, TypeDefinition> complexTypeDefinitions = new()
		{
			// https://learn.microsoft.com/en-us/dotnet/api/System.Numerics.Vector2
			[typeof(Vector2Node)] = new TypeDefinition("Vector2", [
				"System.Numerics"
			]),
			// https://learn.microsoft.com/en-us/dotnet/api/System.Numerics.Vector3
			[typeof(Vector3Node)] = new TypeDefinition("Vector3", [
				"System.Numerics"
			]),
			// https://learn.microsoft.com/en-us/dotnet/api/System.Numerics.Vector4
			[typeof(Vector4Node)] = new TypeDefinition("Vector4", [
				"System.Numerics"
			]),
			// https://learn.microsoft.com/en-us/dotnet/api/System.Drawing.Drawing2D.matrix
			[typeof(Matrix3x3Node)] = new TypeDefinition("Matrix", [
				"System.Drawing.Drawing2D"
			]),
			// https://learn.microsoft.com/en-us/dotnet/api/System.Numerics.Matrix4x4
			[typeof(Matrix4x4Node)] = new TypeDefinition("Matrix4x4", [
				"System.Numerics"
			])
		};
		private static readonly Dictionary<Type, string> nodeTypeToTypeDefinitionMap = new()
		{
			[typeof(DoubleNode)] = "double",
			[typeof(FloatNode)] = "float",
			[typeof(BoolNode)] = "bool",
			[typeof(Int8Node)] = "sbyte",
			[typeof(Int16Node)] = "short",
			[typeof(Int32Node)] = "int",
			[typeof(Int64Node)] = "long",
			[typeof(NIntNode)] = "IntPtr",
			[typeof(UInt8Node)] = "byte",
			[typeof(UInt16Node)] = "ushort",
			[typeof(UInt32Node)] = "uint",
			[typeof(UInt64Node)] = "ulong",
			[typeof(NUIntNode)] = "UIntPtr",

			[typeof(FunctionPtrNode)] = "IntPtr",
			[typeof(Utf8TextPtrNode)] = "IntPtr",
			[typeof(Utf16TextPtrNode)] = "IntPtr",
			[typeof(Utf32TextPtrNode)] = "IntPtr",
			[typeof(PointerNode)] = "IntPtr",
			[typeof(VirtualMethodTableNode)] = "IntPtr"
		};

		public Language Language => Language.CSharp;

		public string GenerateCode(IReadOnlyList<ClassNode> classes, IReadOnlyList<EnumDescription> enums, ILogger logger)
		{
			List<string> imports = [
				"System.Runtime.InteropServices"
			];
			// we have a "intermediate system"
			// where it never gets written to the actual writer
			// so we can insert stuff at the top.
			using StringWriter sw = new();
			using StringWriter intermediateWriter = new();
			using var iw = new IndentedTextWriter(intermediateWriter, "\t");
			sw.WriteLine($"// Created with {Constants.AppDisplayName} {Constants.AppVersion} by {Constants.Author}");
			sw.WriteLine();
			sw.WriteLine("// Warning: The C# code generator doesn't support all node types!");
			sw.WriteLine();

			using (var en = enums.GetEnumerator())
			{
				if (en.MoveNext())
				{
					WriteEnum(iw, en.Current);

					while (en.MoveNext())
					{
						iw.WriteLine();

						WriteEnum(iw, en.Current);
					}

					iw.WriteLine();
				}
			}

			var classesToWrite = classes
				.Where(c => c.Nodes.None(n => n is FunctionNode)) // Skip data classes
				.Distinct();

			var unicodeStringClassLengthsToGenerate = new HashSet<int>();

			using (var en = classesToWrite.GetEnumerator())
			{
				if (en.MoveNext())
				{
					void FindUnicodeStringClasses(IEnumerable<BaseNode> nodes)
					{
						unicodeStringClassLengthsToGenerate.UnionWith(nodes.OfType<Utf16TextNode>().Select(n => n.Length));
					}

					FindUnicodeStringClasses(en.Current!.Nodes);

					WriteClass(iw, sw, imports, en.Current, logger);

					while (en.MoveNext())
					{
						iw.WriteLine();

						FindUnicodeStringClasses(en.Current!.Nodes);

						WriteClass(iw, sw, imports, en.Current, logger);
					}
				}
			}

			if (unicodeStringClassLengthsToGenerate.Any())
			{
				foreach (var length in unicodeStringClassLengthsToGenerate)
				{
					iw.WriteLine();

					WriteUnicodeStringClass(iw, length);
				}
			}
			sw.Write(intermediateWriter.ToString());
			return sw.ToString();
		}

		/// <summary>
		/// Outputs the C# code for the given enum to the <see cref="TextWriter"/> instance.
		/// </summary>
		/// <param name="writer">The writer to output to.</param>
		/// <param name="enum">The enum to output.</param>
		private static void WriteEnum(IndentedTextWriter writer, EnumDescription @enum)
		{
			Contract.Requires(writer != null);
			Contract.Requires(@enum != null);

			writer.Write($"enum {@enum.Name} : ");
			switch (@enum.Size)
			{
				case EnumDescription.UnderlyingTypeSize.OneByte:
					writer.WriteLine(nodeTypeToTypeDefinitionMap[typeof(Int8Node)]);
					break;
				case EnumDescription.UnderlyingTypeSize.TwoBytes:
					writer.WriteLine(nodeTypeToTypeDefinitionMap[typeof(Int16Node)]);
					break;
				case EnumDescription.UnderlyingTypeSize.FourBytes:
					writer.WriteLine(nodeTypeToTypeDefinitionMap[typeof(Int32Node)]);
					break;
				case EnumDescription.UnderlyingTypeSize.EightBytes:
					writer.WriteLine(nodeTypeToTypeDefinitionMap[typeof(Int64Node)]);
					break;
			}
			writer.WriteLine("{");
			writer.Indent++;
			// most enum implementations assume the starting value is 0,
			// and the next enumeration is current + 1,
			// but if not, you'd clarify and say x = value,
			// and the enum implementation would assume that
			// the next enumeration after `x` would be value + 1,
			// and so on...
			// we shouldn't give it the value
			// if it assumes correctly.
			// TODO: should omitting enumeration values be a setting?
			// Example:
			// enum x {
			// // (0 here)
			//  x = 1,
			//	y, // 2
			//  z = 4
			// }
			long assumedValue = 0;
			for (var j = 0; j < @enum.Values.Count; ++j)
			{
				var kv = @enum.Values[j];

				writer.Write(kv.Key);
				if (assumedValue != kv.Value)
				{
					writer.Write(" = ");
					writer.Write(kv.Value);
					assumedValue = kv.Value + 1;
				}
				if (j < @enum.Values.Count - 1)
				{
					writer.Write(",");
				}
				writer.WriteLine();
			}
			writer.Indent--;
			writer.WriteLine("};");
		}

		/// <summary>
		/// Outputs the C# code for the given class to the <see cref="TextWriter"/> instance.
		/// </summary>
		/// <param name="writer">The writer to output to.</param>
		/// <param name="class">The class to output.</param>
		/// <param name="logger">The logger.</param>
		private static void WriteClass(
			IndentedTextWriter writer,
			TextWriter iw,
			List<string> imports,
			ClassNode @class,
			ILogger logger
		)
		{
			Contract.Requires(writer != null);
			Contract.Requires(@class != null);
			Contract.Requires(logger != null);

			writer.WriteLine("[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]");
			writer.Write("public struct ");
			writer.Write(@class.Name);

			if (!string.IsNullOrEmpty(@class.Comment))
			{
				writer.Write(" // ");
				writer.Write(@class.Comment);
			}

			writer.WriteLine();

			writer.WriteLine("{");
			writer.Indent++;

			var nodes = @class.Nodes
				.WhereNot(n => n is FunctionNode || n is BaseHexNode);
			foreach (var node in nodes)
			{
				var (type, attribute) = GetTypeDefinition(node);
				if (type != null)
				{
					if (type.Includes.Any())
					{
						Contract.Requires(iw != null);
						type.Includes
							.WhereNot(imports.Contains)
							.ForEach(i =>
							{
								iw.WriteLine($"using {i};");
								imports.Add(i);
							});
					}
					if (attribute != null)
					{
						writer.WriteLine(attribute);
					}

					writer.WriteLine($"[FieldOffset(0x{node.Offset:X})]");
					writer.Write($"public {type} {node.Name};");
					if (!string.IsNullOrEmpty(node.Comment))
					{
						writer.Write(" //");
						writer.Write(node.Comment);
					}
					writer.WriteLine();
				}
				else
				{
					logger.Log(LogLevel.Warning, $"Skipping node with unhandled type: {node.GetType()}");
				}
			}
			var vTableNodes = @class.Nodes.OfType<VirtualMethodTableNode>().ToList();
			if (vTableNodes.Any())
			{
				writer.WriteLine();
				var virtualMethodNodes = vTableNodes
					.SelectMany(vt => vt.Nodes)
					.OfType<VirtualMethodNode>();
				foreach (var method in virtualMethodNodes)
				{
					if (!string.IsNullOrWhiteSpace(method.Comment))
					{
						writer.WriteLine($"// {method.Comment}");
					}
					writer.Write("virtual void ");
					writer.WriteLine($"{method.MethodName}();");
				}
			}

			writer.Indent--;
			writer.WriteLine("}");
		}
		/// <summary>
		/// Gets the type definition for the given node. If the node is not expressible <c>null</c> as typename is returned.
		/// </summary>
		/// <param name="node">The target node.</param>
		/// <returns>The type definition for the node or null as typename if the node is not expressible.</returns>
		private static (TypeDefinition typeName, string attribute) GetTypeDefinition(BaseNode node)
		{
			Contract.Requires(node != null);

			if (node is BitFieldNode bitFieldNode)
			{
				var underlayingNode = bitFieldNode.GetUnderlayingNode();
				underlayingNode.CopyFromNode(node);
				node = underlayingNode;
			}

			if (nodeTypeToTypeDefinitionMap.TryGetValue(node.GetType(), out var type))
			{
				return (new TypeDefinition(type), null);
			}
			else if (complexTypeDefinitions.TryGetValue(node.GetType(), out var t))
			{
				return (t, null);
			}

			return node switch
			{
				EnumNode enumNode => (enumNode.Enum.Name, null),
				Utf8TextNode utf8TextNode => ("string", $"[MarshalAs(UnmanagedType.ByValTStr, SizeConst = {utf8TextNode.Length})]"),
				Utf16TextNode utf16TextNode => (GetUnicodeStringClassName(utf16TextNode.Length), "[MarshalAs(UnmanagedType.Struct)]"),
				_ => (null, null)
			};
		}

		private static string GetUnicodeStringClassName(int length) => $"__UnicodeString{length}";

		/// <summary>
		/// Writes a helper class for unicode strings with the specific length.
		/// </summary>
		/// <param name="writer">The writer to output to.</param>
		/// <param name="length">The string length for this class.</param>
		private static void WriteUnicodeStringClass(IndentedTextWriter writer, int length)
		{
			var className = GetUnicodeStringClassName(length);

			writer.WriteLine("[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]");
			writer.WriteLine($"public struct {className}");
			writer.WriteLine("{");
			writer.Indent++;
			writer.WriteLine($"[MarshalAs(UnmanagedType.ByValTStr, SizeConst = {length})]");
			writer.WriteLine("public string Value;");
			writer.WriteLine();
			writer.WriteLine($"public static implicit operator string({className} value) => value.Value;");
			writer.Indent--;
			writer.WriteLine("}");
		}
	}
}