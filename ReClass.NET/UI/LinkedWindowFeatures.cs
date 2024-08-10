using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Forms;
using ReClassNET.Debugger;
using ReClassNET.Forms;
using ReClassNET.MemoryScanner;
using ReClassNET.MemoryScanner.Comparer;
using ReClassNET.Nodes;

namespace ReClassNET.UI
{
	public class LinkedWindowFeatures
	{
		public static ClassNode CreateClassAtAddress(IntPtr address, bool addDefaultBytes)
		{
			Contract.Ensures(Contract.Result<ClassNode>() != null);

			var classView = Program.MainForm.ProjectView;

			var node = ClassNode.Create();
			node.AddressFormula = address.ToString("X");
			if (addDefaultBytes)
			{
				node.AddBytes(16 * IntPtr.Size);
			}

			classView.SelectedClass = node;

			return node;
		}

		public static ClassNode CreateDefaultClass()
		{
			Contract.Ensures(Contract.Result<ClassNode>() != null);

			var address = ClassNode.DefaultAddress;

			var mainModule = Program.RemoteProcess.GetModuleByName(Program.RemoteProcess.UnderlayingProcess?.Name);
			if (mainModule != null)
			{
				address = mainModule.Start;
			}

			return CreateClassAtAddress(address, true);
		}

		public static void SetCurrentClassAddress(IntPtr address)
		{
			var classNode = Program.MainForm.ProjectView.SelectedClass;
			if (classNode == null)
			{
				return;
			}

			classNode.AddressFormula = address.ToString("X");
		}

		public static void FindWhatInteractsWithAddress(IntPtr address, int size, bool writeOnly)
		{
			var debugger = Program.RemoteProcess.Debugger;

			if (!debugger.AskUserAndAttachDebugger())
			{
				return;
			}

			if (writeOnly)
			{
				debugger.FindWhatWritesToAddress(address, size);
			}
			else
			{
				debugger.FindWhatAccessesAddress(address, size);
			}
		}

		public static void StartMemoryScan(IScanComparer comparer)
		{
			Contract.Requires(comparer != null);

			var sf = GlobalWindowManager.Windows.OfType<ScannerForm>().FirstOrDefault();
			if (sf != null)
			{
				if (MessageBox.Show("Open a new scanner window?", Constants.AppDisplayName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
				{
					sf = null;
				}
			}
			if (sf == null)
			{
				sf = new ScannerForm(Program.RemoteProcess);
				sf.Show();
			}

			var settings = ScanSettings.Default;
			switch (comparer)
			{
				case ByteMemoryComparer:
					settings.ValueType = ScanValueType.Byte;
					break;
				case ShortMemoryComparer:
					settings.ValueType = ScanValueType.Short;
					settings.FastScanAlignment = 2;
					break;
				case IntegerMemoryComparer:
					settings.ValueType = ScanValueType.Integer;
					settings.FastScanAlignment = 4;
					break;
				case LongMemoryComparer:
					settings.ValueType = ScanValueType.Long;
					settings.FastScanAlignment = 4;
					break;
				case FloatMemoryComparer:
					settings.ValueType = ScanValueType.Float;
					settings.FastScanAlignment = 4;
					break;
				case DoubleMemoryComparer:
					settings.ValueType = ScanValueType.Double;
					settings.FastScanAlignment = 4;
					break;
				case ArrayOfBytesMemoryComparer:
					settings.ValueType = ScanValueType.ArrayOfBytes;
					break;
				case StringMemoryComparer:
					settings.ValueType = ScanValueType.String;
					break;
			}

			sf.ExcuteScan(settings, comparer);
		}
	}
}