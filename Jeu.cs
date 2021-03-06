﻿using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;
using System.Drawing;
using System.Collections.Generic;
using IrrKlang;
using System;

namespace Canardstein
{
	public class Jeu
	{
		private static void Main(string[] args) { Jeu jeu = new Jeu(); }

		public List<Chose> Choses = new List<Chose>();
		public IrrlichtDevice Device;
		public Texture[] TextureGarde = new Texture[7];
		public int Munitions = 50;
		public float AlphaSang = 0;

		private uint DerniereFrame = 0;
		private bool K_Avant, K_Arriere, K_Gauche, K_Droite;
		private double Rotation = 0;
		private Vector3Df VecteurAvant = new Vector3Df(1, 0, 0);
		private Vector3Df VecteurDroite = new Vector3Df(0, 0, -1);

		private Texture TextureMur, TextureMurDeco, TextureSol, TexturePlafond;
		private bool[,] Murs = new bool[32, 32];
		private Texture[] TexturePistolet = new Texture[3];
		private Texture TexturePolice;
		private int FramePistolet = 0;
		private float ProchaineFramePistolet = 0.1f;

		public ISoundEngine Audio;
		private readonly List<IrrlichtLime.Video.Color> COULEUR_BLANC = new List<IrrlichtLime.Video.Color>(new IrrlichtLime.Video.Color[] { IrrlichtLime.Video.Color.OpaqueWhite, IrrlichtLime.Video.Color.OpaqueWhite, IrrlichtLime.Video.Color.OpaqueWhite, IrrlichtLime.Video.Color.OpaqueWhite });

		public int Vies = 100;

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

			for (int i = 0; i < 7; i++)
				TextureGarde[i] = Device.VideoDriver.GetTexture(@"Textures\nazi" + i.ToString("00") + ".png");

			TextureMur = Device.VideoDriver.GetTexture(@"Textures\mur.png");
			TextureMurDeco = Device.VideoDriver.GetTexture(@"Textures\mur_deco.png");
			TextureSol = Device.VideoDriver.GetTexture(@"Textures\sol.png");
			TexturePlafond = Device.VideoDriver.GetTexture(@"Textures\plafond.png");
			TexturePolice = Device.VideoDriver.GetTexture(@"Textures\police.png");

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
							cube.SetMaterialFlag(MaterialFlag.Fog, true);
							cube.SetMaterialTexture(0, TextureMur);
							Murs[x, y] = true;
						}
						else if ((col.R == 0) && (col.G == 0) && (col.B == 255))
						{
							SceneNode cube = Device.SceneManager.AddCubeSceneNode(1, null, 0, new Vector3Df(x, 0, y));
							cube.SetMaterialFlag(MaterialFlag.Lighting, false);
							cube.SetMaterialFlag(MaterialFlag.Fog, true);
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
			sol.SetMaterialFlag(MaterialFlag.Fog, true);
			sol.SetMaterialTexture(0, TextureSol);
			sol.Position = new Vector3Df(15.5f, -0.5f, 15.5f);

			//Pareil pour le plafond, sauf qu'on le place à une hauteur de 0,5 (et non de −0,5), et qu'on le pivote de 180 sur l'axe X pour le tourner vers le bas.
			MeshSceneNode plafond = Device.SceneManager.AddMeshSceneNode(meshSol);
			plafond.SetMaterialFlag(MaterialFlag.Lighting, false);
			plafond.SetMaterialFlag(MaterialFlag.Fog, true);
			plafond.SetMaterialTexture(0, TexturePlafond);
			plafond.Position = new Vector3Df(15.5f, 0.5f, 15.5f);
			plafond.Rotation = new Vector3Df(180, 0, 0);

			CameraSceneNode camera = Device.SceneManager.AddCameraSceneNode(null, new Vector3Df(1, 0, 1), new Vector3Df(2, 0, 1));

			//Abaisse la distance minimum d'affichage de la caméra. La valeur par défaut est de 1, ce qui ne nous convient pas :
			//plafond et sol se trouvant à 0,5 unité de la caméra, ils seraient trop près pour être dessinés.
			camera.NearValue = 0.1f;

			AjouterChose<Ennemi>(3, 3);
			AjouterChose<Ennemi>(6, 12);
			AjouterChose<Ennemi>(12, 12);
			AjouterChose<Ennemi>(20, 6);
			AjouterChose<Ennemi>(28, 16);
			AjouterChose<Ennemi>(11, 27);

			Device.VideoDriver.Fog = new Fog(new IrrlichtLime.Video.Color(138, 125, 81, 0), FogType.Linear, 0, 10);

			ParticleSystemSceneNode part = Device.SceneManager.AddParticleSystemSceneNode(false, null, 0, new Vector3Df(29, -.5f, 30));
			part.SetMaterialFlag(MaterialFlag.Lighting, false);
			ParticleEmitter em = part.CreateBoxEmitter(
				new AABBox(-.5f, 0, -.5f, .5f, 0, .5f),
				new Vector3Df(0.0f, 0.0015f, 0.0f),
				20, 24,
				new IrrlichtLime.Video.Color(0, 128, 255, 0), new IrrlichtLime.Video.Color(0, 255, 255, 0),
				800, 800, 0,
				new Dimension2Df(0.005f, 0.04f), new Dimension2Df(0.01f, 0.08f));
			part.Emitter = em;

			while (Device.Run())
			{
				float tempsEcoule = (Device.Timer.Time - DerniereFrame) / 1000f;
				DerniereFrame = Device.Timer.Time;

				if ((AlphaSang > 0) && (Vies > 0))
					AlphaSang = Math.Max(0, AlphaSang - tempsEcoule * 500);


				if (((int)camera.Position.X == 29) && ((int)camera.Position.Z == 30)) Device.Close();

				if (Device.CursorControl.Position.X != 400)
				{
					Rotation += (Device.CursorControl.Position.X - 400) * 0.0025;
					Device.CursorControl.Position = new Vector2Di(400, 300);
					VecteurAvant = new Vector3Df((float)Math.Cos(Rotation), 0, -(float)Math.Sin(Rotation));
					VecteurDroite = VecteurAvant;
					VecteurDroite.RotateXZby(-90);
				}


				for (int i = 0; i < Choses.Count; i++)
					Choses[i].MiseAJour(tempsEcoule, camera);


				Vector3Df vitesse = new Vector3Df();
				if (K_Avant)
					vitesse += VecteurAvant;
				else if (K_Arriere)
					vitesse -= VecteurAvant;
				if (K_Gauche)
					vitesse -= VecteurDroite;
				else if (K_Droite)
					vitesse += VecteurDroite;

				vitesse = vitesse.Normalize() * tempsEcoule * 2;


				if (TenterMouvement(camera, vitesse) || TenterMouvement(camera, new Vector3Df(vitesse.X, 0, 0)) || TenterMouvement(camera, new Vector3Df(0, 0, vitesse.Z)))
					camera.Target = camera.Position + VecteurAvant;

				Device.VideoDriver.BeginScene(ClearBufferFlag.Color | ClearBufferFlag.Depth, IrrlichtLime.Video.Color.OpaqueMagenta);
				Device.SceneManager.DrawAll();

				Device.VideoDriver.Draw2DImage(TexturePistolet[FramePistolet], new Recti(new Vector2Di(250, 300), new Dimension2Di(300, 300)), new Recti(0, 0, 512, 512), null, COULEUR_BLANC, true);
				DessinerHUD();
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

		private void DessinerHUD()
		{
			if (AlphaSang > 0)
				Device.VideoDriver.Draw2DRectangle(new Recti(0, 0, 800, 600), new IrrlichtLime.Video.Color(255, 0, 0, (int)AlphaSang));

			AfficherChiffre(0, 536, Vies);
			AfficherChiffre(608, 536, Munitions);
		}

		private void AfficherChiffre(int x, int y, int valeur)
		{
			int diviseur = 100;
			do
			{
				int chiffre = valeur / diviseur;
				Device.VideoDriver.Draw2DImage(TexturePolice,
				 new Recti(x, y, x + 64, y + 64),
				 new Recti(chiffre * 32, 0, (chiffre + 1) * 32, 32));
				x += 64;
				valeur -= chiffre * diviseur;
				diviseur /= 10;
			} while (diviseur > 0);
		}

		private bool Evenement(Event e)
		{
			if (Vies <= 0)
				return false;

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
					Tirer();
					FramePistolet = 1;
					ProchaineFramePistolet = 0.1f;
				}
			}


			return false;
		}

		public bool TenterMouvement(SceneNode objet, Vector3Df direction, float rayon = .25f)
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
					if (x < 0 || y < 0 || x >= 32 || y >= 32)
						return false;

					if (Murs[x, y])
						return false;
				}

			objet.Position += direction;
			return true;
		}


		private void Tirer()
		{
			Vector2Df pos = new Vector2Df(Device.SceneManager.ActiveCamera.Position.X, Device.SceneManager.ActiveCamera.Position.Z);
			Vector2Df v = new Vector2Df(VecteurAvant.X, VecteurAvant.Z) * 0.1f;

			//Console.WriteLine(pos);
			//Console.WriteLine(v);

			for (float f = 0; f < 128; f++)
			{
				for (int i = 0; i < Choses.Count; i++)
					if (Choses[i].Position.GetDistanceFrom(pos) < .25f)
					{
						if (PeutVoir(Device.SceneManager.ActiveCamera, Choses[i].Sprite))
							Choses[i].InfligerDegats(5);

						return;
					}

				pos += v;
			}
		}

		public bool PeutVoir(SceneNode objet1, SceneNode objet2)
		{
			Vector2Df pos = new Vector2Df(objet1.Position.X + .5f, objet1.Position.Z + .5f);
			Vector2Df pos2 = new Vector2Df(objet2.Position.X + .5f, objet2.Position.Z + .5f);
			float dist = pos.GetDistanceFrom(pos2);
			Vector2Df v = (pos2 - pos).Normalize() * 0.1f;

			for (float f = 0; f < dist; f += 0.1f)
			{
				Vector2Di posI = new Vector2Di((int)pos.X, (int)pos.Y);

				if ((posI.X < 0) || (posI.Y < 0) || (posI.X >= 32) || (posI.Y >= 32)) return false;
				if (Murs[posI.X, posI.Y]) return false;

				pos += v;
			}

			return true;
		}

		private void AjouterChose<T>(int x, int y) where T : Chose, new()
		{
			T nouvelleChose = new T();
			nouvelleChose.Creer(this, x, y);
			Choses.Add(nouvelleChose);
		}


	}

	public class Chose
	{
		public bool Detruit { get; private set; } = false;
		public BillboardSceneNode Sprite;
		protected Jeu Jeu;

		public Vector2Df Position { get { return new Vector2Df(Sprite.Position.X, Sprite.Position.Z); } }

		public void Creer(Jeu jeu, int x, int y)
		{
			Jeu = jeu;
			Sprite = Jeu.Device.SceneManager.AddBillboardSceneNode(null);
			Sprite.SetMaterialFlag(MaterialFlag.Lighting, false);
			Sprite.SetMaterialFlag(MaterialFlag.Fog, true);
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

		public virtual void MiseAJour(float tempsEcoule, CameraSceneNode camera) { }

		public virtual void InfligerDegats(int degats) { }
	}

	public class Ennemi : Chose
	{
		private int Frame = 0;
		private float IntervalleFrame = 0.15f;
		private float ProchaineAttaque = 1.5f;
		private int Vies = 10;
		public override void InfligerDegats(int degats) { Vies -= degats; }


		public override void MiseAJour(float tempsEcoule, CameraSceneNode camera)
		{
			if (Jeu.Vies <= 0)
				return;

			if (Vies <= 0)
			{
				if (Frame < 4) Frame = 4;

				IntervalleFrame -= tempsEcoule;
				if (IntervalleFrame < 0)
				{
					IntervalleFrame = 0.15f;
					Frame++;
					if (Frame > 6) Frame = 6;
					Sprite.SetMaterialTexture(0, Jeu.TextureGarde[Frame]);
				}
			}
			else
			{
				IntervalleFrame -= tempsEcoule;
				ProchaineAttaque -= tempsEcoule;

				if (ProchaineAttaque <= 0.0f)
				{
					if (!Jeu.PeutVoir(Sprite, camera))
					{
						ProchaineAttaque = 0.75f;
						return;
					}

					if (IntervalleFrame < 0)
					{
						if (Frame < 2) Frame = 2;
						else if (Frame == 2)
						{
							Attaquer();
							Frame = 3;
						}
						else
						{
							Frame = 0;
							ProchaineAttaque = 1.5f;
						}
						IntervalleFrame = 0.15f;
						Sprite.SetMaterialTexture(0, Jeu.TextureGarde[Frame]);
					}
				}
				else
				{
					Jeu.TenterMouvement(Sprite, (camera.Position - Sprite.Position) * tempsEcoule * .25f);

					if (IntervalleFrame < 0)
					{
						IntervalleFrame = 0.15f;
						Frame++;
						if (Frame > 1) Frame = 0;
						Sprite.SetMaterialTexture(0, Jeu.TextureGarde[Frame]);
					}
				}
			}
		}

		private void Attaquer()
		{
			Jeu.AlphaSang = 255;
			Jeu.Audio.Play2D(@"Sound\pistolet.wav");
			Jeu.Vies -= 5;
			Jeu.Device.SetWindowCaption("VIES: " + Jeu.Vies.ToString() + "%");
		}

	}

}

