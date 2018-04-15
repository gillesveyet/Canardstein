using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;
using System.Drawing;

namespace Canardstein
{
	public class Jeu
	{
		private static void Main(string[] args) { Jeu jeu = new Jeu(); }

		private IrrlichtDevice Device;
		private uint DerniereFrame = 0;
		private bool K_Avant, K_Arriere, K_Gauche, K_Droite;
		private Texture TextureMur, TextureMurDeco, TextureSol, TexturePlafond;

		public Jeu()
		{
			Device = IrrlichtDevice.CreateDevice(
				DriverType.Direct3D9,
				new Dimension2Di(800, 600),
				32, false, false, true);

			Device.SetWindowCaption("Canardstein 3D");
			Device.OnEvent += Evenement;

			TextureMur = Device.VideoDriver.GetTexture(@"Textures\mur.png");
			TextureMurDeco = Device.VideoDriver.GetTexture(@"Textures\mur_deco.png");
			TextureSol = Device.VideoDriver.GetTexture(@"Textures\sol.png");
			TexturePlafond = Device.VideoDriver.GetTexture(@"Textures\plafond.png");

			using (Bitmap carte = (Bitmap)System.Drawing.Image.FromFile(@"Textures\carte.png"))
			{
				for (int x = 0; x < 32; x++)
					for (int y = 0; y < 32; y++)
					{
						System.Drawing.Color col = carte.GetPixel(x, y);

						if ((col.R == 255) && (col.G == 255) && (col.B == 255))
						{
							SceneNode cube = Device.SceneManager.AddCubeSceneNode(1, null, 0, new Vector3Df(x, 0, y));
							cube.SetMaterialFlag(MaterialFlag.Lighting, false);
							cube.SetMaterialTexture(0, TextureMur);
						}
						else if ((col.R == 0) && (col.G == 0) && (col.B == 255))
						{
							SceneNode cube = Device.SceneManager.AddCubeSceneNode(1, null, 0, new Vector3Df(x, 0, y));
							cube.SetMaterialFlag(MaterialFlag.Lighting, false);
							cube.SetMaterialTexture(0, TextureMurDeco);
						}
					}
			}



			//On va maintenant créer un plancher et un plafond, histoire de mettre un peu d'ordre. On commence par générer meshSol,
			//le modèle 3D d'un plan que nous allons nommer « plan » (parce qu'il est obligatoire de fournir un nom), divisé en 32 × 32 cases de taille 1 × 1.
			//On ne va pas lui assigner de texture pour le moment (null). Ce plan sera parfaitement horizontal, avec 0 × 0 variation de hauteur 0.
			//Enfin, la texture sera répétée 32 fois horizontalement et verticalement.
			Mesh meshSol = Device.SceneManager.AddHillPlaneMesh("plan", new Dimension2Df(1, 1), new Dimension2Di(32, 32), null, 0, new Dimension2Df(0, 0), new Dimension2Df(32, 32));

			//On désactive l'éclairage dynamique sur notre nouvel objet, comme on l'avait déjà fait pour le cube, et on lui assigne TextureSol. 
			//Puis on le positionne à une distance d'une demi-unité sous le zéro (ou, plus précisément, sous les pieds de notre héros, la position Y de la caméra étant de 0),
			//et à −15.5 sur les axes X et Z (notre plan mesurant 32 par 32 cases de largeur 1, le déplacer de 15,5 unités permet de s'assurer que l'un de ses coins sera à la coordonnée 0,0,
			//ce sera utile dans la prochaine leçon).
			MeshSceneNode sol = Device.SceneManager.AddMeshSceneNode(meshSol);
			sol.SetMaterialFlag(MaterialFlag.Lighting, false);
			sol.SetMaterialTexture(0, TextureSol);
			sol.Position = new Vector3Df(15.5f, -0.5f, 15.5f);

			//Pareil pour le plafond, sauf qu'on le place à une hauteur de 0,5 (et non de −0,5), et qu'on le pivote de 180 sur l'axe X pour le tourner vers le bas.
			MeshSceneNode plafond = Device.SceneManager.AddMeshSceneNode(meshSol);
			plafond.SetMaterialFlag(MaterialFlag.Lighting, false);
			plafond.SetMaterialTexture(0, TexturePlafond);
			plafond.Position = new Vector3Df(15.5f, 0.5f, 15.5f);
			plafond.Rotation = new Vector3Df(180, 0, 0);

			CameraSceneNode camera = Device.SceneManager.AddCameraSceneNode(null, new Vector3Df(1, 0, 1), new Vector3Df(2, 0, 1));

			//Abaisse la distance minimum d'affichage de la caméra. La valeur par défaut est de 1, ce qui ne nous convient pas :
			//plafond et sol se trouvant à 0,5 unité de la caméra, ils seraient trop près pour être dessinés.
			camera.NearValue = 0.1f;

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

				Device.VideoDriver.BeginScene(ClearBufferFlag.Color | ClearBufferFlag.Depth, IrrlichtLime.Video.Color.OpaqueMagenta);
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

