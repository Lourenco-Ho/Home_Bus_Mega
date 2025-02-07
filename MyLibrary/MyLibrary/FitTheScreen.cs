using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyLibrary
{
    public static class FitTheScreen
    {
        public static void Resize(Control control, double given_width)
        {
            double magnification_ratio = 1 + (Screen.PrimaryScreen.Bounds.Width - given_width) / given_width;
            Process_Resize_Prop(control, magnification_ratio);
        }

        public static void Resize_Child(Control parent_control, double given_width) //Resize to fullscreen
        {
            double magnification_ratio = 1 + (Screen.PrimaryScreen.Bounds.Width - given_width) / given_width;

            foreach (Control child_control in parent_control.Controls)
            {
                Process_Resize_Prop(child_control, magnification_ratio);

                if (child_control.Controls.Count > 0)
                {
                    Resize_Child(child_control, given_width);
                }
            }
        }

        private static void Process_Resize_Prop(Control control, double magnification_ratio)
        {
            control.Width = (int)Math.Round(control.Size.Width * magnification_ratio);
            control.Height = (int)Math.Round(control.Size.Height * magnification_ratio);
            control.Location = new Point(
                (int)Math.Round(control.Location.X * magnification_ratio),
                (int)Math.Round(control.Location.Y * magnification_ratio));

            if (control is Button button)
            {
                button.Font = new Font(button.Font.FontFamily, (float)Math.Round(Math.Round(button.Font.Size) * magnification_ratio, 2), button.Font.Style);
                button.FlatAppearance.BorderSize = (int)(button.FlatAppearance.BorderSize * magnification_ratio);
            }
            else if (control is Label label)
            {
                label.Font = new Font(label.Font.FontFamily, (float)Math.Round(Math.Round(label.Font.Size) * magnification_ratio, 2), label.Font.Style);
            }
        }
    }
}