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
        private uint DerniereFrame = 0;
        private bool K_Avant, K_Arriere, K_Gauche, K_Droite;

        public Jeu()
        {
            Device = IrrlichtDevice.CreateDevice(
                DriverType.Direct3D9,
                new Dimension2Di(800, 600),
                32, false, false, true);

            Device.SetWindowCaption("Canardstein 3D");
            Device.OnEvent += Evenement;

            //Ajouter un cube dont les côtés sont de taille 1, qui n'est rattaché à aucun autre objet (null), identifié par le numéro 0, aux coordonnées 2,0,0, avec une rotation de 45 degrés sur l'axe Y ;
            SceneNode cube = Device.SceneManager.AddCubeSceneNode(1, null, 0, new Vector3Df(2, 0, 0), new Vector3Df(0, 45, 0));

            //Désactiver l'éclairage du cube (sans quoi il serait tout noir, vu qu'on n'a aucune source lumineuse) 
            cube.SetMaterialFlag(MaterialFlag.Lighting, false);

            //Ajouter une caméra attachée à aucun autre objet (null), aux coordonnées 0,0,0, tournée vers les coordonnées 2,0,0 (celles du cube).
            CameraSceneNode camera = Device.SceneManager.AddCameraSceneNode(null, new Vector3Df(0, 0, 0), new Vector3Df(2, 0, 0));

            while (Device.Run())
            {
                float tempsEcoule = (Device.Timer.Time - DerniereFrame) / 1000f;
                DerniereFrame = Device.Timer.Time;

                Vector3Df vitesse = new Vector3Df();

                if (K_Avant)
                    vitesse.X = 1;
                else if (K_Arriere)
                    vitesse.X = -1;
                if (K_Gauche)
                    vitesse.Z = 1;
                else if (K_Droite)
                    vitesse.Z = -1;

                vitesse = vitesse.Normalize() * tempsEcoule * 2;
                camera.Position += vitesse;
                camera.Target = camera.Position + new Vector3Df(1, 0, 0);

                Device.VideoDriver.BeginScene(ClearBufferFlag.Color | ClearBufferFlag.Depth, Color.OpaqueMagenta);
                Device.SceneManager.DrawAll();
                Device.VideoDriver.EndScene();
            }
        }

        private bool Evenement(Event e)
        {
            if (e.Type == EventType.Key)
            {
                switch (e.Key.Key)
                {
                    case KeyCode.KeyZ: K_Avant = e.Key.PressedDown; break;
                    case KeyCode.KeyS: K_Arriere = e.Key.PressedDown; break;
                    case KeyCode.KeyQ: K_Gauche = e.Key.PressedDown; break;
                    case KeyCode.KeyD: K_Droite = e.Key.PressedDown; break;
                }
            }
            return false;
        }
    }
}

