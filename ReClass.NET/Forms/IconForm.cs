using System.Windows.Forms;

namespace ReClassNET.Forms;
public class IconForm : Form
{
	public IconForm()
	{
		Icon = Properties.Resources.ReClassNet;
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		// 
		// IconForm
		// 
		BackColor = System.Drawing.Color.Black;
		ClientSize = new System.Drawing.Size(284, 261);
		ForeColor = System.Drawing.Color.White;
		Name = "IconForm";
	}
}