using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StadiumTools
{
    public struct Srf
    {
        //Properties
        public bool isValid { get; set; }
        public Pt3d A { get; set; }
        public Pt3d B { get; set; }
        public Pt3d C { get; set; }
        public Pt3d D { get; set; }

        //Constructors
        public Srf(Pt3d a, Pt3d b, Pt3d c, Pt3d d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            isValid = true; 
        }

        //Methods
        public static Srf[] Construct4x(Pt3d[] pts0, Pt3d[] pts1, Pt3d[] pts2, Pt3d[] pts3)
        {
            if (pts0.Length != pts1.Length || pts1.Length != pts2.Length || pts2.Length != pts3.Length)
            {
                throw new ArgumentException($"Error: Srf.Construct4x requires equal arrays of points [{pts0.Length},{pts1.Length},{pts2.Length},{pts3.Length}]");
            }
            List<Pt3d> vertices = new List<Pt3d>();
            List<Srf> surfaces = new List<Srf>();
            int length = pts0.Length;
            vertices.AddRange(pts0);
            vertices.AddRange(pts1);
            vertices.AddRange(pts2);
            vertices.AddRange(pts3);

            for (int i = 0; i < 3; i++)
            {
                int k = length * i;
                for (int j = 0; j < length - 1; j++)
                {
                    int a = k + j;
                    int b = k + j + 1;
                    int c = k + length + j + 1;
                    int d = k + length + j;
                    surfaces.Add(new Srf(vertices[a], vertices[b], vertices[c], vertices[d]));
                }
            }
            return surfaces.ToArray();
        }

        public static Srf[] Construct2x(Pt3d[] pts0, Pt3d[] pts1)
        {
            if (pts0.Length != pts1.Length)
            {
                throw new ArgumentException($"Error: Srf.Construct2x Inputs must be equal length [{pts0.Length},{pts1.Length}]");
            }
            List<Pt3d> vertices = new List<Pt3d>();
            List<Srf> surfaces = new List<Srf>();
            int length = pts0.Length;
            vertices.AddRange(pts0);
            vertices.AddRange(pts1);

            int i = 0;
            int k = length * i;
            for (int j = 0; j < length - 1; j++)
            {
                int a = k + j;
                int b = k + j + 1;
                int c = k + length + j + 1;
                int d = k + length + j;
                surfaces.Add(new Srf(vertices[a], vertices[b], vertices[c], vertices[d]));
            }
            return surfaces.ToArray();
        }
    }
}
