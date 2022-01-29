using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;



namespace WarpDrive
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    //  HighLogic.CurrentGame.Parameters.CustomParams<WarpDrive>().

    public class WarpDrive : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } } // Column header
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "WarpDrive"; } }
        public override string DisplaySection { get { return "WarpDrive"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }



        [GameParameters.CustomParameterUI("Stock KSP Skin",
            toolTip = "Use the stock KSP skin")]
        public bool stockSkin = false;

        [GameParameters.CustomParameterUI("Unity Skin",
            toolTip = "use an alternative skin")]
        public bool unitySkin = true;

        [GameParameters.CustomParameterUI("Flat skin",
            toolTip = "Use a modern, flat skin")]
        public bool flatSkin = false;

        [GameParameters.CustomParameterUI("Enable Tooltip",
    toolTip = "tooltip")]
        public bool tooltip = true;

        //public enum PostPlacementMode { reload, noreload, jumpto };


        bool initted = false;
        bool oldStockSkin, oldSecSkin, oldFlatSkin;
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (!stockSkin && !unitySkin && !flatSkin)
                stockSkin = true;
            if (initted)
            {
                if (stockSkin && !oldStockSkin)
                {
                    unitySkin = false;
                    flatSkin = false;
                    Styles.lastSkin = Styles.Skin.none;
                    //Styles.InitStyles();
                }
                if (unitySkin && !oldSecSkin)
                {
                    stockSkin = false;
                    flatSkin = false;
                    Styles.lastSkin = Styles.Skin.none;
                    //Styles.InitStyles();
                }
                if (flatSkin && !oldFlatSkin)
                {
                    stockSkin = false;
                    unitySkin = false;
                    Styles.lastSkin = Styles.Skin.none;
                    //Styles.InitStyles();
                }
            }
            else
                initted = true;

            oldStockSkin = stockSkin;
            oldSecSkin = unitySkin;
            oldFlatSkin = flatSkin;

            return true; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }

    }
}
