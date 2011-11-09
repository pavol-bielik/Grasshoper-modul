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
using System.Linq;

//test git
// test martin
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
            pManager.Register_LineParam("Line", "L", "Resulted structure");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        
       {
            List<Line> lineList = new List<Line>();
            List<Line> lineListInput = new List<Line>();
            List<LineD> lineDList = new List<LineD>();
            LineD[] currLinesArr;
            Subdivision InverseGraph;
         
            Dictionary<PointD, LineD> InverseEdgesMapping = new Dictionary<PointD,LineD>();
            Dictionary<LineD, double> RealDist = new Dictionary<LineD,double>();
            Dictionary<LineD, double> RealAngle = new Dictionary<LineD,double>();

            if (DA.GetDataList(0, lineListInput)) //If it works...
            {
                // sort out duplicate Lines
                //========================================================
                List<Point3d> mPointList = new List<Point3d>();
                foreach (Line mLine in lineListInput)
                {
                    //compute middle points
                    Point3d middle = (mLine.From + mLine.To) * 0.5;
                    mPointList.Add(middle);
                }
                // add lines and middle points to dictionary
                Dictionary<Point3d, Line> inputMap = new Dictionary<Point3d, Line>();
                for (int i = 0; i < mPointList.Count; i++)
                {
                    if (inputMap.ContainsKey(mPointList[i]))
                        continue;
                    inputMap.Add(mPointList[i], lineListInput[i]);
                }
                lineList = inputMap.Values.ToList();

                /*
                // filter duplicate points
                mPointList = mPointList.Distinct().ToList();
                for (int i = 0; i < lineListInput.Count; i++)
                {
                    Point3d point = mPointList[i];
                    lineList[i] = inputMap[point];
                }*/

                //======================================================================
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
                
                    float[] finRes = new float[lineDList.Count];
                    float[] tempRes = ShortPahtesMetric.GetNormChoiceArray();
                    for (int i = 0; i < curVertices.Count; i++)
                    {
                        PointD point = curVertices[i];
                        curEdge = InverseEdgesMapping[point];
                        int idx = lineDList.IndexOf(curEdge);

                        finRes[idx] = tempRes[i];
                        
                    }
           
                DA.SetDataList(0, new List<float>(finRes));
                DA.SetDataList(1, lineList);
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

        private float map(float value, float low1, float high1, float low2, float high2)
        {
            float newValue = ((value - low1) * (high2 - low2) / (high1 - low1)) + low2;
            return (newValue);
        }

    }
}
