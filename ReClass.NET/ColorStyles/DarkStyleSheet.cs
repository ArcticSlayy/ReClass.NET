using System.Drawing;
using ColorCode;

namespace ReClassNET.StyleSheets;

public class DarkStyleSheet : IStyleSheet
{
	public static readonly Color DullRed;

	private static readonly StyleDictionary styles;

	public string Name => "DarkStyleSheet";

	public StyleDictionary Styles => styles;

	static DarkStyleSheet()
	{
		DullRed = Color.FromArgb(163, 21, 21);
		styles = new StyleDictionary
		{
			new Style("Plain Text")
			{
				Foreground = Color.White,
				Background = Color.Black,
				CssClassName = "plainText"
			},
			new Style("HTML Server-Side Script")
			{
				Background = Color.Yellow,
				CssClassName = "htmlServerSideScript"
			},
			new Style("HTML Comment")
			{
				Foreground = Color.Green,
				CssClassName = "htmlComment"
			},
			new Style("Html Tag Delimiter")
			{
				Foreground = Color.Blue,
				CssClassName = "htmlTagDelimiter"
			},
			new Style("HTML Element ScopeName")
			{
				Foreground = DullRed,
				CssClassName = "htmlElementName"
			},
			new Style("HTML Attribute ScopeName")
			{
				Foreground = Color.Red,
				CssClassName = "htmlAttributeName"
			},
			new Style("HTML Attribute Value")
			{
				Foreground = Color.Blue,
				CssClassName = "htmlAttributeValue"
			},
			new Style("HTML Operator")
			{
				Foreground = Color.Blue,
				CssClassName = "htmlOperator"
			},
			new Style("Comment")
			{
				Foreground = Color.Green,
				CssClassName = "comment"
			},
			new Style("XML Doc Tag")
			{
				Foreground = Color.Gray,
				CssClassName = "xmlDocTag"
			},
			new Style("XML Doc Comment")
			{
				Foreground = Color.Green,
				CssClassName = "xmlDocComment"
			},
			new Style("String")
			{
				Foreground = DullRed,
				CssClassName = "string"
			},
			new Style("String (C# @ Verbatim)")
			{
				Foreground = DullRed,
				CssClassName = "stringCSharpVerbatim"
			},
			new Style("Keyword")
			{
				Foreground = Color.Blue,
				CssClassName = "keyword"
			},
			new Style("Preprocessor Keyword")
			{
				Foreground = Color.Blue,
				CssClassName = "preprocessorKeyword"
			},
			new Style("HTML Entity")
			{
				Foreground = Color.Red,
				CssClassName = "htmlEntity"
			},
			new Style("XML Attribute")
			{
				Foreground = Color.Red,
				CssClassName = "xmlAttribute"
			},
			new Style("XML Attribute Quotes")
			{
				Foreground = Color.White,
				CssClassName = "xmlAttributeQuotes"
			},
			new Style("XML Attribute Value")
			{
				Foreground = Color.Blue,
				CssClassName = "xmlAttributeValue"
			},
			new Style("XML CData Section")
			{
				Foreground = Color.Gray,
				CssClassName = "xmlCDataSection"
			},
			new Style("XML Comment")
			{
				Foreground = Color.Green,
				CssClassName = "xmlComment"
			},
			new Style("XML Delimiter")
			{
				Foreground = Color.Blue,
				CssClassName = "xmlDelimiter"
			},
			new Style("XML Name")
			{
				Foreground = DullRed,
				CssClassName = "xmlName"
			},
			new Style("Class Name")
			{
				Foreground = Color.MediumTurquoise,
				CssClassName = "className"
			},
			new Style("CSS Selector")
			{
				Foreground = DullRed,
				CssClassName = "cssSelector"
			},
			new Style("CSS Property Name")
			{
				Foreground = Color.Red,
				CssClassName = "cssPropertyName"
			},
			new Style("CSS Property Value")
			{
				Foreground = Color.Blue,
				CssClassName = "cssPropertyValue"
			},
			new Style("SQL System Function")
			{
				Foreground = Color.Magenta,
				CssClassName = "sqlSystemFunction"
			},
			new Style("PowerShell PowerShellAttribute")
			{
				Foreground = Color.PowderBlue,
				CssClassName = "powershellAttribute"
			},
			new Style("PowerShell Operator")
			{
				Foreground = Color.Gray,
				CssClassName = "powershellOperator"
			},
			new Style("PowerShell Type")
			{
				Foreground = Color.Teal,
				CssClassName = "powershellType"
			},
			new Style("PowerShell Variable")
			{
				Foreground = Color.OrangeRed,
				CssClassName = "powershellVariable"
			},
			new Style("Type")
			{
				Foreground = Color.Teal,
				CssClassName = "type"
			},
			new Style("Type Variable")
			{
				Foreground = Color.Teal,
				Italic = true,
				CssClassName = "typeVariable"
			},
			new Style("Name Space")
			{
				Foreground = Color.Navy,
				CssClassName = "namespace"
			},
			new Style("Constructor")
			{
				Foreground = Color.Purple,
				CssClassName = "constructor"
			},
			new Style("Predefined")
			{
				Foreground = Color.Navy,
				CssClassName = "predefined"
			},
			new Style("Pseudo Keyword")
			{
				Foreground = Color.Navy,
				CssClassName = "pseudoKeyword"
			},
			new Style("String Escape")
			{
				Foreground = Color.Gray,
				CssClassName = "stringEscape"
			},
			new Style("Control Keyword")
			{
				Foreground = Color.Blue,
				CssClassName = "controlKeyword"
			},
			new Style("Number")
			{
				CssClassName = "number"
			},
			new Style("Operator")
			{
				CssClassName = "operator"
			},
			new Style("Delimiter")
			{
				CssClassName = "delimiter"
			},
			new Style("Markdown Header")
			{
				Bold = true,
				CssClassName = "markdownHeader"
			},
			new Style("Markdown Code")
			{
				Foreground = Color.Teal,
				CssClassName = "markdownCode"
			},
			new Style("Markdown List Item")
			{
				Bold = true,
				CssClassName = "markdownListItem"
			},
			new Style("Markdown Emphasized")
			{
				Italic = true,
				CssClassName = "italic"
			},
			new Style("Markdown Bold")
			{
				Bold = true,
				CssClassName = "bold"
			}
		};
	}
}