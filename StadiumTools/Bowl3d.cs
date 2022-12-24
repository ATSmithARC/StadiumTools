using Rhino.Commands;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;

namespace StadiumTools
{
    public class Bowl3d : ICloneable
    {
        //Properties
        public bool IsValid { get; set; }
        public Section[] Sections { get; set; }
        public Section ClosestSection { get; set; }
        public BowlPlan BowlPlan { get; set; }

        public double Unit { get; set; }

        //Constructors
        public Bowl3d()
        {
        }

        public Bowl3d(Section sectionParameters, BowlPlan bowlPlan)
        {
            if (sectionParameters.Unit != bowlPlan.Unit)
            {
                throw new ArgumentException($"Error: Section[{sectionParameters.Unit}] and BowlPlan[{bowlPlan.Unit}] must have the same Unit.");
            }
            BowlPlan = bowlPlan;
            // Calculate Worst Case Section (closest section to touchline)  ie. worst c-Values    
            Section closestSection = Section.CalcClosestSection(sectionParameters, bowlPlan);
            ClosestSection = closestSection;
            // Apply worst case section geometry to all sections and re-calculate the spectators;
            Section[] sections = new Section[bowlPlan.SectionCount];
            for (int i = 0; i < bowlPlan.SectionCount; i++)
            {
                double xOffset = bowlPlan.Boundary.PlaneOffsets[i] - bowlPlan.Boundary.ClosestPlaneDist;
                Section newSection = Section.ReCalculateFixedGeometry(closestSection, bowlPlan.Boundary.Planes[i], xOffset);
                sections[i] = newSection;
            }
            Sections = sections;
            
            IsValid = true;
        }

        //Methods   
        public Mesh[,] ToMesh()
        {
            int iCount = (int)BowlPlan.SectionCount / 2;
            int jCount = Sections[0].Tiers.Length;
            Mesh[,] result = new Mesh[iCount, jCount];
            for(int i = 0; i < iCount; i++)
            {
                for (int j = 0; j < Sections[i].Tiers.Length; j++)
                {
                    result[i, j] = CalcTierMesh(this, i * 2, j);
                }
            }
            return result;
        }

        public Srf[][][] ToSurfaces()
        {
            int iCount = (int)BowlPlan.SectionCount / 2;
            int jCount = Sections[0].Tiers.Length;
            Srf[][][] result = new Srf[iCount][][];
            for (int i = 0; i < iCount; i++)
            {
                result[i] = new Srf[jCount][];
                for (int j = 0; j < Sections[i].Tiers.Length; j++)
                {
                    result[i][j] = CalcTierSurfaces(this, i * 2, j);
                }
            }
            return result;
        }

        public Mesh CalcTierMesh(Bowl3d bowl3d, int sectionIndex, int tierIndex)
        {
            Tier thisTier = bowl3d.Sections[sectionIndex].Tiers[tierIndex];
            Tier nextTier = new Tier();
            Pln3d thisPlane = bowl3d.BowlPlan.Boundary.Planes[sectionIndex];
            Pt3d thisFirstPt = new Pt3d(bowl3d.Sections[sectionIndex].Tiers[0].Points2d[0], thisPlane);
            Pt3d nextFirstPt = new Pt3d();
            Pln3d nextPlane = new Pln3d();
            if (sectionIndex == bowl3d.BowlPlan.SectionCount - 2)
            {
                nextPlane = bowl3d.BowlPlan.Boundary.Planes[0];
                nextFirstPt = new Pt3d(bowl3d.Sections[0].Tiers[0].Points2d[0], nextPlane);
                nextTier = bowl3d.Sections[0].Tiers[tierIndex];
            }
            else
            {
                nextPlane = bowl3d.BowlPlan.Boundary.Planes[sectionIndex + 2];
                nextFirstPt = new Pt3d(bowl3d.Sections[sectionIndex + 2].Tiers[0].Points2d[0], nextPlane);
                nextTier = bowl3d.Sections[sectionIndex + 2].Tiers[tierIndex];
            }
            Pt3d[] meshStartPts = Pt3d.FromPt2d(thisTier.Points2d, thisPlane);
            Pt3d[] meshEndPts = Pt3d.FromPt2d(nextTier.Points2d, nextPlane); //shold be nsext tier.Points2d
            
            double tierWidth = thisFirstPt.DistanceTo(nextFirstPt);
            double aisleWidth = thisTier.AisleWidth;
            //throw new ArgumentException($"[{sectionIndex}, {tierIndex}] TW:{tierWidth} AW:{aisleWidth} FP:({thisFirstPt.X},{thisFirstPt.Y}), NP:({nextFirstPt.X},{nextFirstPt.Y})");
            if (aisleWidth < tierWidth)
            {
                double[] aisleParameters = GetParameters(tierWidth, aisleWidth);
                Pt3d[][] meshTweenPts = Pt3d.Tween2(meshStartPts, meshEndPts, aisleParameters);
                Mesh result = Mesh.Construct4x(meshStartPts, meshTweenPts[0], meshTweenPts[1], meshEndPts);
                return result;
            }
            else
            {
                Mesh result = Mesh.Construct2x(meshStartPts, meshEndPts);
                return result;
            }
        }

        public Srf[] CalcTierSurfaces(Bowl3d bowl3d, int sectionIndex, int tierIndex)
        {
            Tier thisTier = bowl3d.Sections[sectionIndex].Tiers[tierIndex];
            Tier nextTier = new Tier();
            Pln3d thisPlane = bowl3d.BowlPlan.Boundary.Planes[sectionIndex];
            Pt3d thisFirstPt = new Pt3d(bowl3d.Sections[sectionIndex].Tiers[0].Points2d[0], thisPlane);
            Pt3d nextFirstPt = new Pt3d();
            Pln3d nextPlane = new Pln3d();
            if (sectionIndex == bowl3d.BowlPlan.SectionCount - 2)
            {
                nextPlane = bowl3d.BowlPlan.Boundary.Planes[0];
                nextFirstPt = new Pt3d(bowl3d.Sections[0].Tiers[0].Points2d[0], nextPlane);
                nextTier = bowl3d.Sections[0].Tiers[tierIndex];
            }
            else
            {
                nextPlane = bowl3d.BowlPlan.Boundary.Planes[sectionIndex + 2];
                nextFirstPt = new Pt3d(bowl3d.Sections[sectionIndex + 2].Tiers[0].Points2d[0], nextPlane);
                nextTier = bowl3d.Sections[sectionIndex + 2].Tiers[tierIndex];
            }
            Pt3d[] brepStartPts = Pt3d.FromPt2d(thisTier.Points2d, thisPlane);
            Pt3d[] brepEndPts = Pt3d.FromPt2d(nextTier.Points2d, nextPlane); //shold be nsext tier.Points2d

            double tierWidth = thisFirstPt.DistanceTo(nextFirstPt);
            double aisleWidth = thisTier.AisleWidth;
            //throw new ArgumentException($"[{sectionIndex}, {tierIndex}] TW:{tierWidth} AW:{aisleWidth} FP:({thisFirstPt.X},{thisFirstPt.Y}), NP:({nextFirstPt.X},{nextFirstPt.Y})");
            if (aisleWidth < tierWidth)
            {
                double[] aisleParameters = GetParameters(tierWidth, aisleWidth);
                Pt3d[][] brepTweenPts = Pt3d.Tween2(brepStartPts, brepEndPts, aisleParameters);
                Srf[] result = Srf.Construct4x(brepStartPts, brepTweenPts[0], brepTweenPts[1], brepEndPts);
                return result;
            }
            else
            {
                Srf[] result = Srf.Construct2x(brepStartPts, brepEndPts);
                return result;
            }
        }

        /// <summary>
        /// create thisFirstPt deep copy clone of thisFirstPt bowl3d
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            //Deep copy
            Bowl3d clone = (Bowl3d)this.MemberwiseClone();
            {
                clone.BowlPlan = (BowlPlan)BowlPlan.Clone();
                clone.Sections = (Section[])Sections.Clone();
            }
            return clone;
        }

        public static double[] GetParameters(double tierWidth, double aisleWidth)
        {
            double[] result = new double[2];
            double halfAisle = (aisleWidth / tierWidth) / 2;
            result[0] = 0.5 - halfAisle;
            result[1] = 0.5 + halfAisle;
            return result;
        }

        
    }
}
