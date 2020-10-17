using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using TShockAPI;

namespace EternalLandPlugin
{
    class StatusSender
    {
        public static void SendStatus()
        {
            new Thread(new ThreadStart(Loop)).Start();
        }

        public static void Loop()
        {
            while (true)
            {
                try
                {
                    EternalLand.OnlineTSPlayer.ForEach(tsp =>
                    {
                        var eplr = tsp.EPlayer();
                        string text = string.Empty;
                        var col = Rambo();
                        string rambotitle = $"[c/{System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(col.R, col.G, col.B)).Replace("#", "")}:  ●永恒之地服务器● ]";
                        if (eplr != null)
                        {
                            text = $"{rambotitle}                                                                \r\n---------------\r\n"
                             + $"[c/9FD1C4:账号资产:] {eplr.Money}{RepeatLineBreaks(59)}";
                        }
                        else
                        {
                            text = $"{rambotitle}                                                                \r\n---------------\r\n"
                            + $"[c/9FD1C4:欢迎加入服务器] {RepeatLineBreaks(59)}";
                        }
                        tsp.SendData(PacketTypes.Status, text);
                    });
                }
                catch { }
                Thread.Sleep(100);
            }
        }

        public static double colornum = 0;
        public static Color Rambo()
        {
            colornum += 0.01;
            if (colornum > 1)
            {
                colornum = 0;
            }
            return HSL2RGB(colornum, 0.5, 0.5);
        }

        public struct ColorRGB
        {
            public byte R;
            public byte G;
            public byte B;
            public ColorRGB(Color value)
            {
                this.R = value.R;
                this.G = value.G;
                this.B = value.B;
            }

            public static implicit operator Color(ColorRGB rgb)
            {
                Color c = new Color(rgb.R, rgb.G, rgb.B);
                return c;

            }

            public static explicit operator ColorRGB(Color c)
            {
                return new ColorRGB(c);
            }

        }

        public static ColorRGB HSL2RGB(double h, double sl, double l)
        {
            double v;
            double r, g, b;
            r = l;   // default to gray
            g = l;
            b = l;
            v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;
                m = l + l - v;
                sv = (v - m) / v;
                h *= 6.0;
                sextant = (int)h;
                fract = h - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            ColorRGB rgb;
            rgb.R = Convert.ToByte(r * 255.0f);
            rgb.G = Convert.ToByte(g * 255.0f);
            rgb.B = Convert.ToByte(b * 255.0f);
            return rgb;
        }

        private static string RepeatLineBreaks(int v)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < v; i++)
            {
                stringBuilder.Append("\r\n");
            }
            return stringBuilder.ToString();
        }
    }
}
