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

            while (Device.Run())
            {
                Device.VideoDriver.BeginScene(ClearBufferFlag.Color | ClearBufferFlag.Depth, Color.OpaqueMagenta);
                Device.VideoDriver.EndScene();
            }
        }
    }
}

