using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Rhino;
using Grasshopper.Kernel;
using GHA_StadiumTools.Properties;
using Rhino.Geometry;

namespace GHA_StadiumTools
{
    /// <summary>
    /// Create a custom GH component called ST_ConstructSection2D using the GH_Component as base. 
    /// </summary>
    public class ST_Bowl3dBreps : GH_Component
    {
        /// <summary>
        /// A custom component for input parameters to generate a new 2D section. 
        /// </summary>
        public ST_Bowl3dBreps()
            : base(nameof(ST_Bowl3dBreps), "B3dB", "Calculate the Brep geometry of a StadiumTools Bowl3d", "StadiumTools", "3D")
        {
        }

        /// <summary>
        /// Registers all input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Bowl3d", "B3Dd", "a valid StadiumTools Bowl3d object", GH_ParamAccess.item);
        }

        //Set parameter indixes to names (for readability)
        private static int IN_Bowl3d = 0;
        private static int OUT_Bowl3d_Brep = 0;
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Bowl3d Breps", "B3dB", "The brep geometry of a StadiumTools Bowl3d object", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Construct a new section from Data Access
            ST_Bowl3dBreps.ConstructBowl3dBrepsFromDA(DA, this);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.ST_Bowl3DBreps;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("282f2fd8-55db-4ea7-8a2f-7b06d0f044d0");

        //Methods  
        private static void ConstructBowl3dBrepsFromDA(IGH_DataAccess DA, GH_Component thisComponent)
        {
            //Item Containers
            var bowl3dGooItem = new StadiumTools.Bowl3dGoo();

            //Get Goos
            DA.GetData<StadiumTools.Bowl3dGoo>(IN_Bowl3d, ref bowl3dGooItem);
            var bowl3dItem = bowl3dGooItem.Value;
            //Get Bowl3d StadiumTools.Srf
            StadiumTools.Srf[][][] bowl3dSrfs = bowl3dItem.ToSurfaces();
            StadiumTools.Mesh[,] bowl3dMesh = bowl3dItem.ToMesh();
            List<Rhino.Geometry.Mesh> meshes = StadiumTools.IO.ListFromMultiArray(bowl3dItem.ToMesh());
            var breps = new List<Rhino.Geometry.Brep>();
            foreach (var m in meshes)
            {
                Brep newBrep = Brep.CreateFromMesh(m, false);
                breps.Add(newBrep);
            }

            //Convert to RhinoCommon Surfaces
            //List<Rhino.Geometry.Surface> bowl3dSurfaces = StadiumTools.IO.SurfaceFromSrf(bowl3dSrfs);
            //Output surface list
            DA.SetDataList(OUT_Bowl3d_Brep, breps);
        }




    }
}
