using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;
using System.Drawing;
using System.Collections.Generic;
using IrrKlang;

namespace Canardstein
{
	public class Jeu
	{
		private static void Main(string[] args) { Jeu jeu = new Jeu(); }

        public List<Chose> Choses = new List<Chose>();
        public IrrlichtDevice Device;

        private uint DerniereFrame = 0;
		private bool K_Avant, K_Arriere, K_Gauche, K_Droite;
		private Texture TextureMur, TextureMurDeco, TextureSol, TexturePlafond;
		private bool[,] Murs = new bool[32, 32];
		private Texture[] TexturePistolet = new Texture[3];
		private int FramePistolet = 0;
		private float ProchaineFramePistolet = 0.1f;
		private ISoundEngine Audio;
        private readonly List<IrrlichtLime.Video.Color> COULEUR_BLANC = new List<IrrlichtLime.Video.Color>(new IrrlichtLime.Video.Color[] { IrrlichtLime.Video.Color.OpaqueWhite, IrrlichtLime.Video.Color.OpaqueWhite, IrrlichtLime.Video.Color.OpaqueWhite, IrrlichtLime.Video.Color.OpaqueWhite });

		public Jeu()
		{
			Device = IrrlichtDevice.CreateDevice(
				DriverType.Direct3D9,
				new Dimension2Di(800, 600),
				32, false, false, true);

			Device.SetWindowCaption("Canardstein 3D");
			Device.OnEvent += Evenement;

			Audio = new ISoundEngine();

			for (int i = 0; i < 3; i++)
				TexturePistolet[i] = Device.VideoDriver.GetTexture(@"Textures\pistolet" + i.ToString() + ".png");

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
						Murs[x, y] = false;

						if ((col.R == 255) && (col.G == 255) && (col.B == 255))
						{
							SceneNode cube = Device.SceneManager.AddCubeSceneNode(1, null, 0, new Vector3Df(x, 0, y));
							cube.SetMaterialFlag(MaterialFlag.Lighting, false);
							cube.SetMaterialTexture(0, TextureMur);
							Murs[x, y] = true;
						}
						else if ((col.R == 0) && (col.G == 0) && (col.B == 255))
						{
							SceneNode cube = Device.SceneManager.AddCubeSceneNode(1, null, 0, new Vector3Df(x, 0, y));
							cube.SetMaterialFlag(MaterialFlag.Lighting, false);
							cube.SetMaterialTexture(0, TextureMurDeco);
							Murs[x, y] = true;
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
                AjouterChose(3, 3);

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

				if (TenterMouvement(camera, vitesse) || TenterMouvement(camera, new Vector3Df(vitesse.X, 0, 0)) || TenterMouvement(camera, new Vector3Df(0, 0, vitesse.Z)))
					camera.Target = camera.Position + new Vector3Df(1, 0, 0);

				Device.VideoDriver.BeginScene(ClearBufferFlag.Color | ClearBufferFlag.Depth, IrrlichtLime.Video.Color.OpaqueMagenta);
				Device.SceneManager.DrawAll();

				Device.VideoDriver.Draw2DImage(TexturePistolet[FramePistolet], new Recti(new Vector2Di(250, 300), new Dimension2Di(300, 300)), new Recti(0, 0, 512, 512), null, COULEUR_BLANC, true);
				Device.VideoDriver.EndScene();

				if (FramePistolet > 0)
				{
					ProchaineFramePistolet -= tempsEcoule;

					if (ProchaineFramePistolet <= 0f)
					{
						FramePistolet++;
						if (FramePistolet > 2) FramePistolet = 0;
						ProchaineFramePistolet = 0.1f;
					}
				}
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
			else if (e.Type == EventType.Mouse)
			{
				if ((e.Mouse.Type == MouseEventType.LeftDown) && (FramePistolet == 0))
				{
					Audio.Play2D(@"Sound\pistolet.wav");
					FramePistolet = 1;
					ProchaineFramePistolet = 0.1f;
				}
			}


			return false;
		}

		private bool TenterMouvement(SceneNode objet, Vector3Df direction, float rayon = .25f)
		{
			Vector2Df nouvellePosition = new Vector2Df(objet.Position.X + direction.X + .5f, objet.Position.Z + direction.Z + .5f);

			int minX = (int)(nouvellePosition.X - rayon);
			int maxX = (int)(nouvellePosition.X + rayon);

			int minY = (int)(nouvellePosition.Y - rayon);
			int maxY = (int)(nouvellePosition.Y + rayon);

			int x, y;

			for (x = minX; x <= maxX; x++)
				for (y = minY; y <= maxY; y++)
				{
					if ((x < 0) || (y < 0) || (x >= 32) || (x >= 32)) return false;
					if (Murs[x, y]) return false;
				}

			objet.Position += direction;
			return true;
		}

        private void AjouterChose(int x, int y)
        {
            Chose nouvelleChose = new Chose();
            nouvelleChose.Creer(this, x, y);
            Choses.Add(nouvelleChose);
        }
    }

    public class Chose
    {
        public bool Detruit { get; private set; } = false;
        private BillboardSceneNode Sprite;
        private Jeu Jeu;

        public Vector2Df Position { get { return new Vector2Df(Sprite.Position.X, Sprite.Position.Z); } }

        public void Creer(Jeu jeu, int x, int y)
        {
            Jeu = jeu;
            Sprite = Jeu.Device.SceneManager.AddBillboardSceneNode(null);
            Sprite.SetMaterialFlag(MaterialFlag.Lighting, false);
            Sprite.SetMaterialType(MaterialType.TransparentAlphaChannel);
            Sprite.SetSize(1, 1, 1);
            Sprite.Position = new Vector3Df(x + 0.5f, 0, y + 0.5f);
        }

        public void Detruire()
        {
            if (!Detruit) return;
            Sprite.Remove();
            Detruit = true;
        }
    }

}

