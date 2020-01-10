using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public class PanelManager {

            private List<IMyTextPanel> panels = null;

            /*  Constructor */
            public PanelManager(IMyTextPanel block) {
                panels = new List<IMyTextPanel>();
                AddPanel(block);
            }
            /*  OVERLOAD    */
            public PanelManager(List<IMyTextPanel> blocks) {
                panels = new List<IMyTextPanel>();
                AddPanels(blocks);
            }

            /*  Add Panel   */
            public void AddPanel(IMyTextPanel block) {
                if (block != null) {
                    panels.Add(block);
                }
            }

            /*  Add a range of Panels  */
            public void AddPanels(List<IMyTextPanel> blocks) {
                if (blocks != null) {
                    for (int e = 0; e < blocks.Count; e++) {
                        if (!panels.Contains(blocks[e])) {
                            AddPanel(blocks[e]);
                        }
                    }
                }
            }

            /*  Remove Panel    */
            public void RemovePanel(IMyTextPanel block) {
                if (block != null) {
                    panels.Remove(block);
                }
            }

            /*  Get Public Title */
            public string GetTitle() {
                if (panels.Count >= 1) {
                    return panels[0].GetPublicTitle();
                }
                else {
                    return null;
                }
            }

            /*  Set Public Title    */
            public int SetTitle(string txt, bool append) {
                int count = 0;
                int error = 0;
                for (int e = 0; e < panels.Count; e++) {
                    if (panels[e].WritePublicTitle(txt, append)) {
                        count += 1;
                    }
                    else {
                        error -= 1;
                    }
                }
                if (error != 0) {
                    return error;
                }
                else {
                    return count;
                }
            }

            /*  Get Text    */
            public string GetText(int index) {
                if (panels.Count >= 1) {
                    return panels[index].GetText();
                }
                else {
                    return null;
                }
            }

            /*  Set Text   */
            public int SetText(string txt, bool append) {
                int count = 0;
                int error = 0;
                for (int e = 0; e < panels.Count; e++) {
                    if (panels[e].WriteText(txt, append)) {
                        TogglePanel(panels[e]);
                        count += 1;
                    }
                    else {
                        error -= 1;
                    }
                }
                if (error != 0) {
                    return error;
                }
                else {
                    return count;
                }
            }

            /*  Set Font Size */
            public void SetFontSize(Single size) {
                for (int e = 0; e < panels.Count; e++) {
                    panels[e].SetValue("FontSize", size);
                }
            }

            /*  Set Font Colour */
            public void SetFontColor(Color col) {
                for (int e = 0; e < panels.Count; e++) {
                    panels[e].SetValue("FontColor", col);
                }
            }

            /*  Set Background Colour   */
            public void SetBackColor(Color col) {
                for (int e = 0; e < panels.Count; e++) {
                    panels[e].SetValue("BackgroundColor", col);
                }
            }

            public void SetStatus(bool on) {
                if (panels.Count != 0) {
                    string Act = "OnOff_Off";
                    if (on) {
                        Act = "OnOff_On";
                    }
                    var Action = (panels[0]).GetActionWithName(Act);
                    if (Action != null) {
                        for (int e = 0; e < panels.Count; e++) {
                            Action.Apply(panels[e]);
                        }
                    }
                }
            }

            public void TogglePanel(IMyTextPanel Block) {
                if (Block != null && Block.Enabled) {
                    var off = Block.GetActionWithName("OnOff_Off");
                    var on = Block.GetActionWithName("OnOff_On");
                    if (off != null && on != null) {
                        off.Apply(Block);
                        on.Apply(Block);
                    }
                }
            }
        }
    }
}
