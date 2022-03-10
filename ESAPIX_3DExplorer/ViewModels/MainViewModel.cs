using ESAPIX.Interfaces;
using g3;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Microsoft.MixedReality.Toolkit.Utilities;
using VMS.TPS.Common.Model.Types;
using System.Collections;
using Unity;
using System.Collections.Generic;

namespace ESAPX_StarterUI.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private IESAPIService _es;
        private Dispatcher _uiThread;

        public MainViewModel(IESAPIService es)
        {
            _es = es;
            _uiThread = Dispatcher.CurrentDispatcher;

            var names = _es.GetValue(sac =>
            {
                if (sac.PlanSetup != null)
                {
                    return sac.PlanSetup
                    .StructureSet
                    .Structures
                    .Select(s => s.Id).ToList();
                }

                return new System.Collections.Generic.List<string>();
            });

            Structures.Clear();
            Structures.AddRange(names);

            ExportCommand = new DelegateCommand(ExportSelectedMesh);
        }

        private void ExportSelectedMesh()
        {
            var mesh = new MeshGeometry3D()
            {
                TriangleIndices = meshIndicies,
                Positions = MeshPositions
            };

            var dlg = new SaveFileDialog();
            dlg.FileName = "structure";
            dlg.DefaultExt = ".obj";
            dlg.Filter = "Obj (.obj) | *.obj";

            var result = dlg.ShowDialog();

            if(result == true)
            {
                var filename = dlg.FileName;
                ESAPIX.Helpers.ObjFileWriter.Write(mesh, filename);
            }
            
        }

        private string _title = "3D Object Explorer";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private Point3DCollection meshPositions;

        public Point3DCollection MeshPositions
        {
            get { return meshPositions; }
            set { SetProperty(ref meshPositions, value); }
        }

        private Int32Collection meshIndicies;

        public Int32Collection MeshIndicies
        {
            get { return meshIndicies; }
            set { SetProperty(ref meshIndicies, value); }
        }


        private Color meshColor;

        public Color MeshColor
        {
            get { return meshColor; }
            set { SetProperty(ref meshColor, value); }
        }

        public ObservableCollection<string> Structures { get; set; } = new ObservableCollection<string>();

        private string selectedStructure;

        public string SelectedStructure
        {
            get { return selectedStructure; }
            set 
            { 
                SetProperty(ref selectedStructure, value);
                SetMeshGeometry(selectedStructure);
            }
        }

        private void SetMeshGeometry(string selectedStructure)
        {
            var (mpoints, mindicies, mcolor, mcenter, mnormals) = _es.GetValue(sac =>
               {
                   var str = sac.PlanSetup
                            .StructureSet
                            .Structures
                            .FirstOrDefault(s => s.Id == selectedStructure);

                   if (str != null)
                   {
                       var mesh = str.MeshGeometry;

                       if (mesh != null)
                       {
                           IEnumerable<Point3D> points = mesh.Positions.ToList();
                           IEnumerable<int> indicies = mesh.TriangleIndices.ToList();
                           var color = str.Color;
                           VVector center = str.CenterPoint;
                           Vector3DCollection normals = mesh.Normals;
                           return (points, indicies, color, center,normals);
                       }
                   }
                   return (null, null, Colors.Transparent, default(VVector),null);
               });
            if (mpoints != null)
            {

                _es.Invoke(() =>
                {
                    //DMesh3 meshnew = DMesh3Builder.Build<Point3D, int>(mpoints, mindicies);



                    //Remesher r = new Remesher(meshnew);
                    //r.PreventNormalFlips = true;
                    //r.SetTargetEdgeLength(0.5);

                    //r.SetTargetEdgeLength(0.75);
                    //r.SmoothSpeedT = 0.5;
                    //r.SetProjectionTarget(MeshProjectionTarget.Auto(meshnew));
                    //MeshConstraintUtil.PreserveBoundaryLoops(r);
                    //double min_edge_len, max_edge_len, avg_edge_len;
                    //MeshQueries.EdgeLengthStats(meshnew, out min_edge_len, out max_edge_len, out avg_edge_len);
                    //r.SetTargetEdgeLength(avg_edge_len * 0.5);
                    //for (int k = 0; k < 20; ++k)
                    //    r.BasicRemeshPass();

                    //MeshPositions = new Point3DCollection(meshnew.Vertices().Select(p =>
                    //{
                    //    return new Point3D((p.x - mcenter.x) / 10, (p.y - mcenter.y) / 10, (p.z - mcenter.z) / 10);
                    //}));

                    //MeshIndicies = new Int32Collection(meshnew.TriangleIndices());
                });


                MeshPositions = new Point3DCollection(mpoints.Select(p =>
                {
                    return new Point3D((p.X - mcenter.x) / 10, (p.Y - mcenter.y) / 10, (p.Z - mcenter.z) / 10);
                }));

                MeshIndicies = new Int32Collection(mindicies);

                MeshColor = mcolor;
            }

        }

        //public MeshGeometry3D Remesh(MeshGeometry3D mesh)
        //{

        //    DMesh3 meshnew = DMesh3Builder.Build(mesh.Positions, mesh.TriangleIndices, mesh.Normals);

        //    Remesher r = new Remesher(meshnew);
        //    r.PreventNormalFlips = true;
        //    //r.SetTargetEdgeLength(0.5);
            
        //    //r.SetTargetEdgeLength(0.75);
        //    r.SmoothSpeedT = 0.5;
        //    r.SetProjectionTarget(MeshProjectionTarget.Auto(meshnew));
        //    MeshConstraintUtil.PreserveBoundaryLoops(r);
        //    double min_edge_len, max_edge_len, avg_edge_len;
        //    MeshQueries.EdgeLengthStats(meshnew, out min_edge_len, out max_edge_len, out avg_edge_len);
        //    r.SetTargetEdgeLength(avg_edge_len * 0.5);
        //    for (int k = 0; k < 20; ++k)
        //        r.BasicRemeshPass();

        //    MeshPositions = new Point3DCollection(r.Mesh.Vertices().Select(p =>
        //    {
        //        return new Point3D((p.x) / 10, (p.y) / 10, (p.z) / 10);
        //    }));

        //    MeshIndicies = new Int32Collection(r.Mesh.TriangleIndices());

        //}

        public DelegateCommand ExportCommand { get; set; }
    }
}
