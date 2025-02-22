using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyLibrary
{
    public static class ScrollingInPanel
    {
        private static Point lastMousePosition;
        public static void Add_Module(FlowLayoutPanel flowLayoutPanel)
        {
            flowLayoutPanel.MouseDown += FlowLayoutPanel_MouseDown;
            flowLayoutPanel.MouseMove += FlowLayoutPanel_MouseMove;
            flowLayoutPanel.MouseUp += FlowLayoutPanel_MouseUp;
        }

        private static void FlowLayoutPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastMousePosition = e.Location;
            }
        }

        private static void FlowLayoutPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //計算浮標移動距離
                int deltaY = e.Location.Y - lastMousePosition.Y;

                //調整滾動位置
                FlowLayoutPanel flowlayoutpanel = sender as FlowLayoutPanel;
                flowlayoutpanel.AutoScrollPosition = new Point(e.Location.X, flowlayoutpanel.AutoScrollPosition.Y + deltaY);
                lastMousePosition = e.Location;
            }
        }

        private static void FlowLayoutPanel_MouseUp(object sender, MouseEventArgs e)
        {

        }
    }
}
