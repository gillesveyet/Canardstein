using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace Canardstein
{
    public class Jeu
    {
        private static void Main(string[] args) { Jeu jeu = new Jeu(); }

        private IrrlichtDevice Device;

        public Jeu()
        {
            Device = IrrlichtDevice.CreateDevice(
                DriverType.Direct3D9,
                new Dimension2Di(800, 600),
                32, false, false, true);

            Device.SetWindowCaption("Canardstein 3D");

            //Ajouter un cube dont les côtés sont de taille 1, qui n'est rattaché à aucun autre objet (null), identifié par le numéro 0, aux coordonnées 2,0,0, avec une rotation de 45 degrés sur l'axe Y ;
            SceneNode cube = Device.SceneManager.AddCubeSceneNode(1, null, 0, new Vector3Df(2, 0, 0), new Vector3Df(0, 45, 0));

            //Désactiver l'éclairage du cube (sans quoi il serait tout noir, vu qu'on n'a aucune source lumineuse) 
            cube.SetMaterialFlag(MaterialFlag.Lighting, false);

            //Ajouter une caméra attachée à aucun autre objet (null), aux coordonnées 0,0,0, tournée vers les coordonnées 2,0,0 (celles du cube).
            CameraSceneNode camera = Device.SceneManager.AddCameraSceneNode(null, new Vector3Df(0, 0, 0), new Vector3Df(2, 0, 0));

            while (Device.Run())
            {
                Device.VideoDriver.BeginScene(ClearBufferFlag.Color | ClearBufferFlag.Depth, Color.OpaqueMagenta);

                Device.SceneManager.DrawAll();

                Device.VideoDriver.EndScene();
            }
        }
    }
}

