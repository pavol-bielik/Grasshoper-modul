using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using Grasshopper.Kernel.Components;
using Rhino.Display;
using Grasshopper.Kernel.Types;
using GeoObj;
using GeoObj.Extensions;
using GeoObj.Helper;
using GeoObj.Graph;
using Tektosyne.Geometry;
using Tektosyne.Collections;
using Tektosyne;
using System.Windows.Forms;


namespace SpaceSyntaxComponent
{
    public class SpaceSyntaxComponent : GH_Component
    {
        public SpaceSyntaxComponent()

            //Call the base constructor
            : base("Space Syntax", "SpSc", "Provide Integration Analysis", "Extra", "Space Syntax")
        { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.Register_LineParam("Line", "L", "The Line Collection", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Number", "N", "Analysis results");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        
       {
            List<Line> lineList = new List<Line>();
            List<LineD> lineDList = new List<LineD>();
            LineD[] currLinesArr;
            Subdivision InverseGraph;
            float[] result;
         
            Dictionary<PointD, LineD> InverseEdgesMapping = new Dictionary<PointD,LineD>();
            Dictionary<LineD, double> RealDist = new Dictionary<LineD,double>();
            Dictionary<LineD, double> RealAngle = new Dictionary<LineD,double>();

            if (DA.GetDataList(0, lineList)) //If it works...
            {
                foreach (Line a in lineList)
                {
                    if (a.IsValid)
                    {
                        
                        PointD start = new PointD(a.FromX, a.FromY);
                        PointD end = new PointD(a.ToX, a.ToY);
                        LineD b = new LineD(start, end);
                        lineDList.Add(b);
                    }
                }
                    
                    Subdivision Graph;
                    Graph = new Subdivision();
                    currLinesArr = lineDList.ToArray();
                    Graph = Subdivision.FromLines(currLinesArr);
                    InverseGraph = Tools.ConstructInverseGraph(Graph, ref RealDist, ref RealAngle, ref InverseEdgesMapping);
                    SubdShortestPath ShortPahtesMetric = new SubdShortestPath(InverseGraph, RealDist);
                    
                    ShortPahtesMetric.EvaluateMetric(null);

                    List<PointD> curVertices = new List<PointD>();
                    foreach (PointD pt in InverseGraph.Vertices.Keys)
                    {
                        curVertices.Add(pt);
                    }

                    LineD curEdge;
                
                    float[] finRes = new float[curVertices.Count];
                    float[] tempRes = ShortPahtesMetric.GetNormChoiceArray();
                    for (int i = 0; i < curVertices.Count; i++)
                    {
                        PointD point = curVertices[i];
                        curEdge = InverseEdgesMapping[point];
                        int idx = lineDList.IndexOf(curEdge);

                        finRes[idx] = tempRes[i];
                        
                    }

                    


                 
                DA.SetDataList(0, new List<float>(finRes));
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                //Do not ever copy a Guid!
                return new Guid("{E3737B66-EE16-4E4E-99C2-A69CC1CF30BD}");

            }
        }
    }
}
