using System;
using System.Collections.Generic;
using System.Text;
using Mogre;

namespace Ymfas {
    class TextRenderer {

        public static void AddTextBox(String id, String text, float x, float y, float width, float height, ColourValue colorTop, ColourValue colorBot) {
            try {       
                OverlayManager overlayMgr = OverlayManager.Singleton;
                OverlayContainer panel = (OverlayContainer)overlayMgr.CreateOverlayElement("Panel", "_panel_" + id);
                panel.MetricsMode = GuiMetricsMode.GMM_PIXELS;
                panel.SetPosition(x, y);
                panel.SetDimensions(width, height);

                TextAreaOverlayElement textArea = (TextAreaOverlayElement)overlayMgr.CreateOverlayElement("TextArea", "_area_" + id);
                textArea.MetricsMode = GuiMetricsMode.GMM_PIXELS;
                textArea.SetPosition(0, 0);
                textArea.SetDimensions(width, height);
                textArea.Caption = text;
                textArea.CharHeight = 16;
                textArea.FontName = "Verdana";
                textArea.ColourTop = colorTop;
                textArea.ColourBottom = colorBot;
                Overlay overlay = overlayMgr.Create("_overlay_" + id);
                overlay.Add2D(panel);
                panel.AddChild(textArea);
                overlay.Show();
            }
            catch (Exception e) {
                Util.Log("Unable to create text area.");
            }
        }
        public static void UpdateTextBox(String id, String text) {
            try {
                OverlayManager overlayMgr = OverlayManager.Singleton;
                TextAreaOverlayElement textArea = (TextAreaOverlayElement)overlayMgr.GetOverlayElement("_area_" + id);
                textArea.Caption = text;
            }
            catch (Exception e) {
                Util.Log("Unable to update text area.");
            }
        }
    }
}
