using DL444.Plotter.Library;
using System;

namespace DL444.Plotter.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            const int hRes = 64;
            const int vRes = 32;

            Composer comp = new Composer(hRes, vRes);

            //LineSegmentBase ddaLine = new BresenhamLineSegment(hRes, vRes);
            //ddaLine.Point0 = new Point(32, 16);
            //ddaLine.Point1 = new Point(32, 48);
            //comp.Add(ddaLine);

            //LineSegmentBase ln = new BresenhamLineSegment(hRes, vRes);
            //ln.Point0 = new Point(0, 32);
            //ln.Point1 = new Point(63, 32);
            //comp.Add(ln);

            //CircleBase circle = new BresenhamCircle(hRes, vRes);
            //circle.Center = new Point(32, 32);
            //circle.Radius = 16;
            //comp.Add(circle);

            //var center = new Point(32, 32);
            //EllipseBase e = new MidpointEllipse(hRes, vRes, 32, 16, center);
            //comp.Add(e);



            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(3, 11), new Point(15, 11)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(3, 3), new Point(15, 3)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(3, 11), new Point(3, 3)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(4, 11), new Point(4, 3)));


            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(26, 11), new Point(38, 3)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(25, 11), new Point(37, 3)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(26, 3), new Point(38, 11)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(25, 3), new Point(37, 11)));

            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(48, 3), new Point(48, 11)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(49, 3), new Point(49, 11)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(49, 7), new Point(60, 11)));
            //comp.Add(new BresenhamLineSegment(hRes, vRes, new Point(49, 7), new Point(60, 3)));

            //var ln = new BresenhamLineSegment(hRes, vRes, new Point(40, 40), new Point(36, 63));
            //ln.CropWindow = new CropWindow(new Point(16, 16), new Point(48, 48));
            //comp.Add(ln);

            //var pol = new ScanlinePolygon(hRes, vRes);
            ////pol.Add(new Point(4, 2));
            ////pol.Add(new Point(10, 12));
            ////pol.Add(new Point(16, 7));
            ////pol.Add(new Point(20, 10));
            ////pol.Add(new Point(24, 6));
            //pol.Add(new Point(4, 2));
            //pol.Add(new Point(10, 12));
            //pol.Add(new Point(16, 7));
            //pol.Add(new Point(20, 10));
            //pol.Add(new Point(25, 10));
            //pol.Add(new Point(28, 6));
            //comp.Add(pol);

            var tri = new ScanlinePolygon(hRes, vRes);
            tri.Add(new Point(4, 2));
            tri.Add(new Point(12, 8));
            tri.Add(new Point(20, 2));
            comp.Add(tri);

            var rect = new ScanlinePolygon(hRes, vRes);
            rect.Add(new Point(42, 2));
            rect.Add(new Point(42, 8));
            rect.Add(new Point(58, 8));
            rect.Add(new Point(58, 2));
            comp.Add(rect);

            var eli = new MidpointEllipse(hRes, vRes, 9, 4, new Point(50, 25));
            comp.Add(eli);

            var dia = new ScanlinePolygon(hRes, vRes);
            dia.Add(new Point(28, 6));
            dia.Add(new Point(14, 16));
            dia.Add(new Point(28, 25));
            dia.Add(new Point(42, 16));
            comp.Add(dia);

            var f = comp.Frame;
            for (int i = 0; i < hRes * vRes; i++)
            {
                bool state = f.GetBit(i);
                System.Console.Write($"{(state ? "1" : "0")}");
                if ((i + 1) % hRes == 0)
                {
                    System.Console.WriteLine();
                }
            }
        }
    }
}
