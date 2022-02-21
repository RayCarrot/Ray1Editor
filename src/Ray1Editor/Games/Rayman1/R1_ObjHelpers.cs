using BinarySerializer.Ray1;

namespace Ray1Editor;

public static class R1_ObjHelpers
{
    public static byte GetDisplayPrio(ObjType type, int hitPoints, World world, int level)
    {
        switch (type)
        {
            case ObjType.TYPE_AUDIOSTART:
            case ObjType.TYPE_SPACE_MAMA:
            case ObjType.TYPE_NEIGE:
            case ObjType.TYPE_PALETTE_SWAPPER:
            case ObjType.TYPE_GENERATING_DOOR:
            case ObjType.TYPE_SCROLL_SAX:
            case ObjType.TYPE_BB1_VIT:
            case ObjType.TYPE_BLACK_RAY:
            case ObjType.TYPE_SPACE_MAMA2:
            case ObjType.TYPE_ANNULE_SORT_DARK:
            case ObjType.TYPE_POING_FEE:
            case ObjType.TYPE_DUNE:
                return 0;

            case ObjType.TYPE_CLE_SOL:
            case ObjType.TYPE_EAU:
            case ObjType.TYPE_MEDAILLON_TOON:
                return 1;

            case ObjType.TYPE_BOUM:
            case ObjType.TYPE_SPLASH:
            case ObjType.TYPE_PHOTOGRAPHE:
            case ObjType.TYPE_OUYE:
            case ObjType.TYPE_MOVE_OUYE:
            case ObjType.TYPE_FLAMME2:
            case ObjType.TYPE_STALAG:
            case ObjType.TYPE_PAILLETTE:
            case ObjType.TYPE_PIRATE_BOMB:
            case ObjType.TYPE_STONECHIP:
            case ObjType.TYPE_EXPLOSION:
            case ObjType.TYPE_NOTE0:
            case ObjType.TYPE_NOTE1:
            case ObjType.TYPE_NOTE2:
            case ObjType.TYPE_BONNE_NOTE:
            case ObjType.TYPE_POING:
            case ObjType.TYPE_BNOTE:
            case ObjType.TYPE_PI_BOUM:
            case ObjType.TYPE_ECLAIR:
            case ObjType.TYPE_ETINC:
            case ObjType.TYPE_NOVA2:
            case ObjType.TYPE_FLASH:
            case ObjType.TYPE_SCORPION:
            case ObjType.TYPE_WIZ:
            case ObjType.TYPE_CYMBAL1:
            case ObjType.TYPE_RAYON:
            case ObjType.TYPE_PIERREACORDE:
            case ObjType.TYPE_CFUMEE:
            case ObjType.TYPE_STOSKO_PINCE:
            case ObjType.TYPE_LAVE:
            case ObjType.TYPE_SKO_PINCE:
                return 2;

            case ObjType.TYPE_ONOFF_PLAT:
            case ObjType.TYPE_CLASH:
            case ObjType.TYPE_BB1:
            case ObjType.TYPE_CAGE:
            case ObjType.TYPE_CAGE2:
            case ObjType.TYPE_DARD:
            case ObjType.TYPE_PIRATE_NGAWE:
            case ObjType.TYPE_RING:
            case ObjType.TYPE_SAXO:
            case ObjType.TYPE_PIRATE_GUETTEUR:
            case ObjType.TYPE_MARACAS:
            case ObjType.TYPE_BBL:
            case ObjType.TYPE_TNT_BOMB:
            case ObjType.TYPE_SUPERHELICO:
            case ObjType.TYPE_ROULETTE:
            case ObjType.TYPE_ROULETTE2:
            case ObjType.TYPE_ROULETTE3:
            case ObjType.TYPE_SAXO2:
            case ObjType.TYPE_SAXO3:
            case ObjType.TYPE_MAMA_PIRATE:
            case ObjType.TYPE_COUTEAU:
            case ObjType.TYPE_BB12:
            case ObjType.TYPE_BB13:
            case ObjType.TYPE_BB14:
            case ObjType.TYPE_SMA_WEAPON:
            case ObjType.TYPE_BOUT_TOTEM:
            case ObjType.TYPE_PIRATE_GUETTEUR2:
            case ObjType.TYPE_RIDEAU:
                return 3;

            default:
                return 4;

            case ObjType.TYPE_PLANCHES:
                return 4; // Note: This is 7 if there is no pirate ship event in the level

            case ObjType.TYPE_MORNINGSTAR:
            case ObjType.TYPE_GENEBADGUY:
            case ObjType.TYPE_TOTEM:
            case ObjType.TYPE_PI:
            case ObjType.TYPE_PI_MUS:
            case ObjType.TYPE_WASHING_MACHINE:
            case ObjType.TYPE_CORDE_DARK:
            case ObjType.TYPE_VAGUE_DEVANT:
                return 5;

            case ObjType.TYPE_POWERUP:
            case ObjType.TYPE_ONEUP_ALWAYS:
            case ObjType.TYPE_MUS_WAIT:
            case ObjType.TYPE_JAUGEUP:
            case ObjType.TYPE_POING_POWERUP:
            case ObjType.TYPE_REDUCTEUR:
            case ObjType.TYPE_ONEUP:
            case ObjType.TYPE_GRAP_BONUS:
            case ObjType.TYPE_BATEAU:
                return 6;

            case ObjType.TYPE_NEUTRAL:
            case ObjType.TYPE_SIGNPOST:
            case ObjType.TYPE_TAMBOUR2:
            case ObjType.TYPE_VAGUE_DERRIERE:
                return 7;

            case ObjType.TYPE_BLACKTOON1:
                return (byte)(world == World.Jungle && level == 14 ? 3 : 2);

            case ObjType.TYPE_MST_SCROLL:
                return (byte)(hitPoints < 1 ? 2 : 0);
        }
    }

    public static byte GetDisplayPrio_Kit(ObjType type, byte hitPoints, bool inEditor)
    {
        switch ((int)type)
        {
            case 245:
                return 0;

            case 10:
            case 161:
                return 2;

            case 263:
                return 7;
        }

        var v1 = (int)type;
        var v2 = v1 + 100;

        if (v2 >= 110u)
        {
            //if (v2 <= 110u)
            //    return 2;

            if (v2 >= 161u)
            {
                //if (v2 <= 161u)
                //    return 2;

                if (v2 >= 245u)
                {
                    //if (v2 <= 245u)
                    //    return 0;

                    if (v2 >= 263u)
                    {
                        //if (v2 <= 263u) 
                        //    return 7;
                            
                        if (v2 < 269u)
                            return 4;
                                
                        if (v2 <= 270u) 
                            return 2;
                                
                        if (v2 < 278u) 
                            return 4;
                                    
                        if (v2 > 279u)
                            return 4;

                        if (inEditor)
                            return 2;
                            
                        return 0;
                    }
                    if (v2 < 253u)
                    {
                        if (v2 != 246)
                            return 4;
                    }
                    else
                    {
                        if (v2 <= 253u)
                            return 7;
                        if (v2 != 262)
                            return 4;
                    }
                    return 5;
                }
                if (v2 >= 204u)
                {
                    if (v2 <= 204u)
                        return 0;
                    if (v2 >= 234u)
                    {
                        if (v2 <= 234u) 
                            return 2;
                            
                        if (v2 != 236)
                            return 4;
                            
                        return 0;
                    }

                    if (v2 < 220u) 
                        return 4;
                        
                    if (v2 > 221u)
                        return 4;
                        
                    return 2;
                }
                if (v2 >= 168u)
                {
                    if (v2 > 168u)
                        return 4;
                    return 2;
                }
                if (v2 != 164)
                    return 4;

                if (inEditor)
                    return 2;
                return 0;
            }
            if (v2 < 142u)
            {
                if (v2 < 123u)
                {
                    if (v2 < 119u)
                    {
                        if (v2 != 111)
                            return 4;
                        return 5;
                    }

                    if (v2 <= 119u) 
                        return 3;
                        
                    if (v2 != 121)
                        return 4;
                        
                    return 2;
                }
                if (v2 <= 123u)
                    return 2;
                if (v2 < 137u)
                {
                    if (v2 != 133)
                        return 4;
                    return 3;
                }

                if (v2 <= 137u) 
                    return 6;
                    
                if (v2 != 138)
                    return 4;
                    
                return 3;
            }
            else if (v2 > 142u)
            {
                if (v2 < 148u)
                {
                    if (v2 < 146u)
                    {
                        if (v2 != 143)
                            return 4;
                    }
                    else if (v2 > 146u && hitPoints != 0)
                    {
                        return 0;
                    }
                    return 2;
                }
                if (v2 > 148u)
                {
                    if (v2 < 154u)
                    {
                        if (v2 != 149)
                            return 4;
                        return 1;
                    }
                    if (v2 > 155u)
                    {
                        if (v2 != 157)
                            return 4;
                        return 1;
                    }
                    return 3;
                }
            }
            return 6;
        }
        if (v2 < 48u)
        {
            if (v2 >= 21u)
            {
                if (v2 <= 21u)
                    return 2;
                if (v2 >= 41u)
                {
                    if (v2 <= 41u)
                        return 2;
                    if (v2 < 44u)
                    {
                        if (v2 != 42)
                            return 4;
                        return 7;
                    }
                    if (v2 > 44u)
                    {
                        if (v2 != 45)
                            return 4;
                        return 2;
                    }
                    return 3;
                }
                if (v2 < 31u)
                {
                    if (v2 != 28)
                        return 4;
                    return 3;
                }
                if (v2 > 31u)
                {
                    if (v2 != 39)
                        return 4;
                    return 5;
                }
                return 6;
            }
            if (v2 >= 7u)
            {
                if (v2 <= 7u) 
                    return 5;
                    
                if (v2 < 19u)
                {
                    if (v2 != 11)
                        return 4;
                    return 2;
                }
                    
                if (v2 <= 19u)
                    return 2;
                return 5;
            }
                
            if (v2 >= 2u)
            {
                if (v2 > 2u)
                {
                    if (v2 != 4)
                        return 4;
                    return 7;
                }
                return 6;
            }
        }
        else
        {
            if (v2 <= 48u)
                return 2;
            if (v2 < 83u)
            {
                if (v2 < 75u)
                {
                    if (v2 >= 57u)
                    {
                        if (v2 <= 57u)
                            return 2;
                        if (v2 != 66)
                            return 4;
                        return 3;
                    }
                    if (v2 != 55)
                        return 4;
                }
                else
                {
                    if (v2 <= 75u)
                        return 2;
                    if (v2 < 79u)
                    {
                        if (v2 != 76)
                            return 4;
                        if (inEditor)
                            return 2;
                        return 0;
                    }
                    if (v2 <= 79u)
                        return 2;
                    if (v2 != 82)
                        return 4;
                }
                return 6;
            }
            if (v2 <= 83u)
                return 2;
            if (v2 >= 95u)
            {
                if (v2 > 95u)
                {
                    if (v2 < 102u)
                    {
                        if (v2 != 99)
                            return 4;
                        return 0;
                    }
                    if (v2 <= 102u)
                        return 2;
                    if (v2 != 109)
                        return 4;
                    return 5;
                }
                return 6;
            }
            if (v2 < 88u)
            {
                if (v2 != 86)
                    return 4;
                return 3;
            }
            if (v2 <= 88u)
                return 7;
            if (v2 >= 90u)
                return 2;
        }
        return 4;
    }

    /// <summary>
    /// Indicates if the HitPoints value is the current frame
    /// </summary>
    /// <param name="et">The event type</param>
    /// <returns></returns>
    public static bool IsHPFrame(this ObjType et) => et == ObjType.TYPE_PUNAISE4 ||
                                                     et == ObjType.TYPE_FALLING_CRAYON ||
                                                     et == ObjType.EDU_ArtworkObject;

    // TODO: Support multi-colored object sprites for EDU/KIT/FAN
    /// <summary>
    /// Indicates if the HitPoints value is the sub-palette to use
    /// </summary>
    /// <param name="et">The event type</param>
    /// <returns></returns>
    public static bool IsMultiColored(this ObjType et) => et == ObjType.TYPE_EDU_LETTRE ||
                                                          et == ObjType.TYPE_EDU_CHIFFRE ||
                                                          et == ObjType.MS_compteur ||
                                                          et == ObjType.MS_wiz_comptage ||
                                                          et == ObjType.MS_pap;

    /// <summary>
    /// Indicates if the event frame should be retained from the editor
    /// </summary>
    /// <param name="et">The event type</param>
    /// <returns></returns>
    public static bool UsesEditorFrame(this ObjType et) => et == ObjType.TYPE_EDU_LETTRE ||
                                                           et == ObjType.TYPE_EDU_CHIFFRE;

    /// <summary>
    /// Indicates if the linked event frames should be randomized in order
    /// </summary>
    /// <param name="et">The event type</param>
    /// <returns></returns>
    public static bool UsesRandomFrameLinks(this ObjType et) => et == ObjType.TYPE_HERSE_HAUT ||
                                                                et == ObjType.TYPE_HERSE_BAS;

    /// <summary>
    /// Indicates if the event frame should be randomized
    /// </summary>
    /// <param name="et">The event type</param>
    /// <returns></returns>
    public static bool UsesRandomFrame(this ObjType et) => et == ObjType.TYPE_CRAYON_BAS ||
                                                           et == ObjType.TYPE_CRAYON_HAUT ||
                                                           et == ObjType.TYPE_HERSE_HAUT ||
                                                           et == ObjType.TYPE_HERSE_BAS;

    /// <summary>
    /// Indicates if the event frame is from the link chain
    /// </summary>
    /// <param name="et">The event type</param>
    /// <returns></returns>
    public static bool UsesFrameFromLinkChain(this ObjType et) => et == ObjType.TYPE_HERSE_BAS_NEXT ||
                                                                  et == ObjType.TYPE_HERSE_HAUT_NEXT;
}